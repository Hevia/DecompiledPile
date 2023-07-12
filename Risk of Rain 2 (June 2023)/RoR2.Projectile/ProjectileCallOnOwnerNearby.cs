using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace RoR2.Projectile;

[RequireComponent(typeof(ProjectileController))]
public class ProjectileCallOnOwnerNearby : MonoBehaviour
{
	[Flags]
	public enum Filter
	{
		None = 0,
		Server = 1,
		Client = 2
	}

	private enum State
	{
		Outside,
		Inside
	}

	public Filter filter;

	public float radius;

	public UnityEvent onOwnerEnter;

	public UnityEvent onOwnerExit;

	private State state;

	private bool ownerInRadius;

	private ProjectileController projectileController;

	private void Awake()
	{
		projectileController = ((Component)this).GetComponent<ProjectileController>();
		Filter filter = Filter.None;
		if (NetworkServer.active)
		{
			filter |= Filter.Server;
		}
		if (NetworkClient.active)
		{
			filter |= Filter.Client;
		}
		if ((this.filter & filter) == 0)
		{
			((Behaviour)this).enabled = false;
		}
	}

	private void OnDisable()
	{
		SetState(State.Outside);
	}

	private void SetState(State newState)
	{
		if (state == newState)
		{
			return;
		}
		state = newState;
		if (state == State.Inside)
		{
			UnityEvent obj = onOwnerEnter;
			if (obj != null)
			{
				obj.Invoke();
			}
		}
		else
		{
			UnityEvent obj2 = onOwnerExit;
			if (obj2 != null)
			{
				obj2.Invoke();
			}
		}
	}

	private void FixedUpdate()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		State state = State.Outside;
		if (Object.op_Implicit((Object)(object)projectileController.owner))
		{
			float num = radius * radius;
			Vector3 val = ((Component)this).transform.position - projectileController.owner.transform.position;
			if (((Vector3)(ref val)).sqrMagnitude < num)
			{
				state = State.Inside;
			}
		}
		SetState(state);
	}
}
