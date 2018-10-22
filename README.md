# MSEngine
#### A Minesweeper Engine built using functional programming paradigms with c# and .NET Standard 2.0

### The Core Concept of MSEngine (pseudocode)
```csharp
public readonly struct GameState
{
    public Board Board { get; }
    public IImmutableQueue<Turn> Turns { get; }
}
while (turns.Any())
{
    var turn = turns.Dequeue();
    board = CalculateBoard(board, turn);
}
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
- More tests
- Extra Z dimension
- Automated Solvers