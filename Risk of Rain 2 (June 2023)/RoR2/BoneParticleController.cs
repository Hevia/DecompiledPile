using System;
using System.Collections.Generic;
using UnityEngine;

namespace RoR2;

public class BoneParticleController : MonoBehaviour
{
	public GameObject childParticlePrefab;

	public float spawnFrequency;

	public SkinnedMeshRenderer skinnedMeshRenderer;

	private float stopwatch;

	private List<Transform> bonesList;

	private void Start()
	{
		bonesList = new List<Transform>();
		Transform[] bones = skinnedMeshRenderer.bones;
		foreach (Transform val in bones)
		{
			if (((Object)val).name.IndexOf("IK", StringComparison.OrdinalIgnoreCase) == -1 && ((Object)val).name.IndexOf("Root", StringComparison.OrdinalIgnoreCase) == -1 && ((Object)val).name.IndexOf("Base", StringComparison.OrdinalIgnoreCase) == -1)
			{
				Debug.LogFormat("added bone {0}", new object[1] { val });
				bonesList.Add(val);
			}
		}
	}

	private void Update()
	{
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)skinnedMeshRenderer))
		{
			return;
		}
		stopwatch += Time.deltaTime;
		if (stopwatch > 1f / spawnFrequency)
		{
			stopwatch -= 1f / spawnFrequency;
			int count = bonesList.Count;
			Transform val = bonesList[Random.Range(0, count)];
			if (Object.op_Implicit((Object)(object)val))
			{
				Object.Instantiate<GameObject>(childParticlePrefab, ((Component)val).transform.position, Quaternion.identity, val);
			}
		}
	}
}
