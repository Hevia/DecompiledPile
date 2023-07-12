using System;
using UnityEngine;

namespace RoR2;

[RequireComponent(typeof(CharacterModel))]
[RequireComponent(typeof(Animator))]
public class SkateSparks : MonoBehaviour
{
	[Serializable]
	private struct FootState
	{
		public float? speed;

		public Vector3? position;

		public float stressAccumulator;

		public float emissionTimer;

		public float debugAcceleration;

		public float debugOverspeed;
	}

	public float maxStress = 4f;

	public float minStressForEmission = 1f;

	public float overspeedStressCoefficient = 1f;

	public float accelerationStressCoefficient = 1f;

	public float perpendicularTravelStressCoefficient = 1f;

	public float maxEmissionRate = 10f;

	public int landingStress = 4;

	public ParticleSystem leftParticleSystem;

	public ParticleSystem rightParticleSystem;

	private Animator animator;

	private Transform leftFoot;

	private Transform rightFoot;

	private CharacterModel characterModel;

	private static readonly int isGroundedParam = Animator.StringToHash("isGrounded");

	private bool previousIsGrounded = true;

	private FootState leftFootState;

	private FootState rightFootState;

	private float debugWalkSpeed;

	private void Awake()
	{
		animator = ((Component)this).GetComponent<Animator>();
		characterModel = ((Component)this).GetComponent<CharacterModel>();
		leftFoot = (Object.op_Implicit((Object)(object)leftParticleSystem) ? ((Component)leftParticleSystem).transform : null);
		rightFoot = (Object.op_Implicit((Object)(object)rightParticleSystem) ? ((Component)rightParticleSystem).transform : null);
	}

	private static void UpdateFoot(ref FootState footState, ParticleSystem particleSystem, Transform transform, float walkSpeed, float overspeedStressCoefficient, float accelerationStressCoefficient, float perpendicularTravelStressCoefficient, bool isGrounded, float maxStress, float minStressForEmission, float maxEmissionRate, float deltaTime)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		Vector3? position = (Object.op_Implicit((Object)(object)transform) ? new Vector3?(transform.position) : null);
		float? speed = null;
		Vector3 val = Vector3.zero;
		if (position.HasValue && footState.position.HasValue)
		{
			val = position.Value - footState.position.Value;
			val.y = 0f;
		}
		float magnitude = ((Vector3)(ref val)).magnitude;
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		if (deltaTime != 0f)
		{
			speed = magnitude / deltaTime;
			if (footState.speed.HasValue)
			{
				num = speed.Value - footState.speed.Value;
			}
			num2 = Mathf.Max(speed.Value - walkSpeed, 0f);
			if (Object.op_Implicit((Object)(object)transform) && magnitude > 0f)
			{
				Vector3 val2 = val / magnitude;
				num3 = Mathf.Abs(Vector3.Dot(transform.right, val2)) * magnitude;
			}
			footState.stressAccumulator += num2 * overspeedStressCoefficient * deltaTime + num * accelerationStressCoefficient + num3 * perpendicularTravelStressCoefficient;
			footState.debugAcceleration = num;
			footState.debugOverspeed = num2;
		}
		if (!isGrounded)
		{
			footState.stressAccumulator = 0f;
		}
		footState.stressAccumulator = Mathf.Clamp(footState.stressAccumulator, 0f, maxStress);
		footState.emissionTimer -= deltaTime * maxEmissionRate;
		if (footState.emissionTimer <= 0f)
		{
			footState.emissionTimer = 1f;
			footState.stressAccumulator -= 1f;
			if (footState.stressAccumulator >= minStressForEmission && Object.op_Implicit((Object)(object)particleSystem))
			{
				particleSystem.Emit(1);
			}
		}
		footState.position = position;
		footState.speed = speed;
	}

	private void Update()
	{
		bool flag = false;
		float num = 0f;
		CharacterBody body = characterModel.body;
		if (Object.op_Implicit((Object)(object)body))
		{
			num = body.moveSpeed;
			if (body.isSprinting)
			{
				num /= body.sprintingSpeedMultiplier;
			}
			CharacterMotor characterMotor = body.characterMotor;
			if (Object.op_Implicit((Object)(object)characterMotor))
			{
				flag = characterMotor.isGrounded;
			}
		}
		if (flag != previousIsGrounded)
		{
			float num2 = landingStress;
			leftFootState.stressAccumulator += num2;
			rightFootState.stressAccumulator += num2;
		}
		float deltaTime = Time.deltaTime;
		UpdateFoot(ref leftFootState, leftParticleSystem, leftFoot, num, overspeedStressCoefficient, accelerationStressCoefficient, perpendicularTravelStressCoefficient, flag, maxStress, minStressForEmission, maxEmissionRate, deltaTime);
		UpdateFoot(ref rightFootState, rightParticleSystem, rightFoot, num, overspeedStressCoefficient, accelerationStressCoefficient, perpendicularTravelStressCoefficient, flag, maxStress, minStressForEmission, maxEmissionRate, deltaTime);
		debugWalkSpeed = num;
		previousIsGrounded = flag;
	}
}
