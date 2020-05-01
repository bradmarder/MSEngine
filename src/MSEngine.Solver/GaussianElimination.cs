using System;
using System.Collections.Generic;

namespace MSEngine.Solver
{
    public static class GaussianElimination
    {
        public static sbyte[,] GaussEliminate(this sbyte[,] matrix)
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
            return matrix;
        }
    }
}
