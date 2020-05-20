using System;
using MSEngine.Core;

namespace MSEngine.Solver
{
    public class EliteSolver : ISolver
    {
        public static ISolver Instance { get; } = new EliteSolver();

        public (Turn, Strategy) ComputeTurn(Span<Tile> tiles)
        {
            if (FinishStrategy.TryUseStrategy(tiles, out var turn))
            {
                return (turn, Strategy.Finish);
            }

            if (FirstTurnStrategy.TryUseStrategy(tiles, out var firstTurn))
            {
                return (firstTurn, Strategy.FirstTurn);
            }

            if (ChordingStrategy.TryUseStrategy(tiles, out var chordingTurn))
            {
                return (chordingTurn, Strategy.Chording);
            }

            if (MineCountStrategy.TryUseStrategy(tiles, out var mineCountTurn))
            {
                return (mineCountTurn, Strategy.MineCount);
            }

            if (PatternStrategy.TryUseStrategy(tiles, out var patternTurn))
            {
                return (patternTurn, Strategy.Pattern);
            }

            var guess = EducatedGuessStrategy.UseStrategy(tiles);
            return (guess, Strategy.EducatedGuess);
        }
    }
}
