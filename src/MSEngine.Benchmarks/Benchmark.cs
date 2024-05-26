using System;
using System.Collections.Generic;
using System.Diagnostics;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using MSEngine.Core;
using MSEngine.Solver;
using System.IO;

BenchmarkRunner.Run<Simulator>();

//foreach (var x in Enumerable.Range(0, 16 * 16))
//{
//	var val = new int[8];
//	OldUtils.FillAdjacentNodeIndexes(val, 16 * 16, x, 16);
//	var arr = string.Join(", ", val.Where(x => x != -1));

//	Console.WriteLine($"{x} => [{arr}],");
//}

public class OldUtils
{
	public static void FillAdjacentNodeIndexes(Span<int> indexes, int nodeCount, int index, int columnCount)
	{
		Debug.Assert(nodeCount > 0);
		Debug.Assert(index >= 0);
		Debug.Assert(index < nodeCount);
		Debug.Assert(columnCount > 0);
		Debug.Assert(nodeCount % columnCount == 0);

		var indexPlusOne = index + 1;
		var isTop = index < columnCount;
		var isLeftSide = index % columnCount == 0;
		var isRightSide = indexPlusOne % columnCount == 0;
		var isBottom = index >= nodeCount - columnCount;

		if (isTop)
		{
			indexes[0] = indexes[1] = indexes[2] = -1;
		}
		else
		{
			var val = index - columnCount;
			if (!isLeftSide)
			{
				indexes[0] = val - 1;
			}
			indexes[1] = val;
			if (!isRightSide)
			{
				indexes[2] = val + 1;
			}
		}

		if (isLeftSide)
		{
			indexes[0] = indexes[3] = indexes[5] = -1;
		}
		else
		{
			indexes[3] = index - 1;
		}

		if (isRightSide)
		{
			indexes[2] = indexes[4] = indexes[7] = -1;
		}
		else
		{
			indexes[4] = indexPlusOne;
		}

		if (isBottom)
		{
			indexes[5] = indexes[6] = indexes[7] = -1;
		}
		else
		{
			var val = index + columnCount;
			if (!isLeftSide)
			{
				indexes[5] = val - 1;
			}
			indexes[6] = val;
			if (!isRightSide)
			{
				indexes[7] = val + 1;
			}
		}
	}
}


[MemoryDiagnoser]
public class Simulator
{
	private static readonly List<Node> _nodes = new();

	public Simulator()
	{
		// beginner boards should be 162 bytes (2 bytes per node * 81 nodes)
		const int beginnerBoardByteSize = 162;

		//var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
		var name = Path.Combine("C:\\repos\\MSEngine\\BeginnerTestGames.bin");
		using var file = File.Open(name, FileMode.Open);
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
		var matrix = new NodeMatrix();
		var buffs = new BufferKeeper
		{
			Turns = stackalloc Turn[NodeMatrix.Length],
			RevealedMineCountNodeIndexes = stackalloc int[NodeMatrix.Length - Minefield.Length],
			AdjacentHiddenNodeIndexes = stackalloc int[NodeMatrix.Length],
			Grid = stackalloc float[NodeMatrix.Length * NodeMatrix.Length],
		};
		var firstTurn = new Turn(20, NodeOperation.Reveal);
		var gameCount = _nodes.Count / NodeMatrix.Length;

		for (var n = 0; n < gameCount; n++)
		{
			for (var i = 0; i < NodeMatrix.Length; i++)
			{
				matrix[i] = _nodes[n * NodeMatrix.Length + i];
			}
			Engine.ComputeBoard(ref matrix, firstTurn);

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
					Engine.ComputeBoard(ref matrix, turn);
				}
			}
		}
	}
}
