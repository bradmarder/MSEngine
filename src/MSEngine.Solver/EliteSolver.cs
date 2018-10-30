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
                Console.WriteLine("FirstTurnStrategy");
                return firstTurn;
            }
            if (ChordingStrategy.TryUseStrategy(board, out var chordingTurn))
            {
                Console.WriteLine("ChordingStrategy");
                return chordingTurn;
            }
            if (MineCountStrategy.TryUseStrategy(board, out var mineCountTurn))
            {
                Console.WriteLine("MineCountStrategy");
                return mineCountTurn;
            }
            if (PatternStrategy.TryUseStrategy(board, out var patternTurn))
            {
                Console.WriteLine("PatternStrategy");
                return patternTurn;
            }

            Console.WriteLine("EducatedGuessStrategy");
            return EducatedGuessStrategy.UseStrategy(board);
        }
    }
}
