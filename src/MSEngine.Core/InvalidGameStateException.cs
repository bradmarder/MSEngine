using System;

namespace MSEngine.Core
{
    public class InvalidGameStateException : Exception
    {
        public InvalidGameStateException(string message) : base(message) { }
    }
}
