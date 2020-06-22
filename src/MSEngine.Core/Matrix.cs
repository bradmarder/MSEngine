using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

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

        public readonly Span<T> Nodes;
        public readonly int ColumnCount { get; }
        public readonly int RowCount { get; }

        public readonly ref T this[int row, int column]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                Debug.Assert(column >= 0 && column < ColumnCount);
                Debug.Assert(row >= 0 && row < RowCount);

                return ref Nodes[row * ColumnCount + column];
            }
        }

        public Enumerator GetEnumerator() => new Enumerator(Nodes, ColumnCount, RowCount);

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
            private readonly Span<T> _span;
            private readonly int _columnCount;
            private readonly int _rowCount;
            private int _row;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal Enumerator(Span<T> span, int columnCount, int rowCount)
            {
                _span = span;
                _row = -1;
                _columnCount = columnCount;
                _rowCount = rowCount;
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

            public Span<T> Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => _span.Slice(_row * _columnCount, _columnCount);
            }
        }
    }
}