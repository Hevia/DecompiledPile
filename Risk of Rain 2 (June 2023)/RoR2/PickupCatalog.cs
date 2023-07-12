using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using HG;
using JetBrains.Annotations;
using UnityEngine;

namespace RoR2;

public static class PickupCatalog
{
	public struct Enumerator : IEnumerator<PickupIndex>, IEnumerator, IDisposable
	{
		private PickupIndex position;

		public PickupIndex Current => position;

		object IEnumerator.Current => Current;

		public bool MoveNext()
		{
			++position;
			return position.value < pickupCount;
		}

		public void Reset()
		{
			position = PickupIndex.none;
		}

		void IDisposable.Dispose()
		{
		}
	}

	private static PickupDef[] entries = Array.Empty<PickupDef>();

	private static PickupIndex[] itemIndexToPickupIndex = Array.Empty<PickupIndex>();

	private static PickupIndex[] equipmentIndexToPickupIndex = Array.Empty<PickupIndex>();

	private static PickupIndex[] artifactIndexToPickupIndex = Array.Empty<PickupIndex>();

	private static PickupIndex[] miscPickupIndexToPickupIndex = Array.Empty<PickupIndex>();

	private static readonly Dictionary<string, PickupIndex> nameToPickupIndex = new Dictionary<string, PickupIndex>();

	private static readonly Dictionary<ItemTier, PickupIndex> itemTierToPickupIndex = new Dictionary<ItemTier, PickupIndex>();

	public static readonly Color invalidPickupColor = Color.black;

	public static readonly string invalidPickupToken = "???";

	public static Action<List<PickupDef>> modifyPickups;

	public static int pickupCount { get; private set; }

	public static GenericStaticEnumerable<PickupIndex, Enumerator> allPickupIndices => default(GenericStaticEnumerable<PickupIndex, Enumerator>);

	public static IEnumerable<PickupDef> allPickups => entries;

	[NotNull]
	public static T[] GetPerPickupBuffer<T>()
	{
		return new T[pickupCount];
	}

	public static void SetEntries([NotNull] PickupDef[] pickupDefs)
	{
		Array.Resize(ref entries, pickupDefs.Length);
		pickupCount = pickupDefs.Length;
		Array.Copy(pickupDefs, entries, entries.Length);
		Array.Resize(ref itemIndexToPickupIndex, ItemCatalog.itemCount);
		Array.Resize(ref equipmentIndexToPickupIndex, EquipmentCatalog.equipmentCount);
		Array.Resize(ref artifactIndexToPickupIndex, ArtifactCatalog.artifactCount);
		Array.Resize(ref miscPickupIndexToPickupIndex, MiscPickupCatalog.pickupCount);
		nameToPickupIndex.Clear();
		itemTierToPickupIndex.Clear();
		for (int i = 0; i < entries.Length; i++)
		{
			PickupDef pickupDef = entries[i];
			PickupIndex pickupIndex2 = (pickupDef.pickupIndex = new PickupIndex(i));
			if (pickupDef.itemIndex != ItemIndex.None)
			{
				itemIndexToPickupIndex[(int)pickupDef.itemIndex] = pickupIndex2;
			}
			else if (pickupDef.itemTier != ItemTier.NoTier)
			{
				itemTierToPickupIndex.Add(pickupDef.itemTier, pickupDef.pickupIndex);
			}
			if (pickupDef.equipmentIndex != EquipmentIndex.None)
			{
				equipmentIndexToPickupIndex[(int)pickupDef.equipmentIndex] = pickupIndex2;
			}
			if (pickupDef.artifactIndex != ArtifactIndex.None)
			{
				artifactIndexToPickupIndex[(int)pickupDef.artifactIndex] = pickupIndex2;
			}
			if (pickupDef.miscPickupIndex != MiscPickupIndex.None)
			{
				miscPickupIndexToPickupIndex[(int)pickupDef.miscPickupIndex] = pickupIndex2;
			}
		}
		for (int j = 0; j < entries.Length; j++)
		{
			PickupDef pickupDef2 = entries[j];
			nameToPickupIndex[pickupDef2.internalName] = pickupDef2.pickupIndex;
		}
	}

