using KinematicCharacterController;
using RoR2;
using RoR2.Networking;
using RoR2.Orbs;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Merc;

public class FocusedAssaultDash : BasicMeleeAttack
{
	[SerializeField]
	public float speedCoefficientOnExit;

	[SerializeField]
	public float speedCoefficient;

	[SerializeField]
	public string endSoundString;

	[SerializeField]
	public float exitSmallHop;

	[SerializeField]
	public float delayedDamageCoefficient;

	[SerializeField]
	public float delayedProcCoefficient;

	[SerializeField]
	public float delay;

	[SerializeField]
	public string enterAnimationLayerName = "FullBody, Override";

	[SerializeField]
	public string enterAnimationStateName = "AssaulterLoop";

	[SerializeField]
	public float enterAnimationCrossfadeDuration = 0.1f;

	[SerializeField]
	public string exitAnimationLayerName = "FullBody, Override";

	[SerializeField]
	public string exitAnimationStateName = "EvisLoopExit";

	[SerializeField]
	public Material enterOverlayMaterial;

	[SerializeField]
	public float enterOverlayDuration = 0.7f;

	[SerializeField]
	public GameObject delayedEffectPrefab;

	[SerializeField]
	public GameObject orbEffect;

	[SerializeField]
	public float delayPerHit;

	[SerializeField]
	public GameObject selfOnHitOverlayEffectPrefab;

	private Transform modelTransform;

	private Vector3 dashVector;

	private int currentHitCount;

	private Vector3 dashVelocity => dashVector * moveSpeedStat * speedCoefficient;

