using System;
using MSEngine.Core;

namespace MSEngine.Solver
{
    public interface ISolver
    {
        (Turn, Strategy) ComputeTurn(Board board);
    }
}
