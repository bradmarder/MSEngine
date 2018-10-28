# MSEngine
#### A Minesweeper Engine built using functional programming paradigms with c# and .NET Standard 2.0


### The core concept of MSEngine (pseudocode)
```csharp
class GameState
{
    Board Board { get; }
    IImmutableQueue<Turn> Turns { get; }
}
static Board ComputeBoard(GameState state) => state.Turns.Aggregate(state.Board, ComputeBoard);
```

### Notes
- With just the initial board and a queue of turns, we can compute the expected state of any minesweeper game
- This approach allows for **replays** and **backwards time travel**
- Inspired by the Starcraft 2 replay engine
- Also inspired by TAS (tool assisted speedruns)
- Everything immutable
- Enforce referential transparency
- The `System.Collections.Immutable` library has abysmal performance relative to it's mutable counterparts

### API
#### All methods are thread safe
```csharp
static Board GenerateRandomBeginnerBoard();
static Board GenerateRandomIntermediateBoard();
static Board GenerateRandomExpertBoard();
static Board GenerateRandomBoard(byte columns, byte rows, byte mineCount);

[Pure]
static void EnsureValidBoardConfiguration(Board board, Turn turn);

[Pure]
static Board ComputeBoard(Board board, Queue<Turn> turns);

[Pure]
static Board ComputeBoard(Board board, Turn turn);
```

### Tests
To run tests, open a terminal and navigate to `src\MSEngine.Tests\` and execute `dotnet test`

### TODO / Future Goals
- ~~NuGet Package~~
- Larger boards (current max is expert which is 30x16)
- Performance Enhancements
- ~~Extensive test suite~~
- Extra Z dimension
- ~~Automated Solvers~~ WIP