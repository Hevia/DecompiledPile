using UnityEngine;

namespace EntityStates.GolemMonster;

public class DeathState : GenericCharacterDeath
{
	public static GameObject initialDeathExplosionPrefab;

	public override void OnEnter()
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		Transform modelTransform = GetModelTransform();
		if (!Object.op_Implicit((Object)(object)modelTransform))
		{
			return;
		}
		ChildLocator component = ((Component)modelTransform).GetComponent<ChildLocator>();
		if (Object.op_Implicit((Object)(object)component))
		{
			Transform val = component.FindChild("Head");
			if (Object.op_Implicit((Object)(object)val) && Object.op_Implicit((Object)(object)initialDeathExplosionPrefab))
			{
				Object.Instantiate<GameObject>(initialDeathExplosionPrefab, val.position, Quaternion.identity).transform.parent = val;
			}
		}
	}
}
