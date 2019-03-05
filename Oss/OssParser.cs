using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Oss.Core;

namespace Oss
{
    public class OssParser
    {
        private readonly ILogger logger = new Logger();
        private readonly FileOp fileOp = new FileOp();

        public ParseRes Parse(string inputPath, string outputPath, bool watch, string dir = null)
        {
            var rootPath = dir ?? new FileInfo(inputPath).Directory?.FullName ?? string.Empty;

            var watchList = new List<string> { inputPath };
            var watchers = new List<FileSystemWatcher>();

            inputPath = Path.Combine(rootPath, inputPath);
            outputPath = Path.Combine(rootPath, outputPath);

            var parseRes = ParseFile();

            watchList.AddRange(parseRes.InsertedFiles);

            if (watch)
            {
                foreach (var inputFile in watchList)
                {
                    var watcher = new FileSystemWatcher
                    {
                        Path = rootPath,
                        NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.CreationTime,
                        Filter = Path.GetFileName(inputFile),
                    };

                    // Add event handlers.
                    watcher.Changed += (sender, eventArgs) => { ParseFile(); };
                    watcher.Renamed += (sender, eventArgs) => { ParseFile(); };

                    // Begin watching.
                    watcher.EnableRaisingEvents = true;

                    watchers.Add(watcher);

                    logger.Message($"watching changes for {inputFile}");
                }
            }

            return new ParseRes
            {
                Watchers = watchers,
                Errors = parseRes.Errors
            };

            OssParseResult ParseFile()
            {
                // read input file
                var str = fileOp.ReadText(inputPath);

                var res = ParseOssString(str, rootPath);

                fileOp.WriteText(outputPath, res.ParseRes);
                Console.WriteLine("parsed to {0} at {1}", outputPath, DateTime.Now);

                return res;
            }
        }

