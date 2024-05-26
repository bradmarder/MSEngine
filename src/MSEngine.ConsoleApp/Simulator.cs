using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MSEngine.Core;
using MSEngine.Solver;

var _watch = Stopwatch.StartNew();
var _wins = 0;
var _gamesPlayedCount = 0;
var count = args.Length == 0 ? 1_000_000 : int.Parse(args[0]);
using var _source = new CancellationTokenSource();

_ = LoopScoreLogic();
RunSimulations(count);

async Task LoopScoreLogic()
{
	while (!_source.Token.IsCancellationRequested)
	{
		await DisplayScore();
		try
		{
			await Task.Delay(250, _source.Token);
		}
		catch (TaskCanceledException)
		{
			await DisplayScore();
			break;
		}
	}
}
async Task DisplayScore()
{
	var x = _gamesPlayedCount;
	var y = _wins;
	var winRatio = x == 0 ? 0 : (decimal)y / x * 100;
	Console.SetCursorPosition(0, Console.CursorTop);
	await Console.Out.WriteAsync($"{y} of {x} | {winRatio:.0000}%  {_watch.ElapsedMilliseconds}ms");
}

void RunSimulations(int count)
{
	if (count < 1) { throw new ArgumentOutOfRangeException(nameof(count)); }

	ParallelEnumerable
		.Range(0, Environment.ProcessorCount)
#if DEBUG
		.WithDegreeOfParallelism(1)
#endif
		.ForAll(_ => Master(count / Environment.ProcessorCount));
}

[MethodImpl(MethodImplOptions.AggressiveInlining)]
void Master(int count)
{
	var firstTurn = new Turn(NodeMatrix.SafeNodeIndex, NodeOperation.Reveal);
	var buffs = new BufferKeeper
	{
		Turns = stackalloc Turn[NodeMatrix.Length],
		RevealedMineCountNodeIndexes = stackalloc int[NodeMatrix.Length - Minefield.Length],
		AdjacentHiddenNodeIndexes = stackalloc int[NodeMatrix.Length],
		Grid = stackalloc float[NodeMatrix.Length * NodeMatrix.Length],
	};

	Span<Node> nodes = stackalloc Node[NodeMatrix.Length];
	while (count > 0)
	{
		Engine.FillCustomBoard(nodes);
        Engine.ComputeBoard(nodes, firstTurn);
		ExecuteGame(nodes, buffs);
		count--;
	}
}

[MethodImpl(MethodImplOptions.AggressiveInlining)]
void ExecuteGame(Span<Node> matrix, in BufferKeeper buffs)
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
		ValidateTurns(matrix, slicedTurns);
#endif

		foreach (var turn in slicedTurns)
		{
			Engine.ComputeBoard(matrix, turn);
		}

		if (!matrix.IsComplete())
		{
			continue;
		}

		Interlocked.Increment(ref _gamesPlayedCount);
		Interlocked.Increment(ref _wins);

		break;
	}
}

[MethodImpl(MethodImplOptions.AggressiveInlining)]
#pragma warning disable CS8321 // Local function is declared but never used
static void ValidateTurns(NodeMatrix nodes, ReadOnlySpan<Turn> turns)
#pragma warning restore CS8321 // Local function is declared but never used
{
	foreach (var turn in turns)
	{
		var node = nodes[turn.NodeIndex];
		if (node.HasMine)
		{
			Debug.Assert(turn.Operation == NodeOperation.Flag, "Revealing a mine");
		}
		else
		{
			Debug.Assert(turn.Operation == NodeOperation.Reveal && node.State == NodeState.Hidden, "Flagging a node w/out a mine");
		}
	}
}

#pragma warning disable CS8321 // Local function is declared but never used
static string GetBoardAsciiArt(NodeMatrix matrix)
#pragma warning restore CS8321 // Local function is declared but never used
{
	var sb = new StringBuilder(NodeMatrix.Length);
	int i = 1;

	foreach (var node in matrix)
	{
		sb.Append(GetNodeChar(node));

		if (i % NodeMatrix.ColumnCount == 0)
		{
			sb.AppendLine();
		}
		i++;
	}

	return sb.ToString();
}

[MethodImpl(MethodImplOptions.AggressiveInlining)]
static char GetNodeChar(in Node node) =>
	node switch
	{
		{ State: NodeState.Hidden } => '_',
		{ HasMine: false, State: NodeState.Flagged } => '!',
		{ State: NodeState.Flagged } => '>',
		{ HasMine: true, State: NodeState.Revealed } => '*',
		{ HasMine: true } => 'x',
		{ State: NodeState.Revealed } => char.Parse(node.MineCount.ToString()),
		_ => throw new NotImplementedException(node.ToString()),
	};
