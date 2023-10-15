using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.LunarGolem;

public class FireTwinShots : BaseState
{
	public static GameObject projectilePrefab;

	public static GameObject effectPrefab;

	public static GameObject dustEffectPrefab;

	public static GameObject hitEffectPrefab;

	public static GameObject tracerEffectPrefab;

	public static float damageCoefficient;

	public static float blastRadius;

	public static float force;

	public static float baseDuration = 2f;

	public static string attackSoundString;

	public static float aimTime = 2f;

	public static string leftMuzzleTop;

	public static string rightMuzzleTop;

	public static string leftMuzzleBot;

	public static string rightMuzzleBot;

	public static int refireCount = 6;

	public static float baseAimDelay = 0.1f;

	public static float minLeadTime = 2f;

	public static float maxLeadTime = 2f;

	public static float fireSoundPlaybackRate;

	public static bool useSeriesFire = true;

	private int refireIndex;

	private Ray initialAimRay;

	private bool fired;

	private float aimDelay;

	private float duration;

	public override void OnEnter()
	{
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		aimDelay = baseAimDelay / attackSpeedStat;
		if (Object.op_Implicit((Object)(object)base.characterBody))
		{
			base.characterBody.SetAimTimer(aimTime);
		}
		if (!useSeriesFire)
		{
			PlayAnimation("Gesture, Additive", "FireTwinShot", "TwinShot.playbackRate", duration);
		}
		else
		{
			PlayAnimation("Gesture, Additive", "BufferEmpty");
		}
		Util.PlayAttackSpeedSound(attackSoundString, base.gameObject, fireSoundPlaybackRate);
		initialAimRay = GetAimRay();
	}

	public override void OnExit()
	{
		base.OnExit();
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (!fired && aimDelay <= base.fixedAge)
		{
			fired = true;
			if (base.isAuthority)
			{
				Fire();
			}
		}
		if (base.fixedAge >= duration && base.isAuthority)
		{
			if (refireIndex < refireCount)
			{
				outer.SetNextState(new FireTwinShots
				{
					refireIndex = refireIndex + 1
				});
			}
			else
			{
				outer.SetNextStateToMain();
			}
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.PrioritySkill;
	}

	public override void OnSerialize(NetworkWriter writer)
	{
		base.OnSerialize(writer);
		writer.WritePackedUInt32((uint)refireIndex);
	}

	public override void OnDeserialize(NetworkReader reader)
	{
		base.OnDeserialize(reader);
		refireIndex = (int)reader.ReadPackedUInt32();
	}

	private void Fire()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		Ray aimRay = GetAimRay();
		Quaternion val = Quaternion.LookRotation(((Ray)(ref initialAimRay)).direction);
		Quaternion val2 = Quaternion.LookRotation(aimRay.direction);
		float num = Util.Remap(Util.Remap(refireIndex, 0f, refireCount - 1, 0f, 1f), 0f, 1f, minLeadTime, maxLeadTime) / aimDelay;
		Quaternion val3 = Quaternion.SlerpUnclamped(val, val2, 1f + num);
		Ray val4 = default(Ray);
		((Ray)(ref val4))._002Ector(aimRay.origin, val3 * Vector3.forward);
		if (refireIndex == 0 && Object.op_Implicit((Object)(object)dustEffectPrefab))
		{
			EffectManager.SimpleMuzzleFlash(dustEffectPrefab, base.gameObject, "Root", transmit: false);
		}
		int num2 = refireIndex;
		if (!useSeriesFire)
		{
			num2 = refireIndex + 3;
		}
		while (refireIndex <= num2)
		{
			string muzzleName = "";
			bool flipProjectile = false;
			switch (refireIndex % 4)
			{
			case 0:
				muzzleName = rightMuzzleTop;
				PlayAnimation("Gesture, Right Additive", "FireRightShot");
				flipProjectile = true;
				break;
			case 1:
				muzzleName = leftMuzzleTop;
				PlayAnimation("Gesture, Left Additive", "FireLeftShot");
				break;
			case 2:
				muzzleName = rightMuzzleBot;
				PlayAnimation("Gesture, Right Additive", "FireRightShot");
				flipProjectile = true;
				break;
			case 3:
				muzzleName = leftMuzzleBot;
				PlayAnimation("Gesture, Left Additive", "FireLeftShot");
				break;
			}
			FireSingle(muzzleName, ((Ray)(ref val4)).direction, flipProjectile);
			refireIndex++;
		}
		refireIndex--;
	}

	private void FireSingle(string muzzleName, Vector3 aimDirection, bool flipProjectile)
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		ChildLocator modelChildLocator = GetModelChildLocator();
		if (!Object.op_Implicit((Object)(object)modelChildLocator))
		{
			return;
		}
		Transform val = modelChildLocator.FindChild(muzzleName);
		if (Object.op_Implicit((Object)(object)val))
		{
			if (Object.op_Implicit((Object)(object)effectPrefab))
			{
				EffectManager.SimpleMuzzleFlash(effectPrefab, base.gameObject, muzzleName, transmit: false);
			}
			if (base.isAuthority)
			{
				ProjectileManager.instance.FireProjectile(projectilePrefab, val.position, Util.QuaternionSafeLookRotation(aimDirection, (!flipProjectile) ? Vector3.up : Vector3.down), base.gameObject, damageStat * damageCoefficient, force, RollCrit());
			}
		}
	}
}
