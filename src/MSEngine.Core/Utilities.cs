using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace MSEngine.Core
{
    internal static class Utilities
    {
        private static readonly Random rng = new Random();
        
        public static List<T> GetShuffledItems<T>(this IEnumerable<T> list)
        {
            if (list == null) { throw new ArgumentNullException(nameof(list)); }

            var shuffled = list.ToList();
            int n = shuffled.Count;

            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = shuffled[k];
                shuffled[k] = shuffled[n];
                shuffled[n] = value;
            }

            return shuffled;
        }

        /// <summary>
        /// This algorithm is broken for large lists...
        /// https://stackoverflow.com/a/1262619/2089286
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static List<T> GetCryptographicallySecureShuffledItems<T>(this IEnumerable<T> list)
        {
            if (list == null) { throw new ArgumentNullException(nameof(list)); }

            using (var provider = new RNGCryptoServiceProvider())
            {
                var items = list.ToList();
                int n = items.Count;

                while (n > 1)
                {
                    byte[] box = new byte[1];
                    do provider.GetBytes(box);
                    while (!(box[0] < n * (byte.MaxValue / n)));
                    int k = (box[0] % n);
                    n--;
                    T value = items[k];
                    items[k] = items[n];
                    items[n] = value;
                }

                return items;
            }
        }
    }
}
