using NUnit.Framework;
using Oss.Core;

namespace Tests
{
    public class EvalCalcTests
    {
        [Test]
        public void ParseNumber()
        {
            var str = "5";

            var res = ExprCalc.Eval(str);
            Assert.AreEqual(5, res);
        }

        [Test]
        public void Calc2Plus2()
        {
            var str = "2 + 2";

            var res = ExprCalc.Eval(str);
            Assert.AreEqual(4, res);
        }

        [Test]
        public void CalcParanMinus2Plus2()
        {
            var str = "(-2 + 2)";

            var res = ExprCalc.Eval(str);
            Assert.AreEqual(0, res);
        }

        [Test]
        public void OpOrder()
        {
            var str = "2 + 2 * 2";

            var res = ExprCalc.Eval(str);
            Assert.AreEqual(6, res);

            var res2 = ExprCalc.Eval("(2*2) + 2");

            Assert.AreEqual(6, res2);
        }

        [Test]
        public void MultiplyToMinus()
        {
            var str = "2 * -1";

            var res = ExprCalc.Eval(str);
            Assert.AreEqual(-2, res);
        }

        [Test]
        public void MinusParan()
        {
            var str = "2 * -(1+1)";

            var res = ExprCalc.Eval(str);
            Assert.AreEqual(-4, res);
        }        
    }
}