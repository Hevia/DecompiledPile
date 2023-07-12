using System.Collections.Generic;
using HG;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace RoR2;

[RequireComponent(typeof(Rigidbody))]
public class AllPlayersTrigger : MonoBehaviour
{
	public UnityEvent onTriggerStart;

	public UnityEvent onTriggerEnd;

	private Queue<Collider> collisionQueueServer;

	private bool triggerActiveServer;

	private void Awake()
	{
		if (NetworkServer.active)
		{
			collisionQueueServer = new Queue<Collider>();
			triggerActiveServer = false;
		}
	}

	private void OnEnable()
	{
		if (!NetworkServer.active)
		{
			((Behaviour)this).enabled = false;
		}
	}

	private void OnTriggerStay(Collider other)
	{
		if (((Behaviour)this).enabled)
		{
			collisionQueueServer.Enqueue(other);
		}
	}

	private void FixedUpdate()
	{
		if (!Object.op_Implicit((Object)(object)Run.instance))
		{
			return;
		}
		int num = 0;
		List<CharacterBody> list = CollectionPool<CharacterBody, List<CharacterBody>>.RentCollection();
		while (collisionQueueServer.Count > 0)
		{
			Collider val = collisionQueueServer.Dequeue();
			if (Object.op_Implicit((Object)(object)val))
			{
				CharacterBody component = ((Component)val).GetComponent<CharacterBody>();
				if (Object.op_Implicit((Object)(object)component) && component.isPlayerControlled && !list.Contains(component))
				{
					list.Add(component);
					num++;
				}
			}
		}
		CollectionPool<CharacterBody, List<CharacterBody>>.ReturnCollection(list);
		bool flag = num == Run.instance.livingPlayerCount && num != 0;
		if (triggerActiveServer != flag)
		{
			triggerActiveServer = flag;
			UnityEvent obj = (triggerActiveServer ? onTriggerStart : onTriggerEnd);
			if (obj != null)
			{
				obj.Invoke();
			}
		}
	}
}
