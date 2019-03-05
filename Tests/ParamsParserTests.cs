using NUnit.Framework;
using Oss.Core;

namespace Tests
{
    public class ParamsParserTests
    {
        [Test]
        public void ParseParams()
        {            
            var str = "a = abc; b = 123; c = { color: red; };";
            
            var res = ParamsParser.Parse(str);

            res.Count.ShouldEqual(3);
                        
            res["a"].ShouldEqual("abc");
            res["b"].ShouldEqual("123");
            res["c"].ShouldEqual("color: red;");
        }
    }
}