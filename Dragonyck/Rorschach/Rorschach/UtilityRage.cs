using EntityStates;
using EntityStates.BeetleGuardMonster;
using EntityStates.ImpBossMonster;
using EntityStates.Toolbot;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace Rorschach;

internal class UtilityRage : BaseSkillState
{
	private float duration = 4.2f;

	private GrabBehaviour behaviour;

	private SphereSearch sphereSearch = new SphereSearch();

	private CollisionCaller col;

	private float damageCoefficient = 4f;

	private float bossHitDamageCoefficient = 4.5f;

	private float impactDamageCoefficient = 3f;

	private float impactRadius = 12f;

	private float speedMultiplier = 5f;

	private bool blast;

	private bool isBoss;

	private float blastStopwatch;

	private RorschachRageBarBehaviour rageBehaviour;

	private GameObject dashEffectInstance;

	public override void OnEnter()
	{
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Expected O, but got Unknown
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		((BaseState)this).OnEnter();
		rageBehaviour = ((EntityState)this).GetComponent<RorschachRageBarBehaviour>();
		if (Object.op_Implicit((Object)(object)rageBehaviour))
		{
			rageBehaviour.canExecute = false;
		}
		((BaseState)this).StartAimMode(2f, false);
		if (Object.op_Implicit((Object)(object)((EntityState)this).modelLocator))
		{
			((EntityState)this).modelLocator.normalizeToFloor = true;
		}
		behaviour = ((EntityState)this).GetComponent<GrabBehaviour>();
		sphereSearch = new SphereSearch();
		sphereSearch.origin = ((Component)((EntityState)this).characterBody.mainHurtBox).transform.position;
		sphereSearch.radius = 2f;
		sphereSearch.mask = ((LayerIndex)(ref LayerIndex.entityPrecise)).mask;
		ChildLocator modelChildLocator = ((EntityState)this).GetModelChildLocator();
		if (Object.op_Implicit((Object)(object)modelChildLocator))
		{
			Transform val = modelChildLocator.FindChild("cubeCollider");
			if (Object.op_Implicit((Object)(object)val))
			{
				col = ((Component)val).gameObject.GetComponent<CollisionCaller>();
				if (Object.op_Implicit((Object)(object)col))
				{
					col.terrainCollision = false;
					col.bodyCollision = false;
				}
			}
		}
		((EntityState)this).GetModelAnimator().SetBool("skillOver", false);
		((EntityState)this).PlayAnimation("FullBody, Override", "rage_utility");
		dashEffectInstance = Object.Instantiate<GameObject>(Prefabs.dashEffect, ((BaseState)this).FindModelChild("dashMuzzle"));
		dashEffectInstance.transform.localPosition = Vector3.zero;
	}

