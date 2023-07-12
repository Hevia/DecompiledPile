using System;
using UnityEngine;

namespace RoR2;

public static class HGPhysics
{
	public static readonly Collider[] sharedCollidersBuffer = (Collider[])(object)new Collider[65536];

	private static int _sharedCollidersBufferEntriesCount = 0;

	public static int sharedCollidersBufferEntriesCount
	{
		get
		{
			return _sharedCollidersBufferEntriesCount;
		}
		private set
		{
			int num = sharedCollidersBufferEntriesCount - value;
			if (num > 0)
			{
				Array.Clear(sharedCollidersBuffer, sharedCollidersBufferEntriesCount, num);
			}
			_sharedCollidersBufferEntriesCount = value;
		}
	}

	public static int OverlapBoxNonAllocShared(Vector3 center, Vector3 halfExtents, Quaternion orientation, int layerMask, QueryTriggerInteraction queryTriggerInteraction = 0)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		return sharedCollidersBufferEntriesCount = Physics.OverlapBoxNonAlloc(center, halfExtents, sharedCollidersBuffer, orientation, layerMask, queryTriggerInteraction);
	}

	public static int OverlapSphereNonAllocShared(Vector3 position, float radius, int layerMask, QueryTriggerInteraction queryTriggerInteraction = 0)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		return sharedCollidersBufferEntriesCount = Physics.OverlapSphereNonAlloc(position, radius, sharedCollidersBuffer, layerMask, queryTriggerInteraction);
	}

	public static float CalculateDistance(float initialVelocity, float acceleration, float time)
	{
		return initialVelocity * time + 0.5f * acceleration * time * time;
	}
}
