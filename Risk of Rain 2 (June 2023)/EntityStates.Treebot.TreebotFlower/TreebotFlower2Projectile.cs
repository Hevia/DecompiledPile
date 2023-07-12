using System.Collections.Generic;
using RoR2;
using RoR2.Orbs;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Treebot.TreebotFlower;

public class TreebotFlower2Projectile : BaseState
{
	public static float yankIdealDistance;

	public static AnimationCurve yankSuitabilityCurve;

	public static float healthFractionYieldPerHit;

	public static float radius;

	public static float healPulseCount;

	public static float duration;

	public static float rootPulseCount;

	public static string enterSoundString;

	public static string exitSoundString;

	public static GameObject enterEffectPrefab;

	public static GameObject exitEffectPrefab;

	private List<CharacterBody> rootedBodies;

	private float healTimer;

	private float rootPulseTimer;

	private GameObject owner;

	private ProcChainMask procChainMask;

	private float procCoefficient;

	private TeamIndex teamIndex = TeamIndex.None;

	private float damage;

	private DamageType damageType;

	private bool crit;

	private float healPulseHealthFractionValue;

	public override void OnEnter()
	{
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		ProjectileController component = GetComponent<ProjectileController>();
		if (Object.op_Implicit((Object)(object)component))
		{
			owner = component.owner;
			procChainMask = component.procChainMask;
			procCoefficient = component.procCoefficient;
			teamIndex = component.teamFilter.teamIndex;
		}
		ProjectileDamage component2 = GetComponent<ProjectileDamage>();
		if (Object.op_Implicit((Object)(object)component2))
		{
			damage = component2.damage;
			damageType = component2.damageType;
			crit = component2.crit;
		}
		if (NetworkServer.active)
		{
			rootedBodies = new List<CharacterBody>();
		}
		PlayAnimation("Base", "SpawnToIdle");
		Util.PlaySound(enterSoundString, base.gameObject);
		if (Object.op_Implicit((Object)(object)enterEffectPrefab))
		{
			EffectManager.SimpleEffect(enterEffectPrefab, base.transform.position, base.transform.rotation, transmit: false);
		}
		ChildLocator component3 = ((Component)GetModelTransform()).GetComponent<ChildLocator>();
		if (Object.op_Implicit((Object)(object)component3))
		{
			Transform obj = component3.FindChild("AreaIndicator");
			obj.localScale = new Vector3(radius, radius, radius);
			((Component)obj).gameObject.SetActive(true);
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (NetworkServer.active)
		{
			rootPulseTimer -= Time.fixedDeltaTime;
			healTimer -= Time.fixedDeltaTime;
			if (rootPulseTimer <= 0f)
			{
				rootPulseTimer += duration / rootPulseCount;
				RootPulse();
			}
			if (healTimer <= 0f)
			{
				healTimer += duration / healPulseCount;
				HealPulse();
			}
			if (base.fixedAge >= duration)
			{
				EntityState.Destroy((Object)(object)base.gameObject);
			}
		}
	}

	private void RootPulse()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = base.transform.position;
		HurtBox[] hurtBoxes = new SphereSearch
		{
			origin = position,
			radius = radius,
			mask = LayerIndex.entityPrecise.mask
		}.RefreshCandidates().FilterCandidatesByHurtBoxTeam(TeamMask.GetEnemyTeams(teamIndex)).OrderCandidatesByDistance()
			.FilterCandidatesByDistinctHurtBoxEntities()
			.GetHurtBoxes();
		foreach (HurtBox hurtBox in hurtBoxes)
		{
			CharacterBody body = hurtBox.healthComponent.body;
			if (!rootedBodies.Contains(body))
			{
				rootedBodies.Add(body);
				body.AddBuff(RoR2Content.Buffs.Entangle);
				body.RecalculateStats();
				Vector3 val = ((Component)hurtBox).transform.position - position;
				float magnitude = ((Vector3)(ref val)).magnitude;
				Vector3 val2 = val / magnitude;
				Rigidbody component = ((Component)hurtBox.healthComponent).GetComponent<Rigidbody>();
				float num = (Object.op_Implicit((Object)(object)component) ? component.mass : 1f);
				float num2 = magnitude - yankIdealDistance;
				float num3 = yankSuitabilityCurve.Evaluate(num);
				Vector3 val3 = (Object.op_Implicit((Object)(object)component) ? component.velocity : Vector3.zero);
				if (HGMath.IsVectorNaN(val3))
				{
					val3 = Vector3.zero;
				}
				Vector3 val4 = -val3;
				if (num2 > 0f)
				{
					val4 = val2 * (0f - Trajectory.CalculateInitialYSpeedForHeight(num2, 0f - body.acceleration));
				}
				Vector3 force = val4 * (num * num3);
				DamageInfo damageInfo = new DamageInfo
				{
					attacker = owner,
					inflictor = base.gameObject,
					crit = crit,
					damage = damage,
					damageColorIndex = DamageColorIndex.Default,
					damageType = damageType,
					force = force,
					position = ((Component)hurtBox).transform.position,
					procChainMask = procChainMask,
					procCoefficient = procCoefficient
				};
				hurtBox.healthComponent.TakeDamage(damageInfo);
				HurtBox hurtBoxReference = hurtBox;
				HurtBoxGroup hurtBoxGroup = hurtBox.hurtBoxGroup;
				for (int j = 0; (float)j < Mathf.Min(4f, body.radius * 2f); j++)
				{
					EffectData effectData = new EffectData
					{
						scale = 1f,
						origin = position,
						genericFloat = Mathf.Max(0.2f, duration - base.fixedAge)
					};
					effectData.SetHurtBoxReference(hurtBoxReference);
					EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/OrbEffects/EntangleOrbEffect"), effectData, transmit: true);
					hurtBoxReference = hurtBoxGroup.hurtBoxes[Random.Range(0, hurtBoxGroup.hurtBoxes.Length)];
				}
			}
		}
	}

	public override void OnExit()
	{
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		if (rootedBodies != null)
		{
			foreach (CharacterBody rootedBody in rootedBodies)
			{
				rootedBody.RemoveBuff(RoR2Content.Buffs.Entangle);
			}
			rootedBodies = null;
		}
		Util.PlaySound(exitSoundString, base.gameObject);
		if (Object.op_Implicit((Object)(object)exitEffectPrefab))
		{
			EffectManager.SimpleEffect(exitEffectPrefab, base.transform.position, base.transform.rotation, transmit: false);
		}
		base.OnExit();
	}

	private void HealPulse()
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		HealthComponent healthComponent = (Object.op_Implicit((Object)(object)owner) ? owner.GetComponent<HealthComponent>() : null);
		if (Object.op_Implicit((Object)(object)healthComponent) && rootedBodies.Count > 0)
		{
			float num = 1f / healPulseCount;
			HealOrb healOrb = new HealOrb();
			healOrb.origin = base.transform.position;
			healOrb.target = healthComponent.body.mainHurtBox;
			healOrb.healValue = num * healthFractionYieldPerHit * healthComponent.fullHealth * (float)rootedBodies.Count;
			healOrb.overrideDuration = 0.3f;
			OrbManager.instance.AddOrb(healOrb);
		}
	}
}
