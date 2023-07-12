using RoR2;
using UnityEngine;

namespace EntityStates.Barrel;

public class OpeningLunar : BaseState
{
	public static float duration = 1f;

	public override void OnEnter()
	{
		base.OnEnter();
		PlayAnimation("Body", "Opening", "Opening.playbackRate", duration);
		if (Object.op_Implicit((Object)(object)base.sfxLocator))
		{
			Util.PlaySound(base.sfxLocator.openSound, base.gameObject);
		}
		StopSteamEffect();
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.fixedAge >= duration)
		{
			outer.SetNextState(new Opened());
		}
	}

	private void StopSteamEffect()
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		Transform modelTransform = GetModelTransform();
		if (!Object.op_Implicit((Object)(object)modelTransform))
		{
			return;
		}
		ChildLocator component = ((Component)modelTransform).GetComponent<ChildLocator>();
		if (!Object.op_Implicit((Object)(object)component))
		{
			return;
		}
		Transform val = component.FindChild("SteamEffect");
		if (Object.op_Implicit((Object)(object)val))
		{
			ParticleSystem component2 = ((Component)val).GetComponent<ParticleSystem>();
			if (Object.op_Implicit((Object)(object)component2))
			{
				MainModule main = component2.main;
				((MainModule)(ref main)).loop = false;
			}
		}
	}
}
