using System;
using System.Diagnostics;

namespace MSEngine.Solver
{
    public readonly ref struct FlatMatrix<T> where T : struct
    {
        private readonly Span<T> _matrix;

        public FlatMatrix(Span<T> matrix, int columnCount)
        {
            Debug.Assert(matrix.Length > 0);
            Debug.Assert(columnCount > 0);
            Debug.Assert(matrix.Length % columnCount == 0);

            _matrix = matrix;
            ColumnCount = columnCount;
        }

        public readonly int ColumnCount { get; }
        public readonly int RowCount => _matrix.Length / ColumnCount;
        public readonly ref T this[int row, int column] => ref _matrix[row * ColumnCount + column];

        public override string ToString()
        {
            var foo = new T[,] { };
            for (var row = 0; row < RowCount; row++)
            {
                for (var column = 0; column < ColumnCount; column++)
                {
                    foo[row, column] = this[row, column];
                }
            }
            throw new NotImplementedException();
        }
    }
}