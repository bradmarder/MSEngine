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
            Span<Node> nodes = stackalloc Node[8 * 8];
            Span<Turn> turns = stackalloc Turn[0];
            Engine.Instance.FillBeginnerBoard(nodes);

            var turnCount = 0;

            while (true)
            {
                if (turnCount == 0)
                {
                    turns = stackalloc Turn[1]
                    {
                        new Turn(27, NodeOperation.Reveal)
                    };
                }
                if (turns.Length == 0)
                {
                    turns = stackalloc Turn[nodes.Length];
                    MatrixSolver.CalculateTurns(nodes, ref turns);
                }

                // if the matrix solver couldn't calculate any turns, we just select a "random" hidden node
                if (turns.Length == 0)
                {
                    turns = stackalloc Turn[1]
                    {
                        EducatedGuessStrategy.UseStrategy(nodes)
                    };
                }

                var turn = turns[0];
                turns = turns.Slice(1, turns.Length - 1);
                //if (turnCount > 0)
                //{
                //    BoardStateMachine.Instance.EnsureValidBoardConfiguration(nodes, turn);
                //}
                BoardStateMachine.Instance.ComputeBoard(nodes, turn);

                // Get new board unless node has no mine and zero AMC
                var status = nodes.Status();
                if (turnCount == 0 && (nodes[turn.NodeIndex].MineCount > 0 || status == BoardStatus.Failed))
                {
                    // nodes.Clear(); not required since every node is always reset
                    Engine.Instance.FillBeginnerBoard(nodes);
                    turns = Span<Turn>.Empty;
                    continue;
                }
                turnCount++;

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