using NUnit.Framework;
using Oss;
using System.Text;

namespace Tests
{
    public class ParserTests
    {
        [Test]
        public void ParseVarWithParam()
        {
            var parser = new OssParser();
            var str = new StringBuilder();
            str.AppendLine("var x = 7;");
            str.AppendLine("var bl1 = { abc: var.x; };");
            str.AppendLine(".cl1 { var(bl1){ x = 5; } }");

            var res = parser.ParseOssString(str.ToString(), null);

            res.ParseRes.Out();
            res.ParseRes.Contains(".cl1 { abc: 5; }").IsTrue();
        }

        [Test]
        public void ParseVarWithBlockParam()
        {
            var parser = new OssParser();
            var str = new StringBuilder();
            str.AppendLine("var x1 = { color: red; };");
            str.AppendLine("var x = 7;");
            str.AppendLine("var :bl1 = { abc: var.y; var.x; };");
            str.AppendLine(".cl1 { var(bl1){ y = 3; x = { qwe: 123; }; } }");

            var res = parser.ParseOssString(str.ToString(), null);

            res.ParseRes.Out();
            res.ParseRes.Contains(".cl1 { abc: 3; qwe: 123; }").IsTrue();
        }

        [Test]
        public void ParseAtCalcWithVars()
        {
            var parser = new OssParser();
            var str = new StringBuilder();
            str.AppendLine("var a = 2;");
            str.AppendLine("var b = 3;");
            str.AppendLine("var x = @calc(var.a + var.b);");
            str.AppendLine(".cl1 { abc: var.x; }");

            var res = parser.ParseOssString(str.ToString(), null);

            res.ParseRes.Out();
            res.ParseRes.Contains(".cl1 { abc: 5; }").IsTrue();
        }

        [Test]
        public void ParseAtCalc()
        {
            var parser = new OssParser();
            var str = new StringBuilder();
            str.AppendLine("var x = @calc(2 + 3);");
            str.AppendLine(".cl1 { abc: var.x; }");

            var res = parser.ParseOssString(str.ToString(), null);

            res.ParseRes.Out();
            res.ParseRes.Contains(".cl1 { abc: 5; }").IsTrue();
        }        

        [Test]
        public void ParseVar()
        {
            var parser = new OssParser();
            var str = new StringBuilder();
            str.AppendLine("var color1 = red;");
            str.AppendLine(".cl1 { color: var.color1; }");
            var input = str.ToString();

            var res = parser.ParseOssString(input, null);
            res.ParseRes.Out();
            Assert.IsTrue(res.ParseRes.Contains(".cl1 { color: red; }"));
        }

        [Test]
        public void ParseVarParan()
        {
            var parser = new OssParser();
            var str = new StringBuilder();
            str.AppendLine("var color1 = red;");
            str.AppendLine(".cl1 { color: var( color1 ); }");
            var input = str.ToString();

            var res = parser.ParseOssString(input, null);
            res.ParseRes.Out();
            Assert.IsTrue(res.ParseRes.Contains(".cl1 { color: red; }"));
        }

        [Test]
        public void ParseVarRulesContent()
        {
            var parser = new OssParser();
            var str = new StringBuilder();
            str.AppendLine("var x1 = { .rule1 { color: red; } }");
            str.AppendLine("@var.x1;");

            var res = parser.ParseOssString(str.ToString(), null);

            res.ParseRes.Out();
            Assert.IsTrue(res.ParseRes.Contains(".rule1 { color: red; }") && !res.ParseRes.Contains("var"));
        }

        [Test]
        public void ParseVarRulesContentVar()
        {
            var parser = new OssParser();
            var str = new StringBuilder();
            str.AppendLine("var x2 = red;");
            str.AppendLine("var x1 = { .rule1 { color: var.x2; } }");
            str.AppendLine("@var.x1;");

            var res = parser.ParseOssString(str.ToString(), null);

            res.ParseRes.Out();
            Assert.IsTrue(res.ParseRes.Contains(".rule1 { color: red; }") && !res.ParseRes.Contains("var"));
        }

        [Test]
        public void ParseVarContent()
        {
            var parser = new OssParser();
            var str = new StringBuilder();
            str.AppendLine("var x1 = { color: red; }");
            str.AppendLine(".cl1 { var.x1; }");
            var input = str.ToString();

            var res = parser.ParseOssString(input, null);
            res.ParseRes.Out();
            Assert.IsTrue(res.ParseRes.Contains(".cl1 { color: red; }"));
        }

        [Test]
        public void ParseVarRem()
        {
            var parser = new OssParser();
            var str = new StringBuilder();
            str.AppendLine("var x1 = rem;");
            str.AppendLine(".cl1 { var.x1; }");
            var input = str.ToString();

            var res = parser.ParseOssString(input, null);
            res.ParseRes.Out();
            Assert.IsTrue(!res.ParseRes.Contains("cl1"));
        }

        [Test]
        public void ParseVarCalc()
        {
            var parser = new OssParser();
            var str = new StringBuilder();
            str.AppendLine("var x1 = 5;");
            str.AppendLine("var b = 3;");
            str.AppendLine("var x2 = @calc(var.x1 + var.x1 * 2 - (11 - var.b));");
            str.AppendLine(".cl1 { var.x2; }");
            var input = str.ToString();

            var res = parser.ParseOssString(input, null);
            res.ParseRes.Out();
            Assert.IsTrue(res.ParseRes.Contains(".cl1 { 7; }"));
        }

        [Test]
        public void ParseVarCalcFloat()
        {
            var parser = new OssParser();
            var str = new StringBuilder();
            str.AppendLine("var x1 = 5;");
            str.AppendLine(".cl1 { @calc(var.x1 / 2 * -1); }");
            var input = str.ToString();

            var res = parser.ParseOssString(input, null);
            res.ParseRes.Out();
            Assert.IsTrue(res.ParseRes.Contains(".cl1 { -2.5; }"));
        }

        [Test]
        public void ParseVarCalcFloat2()
        {
            var parser = new OssParser();
            var str = new StringBuilder();
            str.AppendLine("var x1 = 10;");
            str.AppendLine(".cl1 { @calc(var.x1 / 3)px; }");
            var input = str.ToString();

            var res = parser.ParseOssString(input, null);
            res.ParseRes.Out();
            Assert.IsTrue(res.ParseRes.Contains(".cl1 { 3.33px; }"));
        }

        [Test]
        public void ParseVarCalcFloat1()
        {
            var parser = new OssParser();
            var str = new StringBuilder();
            str.AppendLine(".cl1 { @calc( 4.5 / 3)px; }");
            var input = str.ToString();

            var res = parser.ParseOssString(input, null);
            res.ParseRes.Out();
            Assert.IsTrue(res.ParseRes.Contains(".cl1 { 1.5px; }"));
        }
    }
}