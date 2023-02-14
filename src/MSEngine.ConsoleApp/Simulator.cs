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

args = args.Length == 0 ? new[] { "0", "100000" } : args;
var difficulty = Enum.Parse<Difficulty>(args[0]);
var count = int.Parse(args[1]);
using var _source = new CancellationTokenSource();

_ = LoopScoreLogic();
RunSimulations(difficulty, count);

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

void RunSimulations(Difficulty difficulty, int count)
{
	if (count < 1) { throw new ArgumentOutOfRangeException(nameof(count)); }

	ParallelEnumerable
		.Range(0, Environment.ProcessorCount)
#if DEBUG
		.WithDegreeOfParallelism(1)
#endif
		.ForAll(_ => Master(difficulty, count / Environment.ProcessorCount));
}

[MethodImpl(MethodImplOptions.AggressiveInlining)]
void Master(Difficulty difficulty, int count)
{
	var (nodeCount, columnCount, mineCount, firstTurnNodeIndex) = difficulty switch
	{
		Difficulty.Beginner => (81, 9, 10, 20),
		Difficulty.Intermediate => (16 * 16, 16, 40, 49),
		Difficulty.Expert => (30 * 16, 30, 99, 93),
		_ => throw new NotImplementedException(),
	};

	Span<Node> nodes = stackalloc Node[nodeCount];

	var matrix = new Matrix<Node>(nodes, columnCount);
	var firstTurn = new Turn(firstTurnNodeIndex, NodeOperation.Reveal);

	var buffs = new BufferKeeper
	{
		Turns = stackalloc Turn[nodeCount],
		EdgeIndexes = stackalloc int[Engine.MaxNodeEdges],
		Mines = stackalloc int[mineCount],
		RevealedMineCountNodeIndexes = stackalloc int[nodeCount - mineCount],
		AdjacentHiddenNodeIndexes = stackalloc int[nodeCount],
		Grid = stackalloc float[nodeCount * nodeCount],
	};

	while (count > 0)
	{
		Engine.FillCustomBoard(matrix, buffs.Mines, firstTurnNodeIndex);
		Engine.ComputeBoard(matrix, firstTurn);
		ExecuteGame(matrix, buffs);
		count--;
	}
}

[MethodImpl(MethodImplOptions.AggressiveInlining)]
void ExecuteGame(in Matrix<Node> matrix, in BufferKeeper buffs)
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
		ValidateTurns(matrix.Nodes, slicedTurns);
#endif

		foreach (var turn in slicedTurns)
		{
			Engine.ComputeBoard(matrix, turn);
		}

		if (!matrix.Nodes.IsComplete())
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
			Debug.Assert(turn.Operation == NodeOperation.Flag, "Revealing a mine");
		}
		else
		{
			Debug.Assert(turn.Operation == NodeOperation.Reveal && node.State == NodeState.Hidden, "Flagging a node w/out a mine");
		}
	}
}

#pragma warning disable CS8321 // Local function is declared but never used
static string GetBoardAsciiArt(Matrix<Node> matrix)
#pragma warning restore CS8321 // Local function is declared but never used
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