        public OssParseResult ParseOssString(string str, string rootPath)
        {
            int errors = 0;
            var insertedList = new List<string>();
            var lres = new List<Rule>();
            var namedRules = new Dictionary<string, Rule>();
            var rheader = string.Empty;

            // named rule, used for inheritance
            string ruleName = null;

            var vars = new SortedDictionary<string, OssVar>(new ReverseComparer<string>(StringComparer.InvariantCulture));

            void ClearEmpty()
            {
                if (rheader.Trim(StrConst.EmptyChars).Length == 0)
                {
                    rheader = string.Empty;
                }
            }

            void AddHeaderToBase(IEnumerable<string> inherits)
            {
                if (inherits == null) return;

                foreach (var name in inherits)
                {
                    if (!namedRules.ContainsKey(name))
                    {
                        logger.Error($"could not find name {name} to inherit");
                        errors++;
                        continue;
                    }

                    //if (namedRules[name].IsEmpty)
                    //{
                    //    logger.Info($"inheriting empty rule {name}, statement can be removed");
                    //}

                    var baseRule = namedRules[name];

                    if (baseRule.Header != null)
                    {
                        baseRule.Header = StrUtil.InsertStrBeforeEmptys(baseRule.Header,
                        ",\r\n" + StrUtil.GetLastIndentation(baseRule.Header) + StrUtil.TrimEmptys(rheader));
                    }
                    else
                    {
                        baseRule.Header = rheader;
                    }

                    AddHeaderToBase(baseRule.Inherits);
                }
            }

            for (var i = 0; i < str.Length; i++)
            {
                if (str.IsNextStr(i, OssConst.VarDeclKey))
                {
                    // var = x / {x:y} / { header { x: y } .. };

                    i += OssConst.VarDeclKey.Length;

                    var varname = str.GetVarName(i, out i);
                    var varval = str.GetVarVal(i, out i);
                    var keepraw = false;

                    if (varname[0] == ':')
                    {
                        keepraw = true;
                        varname = varname.Substring(1);
                    }

                    var osv = new OssVar();
                    osv.Name = varname;
                    osv.IsRules = varval.IsRules;

                    osv.Val = osv.IsRules || keepraw ? new ParseContentRes { Content = varval.Val, IsRaw = true } : ParseContent(varval.Val, vars);

                    osv.Val.RawContent = varval.Val;

                    vars.Add(varname, osv);

                    if (i < str.Length - 1 && str[i + 1] == ';') i++;

                    rheader = rheader.TrimEnd(StrConst.EmptyChars);
                }
                else if (str.IsNextStr(i, "{"))
                {
                    // rule block

                    var endi = str.GetBlockEnd(i);

                    var block = str.FromTo(i, endi);
                    i = endi;
                    var vpr = ParseContent(block, vars);

                    var val = vpr.Content;

                    var lrule = new Rule();

                    if (rheader.Trim(StrConst.EmptyChars) == string.Empty)
                    {
                        rheader = null;
                        lrule.EmptyHeader = true;
                    }

                    lrule.Header = rheader;
                    lrule.Body = val;
                    lrule.Name = ruleName;
                    lrule.Inherits = vpr.Inherits;
                    lrule.IsEmpty = val.Trim(StrConst.RuleTrimChars).Length == 0;

                    lres.Add(lrule);

                    if (ruleName != null)
                    {
                        if (namedRules.ContainsKey(ruleName))
                        {
                            logger.Error($"name {ruleName} already defined");
                        }

                        namedRules.Add(ruleName, lrule);
                    }

                    if (vpr.Inherits.Any())
                    {
                        AddHeaderToBase(vpr.Inherits);
                    }

                    rheader = string.Empty;
                    ruleName = null;
                }
                else if (str.IsNextStr(i, OssConst.InsertKey))
                {
                    // insert file.txt content

                    i += OssConst.InsertKey.Length;
                    var url = str.GetNextWordToSemicol(i, out i);
                    var insPath = rootPath + "\\" + url;

                    // add inserted file to watchlist
                    insertedList.Add(insPath);

                    // read inserted file
                    var instr = fileOp.ReadText(insPath);
                    str = str.Insert(i + 1, instr);
                }
                else if (str.IsNextStr(i, OssConst.NameKey))
                {
                    // define rule name (for inheritance)

                    i += OssConst.NameKey.Length;
                    ruleName = str.GetNextWordToSemicol(i, out i);
                }
                else if (str.IsNextStr(i, OssConst.Atvar))
                {
                    // insert var content

                    i += OssConst.Atvar.Length;
                    var name = str.GetNextWordToSemicol(i, out i);
                    if (vars.ContainsKey(name))
                    {
                        var osv = vars[name];
                        if (string.IsNullOrWhiteSpace(osv.Val.RawContent))
                        {
                            ClearEmpty();
                        }
                        else
                        {
                            str = str.Insert(i + 1, osv.Val.RawContent);
                        }
                    }
                    else
                    {
                        ClearEmpty();
                        logger.Info($"could not find var {name}");
                    }
                }
                else
                {
                    var c = str[i];
                    rheader += c;
                }
            }

            lres.Add(new Rule { IsComment = true, Body = rheader });

            var slres = string.Empty;

            foreach (var rule in lres.Where(o => !o.IsEmpty && o.Header != null))
            {
                slres += rule.Header + rule.Body;
            }

            slres = slres.Trim(StrConst.EmptyChars);

            return new OssParseResult
            {
                ParseRes = slres,
                InsertedFiles = insertedList,
                Errors = errors
            };
        }

