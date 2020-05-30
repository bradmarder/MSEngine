﻿using System;
using System.Diagnostics;

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
        }

        public readonly Span<T> Nodes;
        public readonly int ColumnCount { get; }
        public readonly int RowCount => Nodes.Length / ColumnCount;

        public readonly ref T this[int row, int column] => ref Nodes[row * ColumnCount + column];

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