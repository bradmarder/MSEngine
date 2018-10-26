using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

using MSEngine.Core;

namespace MSEngine.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            const int boardCount = 100;
            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            ParallelEnumerable
                .Range(0, boardCount)
                .ForAll(_ => Engine.GenerateRandomExpertBoard());
            watch.Stop();
            Console.WriteLine($"Time to generate {boardCount} expert boards = {watch.ElapsedMilliseconds} milliseconds");

            var board = Engine.GenerateRandomExpertBoard();
            var turn = new Turn(new Coordinates(4, 4), TileOperation.Reveal);
            var foo = Engine.CalculateBoard(board, turn);

            Console.WriteLine(GetBoardAsciiArt(foo));
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
