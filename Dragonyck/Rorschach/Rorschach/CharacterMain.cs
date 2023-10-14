using EntityStates;
using RoR2;
using RoR2.UI;
using UnityEngine;

namespace Rorschach;

internal class CharacterMain : GenericCharacterMain
{
	private static RorschachRageBarBehaviour behaviour;

	public override void OnEnter()
	{
		((GenericCharacterMain)this).OnEnter();
		behaviour = ((EntityState)this).gameObject.GetComponent<RorschachRageBarBehaviour>();
	}

	public override void FixedUpdate()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		((GenericCharacterMain)this).FixedUpdate();
		if (buttonPressed(MainPlugin.emote1Key.Value))
		{
			((EntityState)this).outer.SetNextState((EntityState)(object)new Emote1());
		}
		if (buttonPressed(MainPlugin.emote2Key.Value))
		{
			((EntityState)this).outer.SetNextState((EntityState)(object)new Emote2());
		}
	}

	internal static void AddRage(float amount)
	{
		if (Object.op_Implicit((Object)(object)behaviour))
		{
			behaviour.AddRage(amount);
		}
	}

	private bool buttonPressed(KeyCode key)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		if (PauseScreenController.instancesList.Count == 0 && ((BaseState)this).isGrounded && Util.HasEffectiveAuthority(((EntityState)this).characterBody.networkIdentity) && Input.GetKeyDown(key))
		{
			return true;
		}
		return false;
	}
}
