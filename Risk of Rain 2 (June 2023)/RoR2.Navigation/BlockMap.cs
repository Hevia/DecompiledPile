using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HG;
using Unity.Collections;
using UnityEngine;

namespace RoR2.Navigation;

public class BlockMap<TItem, TItemPositionGetter> where TItemPositionGetter : IPosition3Getter<TItem>
{
	private struct ItemDistanceSqrPair
	{
		public int itemIndex;

		public float distanceSqr;
	}

	private interface ISearchResultHandler
	{
		bool OnEncounterResult(TItem result);
	}

	private struct SingleSearchResultHandler : ISearchResultHandler
	{
		public bool foundResult { get; private set; }

		public TItem result { get; private set; }

		public bool OnEncounterResult(TItem result)
		{
			foundResult = true;
			this.result = result;
			return false;
		}
	}

	private struct ListWriteSearchResultHandler : ISearchResultHandler
	{
		private readonly List<TItem> dest;

		public ListWriteSearchResultHandler(List<TItem> dest)
		{
			this.dest = dest;
		}

		public bool OnEncounterResult(TItem result)
		{
			dest.Add(result);
			return true;
		}
	}

	private struct GridEnumerator
	{
		private readonly Vector3Int startPos;

		private readonly Vector3Int endPos;

		private Vector3Int _current;

		public Vector3Int Current => _current;

		public GridEnumerator(in Vector3Int startCellIndex, in Vector3Int endCellIndex)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			startPos = startCellIndex;
			endPos = endCellIndex;
			_current = startCellIndex;
			ref Vector3Int current = ref _current;
			int x = ((Vector3Int)(ref current)).x - 1;
			((Vector3Int)(ref current)).x = x;
		}

		public bool MoveNext()
		{
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_0098: Unknown result type (might be due to invalid IL or missing references)
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
			ref Vector3Int current = ref _current;
			int num2 = (((Vector3Int)(ref current)).x = ((Vector3Int)(ref current)).x + 1);
			int num3 = num2;
			Vector3Int val = endPos;
			if (num3 >= ((Vector3Int)(ref val)).x)
			{
				ref Vector3Int current2 = ref _current;
				val = startPos;
				((Vector3Int)(ref current2)).x = ((Vector3Int)(ref val)).x;
				ref Vector3Int current3 = ref _current;
				num2 = (((Vector3Int)(ref current3)).z = ((Vector3Int)(ref current3)).z + 1);
				int num5 = num2;
				val = endPos;
				if (num5 >= ((Vector3Int)(ref val)).z)
				{
					ref Vector3Int current4 = ref _current;
					val = startPos;
					((Vector3Int)(ref current4)).z = ((Vector3Int)(ref val)).z;
					ref Vector3Int current5 = ref _current;
					num2 = (((Vector3Int)(ref current5)).y = ((Vector3Int)(ref current5)).y + 1);
					int num7 = num2;
					val = endPos;
					if (num7 >= ((Vector3Int)(ref val)).y)
					{
						ref Vector3Int current6 = ref _current;
						val = startPos;
						((Vector3Int)(ref current6)).y = ((Vector3Int)(ref val)).y;
						return false;
					}
				}
			}
			return true;
		}

