using MSEngine.Core;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSEngine.GenerateTestBoards
{
    class Program
    {
        static void Main(string[] args)
        {
            var gameCount = int.Parse(args[0]);
            var matrix = new Matrix<Node>(stackalloc Node[81], 9);
            Span<int> mines = stackalloc int[10];

            var name = Path.Combine("C:", "MSEngine", "TestBeginnerGames.bin");
            using var file = File.Open(name, FileMode.Create);
            using var serializer = new BinaryWriter(file);

            for (var i = 0; i < gameCount; i++)
            {
                Engine.FillCustomBoard(matrix, mines, 20);
                foreach (var node in matrix.Nodes)
                {
                    serializer.Write(node.HasMine);
                    serializer.Write(node.MineCount);
                }
            }
        }
    }
}
