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

	Span<Node> nodes = stackalloc Node[NodeMatrix.Length];
	Span<Turn> turns = stackalloc Turn[NodeMatrix.Length];
	Span<int> buffer = stackalloc int[NodeMatrix.Length * 2];

	while (count > 0)
	{
		Engine.FillCustomBoard(nodes);
		Engine.ComputeBoard(nodes, firstTurn);
		ExecuteGame(nodes, buffer, turns);
		count--;
	}
}

[MethodImpl(MethodImplOptions.AggressiveInlining)]
void ExecuteGame(Span<Node> nodes, Span<int> buffer, Span<Turn> turns)
{
	while (true)
	{
		var turnCount = MatrixSolver.CalculateTurns(nodes, buffer, turns, false) is { } count && count > 0
			? count
			: MatrixSolver.CalculateTurns(nodes, buffer, turns, true);

		if (turnCount == 0)
		{
			Interlocked.Increment(ref _gamesPlayedCount);
			break;
		}

		var slicedTurns = turns.Slice(0, turnCount);

#if DEBUG
		ValidateTurns(nodes, slicedTurns);
#endif

		foreach (var turn in slicedTurns)
		{
			Engine.ComputeBoard(nodes, turn);
		}

		if (!nodes.IsComplete())
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
static void ValidateTurns(ReadOnlySpan<Node> nodes, ReadOnlySpan<Turn> turns)
#pragma warning restore CS8321 // Local function is declared but never used
{
	foreach (var turn in turns)
	{
		var node = nodes[turn.NodeIndex];
		if (node.HasMine)
		{
			Debug.Assert(turn.Operation is NodeOperation.Flag, "Revealing a mine");
		}
		else
		{
			Debug.Assert(turn.Operation is NodeOperation.Reveal && node.State is NodeState.Hidden, "Flagging a node w/out a mine");
		}
	}
}

#pragma warning disable CS8321 // Local function is declared but never used
static string GetBoardAsciiArt(ReadOnlySpan<Node> nodes)
#pragma warning restore CS8321 // Local function is declared but never used
{
	var sb = new StringBuilder(NodeMatrix.Length);
	int i = 1;

	foreach (var node in nodes)
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
		{ State: NodeState.Hidden } => '□',
		{ HasMine: false, State: NodeState.Flagged } => '!',
		{ State: NodeState.Flagged } => '>',
		{ HasMine: true, State: NodeState.Revealed } => '*',
		{ HasMine: true } => 'x',
		{ State: NodeState.Revealed } => char.Parse(node.MineCount.ToString()),
		_ => throw new NotImplementedException(node.ToString()),
	};
