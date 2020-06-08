using MSEngine.Core;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace MSEngine.Benchmarks
{
    public class UnrollBufferLoop
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasHiddenAdjacentNodes(Matrix<Node> matrix, Span<int> buffer, int nodeIndex)
        {

            buffer.FillAdjacentNodeIndexes(matrix.Nodes.Length, nodeIndex, matrix.ColumnCount);

            var a = buffer[0];
            if (a != -1 && matrix.Nodes[a].State == NodeState.Hidden) { return true; }
            a = buffer[1];
            if (a != -1 && matrix.Nodes[a].State == NodeState.Hidden) { return true; }
            a = buffer[2];
            if (a != -1 && matrix.Nodes[a].State == NodeState.Hidden) { return true; }
            a = buffer[3];
            if (a != -1 && matrix.Nodes[a].State == NodeState.Hidden) { return true; }
            a = buffer[4];
            if (a != -1 && matrix.Nodes[a].State == NodeState.Hidden) { return true; }
            a = buffer[5];
            if (a != -1 && matrix.Nodes[a].State == NodeState.Hidden) { return true; }
            a = buffer[6];
            if (a != -1 && matrix.Nodes[a].State == NodeState.Hidden) { return true; }
            a = buffer[7];
            if (a != -1 && matrix.Nodes[a].State == NodeState.Hidden) { return true; }


            return false;
        }
    }
}
