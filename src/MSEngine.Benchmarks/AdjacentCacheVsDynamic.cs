using MSEngine.Core;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace MSEngine.Benchmarks
{
    static class AdjacentCacheVsDynamic
    {
        public static void FillAdjacentNodeIndexesNEW(this Span<int> indexes, int nodeCount, int index, int columnCount)
        {
            var key = GetKey(nodeCount, index, columnCount);
            _keyToAdjacentNodeIndexMap![key].CopyTo(indexes);
        }

        private static IReadOnlyDictionary<uint, int[]>? _keyToAdjacentNodeIndexMap;

        static AdjacentCacheVsDynamic()
        {
            var map = new Dictionary<uint, int[]>(64 + 256 + 480); // beginner/int/expert

            Span<int> buffer = stackalloc int[Engine.MaxNodeEdges];
            for (var i = 0; i < 64; i++)
            {
                var key = GetKey(64, i, 8);
                buffer.FillAdjacentNodeIndexes(64, i, 8);
                map.Add(key, buffer.ToArray());
            }

            _keyToAdjacentNodeIndexMap = map;
        }

        /// <summary>
        /// 4 byte key
        /// 8 bits for columnCount, 12/12 for index/nodeCount
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint GetKey(int nodeCount, int index, int columnCount)
            => (uint)columnCount | ((uint)nodeCount << 8) | ((uint)index << 20);
    }
}
