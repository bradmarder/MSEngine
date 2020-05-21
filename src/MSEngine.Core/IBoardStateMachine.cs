using System;

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
        void EnsureValidBoardConfiguration(ReadOnlySpan<Tile> tiles, Turn turn);
        
        void ComputeBoard(Span<Tile> tiles, ReadOnlySpan<Turn> turns);
        void ComputeBoard(Span<Tile> tiles, Turn turn);
    }
}