using RoR2;
using UnityEngine;

namespace EntityStates.LaserTurbine;

public class ChargeMainBeamState : LaserTurbineBaseState
{
	public static float baseDuration;

	public static GameObject beamIndicatorPrefab;

	private GameObject beamIndicatorInstance;

	private ChildLocator beamIndicatorChildLocator;

	private Transform beamIndicatorEndTransform;

	protected override bool shouldFollow => false;

	public override void OnEnter()
	{
		base.OnEnter();
		beamIndicatorInstance = Object.Instantiate<GameObject>(beamIndicatorPrefab, GetMuzzleTransform(), false);
		beamIndicatorChildLocator = beamIndicatorInstance.GetComponent<ChildLocator>();
		if (Object.op_Implicit((Object)(object)beamIndicatorChildLocator))
		{
			beamIndicatorEndTransform = beamIndicatorChildLocator.FindChild("End");
		}
	}

	public override void OnExit()
	{
		EntityState.Destroy((Object)(object)beamIndicatorInstance);
		base.OnExit();
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.isAuthority && base.fixedAge >= baseDuration)
		{
			outer.SetNextState(new FireMainBeamState());
		}
	}

	public override void Update()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		base.Update();
		if (Object.op_Implicit((Object)(object)beamIndicatorInstance) && Object.op_Implicit((Object)(object)beamIndicatorEndTransform))
		{
			float num = 1000f;
			Ray aimRay = GetAimRay();
			_ = beamIndicatorInstance.transform.parent.position;
			Vector3 point = aimRay.GetPoint(num);
			if (Util.CharacterRaycast(((Component)base.ownerBody).gameObject, aimRay, out var hitInfo, num, LayerMask.op_Implicit(LayerMask.op_Implicit(LayerIndex.entityPrecise.mask) | LayerMask.op_Implicit(LayerIndex.world.mask)), (QueryTriggerInteraction)0))
			{
				point = ((RaycastHit)(ref hitInfo)).point;
			}
			((Component)beamIndicatorEndTransform).transform.position = point;
		}
	}
}
