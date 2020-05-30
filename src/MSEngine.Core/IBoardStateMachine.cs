namespace MSEngine.Core
{
    public interface IBoardStateMachine
    {
        /// <summary>
        /// Validates a board-turn configuration-operation.
        /// Calling this method is optional - It's intended usage is only when building-testing a client
        /// </summary>
        /// <exception cref="InvalidGameStateException">
        /// -Multiple nodes have matching coordinates
        /// -Turns are not allowed if board status is completed/failed
        /// -Turn has coordinates that are outside the board
        /// -No more flags available
        /// -Only chord operations are allowed on revealed nodes
        /// -May not flag a node that is already flagged
        /// -Impossible to remove flag from un-flagged node
        /// -May only chord a revealed node
        /// -May only chord a node that has adjacent mines
        /// -May only chord a node when adjacent mine count equals adjacent node flag count
        /// -May only chord a node that has hidden adjacent nodes
        /// </exception>
        void EnsureValidBoardConfiguration(Matrix<Node> matrix, Turn turn);
        
        void ComputeBoard(Matrix<Node> matrix, Turn turn);
    }
}