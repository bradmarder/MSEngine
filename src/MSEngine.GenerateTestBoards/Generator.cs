using MSEngine.Core;
using System;
using System.IO;

var difficulty = Enum.Parse<Difficulty>(args[0]);
var gameCount = int.Parse(args[1]);

var (nodeCount, columnCount, mineCount, firstTurnNodeIndex) = difficulty switch
{
	Difficulty.Beginner => (81, 9, 10, 20),
	Difficulty.Intermediate => (16 * 16, 16, 40, 49),
	Difficulty.Expert => (30 * 16, 30, 99, 93),
	_ => throw new NotImplementedException(),
};

var matrix = new Matrix<Node>(stackalloc Node[nodeCount], columnCount);
Span<int> mines = stackalloc int[mineCount];

var name = Path.Combine("C:", "MSEngine", $"{difficulty}TestGames.bin");
using var file = File.Open(name, FileMode.Create);
using var serializer = new BinaryWriter(file);

for (var i = 0; i < gameCount; i++)
{
	Engine.FillCustomBoard(matrix, mines, firstTurnNodeIndex);
	foreach (var node in matrix.Nodes)
	{
		serializer.Write(node.HasMine);
		serializer.Write(node.MineCount);
	}
}
