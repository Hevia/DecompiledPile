using EntityStates.SurvivorPod;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.VoidSurvivorPod;

public class Release : SurvivorPodBaseState
{
	[SerializeField]
	public string animationLayerName;

	[SerializeField]
	public string animationStateName;

	[SerializeField]
	public string enterSoundString;

	[SerializeField]
	public float exitForceAmount;

	public override void OnEnter()
	{
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		PlayAnimation(animationLayerName, animationStateName);
		Util.PlaySound(enterSoundString, base.gameObject);
		if (Object.op_Implicit((Object)(object)base.survivorPodController) && NetworkServer.active && Object.op_Implicit((Object)(object)base.vehicleSeat) && Object.op_Implicit((Object)(object)base.vehicleSeat.currentPassengerBody))
		{
			CharacterBody currentPassengerBody = base.vehicleSeat.currentPassengerBody;
			base.vehicleSeat.EjectPassenger(((Component)currentPassengerBody).gameObject);
			HealthComponent component = ((Component)currentPassengerBody).GetComponent<HealthComponent>();
			if (Object.op_Implicit((Object)(object)component))
			{
				DamageInfo damageInfo = new DamageInfo();
				damageInfo.attacker = base.gameObject;
				damageInfo.force = Vector3.up * exitForceAmount;
				damageInfo.position = currentPassengerBody.corePosition;
				component.TakeDamageForce(damageInfo, alwaysApply: true);
			}
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (NetworkServer.active && (!Object.op_Implicit((Object)(object)base.vehicleSeat) || !Object.op_Implicit((Object)(object)base.vehicleSeat.currentPassengerBody)))
		{
			outer.SetNextStateToMain();
		}
	}
}
