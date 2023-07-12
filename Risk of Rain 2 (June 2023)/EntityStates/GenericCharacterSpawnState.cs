using RoR2;
using UnityEngine;

namespace EntityStates;

public abstract class GenericCharacterSpawnState : BaseState
{
	[SerializeField]
	public float duration = 2f;

	[SerializeField]
	public string spawnSoundString;

	public override void OnEnter()
	{
		base.OnEnter();
		Util.PlaySound(spawnSoundString, base.gameObject);
		PlayAnimation("Body", "Spawn1", "Spawn1.playbackRate", duration);
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.fixedAge >= duration && base.isAuthority)
		{
			outer.SetNextStateToMain();
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Death;
	}
}
