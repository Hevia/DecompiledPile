using RoR2;
using UnityEngine;

namespace EntityStates.MinorConstruct;

public class FindSurface : NoCastSpawn
{
	[SerializeField]
	public int raycastCount;

	[SerializeField]
	public float maxRaycastLength;

	public override void OnEnter()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		RaycastHit val = default(RaycastHit);
		Vector3 corePosition = base.characterBody.corePosition;
		if (base.isAuthority && !Object.op_Implicit((Object)(object)base.characterBody.master.minionOwnership?.ownerMaster))
		{
			for (int i = 0; i < raycastCount; i++)
			{
				if (Physics.Raycast(corePosition, Random.onUnitSphere, ref val, maxRaycastLength, LayerMask.op_Implicit(LayerIndex.world.mask)))
				{
					base.transform.position = ((RaycastHit)(ref val)).point;
					base.transform.up = ((RaycastHit)(ref val)).normal;
				}
			}
		}
		base.OnEnter();
	}
}
