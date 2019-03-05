using NUnit.Framework;
using Oss.Core;

namespace Tests
{
    public class StrUtilTests
    {
        [Test]
        public void FindNextNonEmptyIndex()
        {
            var str = "a ( b )";
            var res = StrUtil.FindNextNonEmpty(str, "(", 1);

            res.ShouldEqual(2);
        }

        [Test]
        public void NotFindNextNonEmptyIndex()
        {
            var str = "a c( b )";
            var res = StrUtil.FindNextNonEmpty(str, "(", 1);

            res.ShouldEqual(-1);
        }
    }
}