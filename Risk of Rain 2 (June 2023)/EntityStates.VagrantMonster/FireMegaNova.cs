using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.VagrantMonster;

public class FireMegaNova : BaseState
{
	public static float baseDuration = 3f;

	public static GameObject novaEffectPrefab;

	public static GameObject novaImpactEffectPrefab;

	public static string novaSoundString;

	public static float novaDamageCoefficient;

	public static float novaForce;

	public float novaRadius;

	private float duration;

	private float stopwatch;

	public override void OnEnter()
	{
		base.OnEnter();
		stopwatch = 0f;
		duration = baseDuration / attackSpeedStat;
		PlayAnimation("Gesture, Override", "FireMegaNova", "FireMegaNova.playbackRate", duration);
		Detonate();
	}

	public override void OnExit()
	{
		base.OnExit();
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		stopwatch += Time.fixedDeltaTime;
		if (stopwatch >= duration && base.isAuthority)
		{
			outer.SetNextStateToMain();
		}
	}

	private void Detonate()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = base.transform.position;
		Util.PlaySound(novaSoundString, base.gameObject);
		if (Object.op_Implicit((Object)(object)novaEffectPrefab))
		{
			EffectManager.SimpleMuzzleFlash(novaEffectPrefab, base.gameObject, "NovaCenter", transmit: false);
		}
		Transform modelTransform = GetModelTransform();
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			TemporaryOverlay temporaryOverlay = ((Component)modelTransform).gameObject.AddComponent<TemporaryOverlay>();
			temporaryOverlay.duration = 3f;
			temporaryOverlay.animateShaderAlpha = true;
			temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
			temporaryOverlay.destroyComponentOnEnd = true;
			temporaryOverlay.originalMaterial = LegacyResourcesAPI.Load<Material>("Materials/matVagrantEnergized");
			temporaryOverlay.AddToCharacerModel(((Component)modelTransform).GetComponent<CharacterModel>());
		}
		if (NetworkServer.active)
		{
			BlastAttack blastAttack = new BlastAttack();
			blastAttack.attacker = base.gameObject;
			blastAttack.baseDamage = damageStat * novaDamageCoefficient;
			blastAttack.baseForce = novaForce;
			blastAttack.bonusForce = Vector3.zero;
			blastAttack.attackerFiltering = AttackerFiltering.NeverHitSelf;
			blastAttack.crit = base.characterBody.RollCrit();
			blastAttack.damageColorIndex = DamageColorIndex.Default;
			blastAttack.damageType = DamageType.Generic;
			blastAttack.falloffModel = BlastAttack.FalloffModel.None;
			blastAttack.inflictor = base.gameObject;
			blastAttack.position = position;
			blastAttack.procChainMask = default(ProcChainMask);
			blastAttack.procCoefficient = 3f;
			blastAttack.radius = novaRadius;
			blastAttack.losType = BlastAttack.LoSType.NearestHit;
			blastAttack.teamIndex = base.teamComponent.teamIndex;
			blastAttack.impactEffect = EffectCatalog.FindEffectIndexFromPrefab(novaImpactEffectPrefab);
			blastAttack.Fire();
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Pain;
	}
}
