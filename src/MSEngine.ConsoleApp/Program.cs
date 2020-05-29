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
        private static readonly Stopwatch _watch = Stopwatch.StartNew();

        static void Main(string[] args)
        {
            RunSimulations(100000);
        }

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
            const int nodeCount = 8 * 8;
            const int columnCount = 8;
            //const int nodeCount = 30 * 16;
            //const int columnCount = 30;
            const int firstTurnNodeIndex = nodeCount / 2;

            Span<Node> nodes = stackalloc Node[nodeCount];
            Span<Turn> turns = stackalloc Turn[nodeCount];

            var iteration = 0;

            while (true)
            {
                if (iteration == 0)
                {
                    Engine.Instance.FillBeginnerBoard(nodes);
                    var turn = new Turn(firstTurnNodeIndex, NodeOperation.Reveal);

                    // Technically, computing the board *before* the check is redundant here, since we can just
                    // inspect the node directly. We do this to maintain strict separation of clients
                    // We could place this ComputeBoard method after the node inspection for perf
                    BoardStateMachine.Instance.ComputeBoard(nodes, columnCount, turn);

                    var node = nodes[turn.NodeIndex];
                    Debug.Assert(node.State == NodeState.Revealed);
                    if (node.HasMine || node.MineCount > 0)
                    {
                        continue;
                    }
                }
                else
                {
                    var turnCount = MatrixSolver.CalculateTurns(nodes, ref turns, columnCount, false);
                    if (turnCount == 0)
                    {
                        turnCount = MatrixSolver.CalculateTurns(nodes, ref turns, columnCount, true);
                    }
                    foreach (var turn in turns.Slice(0, turnCount))
                    {
                        BoardStateMachine.Instance.ComputeBoard(nodes, columnCount, turn);
                    }
                    if (turnCount == 0)
                    {
                        var turn = NodeStrategies.RevealFirstHiddenNode(nodes);
                        BoardStateMachine.Instance.ComputeBoard(nodes, columnCount, turn);
                    }
                }
                iteration++;
                
                var status = nodes.Status();
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

        private static string GetBoardAsciiArt(ReadOnlySpan<Node> nodes, int columnCount)
        {
            var sb = new StringBuilder(nodes.Length);

            for (var i = 0; i < nodes.Length; i++)
            {
                var node = nodes[i];
                var nodeChar = GetNodeChar(node);
                sb.Append(nodeChar);

                if (i % columnCount == 1) 
                {
                    sb.AppendLine();
                    throw new NotImplementedException();
                }
            }

            return sb.ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static char GetNodeChar(Node node)
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