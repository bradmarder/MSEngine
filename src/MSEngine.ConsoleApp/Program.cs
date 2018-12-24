using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

using MSEngine.Core;
using MSEngine.Solver;

namespace MSEngine.ConsoleApp
{
    class Program
    {
        private static readonly object _lock = new object();
        private static int _wins = 0;
        private static int _gamesPlayedCount = 0;

        static void Main(string[] args)
        {
            //RunRandomDistributionTest(Engine.Instance.GenerateRandomBeginnerBoard);
            RunSimulations(10000, Engine.Instance.GenerateExpertBoard);
        }

        private static void RunRandomDistributionTest(Func<Board> boardGenerator, int maxIterationCount = int.MaxValue)
        {
            if (boardGenerator == null) { throw new ArgumentNullException(nameof(boardGenerator)); }

            var iteration = 0;
            var board = boardGenerator();
            var expectedAverage = board.MineCount / (decimal)(board.Width * board.Height);
            var map = board.Tiles.ToDictionary(x => x.Coordinates, _ => 0);

            while (iteration < maxIterationCount)
            {
                iteration++;

                boardGenerator()
                    .Tiles
                    .Where(x => x.HasMine)
                    .ToList()
                    .ForEach(x => map[x.Coordinates]++);

                var means = map
                    .Select(y => y.Value / (decimal)iteration)
                    .ToArray();
                var min = means.Min();
                var max = means.Max();
                var minDiff = Math.Abs(expectedAverage - min); //.00369639666
                var maxDiff = Math.Abs(expectedAverage - max); //.00333032896

                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write($"MinDiff = {minDiff} and MaxDiff = {maxDiff}");

                // beginner
                // MinDiff = 0.0008879570668942427624236854 and MaxDiff = 0.0007066073655878684435107989
                // MinDiff = 0.0003602253545151916915500224 and MaxDiff = 0.0004654803596709192191884712
            }

            Console.ReadLine();
        }

        private static void RunSimulations(int count, Func<Board> boardGenerator)
        {
            if (count < 1) { throw new ArgumentOutOfRangeException(nameof(count)); }
            if (boardGenerator == null) { throw new ArgumentNullException(nameof(boardGenerator)); }

            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();

            ParallelEnumerable
                .Range(0, count)
                .ForAll(_ =>
                {
                    var board = boardGenerator();
                    var turnCount = 0;

                    while (board.Status == BoardStatus.Pending)
                    {
                        var (turn, strategy) = EliteSolver.Instance.ComputeTurn(board);
                        board = BoardStateMachine.Instance.ComputeBoard(board, turn);

                        // Get new board unless tile has no mine and zero AMC
                        var targetTile = board.Tiles.First(x => x.Coordinates == turn.Coordinates);
                        if (turnCount == 0 && (board.Status == BoardStatus.Failed || targetTile.AdjacentMineCount > 0))
                        {
                            board = boardGenerator();
                            continue;
                        }
                        turnCount++;

                        if (board.Status == BoardStatus.Pending)
                        {
                            continue;
                        }

                        Interlocked.Increment(ref _gamesPlayedCount);

                        if (board.Status == BoardStatus.Completed)
                        {
                            Interlocked.Increment(ref _wins);
                        }

                        lock (_lock)
                        {
                            var winRatio = ((decimal)_wins / _gamesPlayedCount) * 100;
                            Console.SetCursorPosition(0, Console.CursorTop);
                            Console.Write($"{_wins} of {_gamesPlayedCount} --- Win Ratio = {winRatio}%  within {watch.ElapsedMilliseconds} milliseconds");
                        }
                    }
                });
        }

        private static string GetBoardAsciiArt(Board board)
        {
            if (board == null) { throw new ArgumentNullException(nameof(board)); }

            var sb = new StringBuilder(board.Tiles.Count());

            for (byte y = 0; y < board.Height; y++)
            {
                for (byte x = 0; x < board.Width; x++)
                {
                    var tile = board.Tiles.Single(t => t.Coordinates.X == x && t.Coordinates.Y == y);
                    var tileChar = GetTileChar(tile);
                    sb.Append(tileChar);

                    if (x + 1 == board.Width)
                    {
                        sb.AppendLine();
                    }
                }
            }

            return sb.ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static char GetTileChar(in Tile tile)
        {
            switch (tile)
            {
                case var z when z.State == TileState.Hidden:
                    return '_';
                case var z when z.State == TileState.Flagged:
                    return '>';
                case var z when z.HasMine:
                    return 'x';
                case var z when z.State == TileState.Revealed:
                    return z.AdjacentMineCount.ToString().First();
                default:
                    throw new NotImplementedException(tile.ToString());
            }
        }
    }
}
