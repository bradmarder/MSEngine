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
            var watch = new System.Diagnostics.Stopwatch();
            Func<Board> getBoard = () => Engine.GenerateRandomIntermediateBoard();
            watch.Start();

            // 100ms per iteration for random solving expert board
            // this indicates that more complex solving strategies may take significantly longer?
            Enumerable
                .Range(0, 1000).ToList()
                .ForEach(_ =>
                {
                    var board = getBoard();
                    var turnCount = 0;
                    while (board.Status == BoardStatus.Pending)
                    {
                        var foo = GetBoardAsciiArt(board);
                        var (turn, strategy) = EliteSolver.ComputeTurn(board);
                        Computer.EnsureValidBoardConfiguration(board, turn);
                        board = Computer.ComputeBoard(board, turn);

                        // Get new board unless tile has no mine and zero AMC
                        var targetTile = board.Tiles.Single(x => x.Coordinates == turn.Coordinates);
                        if (turnCount == 0 && (board.Status == BoardStatus.Failed || targetTile.AdjacentMineCount > 0))
                        {
                            board = getBoard();
                            continue;
                        }
                        turnCount++;

                        if (turnCount > 255)
                        {
                            Console.WriteLine(turn.Coordinates.X + "-" + turn.Coordinates.Y + "-" + turn.Operation.ToString());
                            Console.WriteLine(foo);
                            Console.WriteLine(GetBoardAsciiArt(board));
                            throw new Exception("TURN OVER 255");
                        }

                        // this region could exist outside the while loop?
                        if (board.Status == BoardStatus.Completed)
                        {
                            Interlocked.Increment(ref wins);
                            Console.WriteLine("win");
                        }
                        if (board.Status == BoardStatus.Failed)
                        {
                            Console.WriteLine("lose");
                        }
                        if (board.Tiles.Any(x => x.State == TileState.Flagged && !x.HasMine))
                        {
                            Console.WriteLine("FALSE FLAG");
                            Console.WriteLine(turn.Coordinates.X + "-" + turn.Coordinates.Y + "-" + turn.Operation.ToString());
                            Console.WriteLine(foo);
                            Console.WriteLine(GetBoardAsciiArt(board));
                            throw new Exception("FIN");
                        }

                        if (board.Status == BoardStatus.Failed && strategy == Strategy.Pattern)
                        {
                            Console.WriteLine("MINE REVEAL");
                            Console.WriteLine(turn.Coordinates.X + "-" + turn.Coordinates.Y + "-" + turn.Operation.ToString());
                            Console.WriteLine(foo);
                            Console.WriteLine(GetBoardAsciiArt(board));
                            throw new Exception("FIN");
                        }
                    }
                });
            watch.Stop();
            Console.WriteLine($"wins = {wins} in {watch.ElapsedMilliseconds} milliseconds");
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
        public static char GetTileChar(in Tile tile)
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
