using UnityEngine;

namespace RoR2.Orbs;

public class Orb
{
	public Vector3 origin;

	public HurtBox target;

	public float arrivalTime;

	public Orb nextOrb;

	public float duration { get; protected set; }

	public float timeUntilArrival => arrivalTime - OrbManager.instance.time;

	protected float distanceToTarget
	{
		get
		{
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			if (Object.op_Implicit((Object)(object)target))
			{
				return Vector3.Distance(((Component)target).transform.position, origin);
			}
			return 0f;
		}
	}

	public virtual void Begin()
	{
	}

	public virtual void OnArrival()
	{
	}
}
