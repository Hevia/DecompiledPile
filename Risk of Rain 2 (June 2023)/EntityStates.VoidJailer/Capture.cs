using System.Collections.Generic;
using System.Linq;
using EntityStates.VoidJailer.Weapon;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.VoidJailer;

public class Capture : BaseState
{
	public static string animationLayerName;

	public static string animationStateName;

	public static string animationPlaybackRateName;

	public static GameObject chargeVfxPrefab;

	[SerializeField]
	public float duration;

	public static GameObject tetherPrefab;

	public static GameObject captureRangeEffect;

	public static float effectScaleCoefficient;

	public static float effectScaleAddition;

	public static string muzzleName;

	public static string tetherOriginName;

	public static float maxTetherDistance;

	public static float tetherReelSpeed;

	public static float damagePerSecond;

	public static float damageTickFrequency;

	public static BuffDef tetherDebuff;

	public static float innerRange;

	public static BuffDef innerRangeDebuff;

	public static float innerRangeDebuffDuration;

	public static GameObject innerRangeDebuffEffect;

	public static bool singleDurationReset;

	public static string captureLoopSoundString;

	private Transform muzzleTransform;

	private List<JailerTetherController> tetherControllers = new List<JailerTetherController>();

	private uint soundID;

	private bool initialized;

	private bool shouldModifyTetherDuration = true;

	private GameObject rangeEffect;

	private GameObject _chargeVfxInstance;

	public override void OnEnter()
	{
		muzzleTransform = FindModelChild(muzzleName);
		if (NetworkServer.active)
		{
			InitializeTethers();
		}
		if (NetworkServer.active && base.isAuthority && tetherControllers.Count == 0)
		{
			outer.SetNextState(new ExitCapture());
			return;
		}
		base.OnEnter();
		duration /= attackSpeedStat;
		FireTethers();
	}

