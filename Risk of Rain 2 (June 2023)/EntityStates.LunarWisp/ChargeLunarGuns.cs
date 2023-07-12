using RoR2;
using UnityEngine;

namespace EntityStates.LunarWisp;

public class ChargeLunarGuns : BaseState
{
	public static string muzzleNameRoot;

	public static string muzzleNameOne;

	public static string muzzleNameTwo;

	public static float baseDuration;

	public static string windUpSound;

	public static GameObject chargeEffectPrefab;

	private GameObject chargeInstance;

	private GameObject chargeInstanceTwo;

	private float duration;

	public static float spinUpDuration;

	public static float chargeEffectDelay;

	private bool chargeEffectSpawned;

	private bool upToSpeed;

	private uint loopedSoundID;

	protected Transform muzzleTransformRoot;

	protected Transform muzzleTransformOne;

	protected Transform muzzleTransformTwo;

	public override void OnEnter()
	{
		base.OnEnter();
		duration = (baseDuration + spinUpDuration) / attackSpeedStat;
		muzzleTransformRoot = FindModelChild(muzzleNameRoot);
		muzzleTransformOne = FindModelChild(muzzleNameOne);
		muzzleTransformTwo = FindModelChild(muzzleNameTwo);
		loopedSoundID = Util.PlaySound(windUpSound, base.gameObject);
		PlayCrossfade("Gesture", "MinigunSpinUp", 0.2f);
		if (Object.op_Implicit((Object)(object)base.characterBody))
		{
			base.characterBody.SetAimTimer(duration);
		}
	}

	public override void FixedUpdate()
	{
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		StartAimMode(0.5f);
		if (base.fixedAge >= chargeEffectDelay && !chargeEffectSpawned)
		{
			chargeEffectSpawned = true;
			if (Object.op_Implicit((Object)(object)muzzleTransformOne) && Object.op_Implicit((Object)(object)muzzleTransformTwo) && Object.op_Implicit((Object)(object)chargeEffectPrefab))
			{
				chargeInstance = Object.Instantiate<GameObject>(chargeEffectPrefab, muzzleTransformOne.position, muzzleTransformOne.rotation);
				chargeInstance.transform.parent = muzzleTransformOne;
				chargeInstanceTwo = Object.Instantiate<GameObject>(chargeEffectPrefab, muzzleTransformTwo.position, muzzleTransformTwo.rotation);
				chargeInstanceTwo.transform.parent = muzzleTransformTwo;
				ScaleParticleSystemDuration component = chargeInstance.GetComponent<ScaleParticleSystemDuration>();
				if (Object.op_Implicit((Object)(object)component))
				{
					component.newDuration = duration;
				}
			}
		}
		if (base.fixedAge >= spinUpDuration && !upToSpeed)
		{
			upToSpeed = true;
		}
		if (base.fixedAge >= duration && base.isAuthority)
		{
			FireLunarGuns fireLunarGuns = new FireLunarGuns();
			fireLunarGuns.muzzleTransformOne = muzzleTransformOne;
			fireLunarGuns.muzzleTransformTwo = muzzleTransformTwo;
			fireLunarGuns.muzzleNameOne = muzzleNameOne;
			fireLunarGuns.muzzleNameTwo = muzzleNameTwo;
			outer.SetNextState(fireLunarGuns);
		}
	}

	public override void OnExit()
	{
		base.OnExit();
		AkSoundEngine.StopPlayingID(loopedSoundID);
		if (Object.op_Implicit((Object)(object)chargeInstance))
		{
			EntityState.Destroy((Object)(object)chargeInstance);
		}
		if (Object.op_Implicit((Object)(object)chargeInstanceTwo))
		{
			EntityState.Destroy((Object)(object)chargeInstanceTwo);
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.PrioritySkill;
	}
}
