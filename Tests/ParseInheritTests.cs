using NUnit.Framework;
using Oss;
using System.Text;

namespace Tests
{
    public class ParseInheritTests
    {
        [Test]
        public void InheritMultilevel()
        {
            var parser = new OssParser();
            var str = new StringBuilder();
            str.AppendLine("@name ncl1;");
            str.AppendLine(".cl1 { color: red; }");
            str.AppendLine("@name ncl2;");
            str.AppendLine(".cl2 { @inherit ncl1; }");
            str.AppendLine(".cl3 { @inherit ncl2; }");

            var res = parser.ParseOssString(str.ToString(), null);

            res.ParseRes.Out();
            res.ParseRes.Contains(".cl1,\r\n.cl2,\r\n.cl3 { color: red; }").IsTrue();
        }

        [Test]
        public void InheritMultilevel2()
        {
            var parser = new OssParser();
            var str = new StringBuilder();
            str.AppendLine("@name ncl1;");
            str.AppendLine(".cl1 { color: red; }");
            str.AppendLine("@name ncl2;");
            str.AppendLine(".cl2 { @inherit ncl1; }");
            str.AppendLine(".cl3 { @inherit ncl2; color: blue; }");

            var res = parser.ParseOssString(str.ToString(), null);

            res.ParseRes.Out();
            res.ParseRes.Contains(".cl1,\r\n.cl2,\r\n.cl3 { color: red; }").IsTrue();
            res.ParseRes.Contains(".cl3 { color: blue; }").IsTrue();
        }

        [Test]
        public void Inherit()
        {
            var parser = new OssParser();
            var str = new StringBuilder();
            str.AppendLine("@name ncl1;");
            str.AppendLine(".cl1 { color: red; }");
            str.AppendLine(".cl2 { @inherit ncl1; }");

            var res = parser.ParseOssString(str.ToString(), null);

            res.ParseRes.Out();
            res.ParseRes.Contains(".cl1,\r\n.cl2 { color: red; }").IsTrue();
        }

        [Test]
        public void InheritViaVar()
        {
            var parser = new OssParser();
            var str = new StringBuilder();
            str.AppendLine("var color1 = red;");
            str.AppendLine("var x1 = { @inherit ncl1; }");
            str.AppendLine("@name ncl1;");
            str.AppendLine(".cl1 { color: var.color1; }");
            str.AppendLine(".cl2 { var.x1; }");

            var res = parser.ParseOssString(str.ToString(), null);

            res.ParseRes.Out();
            res.ParseRes.Contains(".cl1,\r\n.cl2 { color: red; }").IsTrue();
        }

        [Test]
        public void Inherit3()
        {
            var parser = new OssParser();
            var str = @"
@name base1;
.rule1
{
	border-witdh: 1px;	
}

.rule2
{
	@inherit base1;
	color: blue;
}

.rule3
{
	@inherit base1;
}";

var sres = @".rule1,
.rule2,
.rule3
{
	border-witdh: 1px;	
}

.rule2
{
	color: blue;
}";
            var res = parser.ParseOssString(str, null);

            res.ParseRes.Out().ShouldEqual(sres);            
        }

        [Test]
        public void ParseVarRulesContentInherit()
        {
            var parser = new OssParser();
            var str = new StringBuilder();
            str.AppendLine("var x1 = { .rule1 { @inherit base; } }");
            str.AppendLine("@name base; .main { color: red; }");
            str.AppendLine("@var.x1;");

            var res = parser.ParseOssString(str.ToString(), null);

            res.ParseRes.Out();
            Assert.AreEqual(".main,\r\n .rule1 { color: red; }", res.ParseRes);
        }

        [Test]
        public void InheritNameWitoutHeader()
        {
            var parser = new OssParser();
            var str = @"@name base1;
{
	border-witdh: 1px;	
}

.rule2
{
	@inherit base1;
	color: blue;
}";

            var sres = @".rule2
{
	border-witdh: 1px;	
}

.rule2
{
	color: blue;
}";
            var res = parser.ParseOssString(str, null);

            res.ParseRes.Out().ShouldEqual(sres);
        }

        [Test]
        public void EmptyRule()
        {
            var parser = new OssParser();
            var str = @"@name base1;
{
	border-witdh: 1px;	
}";
            var sres = "";
            var res = parser.ParseOssString(str, null);

            res.ParseRes.Out().ShouldEqual(sres);
        }
    }
}