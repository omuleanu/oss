using NUnit.Framework;
using Oss;
using System.Text;

namespace Tests
{
    public class ParseRulesVarTests
    {
        [Test]
        public void Inden()
        {
            var sb = new StringBuilder();
            var s1 = @"
var bl1 = { font-size: 32; };
var rls1 = {	
	.cl1
	{
		zoom: 2;
	}
};

@var.rls1;
";

            var res2 =
                @".cl1
{
	zoom: 2;
}";
            var parser = new OssParser();
            var res = parser.ParseOssString(s1, null);

            res.ParseRes.Out();

            res.ParseRes.ShouldEqual(res2);
        }
    }
}