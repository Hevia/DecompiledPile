using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Commando;

public class DeathState : GenericCharacterDeath
{
	private Vector3 previousPosition;

	private float upSpeedVelocity;

	private float upSpeed;

	private Animator modelAnimator;

	protected override bool shouldAutoDestroy => false;

	public override void OnEnter()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		Vector3 val = Vector3.up * 3f;
		if (Object.op_Implicit((Object)(object)base.characterMotor))
		{
			val += base.characterMotor.velocity;
			((Behaviour)base.characterMotor).enabled = false;
		}
		if (Object.op_Implicit((Object)(object)base.cachedModelTransform))
		{
			RagdollController component = ((Component)base.cachedModelTransform).GetComponent<RagdollController>();
			if (Object.op_Implicit((Object)(object)component))
			{
				component.BeginRagdoll(val);
			}
		}
	}

	protected override void PlayDeathAnimation(float crossfadeDuration = 0.1f)
	{
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (NetworkServer.active && base.fixedAge > 4f)
		{
			EntityState.Destroy((Object)(object)base.gameObject);
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Death;
	}
}
