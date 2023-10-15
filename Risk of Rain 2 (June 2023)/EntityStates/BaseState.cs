using System;
using System.Collections.Generic;
using KinematicCharacterController;
using RoR2;
using UnityEngine;

namespace EntityStates;

public class BaseState : EntityState
{
	protected struct HitStopCachedState
	{
		public Vector3 characterVelocity;

		public string playbackName;

		public float playbackRate;
	}

	protected float attackSpeedStat = 1f;

	protected float damageStat;

	protected float critStat;

	protected float moveSpeedStat;

	private const float defaultAimDuration = 2f;

	protected bool isGrounded
	{
		get
		{
			if (Object.op_Implicit((Object)(object)base.characterMotor))
			{
				return base.characterMotor.isGrounded;
			}
			return false;
		}
	}

	public override void OnEnter()
	{
		base.OnEnter();
		if (Object.op_Implicit((Object)(object)base.characterBody))
		{
			attackSpeedStat = base.characterBody.attackSpeed;
			damageStat = base.characterBody.damage;
			critStat = base.characterBody.crit;
			moveSpeedStat = base.characterBody.moveSpeed;
		}
	}

	protected Ray GetAimRay()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)base.inputBank))
		{
			return new Ray(base.inputBank.aimOrigin, base.inputBank.aimDirection);
		}
		return new Ray(base.transform.position, base.transform.forward);
	}

	protected void AddRecoil(float verticalMin, float verticalMax, float horizontalMin, float horizontalMax)
	{
		base.cameraTargetParams.AddRecoil(verticalMin, verticalMax, horizontalMin, horizontalMax);
	}

	public OverlapAttack InitMeleeOverlap(float damageCoefficient, GameObject hitEffectPrefab, Transform modelTransform, string hitboxGroupName)
	{
		OverlapAttack overlapAttack = new OverlapAttack();
		overlapAttack.attacker = base.gameObject;
		overlapAttack.inflictor = base.gameObject;
		overlapAttack.teamIndex = TeamComponent.GetObjectTeam(overlapAttack.attacker);
		overlapAttack.damage = damageCoefficient * damageStat;
		overlapAttack.hitEffectPrefab = hitEffectPrefab;
		overlapAttack.isCrit = RollCrit();
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			overlapAttack.hitBoxGroup = Array.Find(((Component)modelTransform).GetComponents<HitBoxGroup>(), (HitBoxGroup element) => element.groupName == hitboxGroupName);
		}
		return overlapAttack;
	}

	public bool FireMeleeOverlap(OverlapAttack attack, Animator animator, string mecanimHitboxActiveParameter, float forceMagnitude, bool calculateForceVector = true)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		bool result = false;
		if (Object.op_Implicit((Object)(object)animator) && animator.GetFloat(mecanimHitboxActiveParameter) > 0.1f)
		{
			if (calculateForceVector)
			{
				attack.forceVector = base.transform.forward * forceMagnitude;
			}
			result = attack.Fire();
		}
		return result;
	}

	public void SmallHop(CharacterMotor characterMotor, float smallHopVelocity)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)characterMotor))
		{
			((BaseCharacterController)characterMotor).Motor.ForceUnground();
			characterMotor.velocity = new Vector3(characterMotor.velocity.x, Mathf.Max(characterMotor.velocity.y, smallHopVelocity), characterMotor.velocity.z);
		}
	}

	protected HitStopCachedState CreateHitStopCachedState(CharacterMotor characterMotor, Animator animator, string playbackRateAnimationParameter)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		HitStopCachedState result = default(HitStopCachedState);
		result.characterVelocity = new Vector3(characterMotor.velocity.x, Mathf.Max(0f, characterMotor.velocity.y), characterMotor.velocity.z);
		result.playbackName = playbackRateAnimationParameter;
		result.playbackRate = animator.GetFloat(result.playbackName);
		return result;
	}

	protected void ConsumeHitStopCachedState(HitStopCachedState hitStopCachedState, CharacterMotor characterMotor, Animator animator)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		characterMotor.velocity = hitStopCachedState.characterVelocity;
		animator.SetFloat(hitStopCachedState.playbackName, hitStopCachedState.playbackRate);
	}

	protected void StartAimMode(float duration = 2f, bool snap = false)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		StartAimMode(GetAimRay(), duration, snap);
	}

	protected void StartAimMode(Ray aimRay, float duration = 2f, bool snap = false)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)base.characterDirection) && aimRay.direction != Vector3.zero)
		{
			if (snap)
			{
				base.characterDirection.forward = aimRay.direction;
			}
			else
			{
				base.characterDirection.moveVector = aimRay.direction;
			}
		}
		if (Object.op_Implicit((Object)(object)base.characterBody))
		{
			base.characterBody.SetAimTimer(duration);
		}
		if (!Object.op_Implicit((Object)(object)base.modelLocator))
		{
			return;
		}
		Transform modelTransform = base.modelLocator.modelTransform;
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			AimAnimator component = ((Component)modelTransform).GetComponent<AimAnimator>();
			if (Object.op_Implicit((Object)(object)component) && snap)
			{
				component.AimImmediate();
			}
		}
	}

	protected bool RollCrit()
	{
		if (Object.op_Implicit((Object)(object)base.characterBody) && Object.op_Implicit((Object)(object)base.characterBody.master))
		{
			return Util.CheckRoll(critStat, base.characterBody.master);
		}
		return false;
	}

	protected Transform FindModelChild(string childName)
	{
		ChildLocator modelChildLocator = GetModelChildLocator();
		if (Object.op_Implicit((Object)(object)modelChildLocator))
		{
			return modelChildLocator.FindChild(childName);
		}
		return null;
	}

	protected T FindModelChildComponent<T>(string childName)
	{
		ChildLocator modelChildLocator = GetModelChildLocator();
		if (Object.op_Implicit((Object)(object)modelChildLocator))
		{
			return modelChildLocator.FindChildComponent<T>(childName);
		}
		return default(T);
	}

	protected GameObject FindModelChildGameObject(string childName)
	{
		ChildLocator modelChildLocator = GetModelChildLocator();
		if (Object.op_Implicit((Object)(object)modelChildLocator))
		{
			return modelChildLocator.FindChildGameObject(childName);
		}
		return null;
	}

	public TeamIndex GetTeam()
	{
		return TeamComponent.GetObjectTeam(base.gameObject);
	}

	public bool HasBuff(BuffIndex buffType)
	{
		if (Object.op_Implicit((Object)(object)base.characterBody))
		{
			return base.characterBody.HasBuff(buffType);
		}
		return false;
	}

	public bool HasBuff(BuffDef buffType)
	{
		if (Object.op_Implicit((Object)(object)base.characterBody))
		{
			return base.characterBody.HasBuff(buffType);
		}
		return false;
	}

	public int GetBuffCount(BuffIndex buffType)
	{
		if (!Object.op_Implicit((Object)(object)base.characterBody))
		{
			return 0;
		}
		return base.characterBody.GetBuffCount(buffType);
	}

	public int GetBuffCount(BuffDef buffType)
	{
		if (!Object.op_Implicit((Object)(object)base.characterBody))
		{
			return 0;
		}
		return base.characterBody.GetBuffCount(buffType);
	}

	protected void AttemptToStartSprint()
	{
		if (Object.op_Implicit((Object)(object)base.inputBank))
		{
			base.inputBank.sprint.down = true;
		}
	}

	protected HitBoxGroup FindHitBoxGroup(string groupName)
	{
		Transform modelTransform = GetModelTransform();
		if (!Object.op_Implicit((Object)(object)modelTransform))
		{
			return null;
		}
		HitBoxGroup result = null;
		List<HitBoxGroup> gameObjectComponents = GetComponentsCache<HitBoxGroup>.GetGameObjectComponents(((Component)modelTransform).gameObject);
		int i = 0;
		for (int count = gameObjectComponents.Count; i < count; i++)
		{
			if (gameObjectComponents[i].groupName == groupName)
			{
				result = gameObjectComponents[i];
				break;
			}
		}
		GetComponentsCache<HitBoxGroup>.ReturnBuffer(gameObjectComponents);
		return result;
	}
}
