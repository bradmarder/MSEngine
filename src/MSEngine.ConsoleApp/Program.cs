using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
            // RunSimulations(1, () => Engine.Instance.GenerateCustomBoard(4, 4, 2));
            RunSimulations(10000);

            //GetCoordinates(5, 1).ToList().ForEach(x => Console.Write(x));
        }


        //private static void RunRandomDistributionTest(Func<Board> boardGenerator, int maxIterationCount = int.MaxValue)
        //{
        //    var iteration = 0;
        //    var board = boardGenerator();
        //    var expectedAverage = board.MineCount / (decimal)(board.Width * board.Height);
        //    var map = board.Tiles.ToDictionary(x => x.Coordinates, _ => 0);

        //    while (iteration < maxIterationCount)
        //    {
        //        iteration++;

        //        boardGenerator()
        //            .Tiles
        //            .Where(x => x.HasMine)
        //            .ToList()
        //            .ForEach(x => map[x.Coordinates]++);

        //        var means = map
        //            .Select(y => y.Value / (decimal)iteration)
        //            .ToArray();
        //        var min = means.Min();
        //        var max = means.Max();
        //        var minDiff = Math.Abs(expectedAverage - min); //.00369639666
        //        var maxDiff = Math.Abs(expectedAverage - max); //.00333032896

        //        Console.SetCursorPosition(0, Console.CursorTop);
        //        Console.Write($"MinDiff = {minDiff} and MaxDiff = {maxDiff}");

        //        // beginner
        //        // MinDiff = 0.0008879570668942427624236854 and MaxDiff = 0.0007066073655878684435107989
        //        // MinDiff = 0.0003602253545151916915500224 and MaxDiff = 0.0004654803596709192191884712
        //    }

        //    Console.ReadLine();
        //}

        private static void RunSimulations(int count)
        {
            if (count < 1) { throw new ArgumentOutOfRangeException(nameof(count)); }

            var watch = System.Diagnostics.Stopwatch.StartNew();

            ParallelEnumerable
                .Range(0, count)
                //.WithDegreeOfParallelism(1)
                .ForAll(_ =>
                {
                    Span<Tile> tiles = stackalloc Tile[8 * 8];
                    Span<Turn> turns = stackalloc Turn[0];
                    Engine.Instance.GenerateBeginnerBoard(tiles);

                    var turnCount = 0;

                    while (true)
                    {
                        if (turnCount == 0)
                        {
                            turns = stackalloc Turn[1]
                            {
                                new Turn(3, 3, TileOperation.Reveal)
                            };
                        }
                        if (turns.Length == 0)
                        {
                            turns = stackalloc Turn[tiles.Length];
                            MatrixSolver.CalculateTurns(tiles, ref turns);
                        }

                        // if the matrix solver couldn't calculate any turns, we just select a "random" hidden tile
                        if (turns.Length == 0)
                        {
                            turns = stackalloc Turn[1]
                            {
                                EducatedGuessStrategy.UseStrategy(tiles)
                            };
                        }

                        // dequeue the final (or first?) turn and slice the turns
                        var bar = turns.Length - 1;
                        var turn = turns[bar];
                        turns = turns.Slice(0, bar);

                        if (turnCount > 0)
                        {
                            try
                            {
                               // BoardStateMachine.Instance.EnsureValidBoardConfiguration(tiles, turn);
                            }
                            catch (Exception)
                            {
                                Console.WriteLine("EVIL TURN = " + turn.ToString());
                                Console.WriteLine(GetBoardAsciiArt(tiles));
                                throw;
                            }
                        }
                        
                        BoardStateMachine.Instance.ComputeBoard(tiles, turn);

                        // Get new board unless tile has no mine and zero AMC
                        var targetTile = BoardStateMachine.GetTargetTile(tiles, turn.Coordinates);

                        var status = tiles.Status();
                        if (turnCount == 0 && (targetTile.AdjacentMineCount > 0 || status == BoardStatus.Failed))
                        {
                            Engine.Instance.GenerateBeginnerBoard(tiles);
                            turns = Span<Turn>.Empty;
                            continue;
                        }
                        turnCount++;

                        if (status == BoardStatus.Pending)
                        {
                            continue;
                        }

                        Interlocked.Increment(ref _gamesPlayedCount);
                        if (status == BoardStatus.Completed)
                        {
                            Interlocked.Increment(ref _wins);
                        }

                        lock (_lock)
                        {
                            var winRatio = ((decimal)_wins / _gamesPlayedCount) * 100;
                            Console.SetCursorPosition(0, Console.CursorTop);
                            Console.Write($"{_wins} of {_gamesPlayedCount} | {winRatio}%  {watch.ElapsedMilliseconds}ms");
                        }

                        break;
                    }
                });
        }

        private static string GetBoardAsciiArt(Span<Tile> tiles)
        {
            var sb = new StringBuilder(tiles.Length);

            for (byte y = 0; y < tiles.Height(); y++)
            {
                for (byte x = 0; x < tiles.Width(); x++)
                {
                    var tile = tiles.ToArray().Single(t => t.Coordinates.X == x && t.Coordinates.Y == y);
                    var tileChar = GetTileChar(tile);
                    sb.Append(tileChar);

                    if (x + 1 == tiles.Width())
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