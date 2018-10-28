using System;
using System.Linq;
using System.Collections.Generic;

using MSEngine.Core;

namespace MSEngine.Solver
{
    public class EliteSolver : ISolver
    {
        public Turn ComputeTurn(Board board)
        {
            if (FirstTurnStrategy.TryUseStrategy(board, out var firstTurn))
            {
                return firstTurn;
            }
            if (ChordingStrategy.TryUseStrategy(board, out var chordingTurn))
            {
                return chordingTurn;
            }
            if (MineCountStrategy.TryUseStrategy(board, out var mineCountTurn))
            {
                return mineCountTurn;
            }
            if (PatternStrategy.TryUseStrategy(board, out var patternTurn))
            {
                return patternTurn;
            }

            return EducatedGuessStrategy.UseStrategy(board);
        }
    }
}