	[SystemInitializer(new Type[]
	{
		typeof(ItemCatalog),
		typeof(EquipmentCatalog),
		typeof(ArtifactCatalog),
		typeof(MiscPickupCatalog)
	})]
	private static void Init()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		List<PickupDef> list = new List<PickupDef>();
		Enumerator<ItemTierDef> enumerator = ItemTierCatalog.allItemTierDefs.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				ItemTierDef current = enumerator.Current;
				PickupDef pickupDef = new PickupDef();
				pickupDef.internalName = "ItemTier." + current.tier;
				pickupDef.itemTier = current.tier;
				pickupDef.dropletDisplayPrefab = current?.dropletDisplayPrefab;
				pickupDef.baseColor = Color32.op_Implicit(ColorCatalog.GetColor(current.colorIndex));
				pickupDef.darkColor = Color32.op_Implicit(ColorCatalog.GetColor(current.darkColorIndex));
				pickupDef.interactContextToken = "ITEM_PICKUP_CONTEXT";
				pickupDef.isLunar = current.tier == ItemTier.Lunar;
				pickupDef.isBoss = current.tier == ItemTier.Boss;
				list.Add(pickupDef);
			}
		}
		finally
		{
			((IDisposable)enumerator).Dispose();
		}
		for (int i = 0; i < ItemCatalog.itemCount; i++)
		{
			PickupDef item = ItemCatalog.GetItemDef((ItemIndex)i).CreatePickupDef();
			list.Add(item);
		}
		for (int j = 0; j < EquipmentCatalog.equipmentCount; j++)
		{
			PickupDef item2 = EquipmentCatalog.GetEquipmentDef((EquipmentIndex)j).CreatePickupDef();
			list.Add(item2);
		}
		for (int k = 0; k < MiscPickupCatalog.pickupCount; k++)
		{
			PickupDef item3 = MiscPickupCatalog.miscPickupDefs[k].CreatePickupDef();
			list.Add(item3);
		}
		for (int l = 0; l < ArtifactCatalog.artifactCount; l++)
		{
			PickupDef item4 = ArtifactCatalog.GetArtifactDef((ArtifactIndex)l).CreatePickupDef();
			list.Add(item4);
		}
		modifyPickups?.Invoke(list);
		SetEntries(list.ToArray());
	}

	public static PickupIndex FindPickupIndex([NotNull] string pickupName)
	{
		if (nameToPickupIndex.TryGetValue(pickupName, out var value))
		{
			return value;
		}
		return PickupIndex.none;
	}

	public static PickupIndex FindPickupIndex(ItemIndex itemIndex)
	{
		return ArrayUtils.GetSafe<PickupIndex>(itemIndexToPickupIndex, (int)itemIndex, ref PickupIndex.none);
	}

	public static PickupIndex FindPickupIndex(ItemTier tier)
	{
		if (itemTierToPickupIndex.TryGetValue(tier, out var value))
		{
			return value;
		}
		return PickupIndex.none;
	}

	public static PickupIndex FindPickupIndex(EquipmentIndex equipmentIndex)
	{
		return ArrayUtils.GetSafe<PickupIndex>(equipmentIndexToPickupIndex, (int)equipmentIndex, ref PickupIndex.none);
	}

	public static PickupIndex FindPickupIndex(ArtifactIndex artifactIndex)
	{
		return ArrayUtils.GetSafe<PickupIndex>(artifactIndexToPickupIndex, (int)artifactIndex, ref PickupIndex.none);
	}

	public static PickupIndex FindPickupIndex(MiscPickupIndex miscIndex)
	{
		return ArrayUtils.GetSafe<PickupIndex>(miscPickupIndexToPickupIndex, (int)miscIndex, ref PickupIndex.none);
	}

	[CanBeNull]
	public static PickupDef GetPickupDef(PickupIndex pickupIndex)
	{
		return ArrayUtils.GetSafe<PickupDef>(entries, pickupIndex.value);
	}

	[NotNull]
	public static GameObject GetHiddenPickupDisplayPrefab()
	{
		return LegacyResourcesAPI.Load<GameObject>("Prefabs/PickupModels/PickupMystery");
	}

	[ConCommand(commandName = "pickup_print_all", flags = ConVarFlags.None, helpText = "Prints all pickup definitions.")]
	private static void CCPickupPrintAll(ConCommandArgs args)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < pickupCount; i++)
		{
			PickupDef pickupDef = GetPickupDef(new PickupIndex(i));
			stringBuilder.Append("[").Append(i).Append("]={internalName=")
				.Append(pickupDef.internalName)
				.Append("}")
				.AppendLine();
		}
		Debug.Log((object)stringBuilder.ToString());
	}
}