	private Vector3 GetIdealVelocity()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		return ((EntityState)this).characterDirection.forward * ((EntityState)this).characterBody.moveSpeed * speedMultiplier;
	}

	private void BlastAttack(float damageCoefficient)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Expected O, but got Unknown
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		EffectManager.SpawnEffect(GroundSlam.slamEffectPrefab, new EffectData
		{
			origin = ((EntityState)this).characterBody.footPosition,
			scale = impactRadius
		}, false);
		Util.PlaySound("Play_parent_attack1_slam", ((EntityState)this).gameObject);
		if (((EntityState)this).isAuthority)
		{
			new BlastAttack
			{
				attacker = ((EntityState)this).gameObject,
				baseDamage = ((BaseState)this).damageStat * impactDamageCoefficient * damageCoefficient,
				baseForce = 200f,
				bonusForce = Vector3.back * 400f,
				crit = ((BaseState)this).RollCrit(),
				damageType = (DamageType)32,
				falloffModel = (FalloffModel)0,
				procCoefficient = 1f,
				radius = impactRadius,
				position = ((EntityState)this).characterBody.footPosition,
				attackerFiltering = (AttackerFiltering)2,
				impactEffect = EffectCatalog.FindEffectIndexFromPrefab(GroundPound.hitEffectPrefab),
				teamIndex = ((EntityState)this).teamComponent.teamIndex
			}.Fire();
		}
		((BaseState)this).AddRecoil(-0.5f * ToolbotDash.recoilAmplitude, -0.5f * ToolbotDash.recoilAmplitude, -0.5f * ToolbotDash.recoilAmplitude, 0.5f * ToolbotDash.recoilAmplitude);
	}

	public override void FixedUpdate()
	{
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_027e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0283: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Expected O, but got Unknown
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_029c: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02db: Unknown result type (might be due to invalid IL or missing references)
		//IL_02df: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0328: Unknown result type (might be due to invalid IL or missing references)
		//IL_0333: Unknown result type (might be due to invalid IL or missing references)
		//IL_0343: Unknown result type (might be due to invalid IL or missing references)
		//IL_034e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0358: Unknown result type (might be due to invalid IL or missing references)
		//IL_035d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0362: Unknown result type (might be due to invalid IL or missing references)
		//IL_036d: Unknown result type (might be due to invalid IL or missing references)
		//IL_036e: Unknown result type (might be due to invalid IL or missing references)
		//IL_055b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0560: Unknown result type (might be due to invalid IL or missing references)
		//IL_056b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0570: Unknown result type (might be due to invalid IL or missing references)
		((EntityState)this).FixedUpdate();
		if (Object.op_Implicit((Object)(object)behaviour) && Object.op_Implicit((Object)(object)behaviour.hurtBox) && Object.op_Implicit((Object)(object)col) && col.terrainCollision)
		{
			((EntityState)this).GetModelAnimator().SetBool("skillOver", true);
			((BaseState)this).AddRecoil(-0.5f * ToolbotDash.recoilAmplitude, -0.5f * ToolbotDash.recoilAmplitude, -0.5f * ToolbotDash.recoilAmplitude, 0.5f * ToolbotDash.recoilAmplitude);
			if (Object.op_Implicit((Object)(object)behaviour.hurtBox.healthComponent) && NetworkServer.active)
			{
				DamageInfo val = new DamageInfo();
				val.attacker = ((EntityState)this).gameObject;
				val.inflictor = ((EntityState)this).gameObject;
				val.damage = ((BaseState)this).damageStat * damageCoefficient;
				val.damageColorIndex = (DamageColorIndex)0;
				val.damageType = (DamageType)32;
				val.position = ((Component)behaviour.hurtBox).transform.position;
				behaviour.hurtBox.healthComponent.TakeDamage(val);
			}
			((EntityState)this).outer.SetNextStateToMain();
		}
		if (Object.op_Implicit((Object)(object)((EntityState)this).characterBody.mainHurtBox))
		{
			sphereSearch.origin = ((Component)((EntityState)this).characterBody.mainHurtBox).transform.position;
		}
		if (Object.op_Implicit((Object)(object)behaviour) && !Object.op_Implicit((Object)(object)behaviour.hurtBox) && !blast)
		{
			HurtBox[] hurtBoxes = sphereSearch.RefreshCandidates().FilterCandidatesByHurtBoxTeam(TeamMask.GetEnemyTeams(((EntityState)this).teamComponent.teamIndex)).OrderCandidatesByDistance()
				.FilterCandidatesByDistinctHurtBoxEntities()
				.GetHurtBoxes();
			if (hurtBoxes.Length != 0 && Object.op_Implicit((Object)(object)hurtBoxes[0]) && Object.op_Implicit((Object)(object)hurtBoxes[0].healthComponent) && Object.op_Implicit((Object)(object)hurtBoxes[0].healthComponent.body))
			{
				if (!hurtBoxes[0].healthComponent.body.isBoss)
				{
					behaviour.coreTransform = ((EntityState)this).GetModelChildLocator().FindChild("grabMuzzle");
					behaviour.hurtBox = hurtBoxes[0];
				}
				else
				{
					isBoss = true;
				}
			}
		}
		Vector3 moveVector = Vector3.one;
		if (Object.op_Implicit((Object)(object)((EntityState)this).inputBank))
		{
			Vector2 val2 = Util.Vector3XZToVector2XY(((EntityState)this).inputBank.aimDirection);
			if (val2 != Vector2.zero)
			{
				((Vector2)(ref val2)).Normalize();
				Vector3 val3 = new Vector3(val2.x, 0f, val2.y);
				moveVector = ((Vector3)(ref val3)).normalized;
			}
		}
		if (Object.op_Implicit((Object)(object)((EntityState)this).characterMotor) && Object.op_Implicit((Object)(object)((EntityState)this).characterDirection) && !blast && !((EntityState)this).characterMotor.disableAirControlUntilCollision)
		{
			CharacterMotor characterMotor = ((EntityState)this).characterMotor;
			characterMotor.rootMotion += ((EntityState)this).inputBank.aimDirection * ((EntityState)this).characterBody.moveSpeed * speedMultiplier * Time.fixedDeltaTime;
			((EntityState)this).characterDirection.moveVector = moveVector;
		}
		if (blast)
		{
			blastStopwatch += Time.fixedDeltaTime;
			if (blastStopwatch >= 1.2f && ((EntityState)this).isAuthority)
			{
				((EntityState)this).outer.SetNextStateToMain();
				return;
			}
		}
		else
		{
			((EntityState)this).characterBody.isSprinting = true;
		}
		if (isBoss && !blast)
		{
			blast = true;
			BlastAttack(bossHitDamageCoefficient);
		}
		if (((EntityState)this).fixedAge >= 3f && Object.op_Implicit((Object)(object)behaviour) && Object.op_Implicit((Object)(object)behaviour.hurtBox) && Object.op_Implicit((Object)(object)behaviour.hurtBox.healthComponent) && Object.op_Implicit((Object)(object)behaviour.hurtBox.healthComponent.body) && Object.op_Implicit((Object)(object)behaviour.hurtBox.healthComponent.body.characterMotor) && !blast)
		{
			blast = true;
			behaviour.DetachHurtBox();
			BlastAttack(behaviour.hurtBox.healthComponent.body.characterMotor.mass / 100f);
		}
		if (((EntityState)this).fixedAge >= duration)
		{
			BlastAttack(((EntityState)this).characterBody.damage * damageCoefficient);
			if (Object.op_Implicit((Object)(object)((EntityState)this).characterMotor) && Object.op_Implicit((Object)(object)((EntityState)this).characterDirection) && !blast && !((EntityState)this).characterMotor.disableAirControlUntilCollision)
			{
				((EntityState)this).characterMotor.velocity = Vector3.zero;
				((EntityState)this).characterMotor.rootMotion = Vector3.zero;
			}
			if (((EntityState)this).isAuthority)
			{
				((EntityState)this).outer.SetNextStateToMain();
			}
		}
	}

	public override void OnExit()
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		((EntityState)this).OnExit();
		((EntityState)this).GetModelAnimator().SetBool("skillOver", true);
		if (Object.op_Implicit((Object)(object)behaviour))
		{
			behaviour.DetachHurtBox();
		}
		if (Object.op_Implicit((Object)(object)((EntityState)this).characterMotor) && !((EntityState)this).characterMotor.disableAirControlUntilCollision && !blast)
		{
			CharacterMotor characterMotor = ((EntityState)this).characterMotor;
			characterMotor.velocity += GetIdealVelocity();
		}
		if (Object.op_Implicit((Object)(object)((EntityState)this).modelLocator))
		{
			((EntityState)this).modelLocator.normalizeToFloor = false;
		}
		if (Object.op_Implicit((Object)(object)rageBehaviour))
		{
			rageBehaviour.canExecute = true;
		}
		if (Object.op_Implicit((Object)(object)dashEffectInstance))
		{
			EntityState.Destroy((Object)(object)dashEffectInstance);
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		return (InterruptPriority)2;
	}
}
