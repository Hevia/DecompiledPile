using System.Linq;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Merc;

public class Evis : BaseState
{
	private Transform modelTransform;

	public static GameObject blinkPrefab;

	public static float duration = 2f;

	public static float damageCoefficient;

	public static float damageFrequency;

	public static float procCoefficient;

	public static string beginSoundString;

	public static string endSoundString;

	public static float maxRadius;

	public static GameObject hitEffectPrefab;

	public static string slashSoundString;

	public static string impactSoundString;

	public static string dashSoundString;

	public static float slashPitch;

	public static float smallHopVelocity;

	public static float lingeringInvincibilityDuration;

	private Animator animator;

	private CharacterModel characterModel;

	private float stopwatch;

	private float attackStopwatch;

	private bool crit;

	private static float minimumDuration = 0.5f;

	private CameraTargetParams.AimRequest aimRequest;

	public override void OnEnter()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		CreateBlinkEffect(Util.GetCorePosition(base.gameObject));
		Util.PlayAttackSpeedSound(beginSoundString, base.gameObject, 1.2f);
		crit = Util.CheckRoll(critStat, base.characterBody.master);
		modelTransform = GetModelTransform();
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			animator = ((Component)modelTransform).GetComponent<Animator>();
			characterModel = ((Component)modelTransform).GetComponent<CharacterModel>();
		}
		if (Object.op_Implicit((Object)(object)characterModel))
		{
			characterModel.invisibilityCount++;
		}
		if (Object.op_Implicit((Object)(object)base.cameraTargetParams))
		{
			aimRequest = base.cameraTargetParams.RequestAimType(CameraTargetParams.AimType.Aura);
		}
		if (NetworkServer.active)
		{
			base.characterBody.AddBuff(RoR2Content.Buffs.HiddenInvincibility);
		}
	}

	public override void FixedUpdate()
	{
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_024b: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		stopwatch += Time.fixedDeltaTime;
		attackStopwatch += Time.fixedDeltaTime;
		float num = 1f / damageFrequency / attackSpeedStat;
		if (attackStopwatch >= num)
		{
			attackStopwatch -= num;
			HurtBox hurtBox = SearchForTarget();
			if (Object.op_Implicit((Object)(object)hurtBox))
			{
				Util.PlayAttackSpeedSound(slashSoundString, base.gameObject, slashPitch);
				Util.PlaySound(dashSoundString, base.gameObject);
				Util.PlaySound(impactSoundString, base.gameObject);
				HurtBoxGroup hurtBoxGroup = hurtBox.hurtBoxGroup;
				HurtBox hurtBox2 = hurtBoxGroup.hurtBoxes[Random.Range(0, hurtBoxGroup.hurtBoxes.Length - 1)];
				if (Object.op_Implicit((Object)(object)hurtBox2))
				{
					Vector3 position = ((Component)hurtBox2).transform.position;
					Vector2 insideUnitCircle = Random.insideUnitCircle;
					Vector2 normalized = ((Vector2)(ref insideUnitCircle)).normalized;
					Vector3 normal = default(Vector3);
					((Vector3)(ref normal))._002Ector(normalized.x, 0f, normalized.y);
					EffectManager.SimpleImpactEffect(hitEffectPrefab, position, normal, transmit: false);
					Transform val = ((Component)hurtBox.hurtBoxGroup).transform;
					TemporaryOverlay temporaryOverlay = ((Component)val).gameObject.AddComponent<TemporaryOverlay>();
					temporaryOverlay.duration = num;
					temporaryOverlay.animateShaderAlpha = true;
					temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
					temporaryOverlay.destroyComponentOnEnd = true;
					temporaryOverlay.originalMaterial = LegacyResourcesAPI.Load<Material>("Materials/matMercEvisTarget");
					temporaryOverlay.AddToCharacerModel(((Component)val).GetComponent<CharacterModel>());
					if (NetworkServer.active)
					{
						DamageInfo damageInfo = new DamageInfo();
						damageInfo.damage = damageCoefficient * damageStat;
						damageInfo.attacker = base.gameObject;
						damageInfo.procCoefficient = procCoefficient;
						damageInfo.position = ((Component)hurtBox2).transform.position;
						damageInfo.crit = crit;
						hurtBox2.healthComponent.TakeDamage(damageInfo);
						GlobalEventManager.instance.OnHitEnemy(damageInfo, ((Component)hurtBox2.healthComponent).gameObject);
						GlobalEventManager.instance.OnHitAll(damageInfo, ((Component)hurtBox2.healthComponent).gameObject);
					}
				}
			}
			else if (base.isAuthority && stopwatch > minimumDuration)
			{
				outer.SetNextStateToMain();
			}
		}
		if (Object.op_Implicit((Object)(object)base.characterMotor))
		{
			base.characterMotor.velocity = Vector3.zero;
		}
		if (stopwatch >= duration && base.isAuthority)
		{
			outer.SetNextStateToMain();
		}
	}

	private HurtBox SearchForTarget()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		BullseyeSearch bullseyeSearch = new BullseyeSearch();
		bullseyeSearch.searchOrigin = base.transform.position;
		bullseyeSearch.searchDirection = Random.onUnitSphere;
		bullseyeSearch.maxDistanceFilter = maxRadius;
		bullseyeSearch.teamMaskFilter = TeamMask.GetUnprotectedTeams(GetTeam());
		bullseyeSearch.sortMode = BullseyeSearch.SortMode.Distance;
		bullseyeSearch.RefreshCandidates();
		bullseyeSearch.FilterOutGameObject(base.gameObject);
		return bullseyeSearch.GetResults().FirstOrDefault();
	}

	private void CreateBlinkEffect(Vector3 origin)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		EffectData effectData = new EffectData();
		effectData.rotation = Util.QuaternionSafeLookRotation(Vector3.up);
		effectData.origin = origin;
		EffectManager.SpawnEffect(blinkPrefab, effectData, transmit: false);
	}

	public override void OnExit()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		Util.PlaySound(endSoundString, base.gameObject);
		CreateBlinkEffect(Util.GetCorePosition(base.gameObject));
		modelTransform = GetModelTransform();
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			TemporaryOverlay temporaryOverlay = ((Component)modelTransform).gameObject.AddComponent<TemporaryOverlay>();
			temporaryOverlay.duration = 0.6f;
			temporaryOverlay.animateShaderAlpha = true;
			temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
			temporaryOverlay.destroyComponentOnEnd = true;
			temporaryOverlay.originalMaterial = LegacyResourcesAPI.Load<Material>("Materials/matMercEvisTarget");
			temporaryOverlay.AddToCharacerModel(((Component)modelTransform).GetComponent<CharacterModel>());
			TemporaryOverlay temporaryOverlay2 = ((Component)modelTransform).gameObject.AddComponent<TemporaryOverlay>();
			temporaryOverlay2.duration = 0.7f;
			temporaryOverlay2.animateShaderAlpha = true;
			temporaryOverlay2.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
			temporaryOverlay2.destroyComponentOnEnd = true;
			temporaryOverlay2.originalMaterial = LegacyResourcesAPI.Load<Material>("Materials/matHuntressFlashExpanded");
			temporaryOverlay2.AddToCharacerModel(((Component)modelTransform).GetComponent<CharacterModel>());
		}
		if (Object.op_Implicit((Object)(object)characterModel))
		{
			characterModel.invisibilityCount--;
		}
		aimRequest?.Dispose();
		if (NetworkServer.active)
		{
			base.characterBody.RemoveBuff(RoR2Content.Buffs.HiddenInvincibility);
			base.characterBody.AddTimedBuff(RoR2Content.Buffs.HiddenInvincibility, lingeringInvincibilityDuration);
		}
		Util.PlaySound(endSoundString, base.gameObject);
		SmallHop(base.characterMotor, smallHopVelocity);
		base.OnExit();
	}
}
