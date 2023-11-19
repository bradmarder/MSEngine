using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace MSEngine.Core;

public readonly ref struct Matrix<T> where T : struct
{
	[SetsRequiredMembers]
	public Matrix(Span<T> nodes, int columnCount)
	{
		Debug.Assert(nodes.Length > 0);
		Debug.Assert(columnCount > 0);
		Debug.Assert(nodes.Length % columnCount == 0);

		Nodes = nodes;
		ColumnCount = columnCount;
		RowCount = nodes.Length / columnCount;
	}

	public required Span<T> Nodes { get; init; }
	public required int ColumnCount { get; init; }
	public required int RowCount { get; init; }

	public ref T this[int i]
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref GetItemByOffset(i);
	}

	public ref T this[int row, int column]
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			Debug.Assert(column >= 0 && column < ColumnCount);
			Debug.Assert(row >= 0 && row < RowCount);

			return ref GetItemByOffset(row * ColumnCount + column);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private ref T GetItemByOffset(int elementOffset)
	{
		Debug.Assert(elementOffset >= 0);
		Debug.Assert(elementOffset < Nodes.Length);

		ref var data = ref MemoryMarshal.GetReference(Nodes);
		return ref Unsafe.Add(ref data, elementOffset);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Span<T> Vector(int row)
	{
		Debug.Assert(ColumnCount > 1);
		Debug.Assert(row >= 0);
		Debug.Assert(row < RowCount);

		return Nodes.Slice(row * ColumnCount, ColumnCount - 1);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ref readonly T Augment(int row)
	{
		Debug.Assert(ColumnCount > 1);
		Debug.Assert(row >= 0);
		Debug.Assert(row < RowCount);

		return ref GetItemByOffset(row * ColumnCount + ColumnCount - 1);
	}

	public Enumerator GetEnumerator() => new(this);

	public override string ToString()
	{
		var sb = new System.Text.StringBuilder();

		foreach (var row in this)
		{
			foreach (var col in row)
			{
				sb.Append(col + "\t");
			}
			sb.AppendLine();
		}

		return sb.ToString();
	}

	public ref struct Enumerator
	{
		private int _row;
		private readonly Matrix<T> _matrix;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal Enumerator(Matrix<T> matrix)
		{
			_row = -1;
			_matrix = matrix;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool MoveNext()
		{
			int row = _row + 1;
			if (row < _matrix.RowCount)
			{
				_row = row;
				return true;
			}

			return false;
		}

		public readonly Span<T> Current
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => _matrix.Nodes.Slice(_row * _matrix.ColumnCount, _matrix.ColumnCount);
		}
	}
}