		public void Reset()
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			_current = startPos;
		}
	}

	private struct GridEnumerable
	{
		private readonly Vector3Int startPos;

		private readonly Vector3Int endPos;

		public GridEnumerable(Vector3Int startPos, Vector3Int endPos)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			this.startPos = startPos;
			this.endPos = endPos;
		}

		public GridEnumerator GetEnumerator()
		{
			return new GridEnumerator(in startPos, in endPos);
		}
	}

	private const bool debugDraw = false;

	private const bool aggressiveDebug = false;

	private Vector3 cellSize;

	private Vector3 invCellSize;

	private Bounds worldBoundingBox;

	private BlockMapCell[] cells = Array.Empty<BlockMapCell>();

	private int cellCount1D;

	private Vector3Int cellCounts;

	private TItem[] itemsPackedByCell = Array.Empty<TItem>();

	private int itemCount;

	private TItemPositionGetter itemPositionGetter;

	public BlockMap()
		: this(new Vector3(15f, 30f, 15f))
	{
	}//IL_0010: Unknown result type (might be due to invalid IL or missing references)


	public BlockMap(Vector3 cellSize)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		this.cellSize = cellSize;
		invCellSize = new Vector3(1f / cellSize.x, 1f / cellSize.y, 1f / cellSize.z);
		Reset();
	}

	public void Reset()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		worldBoundingBox = default(Bounds);
		ArrayUtils.Clear<TItem>(itemsPackedByCell, ref itemCount);
		cellCounts = Vector3Int.zero;
		cellCount1D = 0;
	}

	public void Set<T>(T newItems, int newItemsLength, TItemPositionGetter newItemPositionGetter) where T : IList<TItem>
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		Reset();
		NativeArray<BlockMapCellIndex> val = default(NativeArray<BlockMapCellIndex>);
		val._002Ector(newItemsLength, (Allocator)2, (NativeArrayOptions)0);
		try
		{
			itemPositionGetter = newItemPositionGetter;
			if (newItems.Count > 0)
			{
				worldBoundingBox = new Bounds(itemPositionGetter.GetPosition3(newItems[0]), Vector3.zero);
				for (int i = 1; i < newItems.Count; i++)
				{
					((Bounds)(ref worldBoundingBox)).Encapsulate(itemPositionGetter.GetPosition3(newItems[i]));
				}
			}
			ref Bounds reference = ref worldBoundingBox;
			((Bounds)(ref reference)).min = ((Bounds)(ref reference)).min - Vector3.one;
			ref Bounds reference2 = ref worldBoundingBox;
			((Bounds)(ref reference2)).max = ((Bounds)(ref reference2)).max + Vector3.one;
			Vector3 size = ((Bounds)(ref worldBoundingBox)).size;
			cellCounts = Vector3Int.Max(Vector3Int.CeilToInt(Vector3.Scale(size, invCellSize)), Vector3Int.one);
			cellCount1D = ((Vector3Int)(ref cellCounts)).x * ((Vector3Int)(ref cellCounts)).y * ((Vector3Int)(ref cellCounts)).z;
			Array.Resize(ref cells, cellCount1D);
			Array.Clear(cells, 0, cells.Length);
			_ = ((Bounds)(ref worldBoundingBox)).min;
			for (int j = 0; j < newItems.Count; j++)
			{
				Vector3 worldPosition = itemPositionGetter.GetPosition3(newItems[j]);
				Vector3Int gridPos = WorldPositionToGridPosFloor(in worldPosition);
				BlockMapCellIndex blockMapCellIndex2 = (val[j] = GridPosToCellIndex(in gridPos));
				cells[(int)blockMapCellIndex2].itemCount++;
			}
			int num = 0;
			for (int k = 0; k < cells.Length; k++)
			{
				ref BlockMapCell reference3 = ref cells[k];
				reference3.itemStartIndex = num;
				num += reference3.itemCount;
			}
			itemCount = newItems.Count;
			ArrayUtils.EnsureCapacity<TItem>(ref itemsPackedByCell, itemCount);
			NativeArray<int> val2 = default(NativeArray<int>);
			val2._002Ector(cells.Length, (Allocator)2, (NativeArrayOptions)1);
			for (int l = 0; l < itemCount; l++)
			{
				BlockMapCellIndex blockMapCellIndex3 = val[l];
				ref BlockMapCell reference4 = ref cells[(int)blockMapCellIndex3];
				TItem val3 = newItems[l];
				int num2 = (int)blockMapCellIndex3;
				int num3 = val2[num2];
				val2[num2] = num3 + 1;
				int num4 = num3;
				itemsPackedByCell[reference4.itemStartIndex + num4] = val3;
			}
			val2.Dispose();
		}
		catch
		{
			Reset();
			throw;
		}
		finally
		{
			val.Dispose();
		}
	}

	private Vector3Int WorldPositionToGridPosFloor(in Vector3 worldPosition)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		Vector3 localPosition = worldPosition - ((Bounds)(ref worldBoundingBox)).min;
		return LocalPositionToGridPosFloor(in localPosition);
	}

	private Vector3Int WorldPositionToGridPosCeil(in Vector3 worldPosition)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		Vector3 localPosition = worldPosition - ((Bounds)(ref worldBoundingBox)).min;
		return LocalPositionToGridPosCeil(in localPosition);
	}

	private Vector3Int LocalPositionToGridPosFloor(in Vector3 localPosition)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		return Vector3Int.FloorToInt(Vector3.Scale(localPosition, invCellSize));
	}

	private Vector3Int LocalPositionToGridPosCeil(in Vector3 localPosition)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		return Vector3Int.CeilToInt(Vector3.Scale(localPosition, invCellSize));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private BlockMapCellIndex GridPosToCellIndex(in Vector3Int gridPos)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		Vector3Int val = gridPos;
		int num = ((Vector3Int)(ref val)).y * ((Vector3Int)(ref cellCounts)).z;
		val = gridPos;
		int num2 = (num + ((Vector3Int)(ref val)).z) * ((Vector3Int)(ref cellCounts)).x;
		val = gridPos;
		return (BlockMapCellIndex)(num2 + ((Vector3Int)(ref val)).x);
	}

	private Vector3Int CellIndexToGridPos(BlockMapCellIndex cellIndex)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		int num = (int)cellIndex;
		Vector3Int zero = Vector3Int.zero;
		int num2 = ((Vector3Int)(ref cellCounts)).x * ((Vector3Int)(ref cellCounts)).z;
		((Vector3Int)(ref zero)).y = num / num2;
		num -= ((Vector3Int)(ref zero)).y * num2;
		((Vector3Int)(ref zero)).z = num / ((Vector3Int)(ref cellCounts)).x;
		num -= ((Vector3Int)(ref zero)).z * ((Vector3Int)(ref cellCounts)).x;
		((Vector3Int)(ref zero)).x = num;
		return zero;
	}

	private Bounds GridPosToBounds(in Vector3Int gridPos)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector3Int.op_Implicit(gridPos);
		((Vector3)(ref val)).Scale(cellSize);
		val += ((Bounds)(ref worldBoundingBox)).min;
		Bounds result = default(Bounds);
		((Bounds)(ref result)).SetMinMax(val, val + cellSize);
		return result;
	}

	private bool GridPosIsValid(Vector3Int gridPos)
	{
		if (((Vector3Int)(ref gridPos)).x >= 0 && ((Vector3Int)(ref gridPos)).y >= 0 && ((Vector3Int)(ref gridPos)).z >= 0 && ((Vector3Int)(ref gridPos)).x < ((Vector3Int)(ref cellCounts)).x && ((Vector3Int)(ref gridPos)).y < ((Vector3Int)(ref cellCounts)).y)
		{
			return ((Vector3Int)(ref gridPos)).z < ((Vector3Int)(ref cellCounts)).z;
		}
		return false;
	}

	public void GetItemsInSphere(Vector3 point, float radius, List<TItem> dest)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		Bounds bounds = GetSphereBounds(point, radius);
		int count = dest.Count;
		GetBoundOverlappingCellItems(in bounds, dest);
		float num = radius * radius;
		for (int num2 = dest.Count - 1; num2 >= count; num2--)
		{
			TItem item = dest[num2];
			Vector3 val = itemPositionGetter.GetPosition3(item) - point;
			if (((Vector3)(ref val)).sqrMagnitude > num)
			{
				dest.RemoveAt(num2);
			}
		}
	}

	public void GetItemsInBounds(in Bounds bounds, List<TItem> dest)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		int count = dest.Count;
		GetBoundOverlappingCellItems(in bounds, dest);
		for (int num = dest.Count - 1; num >= count; num--)
		{
			TItem item = dest[num];
			Vector3 position = itemPositionGetter.GetPosition3(item);
			if (!((Bounds)(ref worldBoundingBox)).Contains(position))
			{
				dest.RemoveAt(num);
			}
		}
	}

	private void GetBoundOverlappingCellItems(in Bounds bounds, List<TItem> dest)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		List<BlockMapCellIndex> list = CollectionPool<BlockMapCellIndex, List<BlockMapCellIndex>>.RentCollection();
		GetBoundOverlappingCells(bounds, list);
		foreach (BlockMapCellIndex item in list)
		{
			ref BlockMapCell reference = ref cells[(int)item];
			int i = reference.itemStartIndex;
			for (int num = reference.itemStartIndex + reference.itemCount; i < num; i++)
			{
				dest.Add(itemsPackedByCell[i]);
			}
		}
		list = CollectionPool<BlockMapCellIndex, List<BlockMapCellIndex>>.ReturnCollection(list);
	}

	private void GetBoundOverlappingCells(Bounds bounds, List<BlockMapCellIndex> dest)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		Vector3 worldPosition = ((Bounds)(ref bounds)).min;
		Vector3Int val = WorldPositionToGridPosFloor(in worldPosition);
		worldPosition = ((Bounds)(ref bounds)).max;
		Vector3Int val2 = WorldPositionToGridPosCeil(in worldPosition);
		((Vector3Int)(ref val)).Clamp(Vector3Int.zero, cellCounts);
		((Vector3Int)(ref val2)).Clamp(Vector3Int.zero, cellCounts);
		Vector3Int gridPos = val;
		((Vector3Int)(ref gridPos)).y = ((Vector3Int)(ref val)).y;
		while (((Vector3Int)(ref gridPos)).y < ((Vector3Int)(ref val2)).y)
		{
			((Vector3Int)(ref gridPos)).z = ((Vector3Int)(ref val)).z;
			int x;
			while (((Vector3Int)(ref gridPos)).z < ((Vector3Int)(ref val2)).z)
			{
				((Vector3Int)(ref gridPos)).x = ((Vector3Int)(ref val)).x;
				while (((Vector3Int)(ref gridPos)).x < ((Vector3Int)(ref val2)).x)
				{
					dest.Add(GridPosToCellIndex(in gridPos));
					x = ((Vector3Int)(ref gridPos)).x + 1;
					((Vector3Int)(ref gridPos)).x = x;
				}
				x = ((Vector3Int)(ref gridPos)).z + 1;
				((Vector3Int)(ref gridPos)).z = x;
			}
			x = ((Vector3Int)(ref gridPos)).y + 1;
			((Vector3Int)(ref gridPos)).y = x;
		}
	}

	public bool GetNearestItemWhichPassesFilter<TItemFilter>(Vector3 position, float maxDistance, ref TItemFilter filter, out TItem dest) where TItemFilter : IBlockMapSearchFilter<TItem>
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		SingleSearchResultHandler searchResultHandler = default(SingleSearchResultHandler);
		GetNearestItemsWhichPassFilter(position, maxDistance, ref filter, ref searchResultHandler);
		dest = (searchResultHandler.foundResult ? searchResultHandler.result : default(TItem));
		return searchResultHandler.foundResult;
	}

	public void GetNearestItemsWhichPassFilter<TItemFilter>(Vector3 position, float maxDistance, ref TItemFilter filter, List<TItem> dest) where TItemFilter : IBlockMapSearchFilter<TItem>
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		ListWriteSearchResultHandler searchResultHandler = new ListWriteSearchResultHandler(dest);
		GetNearestItemsWhichPassFilter(position, maxDistance, ref filter, ref searchResultHandler);
	}

	private void GetNearestItemsWhichPassFilter<TItemFilter, TSearchResultHandler>(Vector3 position, float maxDistance, ref TItemFilter filter, ref TSearchResultHandler searchResultHandler) where TItemFilter : IBlockMapSearchFilter<TItem> where TSearchResultHandler : ISearchResultHandler
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		maxDistance = Mathf.Max(maxDistance, 0f);
		List<ItemDistanceSqrPair> candidatesInsideRadius = CollectionPool<BlockMap<ItemDistanceSqrPair, List<ItemDistanceSqrPair>>.ItemDistanceSqrPair, List<BlockMap<ItemDistanceSqrPair, List<ItemDistanceSqrPair>>.ItemDistanceSqrPair>>.RentCollection();
		List<ItemDistanceSqrPair> candidatesOutsideRadius = CollectionPool<BlockMap<ItemDistanceSqrPair, List<ItemDistanceSqrPair>>.ItemDistanceSqrPair, List<BlockMap<ItemDistanceSqrPair, List<ItemDistanceSqrPair>>.ItemDistanceSqrPair>>.RentCollection();
		float radiusSqr;
		try
		{
			Vector3Int gridPos = WorldPositionToGridPosFloor(in position);
			Bounds val = GridPosToBounds(in gridPos);
			Bounds currentBounds = val;
			float additionalRadius2 = Vector3Utils.ComponentMin(ref cellSize);
			float radius = DistanceToNearestWall(in position, in currentBounds);
			radiusSqr = radius * radius;
			BlockMapCellIndex cellIndex2 = GridPosToCellIndex(in gridPos);
			BoundsInt visitedCells = new BoundsInt(gridPos, Vector3Int.one);
			int visitedCellCount = 0;
			bool num = GridPosIsValid(gridPos);
			if (num)
			{
				VisitCell(cellIndex2);
			}
			if (!num)
			{
				AddRadius(Mathf.Sqrt(((Bounds)(ref worldBoundingBox)).SqrDistance(position)));
			}
			bool flag = true;
			while (flag)
			{
				while (candidatesInsideRadius.Count > 0)
				{
					int num2 = -1;
					float num3 = float.PositiveInfinity;
					for (int i = 0; i < candidatesInsideRadius.Count; i++)
					{
						ItemDistanceSqrPair itemDistanceSqrPair = candidatesInsideRadius[i];
						if (itemDistanceSqrPair.distanceSqr < num3)
						{
							num3 = itemDistanceSqrPair.distanceSqr;
							num2 = i;
						}
					}
					if (num2 != -1)
					{
						ItemDistanceSqrPair itemDistanceSqrPair2 = candidatesInsideRadius[num2];
						candidatesInsideRadius.RemoveAt(num2);
						bool shouldFinish = false;
						if ((filter.CheckItem(itemsPackedByCell[itemDistanceSqrPair2.itemIndex], ref shouldFinish) && !searchResultHandler.OnEncounterResult(itemsPackedByCell[itemDistanceSqrPair2.itemIndex])) || shouldFinish)
						{
							return;
						}
					}
				}
				flag = AddRadius(additionalRadius2);
			}
			bool AddRadius(float additionalRadius)
			{
				//IL_0044: Unknown result type (might be due to invalid IL or missing references)
				//IL_004f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0054: Unknown result type (might be due to invalid IL or missing references)
				//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
				//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
				//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
				//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
				//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
				//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
				//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
				//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
				//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
				//IL_0104: Unknown result type (might be due to invalid IL or missing references)
				//IL_0106: Unknown result type (might be due to invalid IL or missing references)
				radius += additionalRadius;
				bool flag2 = radius >= maxDistance;
				if (flag2)
				{
					radius = maxDistance;
				}
				radiusSqr = radius * radius;
				currentBounds = GetSphereBounds(position, radius);
				for (int num4 = candidatesOutsideRadius.Count - 1; num4 >= 0; num4--)
				{
					ItemDistanceSqrPair item = candidatesOutsideRadius[num4];
					if (item.distanceSqr < radiusSqr)
					{
						candidatesOutsideRadius.RemoveAt(num4);
						candidatesInsideRadius.Add(item);
					}
				}
				bool flag3 = visitedCellCount >= cellCount1D;
				if (!flag3)
				{
					BoundsInt val2 = WorldBoundsToOverlappingGridBoundsClamped(currentBounds);
					BoundsIntDifferenceEnumerable val3 = new BoundsIntDifferenceEnumerable(ref val2, ref visitedCells);
					BoundsIntDifferenceEnumerator enumerator = ((BoundsIntDifferenceEnumerable)(ref val3)).GetEnumerator();
					while (((BoundsIntDifferenceEnumerator)(ref enumerator)).MoveNext())
					{
						Vector3Int gridPos2 = ((BoundsIntDifferenceEnumerator)(ref enumerator)).Current;
						VisitCell(GridPosToCellIndex(in gridPos2));
					}
					visitedCells = val2;
				}
				if (candidatesInsideRadius.Count == 0)
				{
					if (flag2)
					{
						return false;
					}
					if (flag3 && candidatesOutsideRadius.Count == 0)
					{
						return false;
					}
				}
				return true;
			}
			void VisitCell(BlockMapCellIndex cellIndex)
			{
				int num5 = visitedCellCount + 1;
				visitedCellCount = num5;
				ref BlockMapCell reference = ref cells[(int)cellIndex];
				int j = reference.itemStartIndex;
				for (int num6 = reference.itemStartIndex + reference.itemCount; j < num6; j++)
				{
					VisitItem(j);
				}
			}
		}
		finally
		{
			candidatesOutsideRadius = CollectionPool<BlockMap<ItemDistanceSqrPair, List<ItemDistanceSqrPair>>.ItemDistanceSqrPair, List<BlockMap<ItemDistanceSqrPair, List<ItemDistanceSqrPair>>.ItemDistanceSqrPair>>.ReturnCollection((List<BlockMap<ItemDistanceSqrPair, List<ItemDistanceSqrPair>>.ItemDistanceSqrPair>)(object)candidatesOutsideRadius);
			candidatesInsideRadius = CollectionPool<BlockMap<ItemDistanceSqrPair, List<ItemDistanceSqrPair>>.ItemDistanceSqrPair, List<BlockMap<ItemDistanceSqrPair, List<ItemDistanceSqrPair>>.ItemDistanceSqrPair>>.ReturnCollection((List<BlockMap<ItemDistanceSqrPair, List<ItemDistanceSqrPair>>.ItemDistanceSqrPair>)(object)candidatesInsideRadius);
		}
		void VisitItem(int itemIndex)
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			Vector3 position2 = itemPositionGetter.GetPosition3(itemsPackedByCell[itemIndex]);
			ItemDistanceSqrPair itemDistanceSqrPair3 = default(ItemDistanceSqrPair);
			itemDistanceSqrPair3.itemIndex = itemIndex;
			Vector3 val4 = position2 - position;
			itemDistanceSqrPair3.distanceSqr = ((Vector3)(ref val4)).sqrMagnitude;
			ItemDistanceSqrPair item2 = itemDistanceSqrPair3;
			((item2.distanceSqr < radiusSqr) ? candidatesInsideRadius : candidatesOutsideRadius).Add(item2);
		}
	}

	private float DistanceToNearestWall(in Vector3 position, in Bounds bounds)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = position;
		Bounds val2 = bounds;
		Vector3 val3 = val - ((Bounds)(ref val2)).min;
		val2 = bounds;
		Vector3 val4 = ((Bounds)(ref val2)).max - position;
		float x = val3.x;
		x = ((x < val3.y) ? x : val3.y);
		x = ((x < val3.z) ? x : val3.z);
		x = ((x < val4.x) ? x : val4.x);
		x = ((x < val4.y) ? x : val4.y);
		return (x < val4.z) ? x : val4.z;
	}

	private float DistanceToNearestWall(in Bounds innerBounds, in Bounds outerBounds)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		Bounds val = innerBounds;
		Vector3 min = ((Bounds)(ref val)).min;
		val = outerBounds;
		Vector3 val2 = min - ((Bounds)(ref val)).min;
		val = outerBounds;
		Vector3 max = ((Bounds)(ref val)).max;
		val = innerBounds;
		Vector3 val3 = max - ((Bounds)(ref val)).max;
		float x = val2.x;
		x = ((x < val2.y) ? x : val2.y);
		x = ((x < val2.z) ? x : val2.z);
		x = ((x < val3.x) ? x : val3.x);
		x = ((x < val3.y) ? x : val3.y);
		return (x < val3.z) ? x : val3.z;
	}

	private static Bounds GetSphereBounds(Vector3 origin, float radius)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		float num = radius * 2f;
		return new Bounds(origin, new Vector3(num, num, num));
	}

	private BoundsInt WorldBoundsOverlappingToGridBounds(Bounds worldBounds)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		Vector3 worldPosition = ((Bounds)(ref worldBounds)).min;
		Vector3Int val = WorldPositionToGridPosFloor(in worldPosition);
		worldPosition = ((Bounds)(ref worldBounds)).max;
		Vector3Int val2 = WorldPositionToGridPosCeil(in worldPosition);
		BoundsInt result = default(BoundsInt);
		((BoundsInt)(ref result)).SetMinMax(val, val2);
		return result;
	}

	private BoundsInt WorldBoundsToOverlappingGridBoundsClamped(Bounds worldBounds)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		Vector3 worldPosition = ((Bounds)(ref worldBounds)).min;
		Vector3Int val = WorldPositionToGridPosFloor(in worldPosition);
		worldPosition = ((Bounds)(ref worldBounds)).max;
		Vector3Int val2 = WorldPositionToGridPosCeil(in worldPosition);
		((Vector3Int)(ref val)).Clamp(Vector3Int.zero, cellCounts);
		((Vector3Int)(ref val2)).Clamp(Vector3Int.zero, cellCounts);
		BoundsInt result = default(BoundsInt);
		((BoundsInt)(ref result)).SetMinMax(val, val2);
		return result;
	}

	private GridEnumerable ValidGridPositionsInBounds(Bounds bounds)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		Vector3 worldPosition = ((Bounds)(ref bounds)).min;
		Vector3Int startPos = WorldPositionToGridPosFloor(in worldPosition);
		worldPosition = ((Bounds)(ref bounds)).max;
		Vector3Int endPos = WorldPositionToGridPosCeil(in worldPosition);
		((Vector3Int)(ref startPos)).Clamp(Vector3Int.zero, cellCounts);
		((Vector3Int)(ref endPos)).Clamp(Vector3Int.zero, cellCounts);
		return new GridEnumerable(startPos, endPos);
	}

	private GridEnumerable ValidGridPositionsInBoundsWithExclusion(Bounds bounds, BoundsInt excludedCells)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		Vector3 worldPosition = ((Bounds)(ref bounds)).min;
		Vector3Int startPos = WorldPositionToGridPosFloor(in worldPosition);
		worldPosition = ((Bounds)(ref bounds)).max;
		Vector3Int endPos = WorldPositionToGridPosCeil(in worldPosition);
		((Vector3Int)(ref startPos)).Clamp(Vector3Int.zero, cellCounts - Vector3Int.one);
		((Vector3Int)(ref endPos)).Clamp(Vector3Int.zero, cellCounts - Vector3Int.one);
		return new GridEnumerable(startPos, endPos);
	}
}