        public ParseContentRes ParseContent(string str, IDictionary<string, OssVar> vars)
        {
            IDictionary<string, OssVar> localVars = null;

            var res = string.Empty;
            var inherits = new List<string>();

            for (var i = 0; i < str.Length; i++)
            {
                var skipChar = false;
                var varfound = false;

                if (str.IsNextStr(i, OssConst.InheritKey))
                {
                    // inherit

                    i += OssConst.InheritKey.Length;
                    var name = str.GetNextWordToSemicol(i, out i);
                    inherits.Add(name);
                    res = res.TrimEnd(StrConst.EmptyChars); // rem space left from inherit line
                    continue;
                }

                if (str.IsNextStr(i, OssConst.CalcKey))
                {
                    i += OssConst.CalcKey.Length - 1;
                    var endi = StrUtil.GetBlockEndPrn(str, i);
                    if (endi < 0)
                    {
                        throw new Exception("can't find end of calc(");
                    }

                    var expr = str.FromTo(i + 1, endi - 1);

                    var pexpr = ParseContent(expr, vars).Content;

                    var val = ExprCalc.Eval(pexpr);

                    if (val == (int)val)
                    {
                        res = res + val;
                    }
                    else
                    {
                        res = res + val.ToString("0.##");
                    }

                    i = endi;
                    skipChar = true;
                }

                var isvar = str.IsNextStr(i, OssConst.VarKey);
                var isvarprn = str.IsNextStr(i, OssConst.VarPrnKey);

                if (isvar || isvarprn)
                {
                    // var.x

                    string varname = null;
                    if (isvar)
                    {
                        i += OssConst.VarKey.Length;
                        foreach (var key in vars.Keys)
                        {
                            if (!str.IsNextStr(i, key)) continue;
                            i += key.Length - 1;
                            varname = key;
                            varfound = true;
                            skipChar = true;
                            break;
                        }
                    }
                    else
                    {
                        i += OssConst.VarPrnKey.Length;
                        var endi = StrUtil.GetBlockEndPrn(str, i);
                        varname = str.FromTo(i, endi - 1).Trim();

                        i = endi;

                        var psi = str.FindNextNonEmpty("{", i + 1);

                        if (psi > -1)
                        {
                            var pei = str.GetBlockEnd(psi);

                            if (pei < 0)
                            {
                                logger.Error($"can't find closing bracket for var({varname}) parameters ')' ");
                            }

                            var pcon = str.FromTo(psi + 1, pei - 1);

                            var parms = ParamsParser.Parse(pcon);

                            localVars = new Dictionary<string, OssVar>();

                            foreach (var parm in parms)
                            {
                                var osv = new OssVar();
                                osv.Name = parm.Key;
                                osv.IsRules = parm.Value.Contains("{");
                                osv.Val = osv.IsRules ? new ParseContentRes { Content = parm.Value } : ParseContent(parm.Value, vars);
                                //osv.Val.RawContent = parm.Value;
                                localVars.Add(parm.Key, osv);
                            }

                            i = pei;
                        }

                        varfound = true;
                        skipChar = true;
                    }

                    if (varfound)
                    {
                        var vpr = vars[varname].Val;
                        var val = vpr.Content;

                        if (localVars != null)
                        {
                            foreach (var item in vars)
                            {
                                if (!localVars.ContainsKey(item.Key))
                                {
                                    localVars.Add(item);
                                }
                            }

                            val = ParseContent(vpr.RawContent, localVars).Content;
                        }
                        else if (vpr.IsRaw)
                        {
                            val = ParseContent(vpr.RawContent, vars).Content;
                        }

                        // get indentation 
                        if (val.Contains(';'))
                        {
                            var nli = res.LastIndexOf('\n');
                            if (nli > 0)
                            {
                                var space = res.Substring(nli + 1);
                                if (space.Length > 0 && space.Trim(' ', '\t').Length == 0)
                                {
                                    // set indentation in val
                                    var arr = val.Split('\n');
                                    for (var j = 1; j < arr.Length; j++)
                                    {
                                        arr[j] = space + arr[j].TrimStart();
                                    }

                                    val = string.Join("\n", arr);
                                }
                            }
                        }

                        if (val == "rem")
                        {
                            res = res.TrimEnd();
                            var spacei = res.LastIndexOfAny(new[] { ' ', '\t' });
                            if (spacei > 0)
                            {
                                res = res.Remove(spacei).TrimEnd(new[] { ' ', '\t' });
                            }

                            val = string.Empty;
                        }

                        res += val;

                        if (vpr.Inherits != null)
                        {
                            inherits.AddRange(vpr.Inherits);
                        }

                        if ((val.EndsWith(";") || val == string.Empty) && str.Length > i + 1 && str[i + 1] == ';') i++;

                        if (val == string.Empty)
                        {
                            var nli = str.IndexOf("\n", i, StringComparison.Ordinal);
                            if (nli > 0)
                            {
                                if (str.FromTo(i + 1, nli).Trim(StrConst.EmptyChars) == string.Empty)
                                {
                                    i = nli;
                                    res = res.TrimEnd(' ', '\t');
                                }
                            }
                        }
                    }
                    else
                    {
                        logger.Error("could not find var at " + str.GetNextWordToSemicol(i, out _));
                    }
                }

                if (!skipChar)
                {
                    res += str[i];
                }
            }

            return new ParseContentRes { Content = res, Inherits = inherits };
        }
    }
}