using System;

namespace MSEngine.Core
{
    public readonly ref struct BufferKeeper
    {
        public BufferKeeper(
            Span<Turn> turns,
            Span<int> edgeIndexes,
            Span<int> mines,
            Span<int> visitedIndexes,
            Span<int> revealedMineCountNodeIndexes,
            Span<int> adjacentHiddenNodeIndexes,
            Span<float> grid)
        {
            Turns = turns;
            EdgeIndexes = edgeIndexes;
            Mines = mines;
            VisitedIndexes = visitedIndexes;
            RevealedMineCountNodeIndexes = revealedMineCountNodeIndexes;
            AdjacentHiddenNodeIndexes = adjacentHiddenNodeIndexes;
            Grid = grid;
        }

        public Span<Turn> Turns { get; }
        public Span<int> EdgeIndexes { get; }
        public Span<int> Mines { get; }
        public Span<int> VisitedIndexes { get; }
        public Span<int> RevealedMineCountNodeIndexes { get; }
        public Span<int> AdjacentHiddenNodeIndexes { get; }
        public Span<float> Grid { get; }
    }
}