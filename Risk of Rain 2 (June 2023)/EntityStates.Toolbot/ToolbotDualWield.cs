using RoR2;
using UnityEngine;

namespace EntityStates.Toolbot;

public class ToolbotDualWield : ToolbotDualWieldBase
{
	public static string animLayer;

	public static string animStateName;

	public static GameObject coverPrefab;

	public static GameObject coverEjectEffect;

	public static string enterSoundString;

	public static string exitSoundString;

	public static string startLoopSoundString;

	public static string stopLoopSoundString;

	private bool keyReleased;

	private GameObject coverLeftInstance;

	private GameObject coverRightInstance;

	private uint loopSoundID;

	protected override bool shouldAllowPrimarySkills => true;

	public override void OnEnter()
	{
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		PlayAnimation(animLayer, animStateName);
		Util.PlaySound(enterSoundString, base.gameObject);
		loopSoundID = Util.PlaySound(startLoopSoundString, base.gameObject);
		if (Object.op_Implicit((Object)(object)coverPrefab))
		{
			Transform val = FindModelChild("LowerArmL");
			Transform val2 = FindModelChild("LowerArmR");
			if (Object.op_Implicit((Object)(object)val))
			{
				coverLeftInstance = Object.Instantiate<GameObject>(coverPrefab, val.position, val.rotation, val);
			}
			if (Object.op_Implicit((Object)(object)val2))
			{
				coverRightInstance = Object.Instantiate<GameObject>(coverPrefab, val2.position, val2.rotation, val2);
			}
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.isAuthority)
		{
			bool flag = this.IsKeyDownAuthority(base.skillLocator, base.inputBank);
			keyReleased |= !flag;
			if (keyReleased && flag)
			{
				outer.SetNextState(new ToolbotDualWieldEnd
				{
					activatorSkillSlot = base.activatorSkillSlot
				});
			}
		}
	}

	public override void OnExit()
	{
		Util.PlaySound(exitSoundString, base.gameObject);
		Util.PlaySound(stopLoopSoundString, base.gameObject);
		AkSoundEngine.StopPlayingID(loopSoundID);
		if (Object.op_Implicit((Object)(object)coverLeftInstance))
		{
			EffectManager.SimpleMuzzleFlash(coverEjectEffect, base.gameObject, "LowerArmL", transmit: false);
			EntityState.Destroy((Object)(object)coverLeftInstance);
		}
		if (Object.op_Implicit((Object)(object)coverRightInstance))
		{
			EffectManager.SimpleMuzzleFlash(coverEjectEffect, base.gameObject, "LowerArmR", transmit: false);
			EntityState.Destroy((Object)(object)coverRightInstance);
		}
		base.OnExit();
	}
}
