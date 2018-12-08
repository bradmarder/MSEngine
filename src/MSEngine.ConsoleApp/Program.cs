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
            RunBoardRandomSimulation();
            //RunBeginnerSimulations();
        }

        private static void RunBoardRandomSimulation()
        {
            var expectedAverage = 99m / (30 * 16);
            Console.WriteLine("Expected Average = " + expectedAverage);

            var map = Engine
                .GenerateRandomExpertBoard()
                .Tiles
                .ToDictionary(x => x.Coordinates, _ => 0);

            var iteration = 0;
            
            while (true)
            {
                iteration++;

                Engine
                    .GenerateRandomExpertBoard()
                    .Tiles
                    .Where(x => x.HasMine)
                    .ToList()
                    .ForEach(x => map[x.Coordinates]++);

                var means = map
                    .Select(y => y.Value / (decimal)iteration)
                    .ToArray();
                var min = means.Min(); //.00369639666
                var max = means.Max(); //.00333032896

                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write($"Min = {min} and Max = {max}");
            }
        }

        private static void RunBeginnerSimulations()
        {
            var watch = new System.Diagnostics.Stopwatch();
            Func<Board> getBoard = () => Engine.GenerateRandomBeginnerBoard();
            watch.Start();

            ParallelEnumerable
                .Range(0, 20000)
                .ForAll(_ =>
                {
                    var board = getBoard();
                    var turnCount = 0;

                    while (board.Status == BoardStatus.Pending)
                    {
                        var (turn, strategy) = EliteSolver.ComputeTurn(board);
                        board = Computer.ComputeBoard(board, turn);

                        // Get new board unless tile has no mine and zero AMC
                        var targetTile = board.Tiles.Single(x => x.Coordinates == turn.Coordinates);
                        if (turnCount == 0 && (board.Status == BoardStatus.Failed || targetTile.AdjacentMineCount > 0))
                        {
                            board = getBoard();
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
                    throw new NotImplementedException();
            }
        }
    }
}
