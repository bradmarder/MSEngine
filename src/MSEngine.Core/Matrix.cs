using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MSEngine.Core
{
    public readonly ref struct Matrix<T> where T : struct
    {
        public Matrix(Span<T> nodes, int columnCount)
        {
            Debug.Assert(nodes.Length > 0);
            Debug.Assert(columnCount > 0);
            Debug.Assert(nodes.Length % columnCount == 0);

            Nodes = nodes;
            ColumnCount = columnCount;
            RowCount = nodes.Length / columnCount;
        }

        public Span<T> Nodes { get; }
        public int ColumnCount { get; }
        public int RowCount { get; }

        public ref T this[int row, int column]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                Debug.Assert(column >= 0 && column < ColumnCount);
                Debug.Assert(row >= 0 && row < RowCount);

                // return ref Nodes[row * ColumnCount + column];
                return ref GetItemByOffset(row * ColumnCount + column);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref T GetItemByOffset(int elementOffset)
        {
            ref var data = ref MemoryMarshal.GetReference(Nodes);
            return ref Unsafe.Add(ref data, elementOffset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> Vector(int row)
        {
            Debug.Assert(ColumnCount > 1);
            Debug.Assert(row >= 0);
            Debug.Assert(row < RowCount);

            return Nodes.Slice(row * ColumnCount, ColumnCount - 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ref T Augment(int row)
        {
            Debug.Assert(ColumnCount > 1);
            Debug.Assert(row >= 0);
            Debug.Assert(row < RowCount);

            // return Nodes[row * ColumnCount + ColumnCount - 1];
            return ref GetItemByOffset(row * ColumnCount + ColumnCount - 1);
        }

        public Enumerator GetEnumerator() => new(this);

        public override string ToString()
        {
            var sb = new System.Text.StringBuilder();

            foreach (var row in this)
            {
                foreach (var col in row)
                {
                    sb.Append(col + "\t");
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public ref struct Enumerator
        {
            private int _row;
            private readonly Span<T> _span;
            private readonly int _columnCount;
            private readonly int _rowCount;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(in Matrix<T> matrix)
            {
                _row = -1;
                _span = matrix.Nodes;
                _columnCount = matrix.ColumnCount;
                _rowCount = matrix.RowCount;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext()
            {
                int row = _row + 1;
                if (row < _rowCount)
                {
                    _row = row;
                    return true;
                }

                return false;
            }

            public readonly Span<T> Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _span.Slice(_row * _columnCount, _columnCount);
            }
        }
    }
}