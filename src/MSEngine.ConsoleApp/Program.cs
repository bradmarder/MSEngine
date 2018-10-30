using System;
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
        static void Main(string[] args)
        {
            var wins = 0;
            var solver = new EliteSolver();
            var watch = new System.Diagnostics.Stopwatch();
            Func<Board> getBoard = () => Engine.GenerateRandomBeginnerBoard();
            watch.Start();

            // 100ms per iteration for random solving expert board
            // this indicates that more complex solving strategies may take significantly longer?
            ParallelEnumerable
                .Range(0, 100)
                .ForAll(_ =>
                {
                    var board = getBoard();
                    var turnCount = 0;
                    while (board.Status == BoardStatus.Pending)
                    {
                        //Console.WriteLine(GetBoardAsciiArt(board));
                        var turn = solver.ComputeTurn(board);
                        //Console.WriteLine(turn.Coordinates.X + "-" + turn.Coordinates.Y + "-" + turn.Operation.ToString());
                        Engine.EnsureValidBoardConfiguration(board, turn);
                        board = Engine.ComputeBoard(board, turn);

                        // Get new board if mine explodes on first reveal
                        if (turnCount == 0 && board.Status == BoardStatus.Failed)
                        {
                            board = getBoard();
                        }
                        turnCount++;
                    }
                    if (board.Status == BoardStatus.Completed)
                    {
                        Interlocked.Increment(ref wins);
                    }
                    if (board.Status == BoardStatus.Failed)
                    {
                        // display strategy
                        // display the turn coordinates/operation
                        // display board before operation
                        // display board after operation
                        Console.WriteLine(GetBoardAsciiArt(board));
                    }
                });
            watch.Stop();
            Console.WriteLine($"wins = {wins} in {watch.ElapsedMilliseconds} milliseconds");

            //var board = Engine.GenerateRandomExpertBoard();
            //var turn = new Turn(4, 4, TileOperation.Reveal);
            //var foo = Engine.ComputeBoard(board, turn);

            //Console.WriteLine(GetBoardAsciiArt(foo));
        }

        private static void MeasureExpertBoardGeneration()
        {
            const int boardCount = 100;
            var genWatch = new System.Diagnostics.Stopwatch();
            genWatch.Start();
            ParallelEnumerable
                .Range(0, boardCount)
                .ForAll(_ => Engine.GenerateRandomExpertBoard());
            genWatch.Stop();
            Console.WriteLine($"Time to generate {boardCount} expert boards = {genWatch.ElapsedMilliseconds} milliseconds");
        }

        public static string GetBoardAsciiArt(Board board)
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
        public static char GetTileChar(Tile tile)
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
