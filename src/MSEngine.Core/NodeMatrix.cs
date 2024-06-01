namespace MSEngine.Core;

#if BEGINNER
[InlineArray(81)]
#elif INTERMEDIATE
[InlineArray(256)]
#elif EXPERT
[InlineArray(480)]
#elif TEST
[InlineArray(64)]
#endif
public struct NodeMatrix
{
	private Node _element0;

#if BEGINNER
	public const int Length = 81;
	public const int ColumnCount = 9;
	public const int RowCount = 9;
	public const int SafeNodeIndex = 20;
#elif INTERMEDIATE
	public const int Length = 256;
	public const int ColumnCount = 16;
	public const int RowCount = 16;
	public const int SafeNodeIndex = 49;
#elif EXPERT
	public const int Length = 480;
	public const int ColumnCount = 30;
	public const int RowCount = 16;
	public const int SafeNodeIndex = 93;
#elif TEST
	public const int Length = 64;
	public const int ColumnCount = 8;
	public const int RowCount = 8;
	public const int SafeNodeIndex = 4;
#endif
}

#if BEGINNER
[InlineArray(10)]
#elif INTERMEDIATE
[InlineArray(40)]
#elif EXPERT
[InlineArray(99)]
#elif TEST
[InlineArray(8)]
#endif
public struct Minefield
{
	private int _element0;

#if BEGINNER
	public const int Length = 10;
#elif INTERMEDIATE
	public const int Length = 40;
#elif EXPERT
	public const int Length = 99;
#elif TEST
	public const int Length = 4;
#endif
}