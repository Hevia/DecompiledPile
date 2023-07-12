using System;
using KinematicCharacterController;
using RoR2;
using UnityEngine;

namespace EntityStates.Treebot;

public class BurrowDash : BaseCharacterMain
{
	[SerializeField]
	public float baseDuration;

	[SerializeField]
	public static AnimationCurve speedMultiplier;

	public static float chargeDamageCoefficient;

	public static GameObject impactEffectPrefab;

	public static GameObject burrowLoopEffectPrefab;

	public static float hitPauseDuration;

	public static float timeBeforeExitToPlayExitAnimation;

	public static string impactSoundString;

	public static string startSoundString;

	public static string endSoundString;

	public static float healPercent;

	public static bool resetDurationOnImpact;

	[SerializeField]
	public GameObject startEffectPrefab;

	[SerializeField]
	public GameObject endEffectPrefab;

	private float duration;

	private float hitPauseTimer;

	private Vector3 idealDirection;

	private OverlapAttack attack;

	private ChildLocator childLocator;

	private bool inHitPause;

	private bool beginPlayingExitAnimation;

	private Transform modelTransform;

	private GameObject burrowLoopEffectInstance;

	public override void OnEnter()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0285: Unknown result type (might be due to invalid IL or missing references)
		//IL_028b: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		duration = baseDuration;
		if (base.isAuthority)
		{
			if (Object.op_Implicit((Object)(object)base.inputBank))
			{
				idealDirection = base.inputBank.aimDirection;
				idealDirection.y = 0f;
			}
			UpdateDirection();
		}
		if (Object.op_Implicit((Object)(object)base.modelLocator))
		{
			base.modelLocator.normalizeToFloor = true;
		}
		if (Object.op_Implicit((Object)(object)startEffectPrefab) && Object.op_Implicit((Object)(object)base.characterBody))
		{
			EffectManager.SpawnEffect(startEffectPrefab, new EffectData
			{
				origin = base.characterBody.corePosition
			}, transmit: false);
		}
		if (Object.op_Implicit((Object)(object)base.characterDirection))
		{
			base.characterDirection.forward = idealDirection;
		}
		Util.PlaySound(startSoundString, base.gameObject);
		PlayCrossfade("Body", "BurrowEnter", 0.1f);
		HitBoxGroup hitBoxGroup = null;
		modelTransform = GetModelTransform();
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			hitBoxGroup = Array.Find(((Component)modelTransform).GetComponents<HitBoxGroup>(), (HitBoxGroup element) => element.groupName == "BurrowCharge");
			childLocator = ((Component)modelTransform).GetComponent<ChildLocator>();
		}
		attack = new OverlapAttack();
		attack.attacker = base.gameObject;
		attack.inflictor = base.gameObject;
		attack.teamIndex = TeamComponent.GetObjectTeam(attack.attacker);
		attack.damage = chargeDamageCoefficient * damageStat;
		attack.hitEffectPrefab = impactEffectPrefab;
		attack.hitBoxGroup = hitBoxGroup;
		attack.damage = damageStat * chargeDamageCoefficient;
		attack.damageType = DamageType.Freeze2s;
		attack.procCoefficient = 1f;
		base.gameObject.layer = LayerIndex.debris.intVal;
		((BaseCharacterController)base.characterMotor).Motor.RebuildCollidableLayers();
		((Component)modelTransform).GetComponent<HurtBoxGroup>().hurtBoxesDeactivatorCounter++;
		base.characterBody.hideCrosshair = true;
		base.characterBody.isSprinting = true;
		if (Object.op_Implicit((Object)(object)childLocator))
		{
			Transform val = childLocator.FindChild("BurrowCenter");
			if (Object.op_Implicit((Object)(object)val) && Object.op_Implicit((Object)(object)burrowLoopEffectPrefab))
			{
				burrowLoopEffectInstance = Object.Instantiate<GameObject>(burrowLoopEffectPrefab, val.position, val.rotation);
				burrowLoopEffectInstance.transform.parent = val;
			}
		}
	}

	public override void OnExit()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)base.characterBody) && !outer.destroying && Object.op_Implicit((Object)(object)endEffectPrefab))
		{
			EffectManager.SpawnEffect(endEffectPrefab, new EffectData
			{
				origin = base.characterBody.corePosition
			}, transmit: false);
		}
		Util.PlaySound(endSoundString, base.gameObject);
		base.gameObject.layer = LayerIndex.defaultLayer.intVal;
		((BaseCharacterController)base.characterMotor).Motor.RebuildCollidableLayers();
		((Component)modelTransform).GetComponent<HurtBoxGroup>().hurtBoxesDeactivatorCounter--;
		base.characterBody.hideCrosshair = false;
		if (Object.op_Implicit((Object)(object)burrowLoopEffectInstance))
		{
			EntityState.Destroy((Object)(object)burrowLoopEffectInstance);
		}
		Animator val = GetModelAnimator();
		int layerIndex = val.GetLayerIndex("Impact");
		if (layerIndex >= 0)
		{
			val.SetLayerWeight(layerIndex, 2f);
			val.PlayInFixedTime("LightImpact", layerIndex, 0f);
		}
		base.OnExit();
	}

	private void UpdateDirection()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)base.inputBank))
		{
			Vector2 val = Util.Vector3XZToVector2XY(base.inputBank.moveVector);
			if (val != Vector2.zero)
			{
				((Vector2)(ref val)).Normalize();
				Vector3 val2 = new Vector3(val.x, 0f, val.y);
				idealDirection = ((Vector3)(ref val2)).normalized;
			}
		}
	}

	protected override void UpdateAnimationParameters()
	{
	}

	private Vector3 GetIdealVelocity()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		return base.characterDirection.forward * base.characterBody.moveSpeed * speedMultiplier.Evaluate(base.fixedAge / duration);
	}

	public override void FixedUpdate()
	{
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		if (base.fixedAge >= duration)
		{
			outer.SetNextStateToMain();
			return;
		}
		if (base.fixedAge >= duration - timeBeforeExitToPlayExitAnimation && !beginPlayingExitAnimation)
		{
			beginPlayingExitAnimation = true;
			PlayCrossfade("Body", "BurrowExit", 0.1f);
		}
		if (!base.isAuthority)
		{
			return;
		}
		UpdateDirection();
		if (!inHitPause)
		{
			if (Object.op_Implicit((Object)(object)base.characterDirection))
			{
				base.characterDirection.moveVector = idealDirection;
				if (Object.op_Implicit((Object)(object)base.characterMotor) && !base.characterMotor.disableAirControlUntilCollision)
				{
					CharacterMotor obj = base.characterMotor;
					obj.rootMotion += GetIdealVelocity() * Time.fixedDeltaTime;
				}
			}
			if (attack.Fire())
			{
				Util.PlaySound(impactSoundString, base.gameObject);
				inHitPause = true;
				hitPauseTimer = hitPauseDuration;
				if (healPercent > 0f)
				{
					base.healthComponent.HealFraction(healPercent, default(ProcChainMask));
					Util.PlaySound("Play_item_use_fruit", base.gameObject);
					EffectData effectData = new EffectData();
					effectData.origin = base.transform.position;
					effectData.SetNetworkedObjectReference(base.gameObject);
					EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/FruitHealEffect"), effectData, transmit: true);
				}
				if (resetDurationOnImpact)
				{
					base.fixedAge = 0f;
				}
				else
				{
					base.fixedAge -= hitPauseDuration;
				}
			}
		}
		else
		{
			base.characterMotor.velocity = Vector3.zero;
			hitPauseTimer -= Time.fixedDeltaTime;
			if (hitPauseTimer < 0f)
			{
				inHitPause = false;
			}
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Death;
	}
}
