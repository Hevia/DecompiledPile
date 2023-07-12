using System;
using System.Collections.Generic;
using System.Linq;
using HG;
using JetBrains.Annotations;
using RoR2.ContentManagement;
using RoR2.Modding;
using UnityEngine;

namespace RoR2;

public static class SurvivorCatalog
{
	public static int survivorMaxCount = 10;

	private static SurvivorDef[] survivorDefs = Array.Empty<SurvivorDef>();

	private static SurvivorDef[] _orderedSurvivorDefs = Array.Empty<SurvivorDef>();

	private static SurvivorIndex[] bodyIndexToSurvivorIndex = Array.Empty<SurvivorIndex>();

	private static BodyIndex[] survivorIndexToBodyIndex = Array.Empty<BodyIndex>();

	private static int[] survivorOrderPositions = Array.Empty<int>();

	private static string[] cachedSurvivorNames = Array.Empty<string>();

	public static SurvivorDef defaultSurvivor;

	public static int survivorCount => survivorDefs.Length;

	public static SurvivorIndex endIndex => (SurvivorIndex)survivorDefs.Length;

	public static IEnumerable<SurvivorDef> allSurvivorDefs => survivorDefs;

	public static IEnumerable<SurvivorDef> orderedSurvivorDefs => _orderedSurvivorDefs;

	[Obsolete("Use IContentPackProvider instead.")]
	public static event Action<List<SurvivorDef>> getAdditionalSurvivorDefs
	{
		add
		{
			LegacyModContentPackProvider.instance.HandleLegacyGetAdditionalEntries("RoR2.SurvivorCatalog.getAdditionalSurvivorDefs", value, LegacyModContentPackProvider.instance.registrationContentPack.survivorDefs);
		}
		remove
		{
		}
	}

	private static void ValidateEntry(SurvivorDef survivorDef)
	{
		if (Object.op_Implicit((Object)(object)survivorDef.bodyPrefab))
		{
			CharacterBody component = survivorDef.bodyPrefab.GetComponent<CharacterBody>();
			if (Object.op_Implicit((Object)(object)component))
			{
				if (string.IsNullOrEmpty(survivorDef.cachedName))
				{
					string text = BodyCatalog.GetBodyName(component.bodyIndex);
					string text2 = "Body";
					if (text.EndsWith(text2))
					{
						text = text.Substring(0, text.Length - text2.Length);
					}
					survivorDef.cachedName = text;
				}
				if (survivorDef.displayNameToken == null)
				{
					survivorDef.displayNameToken = component.baseNameToken;
				}
			}
		}
		survivorDef.displayNameToken = survivorDef.displayNameToken ?? "";
	}

	[CanBeNull]
	public static SurvivorDef GetSurvivorDef(SurvivorIndex survivorIndex)
	{
		return ArrayUtils.GetSafe<SurvivorDef>(survivorDefs, (int)survivorIndex);
	}

	public static SurvivorIndex GetSurvivorIndexFromBodyIndex(BodyIndex bodyIndex)
	{
		SurvivorIndex[] array = bodyIndexToSurvivorIndex;
		SurvivorIndex survivorIndex = SurvivorIndex.None;
		return ArrayUtils.GetSafe<SurvivorIndex>(array, (int)bodyIndex, ref survivorIndex);
	}

	public static BodyIndex GetBodyIndexFromSurvivorIndex(SurvivorIndex survivorIndex)
	{
		BodyIndex[] array = survivorIndexToBodyIndex;
		BodyIndex bodyIndex = BodyIndex.None;
		return ArrayUtils.GetSafe<BodyIndex>(array, (int)survivorIndex, ref bodyIndex);
	}

	public static bool SurvivorIsUnlockedOnThisClient(SurvivorIndex survivorIndex)
	{
		return LocalUserManager.readOnlyLocalUsersList.Any((LocalUser localUser) => localUser.userProfile.HasSurvivorUnlocked(survivorIndex));
	}

	[CanBeNull]
	public static SurvivorDef FindSurvivorDefFromBody([CanBeNull] GameObject characterBodyPrefab)
	{
		for (int i = 0; i < survivorDefs.Length; i++)
		{
			SurvivorDef survivorDef = survivorDefs[i];
			GameObject val = (((Object)(object)survivorDef != (Object)null) ? survivorDef.bodyPrefab : null);
			if ((Object)(object)characterBodyPrefab == (Object)(object)val)
			{
				return survivorDef;
			}
		}
		return null;
	}

