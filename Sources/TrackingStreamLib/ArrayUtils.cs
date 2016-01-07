namespace TrackingStreamLib
{
    using System;
    using System.Linq;

    internal static class ArrayUtils
    {
        /// <summary>
        ///     Attempts to find some value T in and array of sorted values using binary search algorithm
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="_values"></param>
        /// <param name="_needle"></param>
        /// <param name="_index"></param>
        /// <returns></returns>
        public static bool TryToFindValue<T>(T[] _values, T _needle, out int _index) where T : IComparable
        {
            var result = FindValueIndexBinarySearch(_values, _needle);
            _index = result;
            return result >= 0;
        }

        /// <summary>
        ///     Returns index of value T in T[] using brute-force arlorithm
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="_values"></param>
        /// <param name="_needle"></param>
        /// <returns></returns>
        private static int FindValueIndexRaw<T>(T[] _values, T _needle) where T : IComparable
        {
            if (_values == null)
            {
                throw new ArgumentNullException(nameof(_values));
            }
            var result = -1;

            if (_values.Length == 0 || _needle.CompareTo(_values[0]) < 0 || _needle.CompareTo(_values[_values.Length - 1]) > 0)
            {
                // граничные случаи
                return result;
            }

            for (var i = 0; i < _values.Length; i++)
            {
                var comparisonResult = _needle.CompareTo(_values[i]);
                if (comparisonResult == 0)
                {
                    result = i;
                    break;
                }
                if (comparisonResult < 0)
                {
                    result = i - 1;
                    break;
                }
            }
            return result;
        }

        /// <summary>
        ///     Returns index of value T in T[] using binary search arlgorithm
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values"></param>
        /// <param name="needle"></param>
        /// <returns></returns>
        public static int FindValueIndexBinarySearch<T>(T[] values, T needle) where T : IComparable
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }
            var result = -1;

            if (values.Length == 0 || needle.CompareTo(values[0]) < 0 || needle.CompareTo(values[values.Length - 1]) > 0)
            {
                // граничные случаи
                return result;
            }
            var index = Array.BinarySearch(values, needle);
            if (index < 0)
            {
                index = ~index - 1;
            }
            return index;
        }

        /// <summary>
        ///     Finds sequence of T in T[]
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="buffer"></param>
        /// <param name="sequence"></param>
        /// <param name="bufferOffset">Index of first found element</param>
        /// <returns></returns>
        public static int FindFirstSequence<T>(T[] buffer, T[] sequence, int bufferOffset) where T : IComparable
        {
            if (buffer == null || sequence == null || buffer.Length == 0 || sequence.Length == 0 || sequence.Length > buffer.Length)
            {
                return -1;
            }
            if (bufferOffset > buffer.Length)
            {
                return -1;
            }
            for (var i = bufferOffset; i < buffer.Length; i++)
            {
                if (IsMatch(buffer, i, sequence))
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        ///     Finds sequence of T in T[], starting from offset and going BACKWARD, i.e. last sequence of T in T[]
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="buffer"></param>
        /// <param name="sequence"></param>
        /// <param name="bufferOffset">Index of first found element of last sequence</param>
        /// <returns></returns>
        public static int FindLastSequence<T>(T[] buffer, T[] sequence, int bufferOffset) where T : IComparable
        {
            if (buffer == null || sequence == null || buffer.Length == 0 || sequence.Length == 0 || sequence.Length > buffer.Length)
            {
                return -1;
            }
            if (bufferOffset > buffer.Length)
            {
                return -1;
            }
            for (var i = bufferOffset; i >= 0; i--)
            {
                if (IsMatch(buffer, i, sequence))
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        ///     Attempts to find sub-array in array using brute force algorithm
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="bytesToFind"></param>
        /// <param name="bufferOffset"></param>
        /// <param name="firstByteIndex"></param>
        /// <returns></returns>
        public static bool TryToFindFirstSequence<T>(T[] buffer, T[] bytesToFind, int bufferOffset, out int firstByteIndex) where T : IComparable
        {
            var index = FindFirstSequence(buffer, bytesToFind, bufferOffset);
            firstByteIndex = index;
            return firstByteIndex >= 0;
        }

        /// <summary>
        ///     Attempts to find sub-array in array using brute force algorithm
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="bytesToFind"></param>
        /// <param name="bufferOffset"></param>
        /// <param name="firstByteIndex"></param>
        /// <returns></returns>
        public static bool TryToFindLastSequence<T>(T[] buffer, T[] bytesToFind, int bufferOffset, out int firstByteIndex) where T : IComparable
        {
            var index = FindLastSequence(buffer, bytesToFind, bufferOffset);
            firstByteIndex = index;
            return firstByteIndex >= 0;
        }

        private static bool IsMatch<T>(T[] array, int position, T[] candidate) where T : IComparable
        {
            if (candidate.Length > (array.Length - position))
            {
                return false;
            }
            return !candidate.Where((t, i) => t.CompareTo(array[position + i]) != 0).Any();
        }
    }
}