using System;
using System.Collections.Generic;

namespace MSEngine.Solver
{
    public ref struct Foo<T> where T: struct
    {
        private Span<T> _matrix { get; }
        private readonly int _columnCount { get; } // readonly on getter??

        public Foo(ref Span<T> matrix, int columnCount)
        {
            _matrix = matrix;
            _columnCount = columnCount;
        }

        public T this[int row, int column]
        {
            get => _matrix[row * _columnCount + column];
            set => _matrix[row * _columnCount + column] = value;
        }
    }
    public static class GaussianElimination
    {
        //public static ref sbyte Get(ref this Span<sbyte> matrix, int x, int y, int rowCount, int columnCount)
        //{
        //    var i = x * columnCount + y;
        //    return ref matrix[i];
        //}
        //public static void Set(ref this Span<sbyte> matrix, int x, int y, sbyte val, int rowCount, int columnCount)
        //{
        //    var i = x * columnCount + y;
        //    matrix[i] = val;
        //}

        public static void GaussEliminate(ref this Foo<sbyte> matrix, int rowCount, int columnCount)
        {
            var lead = 0;

            for (var r = 0; r < rowCount; r++)
            {
                if (columnCount <= lead)
                {
                    break;
                }

                var i = r;

                while (matrix[i, lead] == 0)
                {
                    i++;
                    if (i == rowCount)
                    {
                        i = r;
                        lead++;
                        if (columnCount == lead)
                        {
                            lead--;
                            break;
                        }
                    }
                }
                for (var j = 0; j < columnCount; j++)
                {
                    var temp = matrix[r, j];
                    matrix[r, j] = matrix[i, j];
                    matrix[i, j] = temp;
                }
                var div = matrix[r, lead];
                if (div != 0)
                {
                    for (var j = 0; j < columnCount; j++)
                    {
                        matrix[r, j] /= div;
                    }
                }

                for (var j = 0; j < rowCount; j++)
                {
                    if (j != r)
                    {
                        var sub = matrix[j, lead];
                        for (var k = 0; k < columnCount; k++)
                        {
                            matrix[j, k] -= (sbyte)(sub * matrix[r, k]);
                        }
                    }
                }
                lead++;
            }
        }
        public static void GaussEliminate(this sbyte[,] matrix)
        {
            var lead = 0;
            var rowCount = matrix.GetLength(0);
            var columnCount = matrix.GetLength(1);

            for (var r = 0; r < rowCount; r++)
            {
                if (columnCount <= lead)
                {
                    break;
                }

                var i = r;

                while (matrix[i, lead] == 0)
                {
                    i++;
                    if (i == rowCount)
                    {
                        i = r;
                        lead++;
                        if (columnCount == lead)
                        {
                            lead--;
                            break;
                        }
                    }
                }
                for (var j = 0; j < columnCount; j++)
                {
                    var temp = matrix[r, j];
                    matrix[r, j] = matrix[i, j];
                    matrix[i, j] = temp;
                }
                var div = matrix[r, lead];
                if (div != 0)
                {
                    for (var j = 0; j < columnCount; j++)
                    {
                        matrix[r, j] /= div;
                    }
                }

                for (var j = 0; j < rowCount; j++)
                {
                    if (j != r)
                    {
                        var sub = matrix[j, lead];
                        for (var k = 0; k < columnCount; k++)
                        {
                            matrix[j, k] -= (sbyte)(sub * matrix[r, k]);
                        }
                    }
                }
                lead++;
            }
        }
    }
}
