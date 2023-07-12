using RoR2;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.VoidRaidCrab.Weapon;

public class FireGravityBump : BaseGravityBumpState
{
	[SerializeField]
	public float baseDuration;

	[SerializeField]
	public string muzzleName;

	[SerializeField]
	public GameObject muzzleFlashPrefab;

	[SerializeField]
	public bool disableAirControlUntilCollision;

	[SerializeField]
	public GameObject airborneEffectPrefab;

	[SerializeField]
	public GameObject groundedEffectPrefab;

	[SerializeField]
	public string enterSoundString;

	[SerializeField]
	public bool isSoundScaledByAttackSpeed;

	[SerializeField]
	public string leftAnimationLayerName;

	[SerializeField]
	public string leftAnimationStateName;

	[SerializeField]
	public string leftAnimationPlaybackRateParam;

	[SerializeField]
	public string rightAnimationLayerName;

	[SerializeField]
	public string rightAnimationStateName;

	[SerializeField]
	public string rightAnimationPlaybackRateParam;

	[SerializeField]
	public SkillDef skillDefToReplaceAtStocksEmpty;

	[SerializeField]
	public SkillDef nextSkillDef;

	private float duration;

	public override void OnEnter()
	{
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		if (Object.op_Implicit((Object)(object)nextSkillDef))
		{
			GenericSkill genericSkill = base.skillLocator.FindSkillByDef(skillDefToReplaceAtStocksEmpty);
			if (Object.op_Implicit((Object)(object)genericSkill) && genericSkill.stock == 0)
			{
				genericSkill.SetBaseSkill(nextSkillDef);
			}
		}
		if (isLeft)
		{
			PlayAnimation(leftAnimationLayerName, leftAnimationStateName, leftAnimationPlaybackRateParam, duration);
		}
		else
		{
			PlayAnimation(rightAnimationLayerName, rightAnimationStateName, rightAnimationPlaybackRateParam, duration);
		}
		if (Object.op_Implicit((Object)(object)muzzleFlashPrefab))
		{
			EffectManager.SimpleMuzzleFlash(muzzleFlashPrefab, base.gameObject, muzzleName, transmit: false);
		}
		if (!NetworkServer.active)
		{
			return;
		}
		BullseyeSearch bullseyeSearch = new BullseyeSearch();
		bullseyeSearch.viewer = base.characterBody;
		bullseyeSearch.teamMaskFilter = TeamMask.GetEnemyTeams(base.characterBody.teamComponent.teamIndex);
		bullseyeSearch.minDistanceFilter = 0f;
		bullseyeSearch.maxDistanceFilter = maxDistance;
		bullseyeSearch.searchOrigin = base.inputBank.aimOrigin;
		bullseyeSearch.searchDirection = base.inputBank.aimDirection;
		bullseyeSearch.maxAngleFilter = 360f;
		bullseyeSearch.filterByLoS = false;
		bullseyeSearch.filterByDistinctEntity = true;
		bullseyeSearch.RefreshCandidates();
		foreach (HurtBox result in bullseyeSearch.GetResults())
		{
			GameObject val = ((Component)result.healthComponent).gameObject;
			if (!Object.op_Implicit((Object)(object)val))
			{
				continue;
			}
			CharacterMotor component = val.GetComponent<CharacterMotor>();
			if (Object.op_Implicit((Object)(object)component))
			{
				EffectData effectData = new EffectData
				{
					origin = val.transform.position
				};
				GameObject effectPrefab;
				if (component.isGrounded)
				{
					component.ApplyForce(groundedForce, alwaysApply: true, disableAirControlUntilCollision);
					effectPrefab = groundedEffectPrefab;
					effectData.rotation = Util.QuaternionSafeLookRotation(groundedForce);
				}
				else
				{
					component.ApplyForce(airborneForce, alwaysApply: true, disableAirControlUntilCollision);
					effectPrefab = airborneEffectPrefab;
					effectData.rotation = Util.QuaternionSafeLookRotation(airborneForce);
				}
				EffectManager.SpawnEffect(effectPrefab, effectData, transmit: true);
			}
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.isAuthority && base.fixedAge >= duration)
		{
			outer.SetNextStateToMain();
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.PrioritySkill;
	}
}
