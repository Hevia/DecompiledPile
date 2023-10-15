using RoR2;
using UnityEngine;

namespace EntityStates.VoidRaidCrab.Weapon;

public abstract class BaseMultiBeamState : BaseState
{
	public static float beamMaxDistance = 1000f;

	public static string muzzleName;

	protected Transform muzzleTransform { get; private set; }

	public override void OnEnter()
	{
		base.OnEnter();
		muzzleTransform = FindModelChild(muzzleName);
	}

	protected void CalcBeamPath(out Ray beamRay, out Vector3 beamEndPos)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		Ray aimRay = GetAimRay();
		float num = float.PositiveInfinity;
		RaycastHit[] array = Physics.RaycastAll(aimRay, beamMaxDistance, LayerMask.op_Implicit(LayerIndex.CommonMasks.bullet), (QueryTriggerInteraction)1);
		Transform root = GetModelTransform().root;
		for (int i = 0; i < array.Length; i++)
		{
			ref RaycastHit reference = ref array[i];
			float distance = ((RaycastHit)(ref reference)).distance;
			if (distance < num && ((Component)((RaycastHit)(ref reference)).collider).transform.root != root)
			{
				num = distance;
			}
		}
		num = Mathf.Min(num, beamMaxDistance);
		beamEndPos = aimRay.GetPoint(num);
		Vector3 position = muzzleTransform.position;
		beamRay = new Ray(position, beamEndPos - position);
	}
}
