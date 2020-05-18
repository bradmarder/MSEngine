using System;
using System.Runtime.CompilerServices;

namespace MSEngine.Solver
{
    public readonly ref struct FlatMatrix<T> where T : struct
    {
        private Span<T> _matrix { get; }

        public FlatMatrix(ref Span<T> matrix, int columnCount)
        {
            if (columnCount < 1) { throw new ArgumentOutOfRangeException(nameof(columnCount)); }
            _matrix = matrix;
            ColumnCount = columnCount;
        }

        public readonly int ColumnCount { get; }
        public readonly int RowCount => _matrix.Length / ColumnCount;

        public T this[int row, int column]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _matrix[row * ColumnCount + column];

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _matrix[row * ColumnCount + column] = value;
        }
    }
}