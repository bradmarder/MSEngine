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
        private static int _wins = 0;
        private static int _gamesPlayedCount = 0;
        private static Stopwatch? _watch;

        static void Main(string[] args)
        {
            _watch = Stopwatch.StartNew();

            RunSimulations(100000);
            //DisplayScore();
        }

        private static void RunSimulations(int count)
        {
            if (count < 1) { throw new ArgumentOutOfRangeException(nameof(count)); }

            ParallelEnumerable
                .Range(0, Environment.ProcessorCount)
                //.WithDegreeOfParallelism(1)
                .ForAll(_ => Master(count / Environment.ProcessorCount));
        }

        private static void DisplayScore()
        {
            var winRatio = ((decimal)_wins / _gamesPlayedCount) * 100;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write($"{_wins} of {_gamesPlayedCount} | {winRatio}%  {_watch!.ElapsedMilliseconds}ms");
        }

        private static void Master(int count)
        {
            //const Difficulty difficulty = Difficulty.Beginner; const int nodeCount = 9 * 9; const int columnCount = 9; const int firstTurnNodeIndex = 20; // 2:2 for beginner/int
            //const int nodeCount = 16 * 16; const int columnCount = 16; const int firstTurnNodeIndex = 49; // 2:2
            const Difficulty difficulty = Difficulty.Expert; const int nodeCount = 30 * 16;const int columnCount = 30; const int firstTurnNodeIndex = 93; // 3:3 for expert

            Span<Node> nodes = stackalloc Node[nodeCount];
            Span<Turn> turns = stackalloc Turn[nodeCount * 3];
            var matrix = new Matrix<Node>(nodes, columnCount);

            while (count > 0)
            {
                ExecuteGame(matrix, turns, firstTurnNodeIndex, difficulty);
                count--;
            }
        }

        private static void ExecuteGame(Matrix<Node> matrix, Span<Turn> turns, int firstTurnNodeIndex, Difficulty difficulty)
        {
            var iteration = 0;

            while (true)
            {
                if (iteration == 0)
                {
                    switch (difficulty)
                    {
                        case Difficulty.Beginner:
                            Engine.FillBeginnerBoard(matrix.Nodes);
                            break;
                        case Difficulty.Intermediate:
                            Engine.FillIntermediateBoard(matrix.Nodes);
                            break;
                        case Difficulty.Expert:
                            Engine.FillExpertBoard(matrix.Nodes);
                            break;
                        default: throw new NotImplementedException();
                    }
                    var turn = new Turn(firstTurnNodeIndex, NodeOperation.Reveal);

                    // Technically, computing the board *before* the check is redundant here, since we can just
                    // inspect the node directly. We do this to maintain strict separation of clients
                    // We could place this ComputeBoard method after the node inspection for perf
                    Engine.ComputeBoard(matrix, turn);

                    var node = matrix.Nodes[turn.NodeIndex];
                    Debug.Assert(node.State == NodeState.Revealed);
                    if (node.HasMine || node.MineCount > 0)
                    {
                        continue;
                    }
                }
                else
                {
                    var turnCount = MatrixSolver.CalculateTurns(matrix, turns, false);
                    if (turnCount == 0)
                    {
                        turnCount = MatrixSolver.CalculateTurns(matrix, turns, true);
                    }
                    foreach (var turn in turns.Slice(0, turnCount))
                    {
                        var node = matrix.Nodes[turn.NodeIndex];
                        if (node.HasMine)
                        {
                            Debug.Assert(turn.Operation == NodeOperation.Flag);
                        }
                        else
                        {
                            Debug.Assert(turn.Operation == NodeOperation.Reveal && node.State == NodeState.Hidden);
                        }
                    }
                    foreach (var turn in turns.Slice(0, turnCount))
                    {
                        Engine.ComputeBoard(matrix, turn);
                    }
                    if (turnCount == 0)
                    {
                        Interlocked.Increment(ref _gamesPlayedCount);
                        break;
                    }
                }
                iteration++;
                
                var status = matrix.Nodes.Status();
                if (status == BoardStatus.Pending)
                {
                    continue;
                }

                Interlocked.Increment(ref _gamesPlayedCount);
                if (status == BoardStatus.Completed)
                {
                    Interlocked.Increment(ref _wins);
                }

                // we only update the score every 10000 games(because doing so within a lock is expensive, and so are console commands)
                if (_gamesPlayedCount % 1000 == 0)
                {
                    DisplayScore();
                }

                break;
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