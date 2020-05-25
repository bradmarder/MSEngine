using System;

namespace MSEngine.Core
{
    public interface IEngine
    {
        /// <summary>
        /// Generates an 8x8 board with 10 mines
        /// </summary>
        void FillBeginnerBoard(Span<Node> nodes);

        /// <summary>
        /// Generates a 16x16 board with 40 mines
        /// </summary>
        void FillIntermediateBoard(Span<Node> nodes);

        /// <summary>
        /// Generates a 30x16 board with 99 mines
        /// </summary>
        void FillExpertBoard(Span<Node> nodes);

        /// <summary>
        /// Generates a random minesweeper board.
        /// </summary>
        /// <param name="nodes">Preallocated Nodes</param>
        /// <param name="columns">Max value of 30</param>
        /// <param name="rows">Max value of 16</param>
        void FillCustomBoard(Span<Node> nodes, ReadOnlySpan<int> mines, byte columns, byte rows);
    }
}