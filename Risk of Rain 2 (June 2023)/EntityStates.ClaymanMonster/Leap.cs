using RoR2;
using UnityEngine;

namespace EntityStates.ClaymanMonster;

public class Leap : BaseState
{
	public static string leapSoundString;

	public static float minimumDuration;

	public static float verticalJumpSpeed;

	public static float horizontalJumpSpeedCoefficient;

	private Vector3 forwardDirection;

	private Animator animator;

	private float stopwatch;

	private bool playedImpact;

	public override void OnEnter()
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		animator = GetModelAnimator();
		Util.PlaySound(leapSoundString, base.gameObject);
		if (Object.op_Implicit((Object)(object)base.characterMotor))
		{
			base.characterMotor.velocity.y = verticalJumpSpeed;
		}
		forwardDirection = Vector3.ProjectOnPlane(base.inputBank.aimDirection, Vector3.up);
		base.characterDirection.moveVector = forwardDirection;
		PlayCrossfade("Body", "LeapAirLoop", 0.15f);
	}

	public override void FixedUpdate()
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		stopwatch += Time.fixedDeltaTime;
		animator.SetFloat("Leap.cycle", Mathf.Clamp01(Util.Remap(base.characterMotor.velocity.y, 0f - verticalJumpSpeed, verticalJumpSpeed, 1f, 0f)));
		Vector3 velocity = forwardDirection * base.characterBody.moveSpeed * horizontalJumpSpeedCoefficient;
		velocity.y = base.characterMotor.velocity.y;
		base.characterMotor.velocity = velocity;
		base.FixedUpdate();
		if (base.characterMotor.isGrounded && stopwatch > minimumDuration && !playedImpact)
		{
			playedImpact = true;
			int layerIndex = animator.GetLayerIndex("Impact");
			if (layerIndex >= 0)
			{
				animator.SetLayerWeight(layerIndex, 1.5f);
				animator.PlayInFixedTime("LightImpact", layerIndex, 0f);
			}
			if (base.isAuthority)
			{
				outer.SetNextStateToMain();
			}
		}
	}

	public override void OnExit()
	{
		PlayAnimation("Body", "Idle");
		base.OnExit();
	}
}
