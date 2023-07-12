using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.ImpBossMonster;

public class GroundPound : BaseState
{
	private float stopwatch;

	public static float baseDuration = 3.5f;

	public static float damageCoefficient = 4f;

	public static float forceMagnitude = 16f;

	public static float blastAttackRadius;

	private BlastAttack attack;

	public static string initialAttackSoundString;

	public static GameObject chargeEffectPrefab;

	public static GameObject slamEffectPrefab;

	public static GameObject hitEffectPrefab;

	public static GameObject swipeEffectPrefab;

	private Animator modelAnimator;

	private Transform modelTransform;

	private bool hasAttacked;

	private float duration;

	private ChildLocator childLocator;

	private int attackCount;

	public override void OnEnter()
	{
		base.OnEnter();
		modelAnimator = GetModelAnimator();
		modelTransform = GetModelTransform();
		childLocator = ((Component)modelTransform).GetComponent<ChildLocator>();
		Util.PlaySound(initialAttackSoundString, base.gameObject);
		attack = new BlastAttack();
		attack.attacker = base.gameObject;
		attack.inflictor = base.gameObject;
		attack.teamIndex = TeamComponent.GetObjectTeam(base.gameObject);
		attack.baseDamage = damageStat * damageCoefficient;
		attack.baseForce = forceMagnitude;
		attack.radius = blastAttackRadius;
		attack.falloffModel = BlastAttack.FalloffModel.SweetSpot;
		attack.attackerFiltering = AttackerFiltering.NeverHitSelf;
		duration = baseDuration / attackSpeedStat;
		PlayCrossfade("Fullbody Override", "GroundPound", "GroundPound.playbackRate", duration, 0.2f);
		if (Object.op_Implicit((Object)(object)base.characterBody))
		{
			base.characterBody.SetAimTimer(duration + 3f);
		}
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			childLocator = ((Component)modelTransform).GetComponent<ChildLocator>();
		}
	}

	public override void OnExit()
	{
		base.OnExit();
	}

	public override void FixedUpdate()
	{
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		stopwatch += Time.fixedDeltaTime;
		if (Object.op_Implicit((Object)(object)modelAnimator))
		{
			if (modelAnimator.GetFloat("GroundPound.hitBoxActive") > 0.5f)
			{
				if (!hasAttacked)
				{
					if (NetworkServer.active)
					{
						attack.position = ((Component)childLocator.FindChild("GroundPoundCenter")).transform.position;
						attack.Fire();
					}
					if (base.isAuthority)
					{
						EffectManager.SimpleMuzzleFlash(slamEffectPrefab, base.gameObject, "GroundPoundCenter", transmit: true);
					}
					EffectManager.SimpleMuzzleFlash(swipeEffectPrefab, base.gameObject, (attackCount % 2 == 0) ? "FireVoidspikesL" : "FireVoidspikesR", transmit: true);
					attackCount++;
					hasAttacked = true;
				}
			}
			else
			{
				hasAttacked = false;
			}
		}
		if (stopwatch >= duration && base.isAuthority)
		{
			outer.SetNextStateToMain();
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.PrioritySkill;
	}
}