	[CanBeNull]
	public static Texture GetSurvivorPortrait(SurvivorIndex survivorIndex)
	{
		SurvivorDef survivorDef = GetSurvivorDef(survivorIndex);
		if ((Object)(object)survivorDef.bodyPrefab != (Object)null)
		{
			CharacterBody component = survivorDef.bodyPrefab.GetComponent<CharacterBody>();
			if (Object.op_Implicit((Object)(object)component))
			{
				return component.portraitIcon;
			}
		}
		return null;
	}

	public static SurvivorIndex FindSurvivorIndex([CanBeNull] string survivorName)
	{
		return (SurvivorIndex)Array.IndexOf(cachedSurvivorNames, survivorName);
	}

	public static SurvivorDef FindSurvivorDef([CanBeNull] string survivorName)
	{
		return GetSurvivorDef(FindSurvivorIndex(survivorName));
	}

	public static int GetSurvivorOrderedPosition(SurvivorIndex survivorIndex)
	{
		int[] array = survivorOrderPositions;
		int num = -1;
		return ArrayUtils.GetSafe<int>(array, (int)survivorIndex, ref num);
	}

	[SystemInitializer(new Type[] { typeof(BodyCatalog) })]
	private static void Init()
	{
		SetSurvivorDefs(ContentManager.survivorDefs);
	}

	private static void SetSurvivorDefs(SurvivorDef[] newSurvivorDefs)
	{
		SurvivorDef[] array = survivorDefs;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].survivorIndex = SurvivorIndex.None;
		}
		ArrayUtils.CloneTo<SurvivorDef>(newSurvivorDefs, ref survivorDefs);
		Array.Resize(ref survivorIndexToBodyIndex, survivorDefs.Length);
		Array.Resize(ref cachedSurvivorNames, survivorDefs.Length);
		Array.Resize(ref _orderedSurvivorDefs, survivorDefs.Length);
		Array.Resize(ref survivorOrderPositions, survivorDefs.Length);
		Array.Resize(ref bodyIndexToSurvivorIndex, BodyCatalog.bodyCount);
		SurvivorIndex[] array2 = bodyIndexToSurvivorIndex;
		SurvivorIndex survivorIndex = SurvivorIndex.None;
		ArrayUtils.SetAll<SurvivorIndex>(array2, ref survivorIndex);
		for (SurvivorIndex survivorIndex2 = (SurvivorIndex)0; (int)survivorIndex2 < survivorDefs.Length; survivorIndex2++)
		{
			SurvivorDef survivorDef2 = survivorDefs[(int)survivorIndex2];
			survivorDef2.survivorIndex = survivorIndex2;
			ValidateEntry(survivorDef2);
			cachedSurvivorNames[(int)survivorIndex2] = survivorDef2.cachedName;
			BodyIndex bodyIndex = BodyCatalog.FindBodyIndex(survivorDef2.bodyPrefab);
			survivorIndexToBodyIndex[(int)survivorIndex2] = bodyIndex;
			bodyIndexToSurvivorIndex[(int)bodyIndex] = survivorIndex2;
		}
		ArrayUtils.CloneTo<SurvivorDef>(survivorDefs, ref _orderedSurvivorDefs);
		Array.Sort(_orderedSurvivorDefs, (SurvivorDef a, SurvivorDef b) => a.desiredSortPosition.CompareTo(b.desiredSortPosition));
		for (int j = 0; j < _orderedSurvivorDefs.Length; j++)
		{
			SurvivorIndex survivorIndex3 = _orderedSurvivorDefs[j].survivorIndex;
			survivorOrderPositions[(int)survivorIndex3] = j;
		}
		ViewablesCatalog.Node node = new ViewablesCatalog.Node("Survivors", isFolder: true);
		foreach (SurvivorDef survivorDef in allSurvivorDefs)
		{
			ViewablesCatalog.Node survivorEntryNode = new ViewablesCatalog.Node(survivorDef.cachedName, isFolder: false, node);
			survivorEntryNode.shouldShowUnviewed = (UserProfile userProfile) => !userProfile.HasViewedViewable(survivorEntryNode.fullName) && Object.op_Implicit((Object)(object)survivorDef.unlockableDef) && userProfile.HasUnlockable(survivorDef.unlockableDef);
		}
		ViewablesCatalog.AddNodeToRoot(node);
		defaultSurvivor = FindSurvivorDef("Commando");
	}
}
