﻿using System;

using MSEngine.Core;

namespace MSEngine.Solver
{
    public static class EliteSolver
    {
        public static (Turn, Strategy) ComputeTurn(Board board)
        {
            if (board == null) { throw new ArgumentNullException(nameof(board)); }

            if (FinishStrategy.TryUseStrategy(board, out var turn))
            {
                return (turn, Strategy.Finish);
            }

            if (FirstTurnStrategy.TryUseStrategy(board, out var firstTurn))
            {
                return (firstTurn, Strategy.FirstTurn);
            }

            if (ChordingStrategy.TryUseStrategy(board, out var chordingTurn))
            {
                return (chordingTurn, Strategy.Chording);
            }

            if (MineCountStrategy.TryUseStrategy(board, out var mineCountTurn))
            {
                return (mineCountTurn, Strategy.MineCount);
            }

            if (PatternStrategy.TryUseStrategy(board, out var patternTurn))
            {
                return (patternTurn, Strategy.Pattern);
            }

            var guess = EducatedGuessStrategy.UseStrategy(board);
            return (guess, Strategy.EducatedGuess);
        }
    }
}
