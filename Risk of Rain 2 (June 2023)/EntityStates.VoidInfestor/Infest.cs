using System;
using System.Collections.Generic;
using System.Linq;
using KinematicCharacterController;
using RoR2;
using RoR2.CharacterAI;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.VoidInfestor;

public class Infest : BaseState
{
	public static GameObject enterEffectPrefab;

	public static GameObject successfulInfestEffectPrefab;

	public static GameObject infestVfxPrefab;

	public static string enterSoundString;

	public static float searchMaxDistance;

	public static float searchMaxAngle;

	public static float velocityInitialSpeed;

	public static float velocityTurnRate;

	public static float infestDamageCoefficient;

	private Transform targetTransform;

	private GameObject infestVfxInstance;

	private OverlapAttack attack;

	private List<HurtBox> victimsStruck = new List<HurtBox>();

	public override void OnEnter()
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		//IL_023b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0240: Unknown result type (might be due to invalid IL or missing references)
		//IL_0245: Unknown result type (might be due to invalid IL or missing references)
		//IL_0249: Unknown result type (might be due to invalid IL or missing references)
		//IL_0250: Unknown result type (might be due to invalid IL or missing references)
		//IL_0269: Unknown result type (might be due to invalid IL or missing references)
		//IL_0279: Unknown result type (might be due to invalid IL or missing references)
		//IL_0287: Unknown result type (might be due to invalid IL or missing references)
		//IL_029e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c7: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		PlayAnimation("Base", "Infest");
		Util.PlaySound(enterSoundString, base.gameObject);
		if (Object.op_Implicit((Object)(object)enterEffectPrefab))
		{
			EffectManager.SimpleImpactEffect(enterEffectPrefab, base.characterBody.corePosition, Vector3.up, transmit: false);
		}
		if (Object.op_Implicit((Object)(object)infestVfxPrefab))
		{
			infestVfxInstance = Object.Instantiate<GameObject>(infestVfxPrefab, base.transform.position, Quaternion.identity);
			infestVfxInstance.transform.parent = base.transform;
		}
		HitBoxGroup hitBoxGroup = null;
		Transform modelTransform = GetModelTransform();
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			hitBoxGroup = Array.Find(((Component)modelTransform).GetComponents<HitBoxGroup>(), (HitBoxGroup element) => element.groupName == "Infest");
		}
		attack = new OverlapAttack();
		attack.attacker = base.gameObject;
		attack.inflictor = base.gameObject;
		attack.teamIndex = GetTeam();
		attack.damage = infestDamageCoefficient * damageStat;
		attack.hitEffectPrefab = null;
		attack.hitBoxGroup = hitBoxGroup;
		attack.isCrit = RollCrit();
		attack.damageType = DamageType.Stun1s;
		attack.damageColorIndex = DamageColorIndex.Void;
		BullseyeSearch bullseyeSearch = new BullseyeSearch();
		bullseyeSearch.viewer = base.characterBody;
		bullseyeSearch.teamMaskFilter = TeamMask.allButNeutral;
		bullseyeSearch.teamMaskFilter.RemoveTeam(base.characterBody.teamComponent.teamIndex);
		bullseyeSearch.sortMode = BullseyeSearch.SortMode.Distance;
		bullseyeSearch.minDistanceFilter = 0f;
		bullseyeSearch.maxDistanceFilter = searchMaxDistance;
		bullseyeSearch.searchOrigin = base.inputBank.aimOrigin;
		bullseyeSearch.searchDirection = base.inputBank.aimDirection;
		bullseyeSearch.maxAngleFilter = searchMaxAngle;
		bullseyeSearch.filterByLoS = true;
		bullseyeSearch.RefreshCandidates();
		HurtBox hurtBox = bullseyeSearch.GetResults().FirstOrDefault();
		if (Object.op_Implicit((Object)(object)hurtBox))
		{
			targetTransform = ((Component)hurtBox).transform;
			if (Object.op_Implicit((Object)(object)base.characterMotor))
			{
				Vector3 position = targetTransform.position;
				float num = velocityInitialSpeed;
				Vector3 val = position - base.transform.position;
				Vector2 val2 = default(Vector2);
				((Vector2)(ref val2))._002Ector(val.x, val.z);
				float magnitude = ((Vector2)(ref val2)).magnitude;
				float num2 = Trajectory.CalculateInitialYSpeed(magnitude / num, val.y);
				Vector3 val3 = default(Vector3);
				((Vector3)(ref val3))._002Ector(val2.x / magnitude * num, num2, val2.y / magnitude * num);
				base.characterMotor.velocity = val3;
				base.characterMotor.disableAirControlUntilCollision = true;
				((BaseCharacterController)base.characterMotor).Motor.ForceUnground();
				base.characterDirection.forward = val3;
			}
		}
	}

	public override void FixedUpdate()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		if (Object.op_Implicit((Object)(object)targetTransform) && Object.op_Implicit((Object)(object)base.characterMotor))
		{
			Vector3 val = targetTransform.position - base.transform.position;
			Vector3 velocity = base.characterMotor.velocity;
			velocity = Vector3.RotateTowards(velocity, val, velocityTurnRate * Time.fixedDeltaTime * (MathF.PI / 180f), 0f);
			base.characterMotor.velocity = velocity;
			if (NetworkServer.active && attack.Fire(victimsStruck))
			{
				for (int i = 0; i < victimsStruck.Count; i++)
				{
					HealthComponent obj = victimsStruck[i].healthComponent;
					CharacterBody body = obj.body;
					CharacterMaster master = body.master;
					if (obj.alive && (Object)(object)master != (Object)null && !body.isPlayerControlled && !body.bodyFlags.HasFlag(CharacterBody.BodyFlags.Mechanical))
					{
						master.teamIndex = TeamIndex.Void;
						body.teamComponent.teamIndex = TeamIndex.Void;
						body.inventory.SetEquipmentIndex(DLC1Content.Elites.Void.eliteEquipmentDef.equipmentIndex);
						BaseAI component = ((Component)master).GetComponent<BaseAI>();
						if (Object.op_Implicit((Object)(object)component))
						{
							component.enemyAttention = 0f;
							component.ForceAcquireNearestEnemyIfNoCurrentEnemy();
						}
						base.healthComponent.Suicide();
						if (Object.op_Implicit((Object)(object)successfulInfestEffectPrefab))
						{
							EffectManager.SimpleImpactEffect(successfulInfestEffectPrefab, base.transform.position, Vector3.up, transmit: false);
						}
						break;
					}
				}
			}
		}
		if (Object.op_Implicit((Object)(object)base.characterDirection))
		{
			base.characterDirection.moveVector = base.characterMotor.velocity;
		}
		if (base.isAuthority && Object.op_Implicit((Object)(object)base.characterMotor) && base.characterMotor.isGrounded && base.fixedAge > 0.1f)
		{
			outer.SetNextStateToMain();
		}
	}

	public override void OnExit()
	{
		if (Object.op_Implicit((Object)(object)infestVfxInstance))
		{
			EntityState.Destroy((Object)(object)infestVfxInstance);
		}
		base.OnExit();
	}
}
