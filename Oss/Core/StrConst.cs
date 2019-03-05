using System.Linq;

namespace Oss.Core
{
    public static class StrConst
    {
        public static string NewLine = "\r\n";

        public static char[] EmptyChars = { ' ', '\r', '\n', '\t' };

        public static char[] RuleBodyTrimChars = { ' ', '\r', '\n', '\t', '{', '}'};

        public static char[] RuleTrimChars = EmptyChars.Concat(new[] { '{', '}' }).ToArray();

        public static char[] AfterWordChars = EmptyChars.Concat(new[] { ';', '%', ')', '\'', '\"' }).ToArray();
    }
}
