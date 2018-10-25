using System;
using System.Collections.Immutable;
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
            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            Enumerable
                .Range(0, 100)
                .AsParallel()
                .Select(_ => GetTestExpertBoardGenerationTime())
                .ToList();
            watch.Stop();
            Console.WriteLine($"Time to generate 100 expert boards = {watch.ElapsedMilliseconds} milliseconds");

            var board = Engine.GenerateRandomBeginnerBoard();
            var turns = ImmutableQueue.Create(new Turn(new Coordinates(4, 4), TileOperation.Reveal));
            var state = new GameState(board, turns);
            var foo = Engine.CalculateBoard(state);

            Console.WriteLine(GetBoardAsciiArt(foo));

            
            Console.ReadLine();
        }

        private static string GetTestExpertBoardGenerationTime()
        {
            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            var expertBoard = Engine.GenerateRandomExpertBoard();
            watch.Stop();

            return $"{nameof(Engine.GenerateRandomExpertBoard)} time = {watch.ElapsedMilliseconds} milliseconds";
        }

        public static string GetBoardAsciiArt(Board board)
        {
            var sb = new StringBuilder(board.Tiles.Count);

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
