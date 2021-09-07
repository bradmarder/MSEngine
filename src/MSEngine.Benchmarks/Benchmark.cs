using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using MSEngine.Core;
using MSEngine.Solver;
using System.IO;
using MSEngine.Benchmarks;

BenchmarkRunner.Run<Simulator>();

namespace MSEngine.Benchmarks
{
    [MemoryDiagnoser]
    public class Simulator
    {
        private static readonly List<Node> _nodes = new();

        public Simulator()
        {
            // beginner boards should be 162 bytes (2 bytes per node * 81 nodes)
            const int beginnerBoardByteSize = 162;

            using var file = File.Open("~/MSEngine/BeginnerTestGames.bin", FileMode.Open);
            using var serializer = new BinaryReader(file);
            Debug.Assert(file.Length % beginnerBoardByteSize == 0); 

            while (serializer.PeekChar() != -1)
            {
                for (var i = 0; i < 81; i++)
                {
                    var hasMine = serializer.ReadBoolean();
                    var mineCount = serializer.ReadByte();
                    _nodes.Add(new(i, hasMine, mineCount, NodeState.Hidden));
                }
            }
        }

        [Benchmark]
        public void Execute()
        {
            const int mineCount = 10;
            const int nodeCount = 81;
            Span<Node> nodes = stackalloc Node[nodeCount];
            var matrix = new Matrix<Node>(nodes, 9);

            var buffs = new BufferKeeper
            {
                Turns = stackalloc Turn[nodeCount],
                EdgeIndexes = stackalloc int[Engine.MaxNodeEdges],
                Mines = stackalloc int[mineCount],
                VisitedIndexes = stackalloc int[nodeCount - mineCount],
                RevealedMineCountNodeIndexes = stackalloc int[nodeCount - mineCount],
                AdjacentHiddenNodeIndexes = stackalloc int[nodeCount],
                Grid = stackalloc float[nodeCount * nodeCount],
            };

            var firstTurn = new Turn(20, NodeOperation.Reveal);
            var gameCount = _nodes.Count / nodeCount;

            for (var n = 0; n < gameCount; n++)
            {
                for (var i = 0; i < nodeCount; i++)
                {
                    nodes[i] = _nodes[n * nodeCount + i];
                }
                Engine.ComputeBoard(matrix, firstTurn, buffs.VisitedIndexes);

                while (true)
                {
                    var turnCount = MatrixSolver.CalculateTurns(matrix, buffs, false);
                    if (turnCount == 0)
                    {
                        turnCount = MatrixSolver.CalculateTurns(matrix, buffs, true);
                        if (turnCount == 0) { break; }
                    }

                    foreach (var turn in buffs.Turns.Slice(0, turnCount))
                    {
                        Engine.ComputeBoard(matrix, turn, buffs.VisitedIndexes);
                    }
                }
            }
        }
    }
}