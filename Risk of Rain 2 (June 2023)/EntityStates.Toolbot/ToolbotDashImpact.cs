using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Toolbot;

public class ToolbotDashImpact : BaseState
{
	public HealthComponent victimHealthComponent;

	public Vector3 idealDirection;

	public float damageBoostFromSpeed;

	public bool isCrit;

	public override void OnEnter()
	{
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		if (NetworkServer.active)
		{
			if (Object.op_Implicit((Object)(object)victimHealthComponent))
			{
				DamageInfo damageInfo = new DamageInfo
				{
					attacker = base.gameObject,
					damage = damageStat * ToolbotDash.knockbackDamageCoefficient * damageBoostFromSpeed,
					crit = isCrit,
					procCoefficient = 1f,
					damageColorIndex = DamageColorIndex.Item,
					damageType = DamageType.Stun1s,
					position = base.characterBody.corePosition
				};
				victimHealthComponent.TakeDamage(damageInfo);
				GlobalEventManager.instance.OnHitEnemy(damageInfo, ((Component)victimHealthComponent).gameObject);
				GlobalEventManager.instance.OnHitAll(damageInfo, ((Component)victimHealthComponent).gameObject);
			}
			base.healthComponent.TakeDamageForce(idealDirection * (0f - ToolbotDash.knockbackForce), alwaysApply: true);
		}
		if (base.isAuthority)
		{
			AddRecoil(-0.5f * ToolbotDash.recoilAmplitude * 3f, -0.5f * ToolbotDash.recoilAmplitude * 3f, -0.5f * ToolbotDash.recoilAmplitude * 8f, 0.5f * ToolbotDash.recoilAmplitude * 3f);
			EffectManager.SimpleImpactEffect(ToolbotDash.knockbackEffectPrefab, base.characterBody.corePosition, base.characterDirection.forward, transmit: true);
			outer.SetNextStateToMain();
		}
	}

	public override void OnSerialize(NetworkWriter writer)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		base.OnSerialize(writer);
		writer.Write(Object.op_Implicit((Object)(object)victimHealthComponent) ? ((Component)victimHealthComponent).gameObject : null);
		writer.Write(idealDirection);
		writer.Write(damageBoostFromSpeed);
		writer.Write(isCrit);
	}

	public override void OnDeserialize(NetworkReader reader)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		base.OnDeserialize(reader);
		GameObject val = reader.ReadGameObject();
		victimHealthComponent = (Object.op_Implicit((Object)(object)val) ? val.GetComponent<HealthComponent>() : null);
		idealDirection = reader.ReadVector3();
		damageBoostFromSpeed = reader.ReadSingle();
		isCrit = reader.ReadBoolean();
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Frozen;
	}
}
