using RoR2;
using UnityEngine;

namespace EntityStates.Bandit2.Weapon;

public abstract class BasePrepSidearmRevolverState : BaseSidearmState
{
	[SerializeField]
	public string enterSoundString;

	private Animator animator;

	public override void OnEnter()
	{
		base.OnEnter();
		PlayAnimation("Gesture, Additive", "MainToSide", "MainToSide.playbackRate", duration);
		Util.PlaySound(enterSoundString, base.gameObject);
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.fixedAge > duration)
		{
			outer.SetNextState(GetNextState());
		}
	}

	public override void OnExit()
	{
		base.OnExit();
	}

	protected abstract EntityState GetNextState();
}
