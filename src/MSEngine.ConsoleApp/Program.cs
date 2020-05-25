using System;
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
            const int nodeCount = 30 * 16;
            const int columnCount = 30;

            Span<Node> nodes = stackalloc Node[nodeCount];
            Span<Turn> turns = stackalloc Turn[nodeCount];
            Engine.Instance.FillExpertBoard(nodes);

            var iteration = 0;

            while (true)
            {
                if (iteration == 0)
                {
                    var bar = new Turn(27, NodeOperation.Reveal);

                    // Technically, we don't need to compute the board here, since we can just
                    // inspect the node directly. We do this to maintain strict separation of clients
                    BoardStateMachine.Instance.ComputeBoard(nodes, columnCount, bar);

                    var node = nodes[bar.NodeIndex];
                    if (node.HasMine || node.MineCount > 0)
                    {
                        Engine.Instance.FillExpertBoard(nodes);
                        continue;
                    }
                }
                else
                {
                    var turnCount = MatrixSolver.CalculateTurns(nodes, ref turns, columnCount);
                    foreach (var x in turns.Slice(0, turnCount))
                    {
                        //BoardStateMachine.Instance.EnsureValidBoardConfiguration(nodes, columnCount, x);
                        BoardStateMachine.Instance.ComputeBoard(nodes, columnCount, x);
                    }

                    // if the matrix solver couldn't calculate any turns, we just select a "random" hidden node
                    if (turnCount == 0)
                    {
                        var a = EducatedGuessStrategy.UseStrategy(nodes);
                        BoardStateMachine.Instance.ComputeBoard(nodes, columnCount, a);
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

        private static string GetBoardAsciiArt(ReadOnlySpan<Node> nodes)
        {
            var sb = new StringBuilder(nodes.Length);

            for (var i = 0; i < nodes.Length; i++)
            {
                var node = nodes[i];
                var nodeChar = GetNodeChar(node);
                sb.Append(nodeChar);

                if (i == 7 || i == 15 || i == 23 || i == 31 || i == 39 || i == 47 || i == 55)
                {
                    sb.AppendLine();
                }
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
                case var z when z.State == NodeState.Flagged:
                    return '>';
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