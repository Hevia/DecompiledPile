using System.Collections.Generic;
using RoR2;
using UnityEngine;

namespace EntityStates.LunarGolem;

public class ChargeTwinShot : BaseState
{
	public static float baseDuration = 3f;

	public static float laserMaxWidth = 0.2f;

	public static GameObject effectPrefab;

	public static string chargeSoundString;

	private float duration;

	private uint chargePlayID;

	private List<GameObject> chargeEffects = new List<GameObject>();

	public override void OnEnter()
	{
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		Transform modelTransform = GetModelTransform();
		chargePlayID = Util.PlayAttackSpeedSound(chargeSoundString, base.gameObject, attackSpeedStat);
		PlayCrossfade("Gesture, Additive", "ChargeTwinShot", "TwinShot.playbackRate", duration, 0.1f);
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			ChildLocator component = ((Component)modelTransform).GetComponent<ChildLocator>();
			if (Object.op_Implicit((Object)(object)component))
			{
				List<Transform> list = new List<Transform>();
				list.Add(component.FindChild("MuzzleLT"));
				list.Add(component.FindChild("MuzzleLB"));
				list.Add(component.FindChild("MuzzleRT"));
				list.Add(component.FindChild("MuzzleRB"));
				if (Object.op_Implicit((Object)(object)effectPrefab))
				{
					for (int i = 0; i < list.Count; i++)
					{
						if (Object.op_Implicit((Object)(object)list[i]))
						{
							GameObject val = Object.Instantiate<GameObject>(effectPrefab, list[i].position, list[i].rotation);
							val.transform.parent = list[i];
							ScaleParticleSystemDuration component2 = val.GetComponent<ScaleParticleSystemDuration>();
							if (Object.op_Implicit((Object)(object)component2))
							{
								component2.newDuration = duration;
							}
							chargeEffects.Add(val);
						}
					}
				}
			}
		}
		if (Object.op_Implicit((Object)(object)base.characterBody))
		{
			base.characterBody.SetAimTimer(duration);
		}
	}

	public override void OnExit()
	{
		AkSoundEngine.StopPlayingID(chargePlayID);
		base.OnExit();
		for (int i = 0; i < chargeEffects.Count; i++)
		{
			if (Object.op_Implicit((Object)(object)chargeEffects[i]))
			{
				EntityState.Destroy((Object)(object)chargeEffects[i]);
			}
		}
	}

	public override void Update()
	{
		base.Update();
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.fixedAge >= duration && base.isAuthority)
		{
			FireTwinShots nextState = new FireTwinShots();
			outer.SetNextState(nextState);
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Skill;
	}
}
