using System;
using System.Linq;

namespace Oss.Core
{
    public static class ExprCalc
    {
        static char[] operators = new char[] { '+', '-', '*', '/' };

        public static double Eval(string expr)
        {
            return CalcExprOp(expr.Replace(" ", ""), 0);
        }

        static double CalcExprOp(string expr, int opix)
        {
            double? result = null;

            if (opix == operators.Length)
            {
                throw new Exception("can't eval " + expr);
            }

            var op = operators[opix];
            var lastOpIx = -1;

            void AddRes(double newRes)
            {
                if (result.HasValue)
                {
                    result = ExecOp(op, result.Value, newRes);
                }
                else
                {
                    result = newRes;
                }
            }

            void AddPart(string part)
            {
                bool minus = false;
                bool parant = false;
                if (part.Length > 3 && part.StartsWith("-(") && part.EndsWith(')'))
                {
                    minus = true;
                    part = part.Substring(1);
                }

                if (part.Length > 2 && part[0] == '(' && part.EndsWith(')'))
                {
                    parant = true;
                    part = part.FromTo(1, part.Length - 2).Trim();
                }

                if (!double.TryParse(part, out double pres))
                {
                    pres = CalcExprOp(part, parant ? 0 : opix + 1);
                }

                if (minus)
                {
                    pres = -pres;
                }

                AddRes(pres);
            }

            for (var i = 0; i < expr.Length; i++)
            {
                var c = expr[i];

                if (c == '(')
                {
                    i = StrUtil.GetBlockEndPrn(expr, i) - 1; // -1 for last ) to go to last if
                }
                else if (c == op && !(op == '-' && (i == 0 || operators.Contains(expr[i - 1]))))
                {
                    var part = expr.FromTo(lastOpIx + 1, i - 1);

                    lastOpIx = i;

                    AddPart(part);
                }
                else if (i == expr.Length - 1)
                {
                    var part = expr.FromTo(lastOpIx + 1, i);

                    AddPart(part);
                }
            }

            return result ?? 0;
        }

        static double ExecOp(char op, double a, double b)
        {
            if (op == '+')
            {
                return a + b;
            }

            if (op == '-')
            {
                return a - b;
            }

            if (op == '*')
            {
                return a * b;
            }

            if (op == '/')
            {
                return a / b;
            }

            throw new Exception("unknown operator " + op);
        }
    }
}