using System;
using System.Collections.Generic;

namespace MSEngine.Core
{
    public interface IBoardStateMachine
    {
        /// <summary>
        /// Validates a board-turn configuration-operation.
        /// Calling this method is optional - It's intended usage is only when building-testing a client
        /// </summary>
        /// <exception cref="InvalidGameStateException">
        /// -Multiple tiles have matching coordinates
        /// -Turns are not allowed if board status is completed/failed
        /// -Turn has coordinates that are outside the board
        /// -No more flags available
        /// -Only chord operations are allowed on revealed tiles
        /// -May not flag a tile that is already flagged
        /// -Impossible to remove flag from un-flagged tile
        /// -May only chord a revealed tile
        /// -May only chord a tile that has adjacent mines
        /// -May only chord a tile when adjacent mine count equals adjacent tile flag count
        /// -May only chord a tile that has hidden adjacent tiles
        /// </exception>
        void EnsureValidBoardConfiguration(Board board, Turn turn);
        
        /// <summary>
        /// Returns a new Board with turns applied
        /// </summary>
        /// <param name="board"></param>
        /// <param name="turns"></param>
        /// <returns></returns>
        Board ComputeBoard(Board board, IEnumerable<Turn> turns);

        /// <summary>
        /// Returns a new Board with the turn applied
        /// </summary>
        /// <param name="board"></param>
        /// <param name="turn"></param>
        /// <returns></returns>
        Board ComputeBoard(Board board, Turn turn);
    }
}