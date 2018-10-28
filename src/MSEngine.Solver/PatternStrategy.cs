using System;
using System.Linq;
using System.Collections.Generic;

using MSEngine.Core;

namespace MSEngine.Solver
{
    public static class PatternStrategy
    {
        public static bool TryUseStrategy(Board board, out Turn turn)
        {
            if (board == null) { throw new ArgumentNullException(nameof(board)); }

            turn = default;
            return false;
        }
    }
}
