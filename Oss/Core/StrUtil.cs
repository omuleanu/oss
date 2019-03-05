using System;
using System.Linq;

namespace Oss.Core
{
    public static class StrUtil
    {
        public static VarVal GetVarVal(this string src, int starti, out int endi)
        {
            var emptyChars = StrConst.EmptyChars;
            bool isBlock = false;
            bool isRules = false;

            while (emptyChars.Contains(src[starti]))
            {
                starti++;
            }

            var result = new VarVal();

            if (src[starti] == '{')
            {
                isBlock = true;
                endi = GetBlockEnd(src, starti);
                var res = src.FromTo(starti + 1, endi - 1).TrimEnd(emptyChars);

                if (res.Length > 0 && res[res.Length - 1] == '}')
                {
                    isRules = true;
                    // rules var
                    var lines = res.Split("\r\n").ToList();

                    foreach (var line in lines)
                    {
                        if (line.All(c => emptyChars.Contains(c)))
                        {
                            lines.Remove(line);
                            break;
                        }
                    }

                    res = string.Join("\r\n", StrUtil.RemStartSpace(lines.ToArray()));
                }
                else
                {
                    res = res.TrimStart(emptyChars);
                }

                result.Val = res;
            }
            else
            {
                endi = src.IndexOf(';', starti);
                result.Val = src.FromTo(starti, endi - 1);
            }

            result.IsBlock = isBlock;
            result.IsRules = isRules;

            return result;
        }

        public static int GetBlockEndPrn(this string src, int starti)
        {
            return GetBlockEnd(src, starti, new[] { '(', ')' });
        }

        /// <summary>
        /// returns last when index when not found
        /// </summary>
        /// <param name="src"></param>
        /// <param name="starti"></param>
        /// <param name="blockChars">chars used to determine the block start and end</param>
        /// <returns></returns>
        public static int GetBlockEnd(this string src, int starti, char[] blockChars = null)
        {
            if (blockChars == null)
            {
                blockChars = new char[] { '{', '}' };
            }

            var count = 1;
            starti += 1;
            for (var i = starti; i < src.Length; i++)
            {
                if (src[i] == blockChars[0]) count++;
                if (src[i] == blockChars[1]) count--;

                if (count == 0)
                {
                    return i;
                }
            }

            return src.Length - 1;
        }

        public static string GetVarName(this string src, int starti, out int endi)
        {
            var nonVarNameChars = StrConst.EmptyChars;

            var eqsi = src.IndexOf('=', starti);

            var name = src.FromTo(starti, eqsi - 1).Trim();

            if (name.Any(c => nonVarNameChars.Contains(c)))
            {
                throw new Exception("invalid variable name: " + name);
            }

            endi = eqsi + 1;

            return name;
        }

        public static string GetNextWordToSemicol(this string src, int starti, out int endi)
        {
            var nonUrlChars = new[] { '\n', '\r' };

            var signi = src.IndexOf(';', starti);

            var name = src.FromTo(starti, signi - 1).Trim();

            if (name.Any(c => nonUrlChars.Contains(c)))
            {
                throw new Exception($"invalid string: {name} , should not contain \\n \\r (perhaps you forgot a ';') ");
            }

            endi = signi;

            var nli = src.IndexOf('\n', endi);
            if (nli > 0)
            {
                if (src.FromTo(endi, nli).Trim(StrConst.EmptyChars).Length == 0)
                {
                    endi = nli;
                }
            }

            return name;
        }

        public static bool IsNextStr(this string source, int startIndex, string nextStr)
        {
            if (source.Length >= startIndex + nextStr.Length)
            {
                return source.Substring(startIndex, nextStr.Length) == nextStr;
            }

            return false;
        }

        /// <summary>
        /// get string from index to index (both inclusive)
        /// </summary>
        /// <param name="str"></param>
        /// <param name="fromi"></param>
        /// <param name="toi"></param>
        /// <returns></returns>
        public static string FromTo(this string str, int fromi, int toi)
        {
            return str.Substring(fromi, toi - fromi + 1);
        }

        public static string FirstLetterToUpper(this string str)
        {
            if (str == null)
                return null;

            if (str.Length > 1)
                return char.ToUpper(str[0]) + str.Substring(1);

            return str.ToUpper();
        }

        public static int IndexOfAnyCharNotIn(string str, char[] excludeChars, int startAtIndx = 0)
        {
            var index = -1;

            for (var i = startAtIndx; i < str.Length; i++)
            {
                if (!excludeChars.Contains(str[i]))
                {
                    index = i;
                    break;
                }
            }

            return index;
        }

        public static int LastIndexOfAnyCharNotIn(string str, char[] excludeChars)
        {
            var index = -1;

            for (var i = str.Length - 1; i >= 0; i--)
            {
                if (!excludeChars.Contains(str[i]))
                {
                    index = i;
                    break;
                }
            }

            return index;
        }

        public static string InsertStrBeforeEmptys(string mainStr, string str)
        {
            var index = LastIndexOfAnyCharNotIn(mainStr, StrConst.EmptyChars);
            if (index < 0) index = mainStr.Length - 1;
            return mainStr.Insert(index + 1, str);
        }

        public static string TrimEmptys(string str)
        {
            return str.Trim(StrConst.EmptyChars);
        }

        public static string GetLastIndentation(string str)
        {
            var lastreturn = -1;
            var starti = -1;
            for (var i = 0; i < str.Length; i++)
            {
                if (str[i] == '\n')
                {
                    lastreturn = i;
                }

                if (!StrConst.EmptyChars.Contains(str[i]))
                {
                    starti = i;
                    break;
                }
            }

            if (lastreturn > 0 && starti > 0)
            {
                return str.FromTo(lastreturn + 1, starti - 1);
            }

            return string.Empty;
        }

        /// <summary>
        /// will return -1 if closest char is not nextstr
        /// </summary>        
        public static int FindNextNonEmpty(this string str, string nextstr, int starti)
        {
            var result = -1;

            for (var i = starti; i < str.Length; i++)
            {
                var c = str[i];

                if (StrConst.EmptyChars.Contains(c)) continue;

                if (str.IsNextStr(i, nextstr))
                {
                    return i;
                }

                break;
            }

            return result;
        }

        public static string[] RemStartSpace(string[] lines)
        {
            var emptyChar = ' ';
            if (lines.Length == 0 || lines[0].Length == 0)
            {
                return lines;
            }

            if (lines[0][0] == '\t')
            {
                emptyChar = '\t';
            }

            var res = lines;
            int? minws = null;

            foreach (var line in res)
            {
                if (line.Length == 0) continue;

                var lmin = 0;
                foreach (var ch in line)
                {
                    if (ch == emptyChar) lmin++;
                    else break;
                }

                if (minws == null || lmin < minws) minws = lmin;
            }

            if (minws.HasValue)
            {
                for (var i = 0; i < res.Length; i++)
                {
                    var line = res[i];
                    if (line.Length != 0)
                    {
                        res[i] = line.Substring(minws.Value);
                    }
                }
            }

            return res;
        }
    }
}