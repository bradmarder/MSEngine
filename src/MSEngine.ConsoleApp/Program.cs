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
            Func<Board> getBoard = () => Engine.GenerateRandomBeginnerBoard();
            watch.Start();
            
            ParallelEnumerable
                .Range(0, 10000)
                .ForAll(_ =>
                {
                    var board = getBoard();
                    var turnCount = 0;

                    while (board.Status == BoardStatus.Pending)
                    {
                        var foo = GetBoardAsciiArt(board);
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

                        if (board.Status == BoardStatus.Completed)
                        {
                            Interlocked.Increment(ref wins);
                            Console.WriteLine("win");
                        }
                        if (board.Status == BoardStatus.Failed)
                        {
                            Console.WriteLine("lose");
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
