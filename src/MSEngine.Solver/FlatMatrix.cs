using System;

namespace MSEngine.Solver
{
    public readonly ref struct FlatMatrix<T> where T : struct
    {
        private readonly Span<T> _matrix;

        public FlatMatrix(Span<T> matrix, int columnCount)
        {
            if (columnCount < 1) { throw new ArgumentOutOfRangeException(nameof(columnCount)); }
            _matrix = matrix;
            ColumnCount = columnCount;
        }

        public readonly int ColumnCount { get; }
        public readonly int RowCount => _matrix.Length / ColumnCount;
        public readonly ref T this[int row, int column] => ref _matrix[row * ColumnCount + column];
    }
}