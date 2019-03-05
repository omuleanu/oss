using System;
using System.Collections.Generic;

namespace Oss.Core
{
    /// <summary>
    /// Parse var(name)(parameters)
    /// </summary>
    public static class ParamsParser
    {
        public static IDictionary<string, string> Parse(string str)
        {
            var res = new Dictionary<string, string>();
            str = str.Trim(StrConst.EmptyChars);

            for (var i = 0; i < str.Length; i++)
            {
                // var = x / {x:y} / { header { x: y } .. };

                var varname = str.GetVarName(i, out i);
                var varval = str.GetVarVal(i, out i).Val;

                res.Add(varname, varval);                

                if (i < str.Length - 1 && str[i + 1] == ';') i++;
            }

            return res;
        }
    }
}