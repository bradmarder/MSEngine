using System;
using System.Collections.Generic;
using System.Diagnostics;

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

        internal Tile(bool hasMine, int adjacentMineCount)
        {
            Debug.Assert(adjacentMineCount >= 0);
            Debug.Assert(adjacentMineCount <= 8);
            
            HasMine = hasMine;
            AdjacentMineCount = (byte)adjacentMineCount;
            State = TileState.Hidden;
        }

        internal Tile(in Tile tile, TileOperation operation)
        {
            Debug.Assert(Enum.IsDefined(typeof(TileOperation), operation));

            HasMine = tile.HasMine;
            AdjacentMineCount = tile.AdjacentMineCount;
            State = _operationToStateMap[operation];
        }

        internal Tile(bool hasMine, int adjacentMineCount, TileOperation operation)
        {
            Debug.Assert(adjacentMineCount >= 0);
            Debug.Assert(adjacentMineCount <= 8);
            Debug.Assert(Enum.IsDefined(typeof(TileOperation), operation));

            HasMine = hasMine;
            AdjacentMineCount = (byte)adjacentMineCount;
            State = _operationToStateMap[operation];
        }

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
            $"{nameof(HasMine)}: {HasMine}, {nameof(State)}: {State}, {nameof(AdjacentMineCount)}: {AdjacentMineCount}";
        public override int GetHashCode() => HashCode.Combine(HasMine, State, AdjacentMineCount);
        public override bool Equals(object? obj) => obj is Tile x && Equals(x);
        public bool Equals(Tile other) =>
            HasMine == other.HasMine
            && State == other.State
            && AdjacentMineCount == other.AdjacentMineCount;
        public static bool operator ==(in Tile c1, in Tile c2) => c1.Equals(c2);
        public static bool operator !=(in Tile c1, in Tile c2) => !(c1 == c2);
    }
}