	private void InitializeTethers()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = muzzleTransform.position;
		float breakDistanceSqr = maxTetherDistance * maxTetherDistance;
		tetherControllers = new List<JailerTetherController>();
		BullseyeSearch bullseyeSearch = new BullseyeSearch();
		bullseyeSearch.searchOrigin = position;
		bullseyeSearch.maxDistanceFilter = maxTetherDistance;
		bullseyeSearch.teamMaskFilter = TeamMask.GetEnemyTeams(base.teamComponent.teamIndex);
		bullseyeSearch.sortMode = BullseyeSearch.SortMode.Distance;
		bullseyeSearch.filterByLoS = true;
		bullseyeSearch.searchDirection = Vector3.up;
		bullseyeSearch.RefreshCandidates();
		bullseyeSearch.FilterOutGameObject(base.gameObject);
		List<HurtBox> list = bullseyeSearch.GetResults().ToList();
		for (int i = 0; i < list.Count; i++)
		{
			GameObject val = ((Component)list[i].healthComponent).gameObject;
			if (Object.op_Implicit((Object)(object)val) && !TargetIsTethered(val.GetComponent<CharacterBody>()))
			{
				GameObject obj = Object.Instantiate<GameObject>(tetherPrefab, position, Quaternion.identity, muzzleTransform);
				JailerTetherController component = obj.GetComponent<JailerTetherController>();
				component.NetworkownerRoot = base.gameObject;
				component.Networkorigin = ((Component)muzzleTransform).gameObject;
				component.NetworktargetRoot = val;
				component.breakDistanceSqr = breakDistanceSqr;
				component.damageCoefficientPerTick = damagePerSecond / damageTickFrequency;
				component.tickInterval = 1f / damageTickFrequency;
				component.tickTimer = (float)i * 0.1f;
				component.NetworkreelSpeed = tetherReelSpeed;
				component.SetTetheredBuff(tetherDebuff);
				tetherControllers.Add(component);
				NetworkServer.Spawn(obj);
			}
		}
		initialized = true;
	}

	private static bool TargetIsTethered(CharacterBody characterBody)
	{
		if ((Object)(object)characterBody != (Object)null)
		{
			return characterBody.HasBuff(tetherDebuff);
		}
		return false;
	}

	private void FireTethers()
	{
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		PlayAnimation(animationLayerName, animationStateName, animationPlaybackRateName, duration);
		soundID = Util.PlayAttackSpeedSound(captureLoopSoundString, base.gameObject, attackSpeedStat);
		if (!Object.op_Implicit((Object)(object)muzzleTransform))
		{
			return;
		}
		if (Object.op_Implicit((Object)(object)captureRangeEffect))
		{
			rangeEffect = Object.Instantiate<GameObject>(captureRangeEffect, ((Component)base.characterBody).transform);
			rangeEffect.transform.localScale = effectScaleCoefficient * (maxTetherDistance + effectScaleAddition) * Vector3.one;
			ScaleParticleSystemDuration component = rangeEffect.GetComponent<ScaleParticleSystemDuration>();
			if (Object.op_Implicit((Object)(object)component))
			{
				component.newDuration = duration;
			}
		}
		if (Object.op_Implicit((Object)(object)chargeVfxPrefab))
		{
			_chargeVfxInstance = Object.Instantiate<GameObject>(chargeVfxPrefab, muzzleTransform.position, muzzleTransform.rotation, muzzleTransform);
			ScaleParticleSystemDuration component2 = _chargeVfxInstance.GetComponent<ScaleParticleSystemDuration>();
			if (Object.op_Implicit((Object)(object)component2))
			{
				component2.newDuration = duration;
			}
		}
	}

	public override void FixedUpdate()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		if (initialized)
		{
			UpdateTethers(base.gameObject.transform.position);
			if (tetherControllers.Count == 0 || base.fixedAge >= duration)
			{
				outer.SetNextState(new ExitCapture());
			}
		}
	}

	private void UpdateTethers(Vector3 origin)
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		for (int num = tetherControllers.Count - 1; num >= 0; num--)
		{
			JailerTetherController jailerTetherController = tetherControllers[num];
			if (!Object.op_Implicit((Object)(object)jailerTetherController))
			{
				tetherControllers.RemoveAt(num);
			}
			else
			{
				CharacterBody targetBody = jailerTetherController.GetTargetBody();
				if (!((Object)(object)targetBody == (Object)null) && !targetBody.HasBuff(innerRangeDebuff) && Vector3.Distance(origin, ((Component)targetBody).transform.position) < innerRange)
				{
					float num2 = duration - base.fixedAge;
					targetBody.AddTimedBuff(innerRangeDebuff, innerRangeDebuffDuration);
					if (shouldModifyTetherDuration)
					{
						duration = ((num2 > innerRangeDebuffDuration) ? innerRangeDebuffDuration : (innerRangeDebuffDuration + base.fixedAge));
						PlayAnimation(animationLayerName, animationStateName, animationPlaybackRateName, innerRangeDebuffDuration);
						shouldModifyTetherDuration = !singleDurationReset;
						if (Object.op_Implicit((Object)(object)_chargeVfxInstance))
						{
							ScaleParticleSystemDuration component = _chargeVfxInstance.GetComponent<ScaleParticleSystemDuration>();
							if (Object.op_Implicit((Object)(object)component))
							{
								component.newDuration = duration;
							}
						}
					}
					jailerTetherController.NetworkreelSpeed = 0f;
				}
			}
		}
	}

	public override void OnExit()
	{
		DestroyTethers();
		if (Object.op_Implicit((Object)(object)rangeEffect))
		{
			EntityState.Destroy((Object)(object)rangeEffect);
		}
		if (Object.op_Implicit((Object)(object)_chargeVfxInstance))
		{
			EntityState.Destroy((Object)(object)_chargeVfxInstance);
		}
		AkSoundEngine.StopPlayingID(soundID);
		base.OnExit();
	}

	private void DestroyTethers()
	{
		if (tetherControllers == null)
		{
			return;
		}
		for (int num = tetherControllers.Count - 1; num >= 0; num--)
		{
			if (Object.op_Implicit((Object)(object)tetherControllers[num]))
			{
				EntityState.Destroy((Object)(object)((Component)tetherControllers[num]).gameObject);
			}
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.PrioritySkill;
	}
}
