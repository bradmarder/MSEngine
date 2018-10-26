﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace MSEngine.Core
{
    public readonly struct Tile
    {
        private static readonly IReadOnlyDictionary<TileOperation, TileState> _operationToStateMap = new Dictionary<TileOperation, TileState>
        {
            [TileOperation.Flag] = TileState.Flagged,
            [TileOperation.RemoveFlag] = TileState.Hidden,
            [TileOperation.Reveal] = TileState.Revealed
        };

        internal Tile(Coordinates coordinates, bool hasMine, byte adjacentMineCount)
        {
            if (adjacentMineCount < 0 || adjacentMineCount > 8) { throw new ArgumentOutOfRangeException(nameof(adjacentMineCount)); }

            Coordinates = coordinates;
            HasMine = hasMine;
            AdjacentMineCount = adjacentMineCount;
            State = TileState.Hidden;
        }

        internal Tile(Tile tile, TileOperation operation)
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
    }
}
