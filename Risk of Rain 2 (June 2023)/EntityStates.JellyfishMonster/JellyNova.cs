using RoR2;
using UnityEngine;

namespace EntityStates.JellyfishMonster;

public class JellyNova : BaseState
{
	public static float baseDuration = 3f;

	public static GameObject chargingEffectPrefab;

	public static GameObject novaEffectPrefab;

	public static string chargingSoundString;

	public static string novaSoundString;

	public static float novaDamageCoefficient;

	public static float novaRadius;

	public static float novaForce;

	private bool hasExploded;

	private float duration;

	private float stopwatch;

	private GameObject chargeEffect;

	private PrintController printController;

	private uint soundID;

	public override void OnEnter()
	{
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		stopwatch = 0f;
		duration = baseDuration / attackSpeedStat;
		Transform modelTransform = GetModelTransform();
		PlayCrossfade("Body", "Nova", "Nova.playbackRate", duration, 0.1f);
		soundID = Util.PlaySound(chargingSoundString, base.gameObject);
		if (Object.op_Implicit((Object)(object)chargingEffectPrefab))
		{
			chargeEffect = Object.Instantiate<GameObject>(chargingEffectPrefab, base.transform.position, base.transform.rotation);
			chargeEffect.transform.parent = base.transform;
			chargeEffect.transform.localScale = new Vector3(novaRadius, novaRadius, novaRadius);
			chargeEffect.GetComponent<ScaleParticleSystemDuration>().newDuration = duration;
		}
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			printController = ((Component)modelTransform).GetComponent<PrintController>();
			if (Object.op_Implicit((Object)(object)printController))
			{
				((Behaviour)printController).enabled = true;
				printController.printTime = duration;
			}
		}
	}

	public override void OnExit()
	{
		base.OnExit();
		AkSoundEngine.StopPlayingID(soundID);
		if (Object.op_Implicit((Object)(object)chargeEffect))
		{
			EntityState.Destroy((Object)(object)chargeEffect);
		}
		if (Object.op_Implicit((Object)(object)printController))
		{
			((Behaviour)printController).enabled = false;
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		stopwatch += Time.fixedDeltaTime;
		if (stopwatch >= duration && base.isAuthority && !hasExploded)
		{
			Detonate();
		}
	}

	private void Detonate()
	{
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		hasExploded = true;
		Util.PlaySound(novaSoundString, base.gameObject);
		if (Object.op_Implicit((Object)(object)base.modelLocator))
		{
			if (Object.op_Implicit((Object)(object)base.modelLocator.modelBaseTransform))
			{
				EntityState.Destroy((Object)(object)((Component)base.modelLocator.modelBaseTransform).gameObject);
			}
			if (Object.op_Implicit((Object)(object)base.modelLocator.modelTransform))
			{
				EntityState.Destroy((Object)(object)((Component)base.modelLocator.modelTransform).gameObject);
			}
		}
		if (Object.op_Implicit((Object)(object)chargeEffect))
		{
			EntityState.Destroy((Object)(object)chargeEffect);
		}
		if (Object.op_Implicit((Object)(object)novaEffectPrefab))
		{
			EffectManager.SpawnEffect(novaEffectPrefab, new EffectData
			{
				origin = base.transform.position,
				scale = novaRadius
			}, transmit: true);
		}
		BlastAttack blastAttack = new BlastAttack();
		blastAttack.attacker = base.gameObject;
		blastAttack.inflictor = base.gameObject;
		blastAttack.teamIndex = TeamComponent.GetObjectTeam(base.gameObject);
		blastAttack.baseDamage = damageStat * novaDamageCoefficient;
		blastAttack.baseForce = novaForce;
		blastAttack.position = base.transform.position;
		blastAttack.radius = novaRadius;
		blastAttack.procCoefficient = 2f;
		blastAttack.attackerFiltering = AttackerFiltering.NeverHitSelf;
		blastAttack.Fire();
		if (Object.op_Implicit((Object)(object)base.healthComponent))
		{
			base.healthComponent.Suicide();
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Pain;
	}
}
