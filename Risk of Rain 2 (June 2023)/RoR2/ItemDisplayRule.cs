using System;
using UnityEngine;

namespace RoR2;

[Serializable]
public struct ItemDisplayRule : IEquatable<ItemDisplayRule>
{
	public ItemDisplayRuleType ruleType;

	public GameObject followerPrefab;

	public string childName;

	public Vector3 localPos;

	public Vector3 localAngles;

	public Vector3 localScale;

	public LimbFlags limbMask;

	public bool Equals(ItemDisplayRule other)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		if (ruleType == other.ruleType && object.Equals(followerPrefab, other.followerPrefab) && string.Equals(childName, other.childName) && ((Vector3)(ref localPos)).Equals(other.localPos) && ((Vector3)(ref localAngles)).Equals(other.localAngles) && ((Vector3)(ref localScale)).Equals(other.localScale))
		{
			return limbMask == other.limbMask;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is ItemDisplayRule other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return ((((((((((((int)ruleType * 397) ^ (((Object)(object)followerPrefab != (Object)null) ? ((object)followerPrefab).GetHashCode() : 0)) * 397) ^ ((childName != null) ? childName.GetHashCode() : 0)) * 397) ^ ((object)(Vector3)(ref localPos)).GetHashCode()) * 397) ^ ((object)(Vector3)(ref localAngles)).GetHashCode()) * 397) ^ ((object)(Vector3)(ref localScale)).GetHashCode()) * 397) ^ (int)limbMask;
	}
}
