using Oss.Core;
using System.Collections.Generic;
using System.Linq;

namespace Oss
{
    public class Merger
    {
        public string MergeVars(string newstr, string curstr, out List<OssItem> removed)
        {
            var newr = Reader.Read(newstr);
            var curr = Reader.Read(curstr);
            var res = new List<OssItem>();

            var newvars = newr.Vars;
            var keys = newvars.Keys.ToArray();
            var vari = 0;
            var skip = new List<string>();
            removed = new List<OssItem>();

            foreach (var item in curr.Items)
            {
                void Add()
                {
                    var neededVar = keys[vari];
                    vari++;

                    if (item.VarName == neededVar)
                    {
                        res.Add(item);
                    }
                    else
                    {
                        if (curr.Vars.ContainsKey(neededVar))
                        {
                            skip.Add(neededVar);
                            res.Add(curr.Vars[neededVar]);
                        }
                        else
                        {
                            res.Add(newvars[neededVar]);
                        }

                        Add();
                    }
                }

                if (item.VarName == null)
                {
                    res.Add(item);
                }
                else
                {
                    if (!newvars.ContainsKey(item.VarName))
                    {
                        removed.Add(item);
                        continue;
                    }

                    if (skip.Contains(item.VarName) || vari >= keys.Length)
                    {
                        continue;
                    }

                    Add();
                }
            }
            
            return RenderItems(res);
        }

        public string RenderItems(IList<OssItem> res)
        {
            var sres = string.Empty;

            for (var i = 0; i < res.Count; i++)
            {
                var item = res[i];
                if (item.VarName == null)
                {
                    sres += item.Val;
                    if (i != res.Count - 1)
                    {
                        sres += "\r\n";
                    }
                }
                else
                {
                    var val = item.Val;

                    if (item.IsBlock)
                    {
                        sres += "\r\n";

                        if (val.Trim(StrConst.EmptyChars).Length == 0)
                        {
                            val = "{}";
                        }
                        else
                        {
                            var tab = string.Empty;
                            if (!val.Contains("{"))
                            {
                                tab = "\t";
                            }

                            val = $"{{\r\n{tab}{val}\r\n}}";
                        }
                    }

                    sres += $"var {item.VarName} = {val};\r\n";
                }
            }

            return sres;
        }
    }
}