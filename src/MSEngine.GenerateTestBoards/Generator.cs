using MSEngine.Core;
using System;
using System.IO;

var difficulty = Enum.Parse<Difficulty>(args[0]);
var gameCount = int.Parse(args[1]);

Span<Node> nodes = stackalloc Node[NodeMatrix.Length];

var name = Path.Combine(Directory.GetCurrentDirectory(), $"{difficulty}Games_{gameCount}.bin");
using var file = File.Open(name, FileMode.Create);
using var serializer = new BinaryWriter(file);

for (var i = 0; i < gameCount; i++)
{
	Engine.FillCustomBoard(nodes);
	foreach (var node in nodes)
	{
		serializer.Write(node.HasMine);
		serializer.Write(node.MineCount);
	}
}
