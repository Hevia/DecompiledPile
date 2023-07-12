using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using HG;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public class OutsideInteractableLocker : MonoBehaviour
{
	private struct Candidate
	{
		public PurchaseInteraction purchaseInteraction;

		public float distanceSqr;
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	private struct CandidateDistanceCompararer : IComparer<Candidate>
	{
		public int Compare(Candidate a, Candidate b)
		{
			return a.distanceSqr.CompareTo(b.distanceSqr);
		}
	}

	[Tooltip("The networked object which will be instantiated to lock purchasables.")]
	public GameObject lockPrefab;

	[Tooltip("How long to wait between steps.")]
	public float updateInterval = 0.1f;

	[Tooltip("Whether or not to invert the requirements.")]
	public bool lockInside;

	private Dictionary<PurchaseInteraction, GameObject> lockObjectMap;

	private float updateTimer;

	private IEnumerator currentCoroutine;

	public float radius { get; set; }

	private void Awake()
	{
		if (NetworkServer.active)
		{
			lockObjectMap = new Dictionary<PurchaseInteraction, GameObject>();
		}
		else
		{
			((Behaviour)this).enabled = false;
		}
	}

	private void OnEnable()
	{
		if (NetworkServer.active)
		{
			currentCoroutine = ChestLockCoroutine();
			updateTimer = updateInterval;
		}
	}

	private void OnDisable()
	{
		if (NetworkServer.active)
		{
			currentCoroutine = null;
			UnlockAll();
		}
	}

	private void FixedUpdate()
	{
		updateTimer -= Time.fixedDeltaTime;
		if (updateTimer <= 0f)
		{
			updateTimer = updateInterval;
			currentCoroutine?.MoveNext();
		}
	}

	private int CompareCandidatesByDistance(Candidate a, Candidate b)
	{
		return a.distanceSqr.CompareTo(b.distanceSqr);
	}

	private void LockPurchasable(PurchaseInteraction purchaseInteraction)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)purchaseInteraction.lockGameObject))
		{
			GameObject val = Object.Instantiate<GameObject>(lockPrefab, ((Component)purchaseInteraction).transform.position, Quaternion.identity);
			NetworkServer.Spawn(val);
			purchaseInteraction.NetworklockGameObject = val;
			lockObjectMap.Add(purchaseInteraction, val);
		}
	}

	private void UnlockPurchasable(PurchaseInteraction purchaseInteraction)
	{
		if (lockObjectMap.TryGetValue(purchaseInteraction, out var value) && !((Object)(object)value != (Object)(object)purchaseInteraction.lockGameObject))
		{
			Object.Destroy((Object)(object)value);
			lockObjectMap.Remove(purchaseInteraction);
		}
	}

	private void UnlockAll()
	{
		foreach (GameObject value in lockObjectMap.Values)
		{
			Object.Destroy((Object)(object)value);
		}
		lockObjectMap.Clear();
	}

	private IEnumerator ChestLockCoroutine()
	{
		Candidate[] candidates = new Candidate[64];
		int candidatesCount = 0;
		while (true)
		{
			Vector3 position = ((Component)this).transform.position;
			List<PurchaseInteraction> instancesList = InstanceTracker.GetInstancesList<PurchaseInteraction>();
			int num = candidatesCount;
			candidatesCount = instancesList.Count;
			ArrayUtils.EnsureCapacity<Candidate>(ref candidates, candidatesCount);
			for (int j = num; j < candidatesCount; j++)
			{
				candidates[j] = default(Candidate);
			}
			for (int k = 0; k < candidatesCount; k++)
			{
				PurchaseInteraction purchaseInteraction = instancesList[k];
				Candidate[] array = candidates;
				int num2 = k;
				Candidate candidate = new Candidate
				{
					purchaseInteraction = purchaseInteraction
				};
				Vector3 val = ((Component)purchaseInteraction).transform.position - position;
				candidate.distanceSqr = ((Vector3)(ref val)).sqrMagnitude;
				array[num2] = candidate;
			}
			yield return null;
			Array.Sort(candidates, 0, candidatesCount, default(CandidateDistanceCompararer));
			yield return null;
			int i = 0;
			while (i < candidatesCount)
			{
				PurchaseInteraction purchaseInteraction2 = candidates[i].purchaseInteraction;
				if (Object.op_Implicit((Object)(object)purchaseInteraction2))
				{
					float num3 = radius * radius;
					if (candidates[i].distanceSqr <= num3 != lockInside || !purchaseInteraction2.available)
					{
						UnlockPurchasable(purchaseInteraction2);
					}
					else
					{
						LockPurchasable(purchaseInteraction2);
					}
					yield return null;
				}
				int num4 = i + 1;
				i = num4;
			}
			yield return null;
		}
	}
}
