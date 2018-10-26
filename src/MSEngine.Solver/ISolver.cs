using MSEngine.Core;

namespace MSEngine.Solver
{
    public interface ISolver
    {
        Turn ComputeTurn(Board board);
    }
}
