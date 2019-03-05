using System;
using System.Collections.Generic;

namespace Oss
{
    public sealed class ReverseComparer<T> : IComparer<T>
    {
        public readonly IComparer<T> originalComparer;

        /// <summary>
        /// Creates a new reversing comparer.
        /// </summary>
        /// <param name="original">The original comparer to 
        /// use for comparisons.</param>
        public ReverseComparer(IComparer<T> original)
        {
            originalComparer = original ?? throw new ArgumentNullException(nameof(original));
        }

        /// <summary>
        /// Returns the result of comparing the specified
        /// values using the original
        /// comparer, but reversing the order of comparison.
        /// </summary>
        public int Compare(T x, T y)
        {
            return originalComparer.Compare(y, x);
        }
    }
}