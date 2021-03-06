﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

[MemoryDiagnoser]
public class ZeroInit
{
    [Benchmark]
    public void Old()
    {
        Span<int> foo = stackalloc int[8];
        OldFill(foo, 9, 4, 3);
    }

    [Benchmark]
    public void New()
    {
        Span<int> foo = stackalloc int[8];
        NewFill(foo, 9, 4, 3);
    }

    [Benchmark]
    public void Rearranged()
    {
        Span<int> foo = stackalloc int[8];
        NewRearranged(foo, 9, 4, 3);
    }

    public static void OldFill(Span<int> indexes, int nodeCount, int index, int columnCount)
    {
        var indexPlusOne = index + 1;
        var isTop = index < columnCount;
        var isLeftSide = index % columnCount == 0;
        var isRightSide = indexPlusOne % columnCount == 0;
        var isBottom = index >= nodeCount - columnCount;

        if (isTop)
        {
            indexes[0] = indexes[1] = indexes[2] = -1;
        }
        else
        {
            var val = index - columnCount;
            if (!isLeftSide)
            {
                indexes[0] = val - 1;
            }
            indexes[1] = val;
            if (!isRightSide)
            {
                indexes[2] = val + 1;
            }
        }

        if (isLeftSide)
        {
            indexes[0] = indexes[3] = indexes[5] = -1;
        }
        else
        {
            indexes[3] = index - 1;
        }

        if (isRightSide)
        {
            indexes[2] = indexes[4] = indexes[7] = -1;
        }
        else
        {
            indexes[4] = indexPlusOne;
        }

        if (isBottom)
        {
            indexes[5] = indexes[6] = indexes[7] = -1;
        }
        else
        {
            var val = index + columnCount;
            if (!isLeftSide)
            {
                indexes[5] = val - 1;
            }
            indexes[6] = val;
            if (!isRightSide)
            {
                indexes[7] = val + 1;
            }
        }
    }

    public unsafe static void NewFill(Span<int> indexes, int nodeCount, int index, int columnCount)
    {
        _ = indexes[7];

        var indexPlusOne = index + 1;
        var isTop = index < columnCount;
        var isLeftSide = index % columnCount == 0;
        var isRightSide = indexPlusOne % columnCount == 0;
        var isBottom = index >= nodeCount - columnCount;

        if (isTop)
        {
            indexes[0] = indexes[1] = indexes[2] = -1;
        }
        else
        {
            var val = index - columnCount;
            if (!isLeftSide)
            {
                indexes[0] = val - 1;
            }
            indexes[1] = val;
            if (!isRightSide)
            {
                indexes[2] = val + 1;
            }
        }

        if (isLeftSide)
        {
            indexes[0] = indexes[3] = indexes[5] = -1;
        }
        else
        {
            indexes[3] = index - 1;
        }

        if (isRightSide)
        {
            indexes[2] = indexes[4] = indexes[7] = -1;
        }
        else
        {
            indexes[4] = indexPlusOne;
        }

        if (isBottom)
        {
            indexes[5] = indexes[6] = indexes[7] = -1;
        }
        else
        {
            var val = index + columnCount;
            if (!isLeftSide)
            {
                indexes[5] = val - 1;
            }
            indexes[6] = val;
            if (!isRightSide)
            {
                indexes[7] = val + 1;
            }
        }
    }

    public static void NewRearranged(Span<int> indexes, int nodeCount, int index, int columnCount)
    {
        var indexPlusOne = index + 1;
        var isTop = index < columnCount;
        var isLeftSide = index % columnCount == 0;
        var isRightSide = indexPlusOne % columnCount == 0;
        var isBottom = index >= nodeCount - columnCount;

        if (!isBottom)
        {
            var val = index + columnCount;
            if (!isRightSide)
            {
                indexes[7] = val + 1;
            }

            if (!isLeftSide)
            {
                indexes[5] = val - 1;
            }
            indexes[6] = val;
        }
        else
        {
            indexes[5] = indexes[6] = indexes[7] = -1;
            
        }

        if (isTop)
        {
            indexes[0] = indexes[1] = indexes[2] = -1;
        }
        else
        {
            var val = index - columnCount;
            if (!isLeftSide)
            {
                indexes[0] = val - 1;
            }
            indexes[1] = val;
            if (!isRightSide)
            {
                indexes[2] = val + 1;
            }
        }

        if (isLeftSide)
        {
            indexes[0] = indexes[3] = indexes[5] = -1;
        }
        else
        {
            indexes[3] = index - 1;
        }

        if (isRightSide)
        {
            indexes[2] = indexes[4] = indexes[7] = -1;
        }
        else
        {
            indexes[4] = indexPlusOne;
        }

        
    }
}
