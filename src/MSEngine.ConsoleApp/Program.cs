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
        private static readonly object _lock = new();
        private static readonly Stopwatch _watch = Stopwatch.StartNew();
        private static int _wins = 0;
        private static int _gamesPlayedCount = 0;

        static void Main(string[] args)
        {
            args = args.Length == 0 ? new[] { "0", "100000" } : args;
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
#if DEBUG
                .WithDegreeOfParallelism(1)
#endif
                .ForAll(_ => Master(difficulty, count / Environment.ProcessorCount));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void DisplayScore()
        {
            var x = _gamesPlayedCount;
            var y = _wins;

            // we only update the score every 10000 games(because doing so within a lock is expensive, and so are console commands)
            if (x % 10000 == 0)
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
            var (nodeCount, columnCount, mineCount, firstTurnNodeIndex) = difficulty switch
            {
                Difficulty.Beginner => (81, 9, 10, 20),
                Difficulty.Intermediate => (16 * 16, 16, 40, 49),
                Difficulty.Expert => (30 * 16, 30, 99, 93),
                _ => throw new NotImplementedException()
            };

            Span<Node> nodes = stackalloc Node[nodeCount];

            var matrix = new Matrix<Node>(nodes, columnCount);
            var firstTurn = new Turn(firstTurnNodeIndex, NodeOperation.Reveal);

            var buffs = new BufferKeeper
            {
                Turns = stackalloc Turn[nodeCount],
                EdgeIndexes = stackalloc int[Engine.MaxNodeEdges],
                Mines = stackalloc int[mineCount],
                VisitedIndexes = stackalloc int[nodeCount - mineCount],
                RevealedMineCountNodeIndexes = stackalloc int[nodeCount - mineCount],
                AdjacentHiddenNodeIndexes = stackalloc int[nodeCount],
                Grid = stackalloc float[nodeCount * nodeCount]
            };

            while (count > 0)
            {
                Engine.FillCustomBoard(matrix, buffs.Mines, firstTurnNodeIndex);
                Engine.ComputeBoard(matrix, firstTurn, buffs.VisitedIndexes);
                ExecuteGame(matrix, buffs);
                DisplayScore();
                count--;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ExecuteGame(in Matrix<Node> matrix, in BufferKeeper buffs)
        {
            var turnCount = 0;
            while (true)
            {
                turnCount = MatrixSolver.CalculateTurns(matrix, buffs, false);
                if (turnCount == 0)
                {
                    turnCount = MatrixSolver.CalculateTurns(matrix, buffs, true);
                }

                if (turnCount == 0)
                {
                    Interlocked.Increment(ref _gamesPlayedCount);
                    break;
                }

                var slicedTurns = buffs.Turns.Slice(0, turnCount);

#if DEBUG
                ValidateTurns(matrix.Nodes, slicedTurns);
#endif

                foreach (var turn in slicedTurns)
                {
                    Engine.ComputeBoard(matrix, turn, buffs.VisitedIndexes);
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
        private static void ValidateTurns(ReadOnlySpan<Node> nodes, ReadOnlySpan<Turn> turns)
        {
            foreach (var turn in turns)
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
        private static char GetNodeChar(in Node node) =>
            node switch
            {
                { State: NodeState.Hidden } => '_',
                { HasMine: false, State: NodeState.Flagged } => '!',
                { State: NodeState.Flagged } => '>',
                { HasMine: true, State: NodeState.Revealed } => '*',
                { HasMine: true } => 'x',
                { State: NodeState.Revealed } => char.Parse(node.MineCount.ToString()),
                _ => throw new NotImplementedException(node.ToString())
            };
    }
}