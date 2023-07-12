using System;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.AncientWispMonster;

public class Throw : BaseState
{
	public static float baseDuration = 3.5f;

	public static float returnToIdlePercentage;

	public static float damageCoefficient = 4f;

	public static float forceMagnitude = 16f;

	public static float radius = 3f;

	public static GameObject projectilePrefab;

	public static GameObject swingEffectPrefab;

	private Transform rightMuzzleTransform;

	private Animator modelAnimator;

	private float duration;

	private bool hasSwung;

	public override void OnEnter()
	{
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		modelAnimator = GetModelAnimator();
		Transform modelTransform = GetModelTransform();
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			ChildLocator component = ((Component)modelTransform).GetComponent<ChildLocator>();
			if (Object.op_Implicit((Object)(object)component))
			{
				rightMuzzleTransform = component.FindChild("MuzzleRight");
			}
		}
		if (Object.op_Implicit((Object)(object)modelAnimator))
		{
			int layerIndex = modelAnimator.GetLayerIndex("Gesture");
			AnimatorStateInfo currentAnimatorStateInfo = modelAnimator.GetCurrentAnimatorStateInfo(layerIndex);
			if (((AnimatorStateInfo)(ref currentAnimatorStateInfo)).IsName("Throw1"))
			{
				PlayCrossfade("Gesture", "Throw2", "Throw.playbackRate", duration / (1f - returnToIdlePercentage), 0.2f);
			}
			else
			{
				PlayCrossfade("Gesture", "Throw1", "Throw.playbackRate", duration / (1f - returnToIdlePercentage), 0.2f);
			}
		}
		if (Object.op_Implicit((Object)(object)base.characterBody))
		{
			base.characterBody.SetAimTimer(2f);
		}
	}

	public override void FixedUpdate()
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		if (NetworkServer.active && Object.op_Implicit((Object)(object)modelAnimator) && modelAnimator.GetFloat("Throw.activate") > 0f && !hasSwung)
		{
			Ray aimRay = GetAimRay();
			Vector3 forward = ((Ray)(ref aimRay)).direction;
			RaycastHit val = default(RaycastHit);
			if (Physics.Raycast(aimRay, ref val, (float)LayerMask.op_Implicit(LayerIndex.world.mask)))
			{
				forward = ((RaycastHit)(ref val)).point - rightMuzzleTransform.position;
			}
			ProjectileManager.instance.FireProjectile(projectilePrefab, rightMuzzleTransform.position, Util.QuaternionSafeLookRotation(forward), base.gameObject, damageStat * damageCoefficient, forceMagnitude, Util.CheckRoll(critStat, base.characterBody.master));
			EffectManager.SimpleMuzzleFlash(swingEffectPrefab, base.gameObject, "RightSwingCenter", transmit: true);
			hasSwung = true;
		}
		if (base.fixedAge >= duration && base.isAuthority)
		{
			outer.SetNextStateToMain();
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.PrioritySkill;
	}

	private static void PullEnemies(Vector3 position, Vector3 direction, float coneAngle, float maxDistance, float force, TeamIndex excludedTeam)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Cos(coneAngle * 0.5f * (MathF.PI / 180f));
		Collider[] array = Physics.OverlapSphere(position, maxDistance);
		foreach (Collider val in array)
		{
			Vector3 position2 = ((Component)val).transform.position;
			Vector3 val2 = position - position2;
			Vector3 normalized = ((Vector3)(ref val2)).normalized;
			if (!(Vector3.Dot(-normalized, direction) >= num))
			{
				continue;
			}
			TeamIndex teamIndex = TeamIndex.Neutral;
			TeamComponent component = ((Component)val).GetComponent<TeamComponent>();
			if (!Object.op_Implicit((Object)(object)component))
			{
				continue;
			}
			teamIndex = component.teamIndex;
			if (teamIndex != excludedTeam)
			{
				CharacterMotor component2 = ((Component)val).GetComponent<CharacterMotor>();
				if (Object.op_Implicit((Object)(object)component2))
				{
					component2.ApplyForce(normalized * force);
				}
				Rigidbody component3 = ((Component)val).GetComponent<Rigidbody>();
				if (Object.op_Implicit((Object)(object)component3))
				{
					component3.AddForce(normalized * force, (ForceMode)1);
				}
			}
		}
	}
}
