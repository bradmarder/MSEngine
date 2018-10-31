using System;
using System.Linq;
using System.Collections.Generic;

using MSEngine.Core;

namespace MSEngine.Solver
{
    public class EliteSolver
    {
        public Turn ComputeTurn(Board board, out Strategy strategy)
        {
            if (FirstTurnStrategy.TryUseStrategy(board, out var firstTurn))
            {
                strategy = Strategy.FirstTurn;
                return firstTurn;
            }
            if (ChordingStrategy.TryUseStrategy(board, out var chordingTurn))
            {
                strategy = Strategy.Chording;
                return chordingTurn;
            }
            if (MineCountStrategy.TryUseStrategy(board, out var mineCountTurn))
            {
                strategy = Strategy.MineCount;
                return mineCountTurn;
            }
            if (PatternStrategy.TryUseStrategy(board, out var patternTurn))
            {
                strategy = Strategy.OneOneRevealPattern;
                return patternTurn;
            }

            strategy = Strategy.EducatedGuess;
            return EducatedGuessStrategy.UseStrategy(board);
        }
    }
}
