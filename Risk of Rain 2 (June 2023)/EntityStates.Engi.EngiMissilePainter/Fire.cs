using System.Collections.Generic;
using RoR2;
using UnityEngine;

namespace EntityStates.Engi.EngiMissilePainter;

public class Fire : BaseEngiMissilePainterState
{
	public static float baseDurationPerMissile;

	public static float damageCoefficient;

	public static GameObject projectilePrefab;

	public static GameObject muzzleflashEffectPrefab;

	public List<HurtBox> targetsList;

	private int fireIndex;

	private float durationPerMissile;

	private float stopwatch;

	public override void OnEnter()
	{
		base.OnEnter();
		durationPerMissile = baseDurationPerMissile / attackSpeedStat;
		PlayAnimation("Gesture, Additive", "IdleHarpoons");
	}

	public override void FixedUpdate()
	{
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		bool flag = false;
		if (base.isAuthority)
		{
			stopwatch += Time.fixedDeltaTime;
			if (stopwatch >= durationPerMissile)
			{
				stopwatch -= durationPerMissile;
				while (fireIndex < targetsList.Count)
				{
					HurtBox hurtBox = targetsList[fireIndex++];
					if (!Object.op_Implicit((Object)(object)hurtBox.healthComponent) || !hurtBox.healthComponent.alive)
					{
						base.activatorSkillSlot.AddOneStock();
						continue;
					}
					string text = ((fireIndex % 2 == 0) ? "MuzzleLeft" : "MuzzleRight");
					Vector3 position = base.inputBank.aimOrigin;
					Transform val = FindModelChild(text);
					if ((Object)(object)val != (Object)null)
					{
						position = val.position;
					}
					EffectManager.SimpleMuzzleFlash(muzzleflashEffectPrefab, base.gameObject, text, transmit: true);
					FireMissile(hurtBox, position);
					flag = true;
					break;
				}
				if (fireIndex >= targetsList.Count)
				{
					outer.SetNextState(new Finish());
				}
			}
		}
		if (flag)
		{
			PlayAnimation((fireIndex % 2 == 0) ? "Gesture Left Cannon, Additive" : "Gesture Right Cannon, Additive", "FireHarpoon");
		}
	}

	private void FireMissile(HurtBox target, Vector3 position)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		MissileUtils.FireMissile(base.inputBank.aimOrigin, base.characterBody, default(ProcChainMask), ((Component)target).gameObject, damageStat * damageCoefficient, RollCrit(), projectilePrefab, DamageColorIndex.Default, Vector3.up, 0f, addMissileProc: false);
	}

	public override void OnExit()
	{
		base.OnExit();
		PlayCrossfade("Gesture, Additive", "ExitHarpoons", 0.1f);
	}
}
