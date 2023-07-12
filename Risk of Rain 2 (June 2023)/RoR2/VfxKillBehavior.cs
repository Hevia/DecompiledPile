using System;
using System.Collections.Generic;
using HG;
using UnityEngine;
using UnityEngine.Events;

namespace RoR2;

public class VfxKillBehavior : MonoBehaviour
{
	public UnityEvent killBehavior;

	public static void KillVfxObject(GameObject gameObject)
	{
		if (!Object.op_Implicit((Object)(object)gameObject))
		{
			return;
		}
		List<VfxKillBehavior> list = CollectionPool<VfxKillBehavior, List<VfxKillBehavior>>.RentCollection();
		gameObject.GetComponents<VfxKillBehavior>(list);
		if (list.Count > 0)
		{
			for (int i = 0; i < list.Count; i++)
			{
				try
				{
					list[i].killBehavior.Invoke();
				}
				catch (Exception ex)
				{
					Debug.LogException(ex);
				}
			}
		}
		else
		{
			Object.Destroy((Object)(object)gameObject);
		}
	}
}
