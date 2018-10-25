using System;
using System.Linq;
using System.Collections.Immutable;

namespace MSEngine.Core
{
    /// <summary>
    /// Absolute minimum state required
    /// </summary>
    public readonly struct GameState
    {
        public GameState(Board board, IImmutableQueue<Turn> turns)
        {
            Board = board ?? throw new ArgumentNullException(nameof(board));
            Turns = turns ?? throw new ArgumentNullException(nameof(turns));
        }

        public Board Board { get; }
        public IImmutableQueue<Turn> Turns { get; }

        /// <summary>
        /// True if any of the turns have coordinates that are outside the board
        /// </summary>
        public bool HasInvalidTurns
        {
            get
            {
                var boardCoordinates = Board.Tiles.Select(x => x.Coordinates);
                return Turns.Any(x => !boardCoordinates.Contains(x.Coordinates));
            }
        }
    }
}
