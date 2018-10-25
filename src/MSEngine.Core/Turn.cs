﻿using System;

namespace MSEngine.Core
{
    public readonly struct Turn
    {
        public Turn(byte columns, byte rows, TileOperation operation) : this(new Coordinates(columns, rows), operation) { }
        public Turn(Coordinates coordinates, TileOperation operation)
        {
            if (!Enum.IsDefined(typeof(TileOperation), operation)) { throw new ArgumentOutOfRangeException(nameof(operation)); }

            Coordinates = coordinates;
            Operation = operation;
            CreatedDate = DateTimeOffset.UtcNow;
        }
        
        public Coordinates Coordinates { get; }
        public TileOperation Operation { get; }
        public DateTimeOffset CreatedDate { get; }
    }
}
