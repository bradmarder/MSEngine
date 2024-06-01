using System;
using System.Collections.Generic;
using System.Diagnostics;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using MSEngine.Core;
using MSEngine.Solver;
using System.IO;
using System.Threading;

//var sim = new Simulator();
//sim.Execute();


BenchmarkRunner.Run<Simulator>();

//foreach (var x in Enumerable.Range(0, 64))
//{
//	var val = new int[8];
//	OldUtils.FillAdjacentNodeIndexes(val, 64, x, 8);
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
		const int nodeTotalBytes = 480 * 2;

		//var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
		//var name = Path.Combine("C:\\repos\\MSEngine\\BeginnerTestGames.bin");
		using var file = File.Open("C:\\Users\\Brad\\Documents\\ExpertGames_1000.bin", FileMode.Open);
		using var serializer = new BinaryReader(file);
		Debug.Assert(file.Length % nodeTotalBytes == 0);

		while (serializer.PeekChar() != -1)
		{
			for (var i = 0; i < NodeMatrix.Length; i++)
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
		Span<Node> nodes = stackalloc Node[NodeMatrix.Length];
		Span<Turn> turns = stackalloc Turn[NodeMatrix.Length];
		Span<int> buffer = stackalloc int[NodeMatrix.Length * 2];

		var firstTurn = new Turn(NodeMatrix.SafeNodeIndex, NodeOperation.Reveal);
		var gameCount = _nodes.Count / NodeMatrix.Length;

		for (var n = 0; n < gameCount; n++)
		{
			_nodes
				.Slice(n * NodeMatrix.Length, NodeMatrix.Length)
				.CopyTo(nodes);

			Engine.ComputeBoard(nodes, firstTurn);

			while (true)
			{
				var turnCount = MatrixSolver.CalculateTurns(nodes, buffer, turns, false) is { } count && count > 0
					? count
					: MatrixSolver.CalculateTurns(nodes, buffer, turns, true);

				if (turnCount == 0) { break; }

				foreach (var turn in turns.Slice(0, turnCount))
				{
					Engine.ComputeBoard(nodes, turn);
				}
			}
		}
	}
}
