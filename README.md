# MSEngine
#### A Minesweeper Engine built using functional programming paradigms with c# and .NET Standard 2.0

### The core concept of MSEngine (pseudocode)
```csharp
readonly struct GameState
{
    Board Board { get; }
    Queue<Turn> Turns { get; }
}
static Board CalculateBoard(GameState state) => state.Turns.Aggregate(state.Board, CalculateBoard);
```

### Notes
- With just the initial board and a queue of turns, we can calculate the expected state of any minesweeper game
- This approach allows for **replays** and **backwards time travel**
- Inspired by the Starcraft 2 replay engine
- Everything immutable
- Enforce referential transparency
- The most complicated aspect of this project is the **chain reaction** tile reveal logic
- The `System.Collections.Immutable` library has abysmal performance relative to it's mutable counterparts

### API
```csharp
static Board GenerateRandomBoard(byte columns, byte rows, byte mineCount);
static Board CalculateBoard(GameState state);
```

### Tests
To run tests, open a terminal and navigate to `src\MSEngine.Tests\` and execute `dotnet test`

### TODO / Future Goals
- NuGet Package
- Larger boards (current max is expert which is 30x16)
- Performance Enhancements
- Extensive test suite
- Extra Z dimension
- Automated Solvers