using System.Collections.Generic;
using System.Linq;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.ClayBoss;

public class Recover : BaseState
{
	private enum SubState
	{
		Entry,
		Tethers
	}

	public static float duration = 15f;

	public static float maxTetherDistance = 40f;

	public static float tetherMulchDistance = 5f;

	public static float tetherMulchDamageScale = 2f;

	public static float tetherMulchTickIntervalScale = 0.5f;

	public static float damagePerSecond = 2f;

	public static float damageTickFrequency = 3f;

	public static float entryDuration = 1f;

	public static GameObject mulchEffectPrefab;

	public static string enterSoundString;

	public static string beginMulchSoundString;

	public static string stopMulchSoundString;

	private GameObject mulchEffect;

	private Transform muzzleTransform;

	private List<TarTetherController> tetherControllers;

	private float stopwatch;

	private uint soundID;

	private SubState subState;

	public override void OnEnter()
	{
		base.OnEnter();
		stopwatch = 0f;
		if (NetworkServer.active && Object.op_Implicit((Object)(object)base.characterBody))
		{
			base.characterBody.AddBuff(RoR2Content.Buffs.ArmorBoost);
		}
		if (Object.op_Implicit((Object)(object)base.modelLocator))
		{
			ChildLocator component = ((Component)base.modelLocator.modelTransform).GetComponent<ChildLocator>();
			if (Object.op_Implicit((Object)(object)component))
			{
				muzzleTransform = component.FindChild("MuzzleMulch");
			}
		}
		subState = SubState.Entry;
		PlayCrossfade("Body", "PrepSiphon", "PrepSiphon.playbackRate", entryDuration, 0.1f);
		soundID = Util.PlayAttackSpeedSound(enterSoundString, base.gameObject, attackSpeedStat);
	}

	private void FireTethers()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = muzzleTransform.position;
		float breakDistanceSqr = maxTetherDistance * maxTetherDistance;
		List<GameObject> list = new List<GameObject>();
		tetherControllers = new List<TarTetherController>();
		BullseyeSearch bullseyeSearch = new BullseyeSearch();
		bullseyeSearch.searchOrigin = position;
		bullseyeSearch.maxDistanceFilter = maxTetherDistance;
		bullseyeSearch.teamMaskFilter = TeamMask.allButNeutral;
		bullseyeSearch.sortMode = BullseyeSearch.SortMode.Distance;
		bullseyeSearch.filterByLoS = true;
		bullseyeSearch.searchDirection = Vector3.up;
		bullseyeSearch.RefreshCandidates();
		bullseyeSearch.FilterOutGameObject(base.gameObject);
		List<HurtBox> list2 = bullseyeSearch.GetResults().ToList();
		Debug.Log((object)list2);
		for (int i = 0; i < list2.Count; i++)
		{
			GameObject val = ((Component)list2[i].healthComponent).gameObject;
			if (Object.op_Implicit((Object)(object)val))
			{
				list.Add(val);
			}
		}
		float tickInterval = 1f / damageTickFrequency;
		float damageCoefficientPerTick = damagePerSecond / damageTickFrequency;
		float mulchDistanceSqr = tetherMulchDistance * tetherMulchDistance;
		GameObject val2 = LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/TarTether");
		for (int j = 0; j < list.Count; j++)
		{
			GameObject obj = Object.Instantiate<GameObject>(val2, position, Quaternion.identity);
			TarTetherController component = obj.GetComponent<TarTetherController>();
			component.NetworkownerRoot = base.gameObject;
			component.NetworktargetRoot = list[j];
			component.breakDistanceSqr = breakDistanceSqr;
			component.damageCoefficientPerTick = damageCoefficientPerTick;
			component.tickInterval = tickInterval;
			component.tickTimer = (float)j * 0.1f;
			component.mulchDistanceSqr = mulchDistanceSqr;
			component.mulchDamageScale = tetherMulchDamageScale;
			component.mulchTickIntervalScale = tetherMulchTickIntervalScale;
			tetherControllers.Add(component);
			NetworkServer.Spawn(obj);
		}
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

	public override void OnExit()
	{
		DestroyTethers();
		if (Object.op_Implicit((Object)(object)mulchEffect))
		{
			EntityState.Destroy((Object)(object)mulchEffect);
		}
		AkSoundEngine.StopPlayingID(soundID);
		Util.PlaySound(stopMulchSoundString, base.gameObject);
		if (NetworkServer.active && Object.op_Implicit((Object)(object)base.characterBody))
		{
			base.characterBody.RemoveBuff(RoR2Content.Buffs.ArmorBoost);
		}
		base.OnExit();
	}

	private static void RemoveDeadTethersFromList(List<TarTetherController> tethersList)
	{
		for (int num = tethersList.Count - 1; num >= 0; num--)
		{
			if (!Object.op_Implicit((Object)(object)tethersList[num]))
			{
				tethersList.RemoveAt(num);
			}
		}
	}

	public override void FixedUpdate()
	{
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		stopwatch += Time.fixedDeltaTime;
		if (subState == SubState.Entry)
		{
			if (!(stopwatch >= entryDuration))
			{
				return;
			}
			subState = SubState.Tethers;
			stopwatch = 0f;
			PlayAnimation("Body", "ChannelSiphon");
			Util.PlaySound(beginMulchSoundString, base.gameObject);
			if (!NetworkServer.active)
			{
				return;
			}
			FireTethers();
			mulchEffect = Object.Instantiate<GameObject>(mulchEffectPrefab, muzzleTransform.position, Quaternion.identity);
			ChildLocator component = mulchEffect.gameObject.GetComponent<ChildLocator>();
			if (Object.op_Implicit((Object)(object)component))
			{
				Transform val = component.FindChild("AreaIndicator");
				if (Object.op_Implicit((Object)(object)val))
				{
					val.localScale = new Vector3(maxTetherDistance * 2f, maxTetherDistance * 2f, maxTetherDistance * 2f);
				}
			}
			mulchEffect.transform.parent = muzzleTransform;
		}
		else if (subState == SubState.Tethers && NetworkServer.active)
		{
			RemoveDeadTethersFromList(tetherControllers);
			if ((stopwatch >= duration || tetherControllers.Count == 0) && base.isAuthority)
			{
				outer.SetNextState(new RecoverExit());
			}
		}
	}
}
