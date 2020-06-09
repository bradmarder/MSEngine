using System;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
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
            //var a = new System.Numerics.BigInteger();
            //Console.WriteLine("1 = " + BitOperations.Log2(1));
            //Console.WriteLine("2 = " + BitOperations.Log2(2));
            //Console.WriteLine("4 = " + BitOperations.Log2(4));
            //Console.WriteLine("10 = " + BitOperations.Log2(10));
            //Console.WriteLine("100 = " + BitOperations.Log2(100));

            
            RunSimulations(100000);
            //DisplayScore();
        }

        private static void RunSimulations(int count)
        {
            if (count < 1) { throw new ArgumentOutOfRangeException(nameof(count)); }

            ParallelEnumerable
                .Range(0, count)
                .WithDegreeOfParallelism(1)
                .ForAll(_ => ExecuteGame());
        }

        private static void DisplayScore()
        {
            var winRatio = ((decimal)_wins / _gamesPlayedCount) * 100;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write($"{_wins} of {_gamesPlayedCount} | {winRatio}%  {_watch!.ElapsedMilliseconds}ms");
        }

        private static void ExecuteGame()
        {
            const int nodeCount = 8 * 8; const int columnCount = 8; const int firstTurnNodeIndex = 18; // 2:2 for beginner/int
            //const int nodeCount = 30 * 16;const int columnCount = 30; const int firstTurnNodeIndex = 93; // 3:3 for expert

            Span<Node> nodes = stackalloc Node[nodeCount];
            Span<Turn> turns = stackalloc Turn[nodeCount];
            var matrix = new Matrix<Node>(nodes, columnCount);

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
                    Engine.Instance.ComputeBoard(matrix, turn);

                    var node = nodes[turn.NodeIndex];
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
                        var node = nodes[turn.NodeIndex];
                        Debug.Assert(node.HasMine ? turn.Operation == NodeOperation.Flag : turn.Operation == NodeOperation.Reveal);
                        Engine.Instance.ComputeBoard(matrix, turn);
                    }
                    if (turnCount == 0)
                    {
                        //Console.WriteLine(GetBoardAsciiArt(matrix));
                        //Interlocked.Increment(ref _gamesPlayedCount);
                        //break;
                        Console.WriteLine("BOARD");
                        Console.WriteLine(GetBoardAsciiArt(matrix));
                        var turn = ProbabilitySolver.ComputeTurn(matrix);
                        Console.WriteLine(turn);
                        Interlocked.Increment(ref _gamesPlayedCount);
                        break;
                        //Engine.Instance.ComputeBoard(matrix, turn);
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

            for (var i = 0; i < matrix.Nodes.Length; i++)
            {
                var node = matrix.Nodes[i];
                var nodeChar = GetNodeChar(node);
                sb.Append(nodeChar);

                if (i > 0 && (i + 1) % matrix.ColumnCount == 0) 
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