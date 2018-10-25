using System;
using System.Linq;
using Xunit;
using MSEngine.Core;

namespace MSEngine.Tests
{
    public class CoordinatesLogicTest
    {
        [Theory]
        [InlineData(1, 1)]
        [InlineData(1, 2)]
        [InlineData(1, 3)]
        [InlineData(2, 1)]
        [InlineData(2, 3)]
        [InlineData(3, 1)]
        [InlineData(3, 2)]
        [InlineData(3, 3)]
        public void Coordinates_are_adjacent(byte x, byte y)
        {
            var origin = new Coordinates(2, 2);
            var coordindates = new Coordinates(x, y);
            var isAdjacent = Engine.IsAdjacentTo(origin, coordindates);

            Assert.True(isAdjacent);
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(0, 1)]
        [InlineData(0, 2)]
        [InlineData(0, 3)]
        [InlineData(0, 4)]
        [InlineData(1, 0)]
        [InlineData(1, 4)]
        [InlineData(2, 0)]
        [InlineData(2, 2)]
        [InlineData(2, 4)]
        [InlineData(3, 0)]
        [InlineData(3, 4)]
        [InlineData(4, 0)]
        [InlineData(4, 1)]
        [InlineData(4, 2)]
        [InlineData(4, 3)]
        [InlineData(4, 4)]
        public void Coordinates_are_not_adjacent(byte x, byte y)
        {
            var origin = new Coordinates(2, 2);
            var coordindates = new Coordinates(x, y);
            var isAdjacent = Engine.IsAdjacentTo(origin, coordindates);

            Assert.False(isAdjacent);
        }
    }
}
