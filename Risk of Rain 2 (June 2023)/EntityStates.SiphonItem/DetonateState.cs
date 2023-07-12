using RoR2;
using RoR2.Orbs;
using UnityEngine;

namespace EntityStates.SiphonItem;

public class DetonateState : BaseSiphonItemState
{
	public static float baseSiphonRange = 50f;

	public static float baseDuration;

	public static float healPulseFraction = 0.5f;

	public static float healMultiplier = 2f;

	public static float damageFraction = 0.1f;

	public static GameObject burstEffectPrefab;

	public static string explosionSound;

	public static string siphonLoopSound;

	public static string retractSound;

	private float duration;

	private float gainedHealth;

	private float gainedHealthFraction;

	private float healTimer;

	private bool burstPlayed;

	public override void OnEnter()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		duration = baseDuration;
		GetItemStack();
		Vector3 position = ((Component)base.attachedBody).transform.position;
		SphereSearch obj = new SphereSearch
		{
			origin = position,
			radius = baseSiphonRange,
			mask = LayerIndex.entityPrecise.mask
		};
		float num = base.attachedBody.healthComponent.fullCombinedHealth * damageFraction;
		TeamMask mask = default(TeamMask);
		mask.AddTeam(base.attachedBody.teamComponent.teamIndex);
		HurtBox[] hurtBoxes = obj.RefreshCandidates().FilterCandidatesByHurtBoxTeam(mask).OrderCandidatesByDistance()
			.FilterCandidatesByDistinctHurtBoxEntities()
			.GetHurtBoxes();
		foreach (HurtBox hurtBox in hurtBoxes)
		{
			if ((Object)(object)hurtBox.healthComponent != (Object)(object)base.attachedBody.healthComponent)
			{
				if (!burstPlayed)
				{
					burstPlayed = true;
					Util.PlaySound(explosionSound, base.gameObject);
					Util.PlaySound(siphonLoopSound, base.gameObject);
					EffectData effectData = new EffectData
					{
						scale = 1f,
						origin = position
					};
					effectData.SetHurtBoxReference(((Component)((Component)base.attachedBody.modelLocator.modelTransform).GetComponent<ChildLocator>().FindChild("Base")).gameObject);
					EffectManager.SpawnEffect(burstEffectPrefab, effectData, transmit: false);
				}
				DamageInfo damageInfo = new DamageInfo
				{
					attacker = ((Component)base.attachedBody).gameObject,
					inflictor = ((Component)base.attachedBody).gameObject,
					crit = false,
					damage = num,
					damageColorIndex = DamageColorIndex.Default,
					damageType = DamageType.Generic,
					force = Vector3.zero,
					position = ((Component)hurtBox).transform.position
				};
				hurtBox.healthComponent.TakeDamage(damageInfo);
				HurtBox hurtBoxReference = hurtBox;
				HurtBoxGroup hurtBoxGroup = hurtBox.hurtBoxGroup;
				for (int j = 0; (float)j < Mathf.Min(4f, base.attachedBody.radius * 2f); j++)
				{
					EffectData effectData2 = new EffectData
					{
						scale = 1f,
						origin = position,
						genericFloat = 3f
					};
					effectData2.SetHurtBoxReference(hurtBoxReference);
					GameObject obj2 = LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/OrbEffects/SiphonOrbEffect");
					obj2.GetComponent<OrbEffect>().parentObjectTransform = ((Component)base.attachedBody.mainHurtBox).transform;
					EffectManager.SpawnEffect(obj2, effectData2, transmit: true);
					hurtBoxReference = hurtBoxGroup.hurtBoxes[Random.Range(0, hurtBoxGroup.hurtBoxes.Length)];
				}
				gainedHealth += num;
			}
		}
		gainedHealthFraction = gainedHealth * healPulseFraction;
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		healTimer += Time.fixedDeltaTime;
		if (base.isAuthority && healTimer > duration * healPulseFraction && gainedHealth > 0f)
		{
			base.attachedBody.healthComponent.Heal(gainedHealthFraction, default(ProcChainMask));
			TurnOnHealingFX();
			healTimer = 0f;
			if (base.fixedAge >= duration)
			{
				Util.PlaySound(retractSound, base.gameObject);
				TurnOffHealingFX();
				outer.SetNextState(new RechargeState());
			}
		}
	}
}
