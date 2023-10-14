using EntityStates;
using RoR2;
using RoR2.UI;
using UnityEngine;
using UnityEngine.Networking;

namespace Rorschach;

internal class BaseEmote : BaseState
{
	private float emoteDuration = 3f;

	public string animName = "";

	public KeyCode key;

	public override void OnEnter()
	{
		((BaseState)this).OnEnter();
		((EntityState)this).PlayAnimation("FullBody, Override", animName, "M1", emoteDuration);
		if (NetworkServer.active)
		{
			((EntityState)this).characterBody.AddTimedBuff(Buffs.HiddenInvincibility, 3f);
		}
	}

	public override void FixedUpdate()
	{
		((EntityState)this).FixedUpdate();
		if ((((EntityState)this).fixedAge >= emoteDuration && ((EntityState)this).isAuthority) || button1Pressed())
		{
			((EntityState)this).outer.SetNextStateToMain();
		}
	}

	private bool button1Pressed()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		if (PauseScreenController.instancesList.Count == 0 && ((BaseState)this).isGrounded && Util.HasEffectiveAuthority(((EntityState)this).characterBody.networkIdentity) && Input.GetKeyDown(key))
		{
			return true;
		}
		return false;
	}

	public override void OnExit()
	{
		((EntityState)this).OnExit();
		if (NetworkServer.active)
		{
			((EntityState)this).characterBody.ClearTimedBuffs(Buffs.HiddenInvincibility);
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		return (InterruptPriority)6;
	}
}
