using UnityEngine;

namespace EntityStates.Vulture;

public class FallingDeath : GenericCharacterDeath
{
	private Animator animator;

	public override void OnEnter()
	{
		base.OnEnter();
		if (Object.op_Implicit((Object)(object)base.characterMotor))
		{
			base.characterMotor.velocity.y = 0f;
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (Object.op_Implicit((Object)(object)animator))
		{
			animator.SetBool("isGrounded", base.characterMotor.isGrounded);
			return;
		}
		animator = GetModelAnimator();
		if (Object.op_Implicit((Object)(object)animator))
		{
			int layerIndex = animator.GetLayerIndex("FlyOverride");
			animator.SetLayerWeight(layerIndex, 0f);
		}
	}
}
