using System;
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
        private static readonly Stopwatch _watch = Stopwatch.StartNew();
        private static int _wins = 0;
        private static int _gamesPlayedCount = 0;

        static void Main(string[] args)
        {
            var difficulty = Enum.Parse<Difficulty>(args[0]);
            var count = int.Parse(args[1]);

            RunSimulations(difficulty, count);
            DisplayScore();
        }

        private static void RunSimulations(Difficulty difficulty, int count)
        {
            if (count < 1) { throw new ArgumentOutOfRangeException(nameof(count)); }

            ParallelEnumerable
                .Range(0, Environment.ProcessorCount)
                //.WithDegreeOfParallelism(1)
                .ForAll(_ => Master(difficulty, count / Environment.ProcessorCount));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void DisplayScore()
        {
            var x = _gamesPlayedCount;
            var y = _wins;

            // we only update the score every 1000 games(because doing so within a lock is expensive, and so are console commands)
            if (x % 1000 == 0)
            {
                var winRatio = ((decimal)y / x) * 100;

                lock (_lock)
                {
                    Console.SetCursorPosition(0, Console.CursorTop);
                    Console.Write($"{y} of {x} | {winRatio:.0000}%  {_watch.ElapsedMilliseconds}ms");
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Master(Difficulty difficulty, int count)
        {
            var nodeCount = difficulty switch
            {
                Difficulty.Beginner => 81,
                Difficulty.Intermediate => 16 * 16,
                Difficulty.Expert => 30 * 16,
                _ => throw new NotImplementedException()
            };
            byte columnCount = difficulty switch
            {
                Difficulty.Beginner => 9,
                Difficulty.Intermediate => 16,
                Difficulty.Expert => 30,
                _ => throw new NotImplementedException()
            };
            var firstTurnNodeIndex = difficulty switch
            {
                Difficulty.Beginner => 20,
                Difficulty.Intermediate => 49,
                Difficulty.Expert => 93,
                _ => throw new NotImplementedException()
            };
            var mineCount = difficulty switch
            {
                Difficulty.Beginner => 10,
                Difficulty.Intermediate => 40,
                Difficulty.Expert => 99,
                _ => throw new NotImplementedException()
            };

            Span<Node> nodes = stackalloc Node[nodeCount];
            Span<Turn> turns = stackalloc Turn[nodeCount];
            Span<int> mines = stackalloc int[mineCount];
            Span<int> visitedIndexes = stackalloc int[nodeCount - mineCount];

            var matrix = new Matrix<Node>(nodes, columnCount);

            while (count > 0)
            {
                ApplyFirstTurn(matrix, mines, columnCount, firstTurnNodeIndex, visitedIndexes);
                ExecuteGame(matrix, turns, visitedIndexes);
                DisplayScore();
                count--;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ApplyFirstTurn(Matrix<Node> matrix, Span<int> mines, byte columnCount, int firstTurnNodeIndex, Span<int> visitedIndexes)
        {
            var firstTurn = new Turn(firstTurnNodeIndex, NodeOperation.Reveal);

            Engine.FillCustomBoard(matrix, mines, columnCount, firstTurnNodeIndex);
            Engine.ComputeBoard(matrix, firstTurn, visitedIndexes);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ExecuteGame(Matrix<Node> matrix, Span<Turn> turns, Span<int> visitedIndexes)
        {
            while (true)
            {
                var turnCount = MatrixSolver.CalculateTurns(matrix, turns, false);
                if (turnCount == 0)
                {
                    turnCount = MatrixSolver.CalculateTurns(matrix, turns, true);
                }

                if (turnCount == 0)
                {
                    Interlocked.Increment(ref _gamesPlayedCount);
                    break;
                }

                ValidateTurns(matrix.Nodes, turns, turnCount);

                foreach (var turn in turns.Slice(0, turnCount))
                {
                    Engine.ComputeBoard(matrix, turn, visitedIndexes);
                }

                if (!matrix.Nodes.IsComplete())
                {
                    continue;
                }

                Interlocked.Increment(ref _gamesPlayedCount);
                Interlocked.Increment(ref _wins);

                break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ValidateTurns(ReadOnlySpan<Node> nodes, Span<Turn> turns, int turnCount)
        {
            foreach (var turn in turns.Slice(0, turnCount))
            {
                var node = nodes[turn.NodeIndex];
                if (node.HasMine)
                {
                    Debug.Assert(turn.Operation == NodeOperation.Flag);
                }
                else
                {
                    Debug.Assert(turn.Operation == NodeOperation.Reveal && node.State == NodeState.Hidden);
                }
            }
        }

        private static string GetBoardAsciiArt(Matrix<Node> matrix)
        {
            var sb = new StringBuilder(matrix.Nodes.Length);

            foreach (var row in matrix)
            {
                foreach (var node in row)
                {
                    sb.Append(GetNodeChar(node));
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static char GetNodeChar(in Node node)
        {
            switch (node)
            {
                case var z when z.State == NodeState.Hidden:
                    return '_';
                case var z when !z.HasMine && z.State == NodeState.Flagged:
                    return '!';
                case var z when z.State == NodeState.Flagged:
                    return '>';
                case var z when z.HasMine && z.State == NodeState.Revealed:
                    return '*';
                case var z when z.HasMine:
                    return 'x';
                case var z when z.State == NodeState.Revealed:
                    return z.MineCount.ToString().First();
                default:
                    throw new NotImplementedException(node.ToString());
            }
        }
    }
}