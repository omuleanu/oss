using Oss.Core;
using System.Collections.Generic;

namespace Oss
{
    public class Reader
    {
        public static OssReadRes Read(string str)
        {
            var varkey = "var ";
            var vars = new Dictionary<string, OssItem>();

            var list = new List<OssItem>();
            var midstr = string.Empty;

            void AddMidstr()
            {
                midstr = midstr.Trim(StrConst.EmptyChars);
                if (midstr.Length == 0) return;
                list.Add(new OssItem { Val = midstr });
                midstr = string.Empty;
            }

            for (var i = 0; i < str.Length; i++)
            {
                if (str.IsNextStr(i, varkey))
                {
                    i += varkey.Length;

                    var varname = str.GetVarName(i, out i);
                    var varobj = str.GetVarVal(i, out i);

                    AddMidstr();

                    var item = new OssItem { IsBlock = varobj.IsBlock, Val = varobj.Val, VarName = varname };
                    vars.Add(varname, item);
                    list.Add(item);

                    if (str[i + 1] == ';') i++;
                }
                else
                {
                    midstr += str[i];
                }
            }

            AddMidstr();

            return new OssReadRes
            {
                Items = list,
                Vars = vars
            };
        }
    }
}