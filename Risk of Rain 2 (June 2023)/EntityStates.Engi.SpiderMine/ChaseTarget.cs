using UnityEngine;

namespace EntityStates.Engi.SpiderMine;

public class ChaseTarget : BaseSpiderMineState
{
	private class OrientationHelper : MonoBehaviour
	{
		private Rigidbody rigidbody;

		private void Awake()
		{
			rigidbody = ((Component)this).GetComponent<Rigidbody>();
		}

		private void OnCollisionStay(Collision collision)
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			int contactCount = collision.contactCount;
			if (contactCount == 0)
			{
				return;
			}
			ContactPoint contact = collision.GetContact(0);
			Vector3 val = ((ContactPoint)(ref contact)).normal;
			for (int i = 1; i < contactCount; i++)
			{
				contact = collision.GetContact(i);
				Vector3 normal = ((ContactPoint)(ref contact)).normal;
				if (val.y < normal.y)
				{
					val = normal;
				}
			}
			rigidbody.MoveRotation(Quaternion.LookRotation(val));
		}
	}

	public static float speed;

	public static float triggerRadius;

	private bool passedDetonationRadius;

	private float bestDistance;

	private OrientationHelper orientationHelper;

	private Transform target => base.projectileTargetComponent.target;

	protected override bool shouldStick => false;

	public override void OnEnter()
	{
		base.OnEnter();
		passedDetonationRadius = false;
		bestDistance = float.PositiveInfinity;
		PlayAnimation("Base", "Chase");
		if (base.isAuthority)
		{
			orientationHelper = base.gameObject.AddComponent<OrientationHelper>();
		}
	}

	public override void FixedUpdate()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		if (!base.isAuthority)
		{
			return;
		}
		if (!Object.op_Implicit((Object)(object)target))
		{
			base.rigidbody.AddForce(Vector3.up, (ForceMode)2);
			outer.SetNextState(new WaitForStick());
			return;
		}
		Vector3 position = target.position;
		Vector3 position2 = base.transform.position;
		Vector3 val = position - position2;
		float magnitude = ((Vector3)(ref val)).magnitude;
		float y = base.rigidbody.velocity.y;
		Vector3 velocity = val * (speed / magnitude);
		velocity.y = y;
		base.rigidbody.velocity = velocity;
		if (!passedDetonationRadius && magnitude <= triggerRadius)
		{
			passedDetonationRadius = true;
		}
		if (magnitude < bestDistance)
		{
			bestDistance = magnitude;
		}
		else if (passedDetonationRadius)
		{
			outer.SetNextState(new PreDetonate());
		}
	}

	public override void OnExit()
	{
		((Component)FindModelChild(childLocatorStringToEnable)).gameObject.SetActive(false);
		if (orientationHelper != null)
		{
			EntityState.Destroy((Object)(object)orientationHelper);
			orientationHelper = null;
		}
		base.OnExit();
	}
}
