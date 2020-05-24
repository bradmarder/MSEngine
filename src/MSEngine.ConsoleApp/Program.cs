﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private static readonly Stopwatch _watch = Stopwatch.StartNew();

        static void Main(string[] args)
        {
            RunSimulations(100000);
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

            ParallelEnumerable
                .Range(0, count)
                //.WithDegreeOfParallelism(1)
                .ForAll(_ => ExecuteGame());
        }

        private static void ExecuteGame()
        {
            Span<Tile> tiles = stackalloc Tile[8 * 8];
            Span<Turn> turns = stackalloc Turn[0];
            Engine.Instance.FillBeginnerBoard(tiles);

            var turnCount = 0;

            while (true)
            {
                if (turnCount == 0)
                {
                    turns = stackalloc Turn[1]
                    {
                        new Turn(24, TileOperation.Reveal)
                    };
                }
                if (turns.Length == 0)
                {
                    turns = stackalloc Turn[tiles.Length];
                    MatrixSolver.CalculateTurns(tiles, ref turns);
                    //foreach (var x in turns)
                    //{
                    //    Console.WriteLine(x.ToString());
                    //}
                    //if (turns.Length == 0)
                    //{
                    //    Console.WriteLine("ZERO TURNS");
                    //}
                    //for (var i = 0; i < tiles.Length; i++)
                    //{
                    //    Console.WriteLine($"index is {i} " + tiles[i].ToString());
                    //}    
                }

                // if the matrix solver couldn't calculate any turns, we just select a "random" hidden tile
                if (turns.Length == 0)
                {
                    turns = stackalloc Turn[1]
                    {
                        EducatedGuessStrategy.UseStrategy(tiles)
                    };
                }

                var turn = turns[0];
                turns = turns.Slice(1, turns.Length - 1);

                BoardStateMachine.Instance.ComputeBoard(tiles, turn);

                // Get new board unless tile has no mine and zero AMC
                var targetTile = tiles[turn.TileIndex];

                var status = tiles.Status();
                if (turnCount == 0 && (targetTile.AdjacentMineCount > 0 || status == BoardStatus.Failed))
                {
                    // tiles.Clear(); not required since every tile is always reset
                    Engine.Instance.FillBeginnerBoard(tiles);
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


                // we only update the score every 1000 games (because doing so within a lock is expensive)
                if (_gamesPlayedCount % 1000 == 0)
                {
                    var winRatio = ((decimal)_wins / _gamesPlayedCount) * 100;
                    Console.SetCursorPosition(0, Console.CursorTop);
                    Console.Write($"{_wins} of {_gamesPlayedCount} | {winRatio}%  {_watch.ElapsedMilliseconds}ms");
                }

                break;
            }
        }

        private static string GetBoardAsciiArt(Span<Tile> tiles)
        {
            var sb = new StringBuilder(tiles.Length);

            for (var i = 0; i < tiles.Length; i++)
            {
                var tile = tiles[i];
                var tileChar = GetTileChar(tile);
                sb.Append(tileChar);

                if (i > 0 && i % 7 == 0)
                {
                    sb.AppendLine();
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