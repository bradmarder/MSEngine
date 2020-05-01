using System;
using System.Collections.Generic;
using System.Text;

namespace MSEngine.Solver
{
    public class BrokenGauss
    {
        //private static Matrix<double> ApplyGaussianElimination(Matrix<double> matrix)
        //{
        //    var clone = matrix.Clone();
        //    var h = 0;
        //    var k = 0;
        //    var m = clone.RowCount - 1;
        //    var n = clone.ColumnCount - 1;

        //    while (h <= m && k <= n)
        //    {
        //        var i_max = Enumerable
        //            .Range(h, m - h)
        //            .Select(x => (int)Math.Abs(clone[x, k]))
        //            .DefaultIfEmpty()
        //            .Max();

        //        if (clone[i_max, k] == 0)
        //        {
        //            k++;
        //            continue;
        //        }

        //        var hRow = clone.Row(h);
        //        var iMaxRow = clone.Row(i_max);
        //        clone.SetRow(h, iMaxRow);
        //        clone.SetRow(i_max, hRow);

        //        for (var i = h + 1; i <= m; i++)
        //        {
        //            var f = clone[i, k] / clone[h, k];
        //            clone[i, k] = 0;
        //            for (var j = k + 1; j <= n; j++)
        //            {
        //                clone[i, j] =- clone[h, j] * f;
        //            }
        //        }
        //        h++;
        //        k++;
        //    }

        //    return clone;
        //}
    }
}
