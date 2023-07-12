using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Commando.CommandoWeapon;

public class CastSmokescreen : BaseState
{
	public static float baseDuration;

	public static float stealthDuration = 3f;

	public static string jumpSoundString;

	public static string startCloakSoundString;

	public static string stopCloakSoundString;

	public static GameObject initialEffectPrefab;

	public static GameObject smokescreenEffectPrefab;

	public static float damageCoefficient = 1.3f;

	public static float radius = 4f;

	public static float forceMagnitude = 100f;

	private float duration;

	private float totalDuration;

	private bool hasCastSmoke;

	private Animator animator;

	private void CastSmoke()
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		if (!hasCastSmoke)
		{
			Util.PlaySound(startCloakSoundString, base.gameObject);
		}
		else
		{
			Util.PlaySound(stopCloakSoundString, base.gameObject);
		}
		EffectManager.SpawnEffect(smokescreenEffectPrefab, new EffectData
		{
			origin = base.transform.position
		}, transmit: false);
		int layerIndex = animator.GetLayerIndex("Impact");
		if (layerIndex >= 0)
		{
			animator.SetLayerWeight(layerIndex, 2f);
			animator.PlayInFixedTime("LightImpact", layerIndex, 0f);
		}
		if (NetworkServer.active)
		{
			BlastAttack blastAttack = new BlastAttack();
			blastAttack.attacker = base.gameObject;
			blastAttack.inflictor = base.gameObject;
			blastAttack.teamIndex = TeamComponent.GetObjectTeam(base.gameObject);
			blastAttack.baseDamage = damageStat * damageCoefficient;
			blastAttack.baseForce = forceMagnitude;
			blastAttack.position = base.transform.position;
			blastAttack.radius = radius;
			blastAttack.falloffModel = BlastAttack.FalloffModel.None;
			blastAttack.damageType = DamageType.Stun1s;
			blastAttack.attackerFiltering = AttackerFiltering.NeverHitSelf;
			blastAttack.Fire();
		}
	}

	public override void OnEnter()
	{
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		totalDuration = stealthDuration + totalDuration;
		PlayCrossfade("Gesture, Smokescreen", "CastSmokescreen", "CastSmokescreen.playbackRate", duration, 0.2f);
		animator = GetModelAnimator();
		Util.PlaySound(jumpSoundString, base.gameObject);
		EffectManager.SpawnEffect(initialEffectPrefab, new EffectData
		{
			origin = base.transform.position
		}, transmit: true);
		if (Object.op_Implicit((Object)(object)base.characterBody) && NetworkServer.active)
		{
			base.characterBody.AddBuff(RoR2Content.Buffs.CloakSpeed.buffIndex);
		}
	}

	public override void OnExit()
	{
		if (Object.op_Implicit((Object)(object)base.characterBody) && NetworkServer.active)
		{
			if (base.characterBody.HasBuff(RoR2Content.Buffs.Cloak))
			{
				base.characterBody.RemoveBuff(RoR2Content.Buffs.Cloak);
			}
			if (base.characterBody.HasBuff(RoR2Content.Buffs.CloakSpeed))
			{
				base.characterBody.RemoveBuff(RoR2Content.Buffs.CloakSpeed);
			}
		}
		if (!outer.destroying)
		{
			CastSmoke();
		}
		base.OnExit();
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.fixedAge >= duration && !hasCastSmoke)
		{
			CastSmoke();
			if (Object.op_Implicit((Object)(object)base.characterBody) && NetworkServer.active)
			{
				base.characterBody.AddBuff(RoR2Content.Buffs.Cloak.buffIndex);
			}
			hasCastSmoke = true;
		}
		if (base.fixedAge >= totalDuration && base.isAuthority)
		{
			outer.SetNextStateToMain();
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		if (!hasCastSmoke)
		{
			return InterruptPriority.PrioritySkill;
		}
		return InterruptPriority.Any;
	}
}
