using EntityStates;
using EntityStates.VagrantMonster;
using RoR2;
using UnityEngine;

namespace RandomSurvivors;

public class RFireMegaNova : BaseState
{
	public static float baseDuration = 1f;

	public static GameObject novaEffectPrefab = FireMegaNova.novaEffectPrefab;

	public static GameObject novaImpactEffectPrefab;

	public static string novaSoundString = FireMegaNova.novaSoundString;

	public static float novaDamageCoefficient = FireMegaNova.novaDamageCoefficient;

	public static float novaForce = FireMegaNova.novaForce;

	public float novaRadius;

	private float duration;

	private float stopwatch;

	public override void OnEnter()
	{
		((BaseState)this).OnEnter();
		stopwatch = 0f;
		duration = baseDuration / base.attackSpeedStat;
		if (((EntityState)this).GetComponent<RandomManager>().myAnim != null)
		{
			string myAnim = ((EntityState)this).GetComponent<RandomManager>().myAnim;
			((EntityState)this).PlayAnimation("Gesture, Additive", myAnim, myAnim + ".playbackRate", duration / base.attackSpeedStat);
			((EntityState)this).PlayAnimation("Gesture, Additive", myAnim, myAnim + ".playbackRate", duration / base.attackSpeedStat);
		}
		((EntityState)this).characterBody.AddTimedBuff(Buffs.Cripple, 5f);
		Detonate();
	}

	public override void OnExit()
	{
		((EntityState)this).OnExit();
	}

	public override void FixedUpdate()
	{
		((EntityState)this).FixedUpdate();
		stopwatch += Time.fixedDeltaTime;
		if (stopwatch >= duration && ((EntityState)this).isAuthority)
		{
			((EntityState)this).outer.SetNextStateToMain();
		}
	}

	private void Detonate()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = ((EntityState)this).transform.position;
		Util.PlaySound(novaSoundString, ((EntityState)this).gameObject);
		if (Object.op_Implicit((Object)(object)novaEffectPrefab))
		{
			EffectManager.SimpleEffect(novaEffectPrefab, ((EntityState)this).transform.position, ((EntityState)this).transform.rotation, true);
		}
		Transform modelTransform = ((EntityState)this).GetModelTransform();
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			TemporaryOverlay val = ((Component)modelTransform).gameObject.AddComponent<TemporaryOverlay>();
			val.duration = 3f;
			val.animateShaderAlpha = true;
			val.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
			val.destroyComponentOnEnd = true;
			val.originalMaterial = Resources.Load<Material>("Materials/matVagrantEnergized");
			val.AddToCharacerModel(((Component)modelTransform).GetComponent<CharacterModel>());
		}
		if (((EntityState)this).isAuthority)
		{
			new BlastAttack
			{
				attacker = ((EntityState)this).gameObject,
				baseDamage = base.damageStat * FireMegaNova.novaDamageCoefficient,
				baseForce = FireMegaNova.novaForce,
				bonusForce = Vector3.zero,
				crit = ((EntityState)this).characterBody.RollCrit(),
				damageColorIndex = (DamageColorIndex)0,
				damageType = (DamageType)0,
				falloffModel = (FalloffModel)0,
				inflictor = ((EntityState)this).gameObject,
				position = position,
				procChainMask = default(ProcChainMask),
				procCoefficient = 3f,
				radius = novaRadius,
				losType = (LoSType)1,
				teamIndex = ((EntityState)this).teamComponent.teamIndex,
				impactEffect = EffectCatalog.FindEffectIndexFromPrefab(FireMegaNova.novaImpactEffectPrefab)
			}.Fire();
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		return (InterruptPriority)4;
	}
}
