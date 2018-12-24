using System;

namespace MSEngine.Core
{
    public interface IEngine
    {
        /// <summary>
        /// Generates an 8x8 board with 10 mines
        /// </summary>
        Board GenerateBeginnerBoard();

        /// <summary>
        /// Generates a 16x16 board with 40 mines
        /// </summary>
        Board GenerateIntermediateBoard();

        /// <summary>
        /// Generates a 30x16 board with 99 mines
        /// </summary>
        Board GenerateExpertBoard();

        /// <summary>
        /// Generates a random minesweeper board.
        /// </summary>
        /// <param name="columns">Max value of 30</param>
        /// <param name="rows">Max value of 16</param>
        /// <param name="mineCount">Must be less than tile count (columns * height)</param>
        Board GenerateBoard(byte columns, byte rows, byte mineCount);
    }
}