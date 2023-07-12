using System.Linq;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.ClayGrenadier;

public class FaceSlam : BaseState
{
	public static float baseDuration = 3.5f;

	public static float baseDurationBeforeBlast = 1.5f;

	public static string animationLayerName = "Body";

	public static string animationStateName = "FaceSlam";

	public static string playbackRateParam = "FaceSlam.playbackRate";

	public static GameObject chargeEffectPrefab;

	public static string chargeEffectMuzzleString;

	public static GameObject blastImpactEffect;

	public static float blastDamageCoefficient = 4f;

	public static float blastForceMagnitude = 16f;

	public static float blastUpwardForce;

	public static float blastRadius = 3f;

	public static string attackSoundString;

	public static string blastMuzzleString;

	public static GameObject projectilePrefab;

	public static float projectileDamageCoefficient;

	public static float projectileForce;

	public static float projectileSnapOnAngle;

	public static float healthCostFraction;

	private BlastAttack attack;

	private Animator modelAnimator;

	private Transform modelTransform;

	private bool hasFiredBlast;

	private float duration;

	private float durationBeforeBlast;

	private GameObject chargeInstance;

	public override void OnEnter()
	{
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		modelAnimator = GetModelAnimator();
		modelTransform = GetModelTransform();
		duration = baseDuration / attackSpeedStat;
		durationBeforeBlast = baseDurationBeforeBlast / attackSpeedStat;
		Util.PlayAttackSpeedSound(attackSoundString, base.gameObject, attackSpeedStat);
		PlayAnimation(animationLayerName, animationStateName, playbackRateParam, duration);
		if (Object.op_Implicit((Object)(object)base.characterDirection))
		{
			base.characterDirection.moveVector = base.characterDirection.forward;
		}
		Transform val = FindModelChild(chargeEffectMuzzleString);
		if (Object.op_Implicit((Object)(object)val) && Object.op_Implicit((Object)(object)chargeEffectPrefab))
		{
			chargeInstance = Object.Instantiate<GameObject>(chargeEffectPrefab, val.position, val.rotation);
			chargeInstance.transform.parent = val;
			ScaleParticleSystemDuration component = chargeInstance.GetComponent<ScaleParticleSystemDuration>();
			if (Object.op_Implicit((Object)(object)component))
			{
				component.newDuration = durationBeforeBlast;
			}
		}
	}

	public override void OnExit()
	{
		if (Object.op_Implicit((Object)(object)chargeInstance))
		{
			EntityState.Destroy((Object)(object)chargeInstance);
		}
		base.OnExit();
	}

	public override void FixedUpdate()
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_021e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		//IL_0236: Unknown result type (might be due to invalid IL or missing references)
		//IL_0292: Unknown result type (might be due to invalid IL or missing references)
		//IL_0297: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0304: Unknown result type (might be due to invalid IL or missing references)
		//IL_0305: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		if (base.fixedAge > durationBeforeBlast && !hasFiredBlast)
		{
			hasFiredBlast = true;
			if (Object.op_Implicit((Object)(object)chargeInstance))
			{
				EntityState.Destroy((Object)(object)chargeInstance);
			}
			Vector3 footPosition = base.characterBody.footPosition;
			EffectManager.SpawnEffect(blastImpactEffect, new EffectData
			{
				origin = footPosition,
				scale = blastRadius
			}, transmit: true);
			if (NetworkServer.active && Object.op_Implicit((Object)(object)base.healthComponent))
			{
				DamageInfo damageInfo = new DamageInfo();
				damageInfo.damage = base.healthComponent.combinedHealth * healthCostFraction;
				damageInfo.position = base.characterBody.corePosition;
				damageInfo.force = Vector3.zero;
				damageInfo.damageColorIndex = DamageColorIndex.Default;
				damageInfo.crit = false;
				damageInfo.attacker = null;
				damageInfo.inflictor = null;
				damageInfo.damageType = DamageType.NonLethal | DamageType.BypassArmor;
				damageInfo.procCoefficient = 0f;
				damageInfo.procChainMask = default(ProcChainMask);
				base.healthComponent.TakeDamage(damageInfo);
			}
			if (base.isAuthority)
			{
				if (Object.op_Implicit((Object)(object)modelTransform))
				{
					Transform val = FindModelChild(blastMuzzleString);
					if (Object.op_Implicit((Object)(object)val))
					{
						attack = new BlastAttack();
						attack.attacker = base.gameObject;
						attack.inflictor = base.gameObject;
						attack.teamIndex = TeamComponent.GetObjectTeam(base.gameObject);
						attack.baseDamage = damageStat * blastDamageCoefficient;
						attack.baseForce = blastForceMagnitude;
						attack.position = val.position;
						attack.radius = blastRadius;
						attack.bonusForce = new Vector3(0f, blastUpwardForce, 0f);
						attack.damageType = DamageType.ClayGoo;
						attack.Fire();
					}
				}
				Vector3 position = footPosition;
				_ = Vector3.up;
				RaycastHit val2 = default(RaycastHit);
				if (Physics.Raycast(GetAimRay(), ref val2, 1000f, LayerMask.op_Implicit(LayerIndex.world.mask)))
				{
					position = ((RaycastHit)(ref val2)).point;
				}
				BullseyeSearch bullseyeSearch = new BullseyeSearch();
				bullseyeSearch.viewer = base.characterBody;
				bullseyeSearch.teamMaskFilter = TeamMask.allButNeutral;
				bullseyeSearch.teamMaskFilter.RemoveTeam(base.characterBody.teamComponent.teamIndex);
				bullseyeSearch.sortMode = BullseyeSearch.SortMode.DistanceAndAngle;
				bullseyeSearch.minDistanceFilter = 0f;
				bullseyeSearch.maxDistanceFilter = 1000f;
				bullseyeSearch.searchOrigin = base.inputBank.aimOrigin;
				bullseyeSearch.searchDirection = base.inputBank.aimDirection;
				bullseyeSearch.maxAngleFilter = projectileSnapOnAngle;
				bullseyeSearch.filterByLoS = false;
				bullseyeSearch.RefreshCandidates();
				HurtBox hurtBox = bullseyeSearch.GetResults().FirstOrDefault();
				if (Object.op_Implicit((Object)(object)hurtBox) && Object.op_Implicit((Object)(object)hurtBox.healthComponent))
				{
					position = hurtBox.healthComponent.body.footPosition;
				}
				ProjectileManager.instance.FireProjectile(projectilePrefab, position, Quaternion.identity, base.gameObject, base.characterBody.damage * projectileDamageCoefficient, projectileForce, Util.CheckRoll(base.characterBody.crit, base.characterBody.master));
			}
		}
		if (base.fixedAge >= duration && base.isAuthority)
		{
			outer.SetNextStateToMain();
		}
	}
}
