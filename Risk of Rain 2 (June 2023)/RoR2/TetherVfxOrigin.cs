using System.Collections.Generic;
using HG;
using UnityEngine;

namespace RoR2;

public class TetherVfxOrigin : MonoBehaviour
{
	public GameObject tetherPrefab;

	private List<Transform> tetheredTransforms;

	private List<TetherVfx> tetherVfxs;

	protected Transform transform { get; private set; }

	protected void Awake()
	{
		transform = ((Component)this).transform;
		tetheredTransforms = CollectionPool<Transform, List<Transform>>.RentCollection();
		tetherVfxs = CollectionPool<TetherVfx, List<TetherVfx>>.RentCollection();
	}

	protected void OnDestroy()
	{
		for (int num = tetherVfxs.Count - 1; num >= 0; num--)
		{
			RemoveTetherAt(num);
		}
		tetherVfxs = CollectionPool<TetherVfx, List<TetherVfx>>.ReturnCollection(tetherVfxs);
		tetheredTransforms = CollectionPool<Transform, List<Transform>>.ReturnCollection(tetheredTransforms);
	}

	protected void AddTether(Transform target)
	{
		if (Object.op_Implicit((Object)(object)target))
		{
			TetherVfx tetherVfx = null;
			if (Object.op_Implicit((Object)(object)tetherPrefab))
			{
				tetherVfx = Object.Instantiate<GameObject>(tetherPrefab, transform).GetComponent<TetherVfx>();
				tetherVfx.tetherTargetTransform = target;
			}
			tetheredTransforms.Add(target);
			tetherVfxs.Add(tetherVfx);
		}
	}

	protected void RemoveTetherAt(int i)
	{
		TetherVfx tetherVfx = tetherVfxs[i];
		if (Object.op_Implicit((Object)(object)tetherVfx))
		{
			((Component)tetherVfx).transform.SetParent((Transform)null);
			tetherVfx.Terminate();
		}
		tetheredTransforms.RemoveAt(i);
		tetherVfxs.RemoveAt(i);
	}

	public void SetTetheredTransforms(List<Transform> newTetheredTransforms)
	{
		List<Transform> list = CollectionPool<Transform, List<Transform>>.RentCollection();
		List<Transform> list2 = CollectionPool<Transform, List<Transform>>.RentCollection();
		ListUtils.FindExclusiveEntriesByReference<Transform>(tetheredTransforms, newTetheredTransforms, list2, list);
		int i = 0;
		for (int count = list2.Count; i < count; i++)
		{
			List<Transform> list3 = tetheredTransforms;
			Transform val = list2[i];
			RemoveTetherAt(ListUtils.FirstOccurrenceByReference<List<Transform>, Transform>(list3, ref val));
		}
		int j = 0;
		for (int count2 = list.Count; j < count2; j++)
		{
			AddTether(list[j]);
		}
		CollectionPool<Transform, List<Transform>>.ReturnCollection(list2);
		CollectionPool<Transform, List<Transform>>.ReturnCollection(list);
	}
}