	public override void OnEnter()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		dashVector = base.inputBank.aimDirection;
		base.gameObject.layer = LayerIndex.fakeActor.intVal;
		((BaseCharacterController)base.characterMotor).Motor.RebuildCollidableLayers();
		((BaseCharacterController)base.characterMotor).Motor.ForceUnground();
		base.characterMotor.velocity = Vector3.zero;
		modelTransform = GetModelTransform();
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			TemporaryOverlay temporaryOverlay = ((Component)modelTransform).gameObject.AddComponent<TemporaryOverlay>();
			temporaryOverlay.duration = enterOverlayDuration;
			temporaryOverlay.animateShaderAlpha = true;
			temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
			temporaryOverlay.destroyComponentOnEnd = true;
			temporaryOverlay.originalMaterial = enterOverlayMaterial;
			temporaryOverlay.AddToCharacerModel(((Component)modelTransform).GetComponent<CharacterModel>());
		}
		PlayCrossfade(enterAnimationLayerName, enterAnimationStateName, enterAnimationCrossfadeDuration);
		base.characterDirection.forward = ((Vector3)(ref base.characterMotor.velocity)).normalized;
		if (NetworkServer.active)
		{
			base.characterBody.AddBuff(RoR2Content.Buffs.HiddenInvincibility);
		}
	}

	public override void OnExit()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		if (NetworkServer.active)
		{
			base.characterBody.RemoveBuff(RoR2Content.Buffs.HiddenInvincibility);
		}
		CharacterMotor obj = base.characterMotor;
		obj.velocity *= speedCoefficientOnExit;
		SmallHop(base.characterMotor, exitSmallHop);
		Util.PlaySound(endSoundString, base.gameObject);
		PlayAnimation(exitAnimationLayerName, exitAnimationStateName);
		base.gameObject.layer = LayerIndex.defaultLayer.intVal;
		((BaseCharacterController)base.characterMotor).Motor.RebuildCollidableLayers();
		base.OnExit();
	}

	protected override void PlayAnimation()
	{
		base.PlayAnimation();
		PlayCrossfade(enterAnimationLayerName, enterAnimationStateName, enterAnimationCrossfadeDuration);
	}

	protected override void AuthorityFixedUpdate()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		base.AuthorityFixedUpdate();
		if (!base.authorityInHitPause)
		{
			CharacterMotor obj = base.characterMotor;
			obj.rootMotion += dashVelocity * Time.fixedDeltaTime;
			base.characterDirection.forward = dashVelocity;
			base.characterDirection.moveVector = dashVelocity;
			base.characterBody.isSprinting = true;
		}
	}

	protected override void AuthorityModifyOverlapAttack(OverlapAttack overlapAttack)
	{
		base.AuthorityModifyOverlapAttack(overlapAttack);
		overlapAttack.damage = damageCoefficient * damageStat;
	}

	protected override void OnMeleeHitAuthority()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		base.OnMeleeHitAuthority();
		float num = hitPauseDuration / attackSpeedStat;
		if (Object.op_Implicit((Object)(object)selfOnHitOverlayEffectPrefab) && num > 1f / 30f)
		{
			EffectData effectData = new EffectData
			{
				origin = base.transform.position,
				genericFloat = hitPauseDuration / attackSpeedStat
			};
			effectData.SetNetworkedObjectReference(base.gameObject);
			EffectManager.SpawnEffect(selfOnHitOverlayEffectPrefab, effectData, transmit: true);
		}
		foreach (HurtBox hitResult in hitResults)
		{
			currentHitCount++;
			float damageValue = base.characterBody.damage * delayedDamageCoefficient;
			float num2 = delay + delayPerHit * (float)currentHitCount;
			bool isCrit = RollCrit();
			HandleHit(base.gameObject, hitResult, damageValue, delayedProcCoefficient, isCrit, num2, orbEffect, delayedEffectPrefab);
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.PrioritySkill;
	}

	private static void HandleHit(GameObject attackerObject, HurtBox victimHurtBox, float damageValue, float procCoefficient, bool isCrit, float delay, GameObject orbEffectPrefab, GameObject orbImpactEffectPrefab)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Expected O, but got Unknown
		if (!NetworkServer.active)
		{
			NetworkWriter val = new NetworkWriter();
			val.StartMessage((short)77);
			val.Write(attackerObject);
			val.Write(HurtBoxReference.FromHurtBox(victimHurtBox));
			val.Write(damageValue);
			val.Write(procCoefficient);
			val.Write(isCrit);
			val.Write(delay);
			val.WriteEffectIndex(EffectCatalog.FindEffectIndexFromPrefab(orbEffectPrefab));
			val.WriteEffectIndex(EffectCatalog.FindEffectIndexFromPrefab(orbImpactEffectPrefab));
			val.FinishMessage();
			NetworkConnection readyConnection = ClientScene.readyConnection;
			if (readyConnection != null)
			{
				readyConnection.SendWriter(val, QosChannelIndex.defaultReliable.intVal);
			}
		}
		else if (Object.op_Implicit((Object)(object)victimHurtBox) && Object.op_Implicit((Object)(object)victimHurtBox.healthComponent))
		{
			SetStateOnHurt.SetStunOnObject(((Component)victimHurtBox.healthComponent).gameObject, delay);
			OrbManager.instance.AddOrb(new DelayedHitOrb
			{
				attacker = attackerObject,
				target = victimHurtBox,
				damageColorIndex = DamageColorIndex.Default,
				damageValue = damageValue,
				damageType = DamageType.ApplyMercExpose,
				isCrit = isCrit,
				procChainMask = default(ProcChainMask),
				procCoefficient = procCoefficient,
				delay = delay,
				orbEffect = orbEffectPrefab,
				delayedEffectPrefab = orbImpactEffectPrefab
			});
		}
	}

	[NetworkMessageHandler(msgType = 77, client = false, server = true)]
	private static void HandleReportMercFocusedAssaultHitReplaceMeLater(NetworkMessage netMsg)
	{
		GameObject attackerObject = netMsg.reader.ReadGameObject();
		HurtBox victimHurtBox = netMsg.reader.ReadHurtBoxReference().ResolveHurtBox();
		float damageValue = netMsg.reader.ReadSingle();
		float num = netMsg.reader.ReadSingle();
		bool isCrit = netMsg.reader.ReadBoolean();
		float num2 = netMsg.reader.ReadSingle();
		GameObject orbEffectPrefab = EffectCatalog.GetEffectDef(netMsg.reader.ReadEffectIndex())?.prefab ?? null;
		GameObject orbImpactEffectPrefab = EffectCatalog.GetEffectDef(netMsg.reader.ReadEffectIndex())?.prefab ?? null;
		HandleHit(attackerObject, victimHurtBox, damageValue, num, isCrit, num2, orbEffectPrefab, orbImpactEffectPrefab);
	}
}
