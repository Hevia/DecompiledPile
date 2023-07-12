using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using HG;
using UnityEngine;
using UnityEngine.Events;

namespace RoR2;

public class MultiBodyTrigger : MonoBehaviour
{
	[Header("Parameters")]
	public bool playerControlledOnly = true;

	[Header("Events")]
	public CharacterBody.CharacterBodyUnityEvent onFirstQualifyingBodyEnter;

	public CharacterBody.CharacterBodyUnityEvent onLastQualifyingBodyExit;

	public CharacterBody.CharacterBodyUnityEvent onAnyQualifyingBodyEnter;

	public CharacterBody.CharacterBodyUnityEvent onAnyQualifyingBodyExit;

	public UnityEvent onAllQualifyingBodiesEnter;

	public UnityEvent onAllQualifyingBodiesExit;

	private readonly Queue<Collider> collisionsQueue = new Queue<Collider>();

	private readonly List<CharacterBody> encounteredBodies = new List<CharacterBody>();

	private bool allCandidatesPreviouslyTriggering;

	private bool cachedEnabled;

	private void OnEnable()
	{
		cachedEnabled = true;
	}

	private void OnDisable()
	{
		cachedEnabled = false;
		collisionsQueue.Clear();
		List<CharacterBody> list = CollectionPool<CharacterBody, List<CharacterBody>>.RentCollection();
		SetEncounteredBodies(list, 0);
		list = CollectionPool<CharacterBody, List<CharacterBody>>.ReturnCollection(list);
	}

	private void OnTriggerStay(Collider collider)
	{
		if (cachedEnabled)
		{
			collisionsQueue.Enqueue(collider);
		}
	}

	private void FixedUpdate()
	{
		if (collisionsQueue.Count == 0 && encounteredBodies.Count == 0)
		{
			return;
		}
		List<CharacterBody> list = CollectionPool<CharacterBody, List<CharacterBody>>.RentCollection();
		while (collisionsQueue.Count > 0)
		{
			Collider val = collisionsQueue.Dequeue();
			if (Object.op_Implicit((Object)(object)val))
			{
				CharacterBody component = ((Component)val).GetComponent<CharacterBody>();
				if (Object.op_Implicit((Object)(object)component) && (!playerControlledOnly || component.isPlayerControlled) && ListUtils.FirstOccurrenceByReference<List<CharacterBody>, CharacterBody>(list, ref component) == -1)
				{
					list.Add(component);
				}
			}
		}
		SetEncounteredBodies(list, playerControlledOnly ? (Run.instance?.livingPlayerCount ?? 0) : 0);
		list = CollectionPool<CharacterBody, List<CharacterBody>>.ReturnCollection(list);
	}

	private void SetEncounteredBodies(List<CharacterBody> newEncounteredBodies, int candidateCount)
	{
		List<CharacterBody> list = CollectionPool<CharacterBody, List<CharacterBody>>.RentCollection();
		List<CharacterBody> list2 = CollectionPool<CharacterBody, List<CharacterBody>>.RentCollection();
		ListUtils.FindExclusiveEntriesByReference<CharacterBody>(encounteredBodies, newEncounteredBodies, list2, list);
		bool flag = encounteredBodies.Count == 0;
		bool flag2 = newEncounteredBodies.Count == 0;
		ListUtils.CloneTo<CharacterBody>(newEncounteredBodies, encounteredBodies);
		foreach (CharacterBody item in list2)
		{
			try
			{
				((UnityEvent<CharacterBody>)onAnyQualifyingBodyExit)?.Invoke(item);
			}
			catch (Exception ex)
			{
				Debug.LogError((object)ex);
			}
		}
		if (flag != flag2)
		{
			if (flag2)
			{
				try
				{
					((UnityEvent<CharacterBody>)onLastQualifyingBodyExit)?.Invoke(list2[list2.Count - 1]);
				}
				catch (Exception ex2)
				{
					Debug.LogError((object)ex2);
				}
			}
			else
			{
				try
				{
					((UnityEvent<CharacterBody>)onFirstQualifyingBodyEnter)?.Invoke(list[0]);
				}
				catch (Exception ex3)
				{
					Debug.LogError((object)ex3);
				}
			}
		}
		foreach (CharacterBody item2 in list)
		{
			try
			{
				((UnityEvent<CharacterBody>)onAnyQualifyingBodyEnter)?.Invoke(item2);
			}
			catch (Exception ex4)
			{
				Debug.LogError((object)ex4);
			}
		}
		list2 = CollectionPool<CharacterBody, List<CharacterBody>>.ReturnCollection(list2);
		list = CollectionPool<CharacterBody, List<CharacterBody>>.ReturnCollection(list);
		bool flag3 = encounteredBodies.Count >= candidateCount && candidateCount > 0;
		if (allCandidatesPreviouslyTriggering == flag3)
		{
			return;
		}
		try
		{
			if (flag3)
			{
				UnityEvent obj = onAllQualifyingBodiesEnter;
				if (obj != null)
				{
					obj.Invoke();
				}
			}
			else
			{
				UnityEvent obj2 = onAllQualifyingBodiesExit;
				if (obj2 != null)
				{
					obj2.Invoke();
				}
			}
		}
		catch (Exception ex5)
		{
			Debug.LogError((object)ex5);
		}
		allCandidatesPreviouslyTriggering = flag3;
	}

	public void KillAllOutsideWithVoidDeath()
	{
		List<CharacterBody> list = CollectionPool<CharacterBody, List<CharacterBody>>.RentCollection();
		ListUtils.AddRange<CharacterBody, ReadOnlyCollection<CharacterBody>>(list, CharacterBody.readOnlyInstancesList);
		foreach (CharacterBody item in list)
		{
			if (encounteredBodies.Contains(item))
			{
				continue;
			}
			CharacterMaster master = item.master;
			if (Object.op_Implicit((Object)(object)master))
			{
				try
				{
					master.TrueKill(BodyCatalog.FindBodyPrefab("BrotherBody"), null, DamageType.VoidDeath);
				}
				catch (Exception ex)
				{
					Debug.LogError((object)ex);
				}
			}
		}
		list = CollectionPool<CharacterBody, List<CharacterBody>>.ReturnCollection(list);
	}
}
