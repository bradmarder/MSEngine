using System;
using System.Linq;
using System.Collections.Immutable;

namespace MSEngine.Core
{
    /// <summary>
    /// Absolute minimum state required
    /// </summary>
    public class GameState
    {
        public GameState(Board board, IImmutableQueue<Turn> turns)
        {
            Board = board ?? throw new ArgumentNullException(nameof(board));
            Turns = turns ?? throw new ArgumentNullException(nameof(turns));
        }

        public Board Board { get; }
        public IImmutableQueue<Turn> Turns { get; }
    }
}
