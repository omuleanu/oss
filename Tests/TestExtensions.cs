using NUnit.Framework;
using System;

namespace Tests
{
    public static class TestExtensions
    {
        public static string Out(this string str)
        {
            Console.WriteLine(str);
            return str;
        }

        public static void IsTrue(this bool condition)
        {
            Assert.IsTrue(condition);
        }

        public static void IsFalse(this bool condition)
        {
            Assert.IsFalse(condition);
        }

        public static void ShouldEqual<T>(this T a, T b)
        {
            Assert.AreEqual(b, a);
        }
    }
}