using System;
using System.Collections.Generic;

namespace MSEngine.Core
{
    public readonly struct Tile : IEquatable<Tile>
    {
        private static readonly IReadOnlyDictionary<TileOperation, TileState> _operationToStateMap = new Dictionary<TileOperation, TileState>
        {
            [TileOperation.Flag] = TileState.Flagged,
            [TileOperation.RemoveFlag] = TileState.Hidden,
            [TileOperation.Reveal] = TileState.Revealed
        };

        internal Tile(Coordinates coordinates, bool hasMine, int adjacentMineCount)
        {
            if (adjacentMineCount < 0 || adjacentMineCount > 8) { throw new ArgumentOutOfRangeException(nameof(adjacentMineCount)); }

            Coordinates = coordinates;
            HasMine = hasMine;
            AdjacentMineCount = (byte)adjacentMineCount;
            State = TileState.Hidden;
        }

        internal Tile(in Tile tile, TileOperation operation)
        {
            if (!Enum.IsDefined(typeof(TileOperation), operation)) { throw new ArgumentOutOfRangeException(nameof(operation)); }

            Coordinates = tile.Coordinates;
            HasMine = tile.HasMine;
            AdjacentMineCount = tile.AdjacentMineCount;
            State = _operationToStateMap[operation];
        }

        public Coordinates Coordinates { get; }
        public bool HasMine { get; }
        public TileState State { get; }

        /// <summary>
        /// Range from 0 to 8
        /// </summary>
        public byte AdjacentMineCount { get; }

        public bool HasMineExploded => HasMine && State == TileState.Revealed;
        public bool SatisfiesWinningCriteria =>
            HasMine
            ? State == TileState.Flagged
            : State == TileState.Revealed;

        public override string ToString() =>
            $"{nameof(Coordinates)}: {Coordinates}, {nameof(HasMine)}: {HasMine}, {nameof(State)}: {State}, {nameof(AdjacentMineCount)}: {AdjacentMineCount}";
        public override int GetHashCode() => (Coordinates, HasMine, State, AdjacentMineCount).GetHashCode();
        public override bool Equals(object obj) => obj is Tile x && Equals(x);
        public bool Equals(Tile other) =>
            Coordinates == other.Coordinates
            && HasMine == other.HasMine
            && State == other.State
            && AdjacentMineCount == other.AdjacentMineCount;
        public static bool operator ==(in Tile c1, in Tile c2) => c1.Equals(c2);
        public static bool operator !=(in Tile c1, in Tile c2) => !(c1 == c2);
    }
}
