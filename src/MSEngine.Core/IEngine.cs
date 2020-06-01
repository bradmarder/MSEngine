using System;

namespace MSEngine.Core
{
    public interface IEngine
    {
        void FillBeginnerBoard(Span<Node> nodes);
        void FillIntermediateBoard(Span<Node> nodes);
        void FillExpertBoard(Span<Node> nodes);
        void FillCustomBoard(Span<Node> nodes, int mineCount, byte columns);
        void FillCustomBoard(Span<Node> nodes, ReadOnlySpan<int> mines, byte columns);
    }
}