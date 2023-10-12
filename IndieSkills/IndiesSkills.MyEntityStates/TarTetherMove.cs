using System.Collections.Generic;
using System.Linq;
using EntityStates;
using EntityStates.ClayBoss;
using PaladinMod.States;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace IndiesSkills.MyEntityStates;

public class TarTetherMove : BaseCastChanneledSpellState
{
	public Vector3 handPosition;

	private List<TarTetherController> tetherControllers = new List<TarTetherController>();

	public List<HurtBox> affectedIndividuals = new List<HurtBox>();

	public static float maxTetherDistance = 60f;

	public static float mulchDistance = 5f;

	public static float mulchDamageScale = 2f;

	public static float mulchTickFrequencyScale = 0.5f;

	public static float damageTickFrequency = 3f;

	public static float damagePerSecond = 1f;

	public uint soundID;

	public override void OnEnter()
	{
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		base.baseDuration = 6f;
		base.overrideDuration = 6f;
		base.muzzleflashEffectPrefab = Resources.Load<GameObject>("Prefabs/Effects/ImpactEffects/ExplosionSolarFlare");
		base.projectilePrefab = null;
		base.castSoundString = "PaladinCastTorpor";
		base.muzzleString = "HandL";
		soundID = Util.PlayAttackSpeedSound(Recover.enterSoundString, ((EntityState)this).gameObject, ((BaseState)this).attackSpeedStat);
		((BaseCastChanneledSpellState)this).OnEnter();
		if (!NetworkServer.active)
		{
			return;
		}
		if (Object.op_Implicit((Object)(object)((EntityState)this).modelLocator))
		{
			ChildLocator modelChildLocator = ((EntityState)this).GetModelChildLocator();
			if (Object.op_Implicit((Object)(object)modelChildLocator))
			{
				Transform val = modelChildLocator.FindChild("HandL");
				if (Object.op_Implicit((Object)(object)val))
				{
					handPosition = val.position;
				}
			}
		}
		FireTethers();
	}

	public override void FixedUpdate()
	{
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		((BaseCastChanneledSpellState)this).FixedUpdate();
		if (!NetworkServer.active)
		{
			return;
		}
		if (Object.op_Implicit((Object)(object)((EntityState)this).modelLocator))
		{
			ChildLocator component = ((Component)((EntityState)this).modelLocator.modelTransform).GetComponent<ChildLocator>();
			if (Object.op_Implicit((Object)(object)component))
			{
				int num = component.FindChildIndex("HandL");
				Transform val = component.FindChild(num);
				if (Object.op_Implicit((Object)(object)val))
				{
					handPosition = val.position;
				}
			}
		}
		for (int num2 = tetherControllers.Count - 1; num2 >= 0; num2--)
		{
			if (Object.op_Implicit((Object)(object)tetherControllers[num2]))
			{
				((Component)tetherControllers[num2]).gameObject.transform.position = handPosition;
				NetMessageExtensions.Send((INetMessage)(object)new Main.SyncTetherPosition(((NetworkBehaviour)tetherControllers[num2]).netId, new Vector3(handPosition.x, NetworkServer.localClientActive ? handPosition.y : (handPosition.y + 2f), handPosition.z)), (NetworkDestination)1);
			}
		}
		RemoveDeadTethersFromList(tetherControllers);
		FireTethers();
	}

	protected override void PlayCastAnimation()
	{
		((EntityState)this).PlayAnimation("Gesture, Override", "CastSun", "Spell.playbackRate", 0.25f);
	}

	public override void OnExit()
	{
		if (NetworkServer.active)
		{
			for (int num = tetherControllers.Count - 1; num >= 0; num--)
			{
				if (Object.op_Implicit((Object)(object)tetherControllers[num]))
				{
					Object.Destroy((Object)(object)((Component)tetherControllers[num]).gameObject);
				}
			}
		}
		((BaseCastChanneledSpellState)this).OnExit();
		AkSoundEngine.StopPlayingID(soundID);
		((EntityState)this).PlayAnimation("Gesture, Override", "CastSunEnd", "Spell.playbackRate", 0.8f);
	}

	private void FireTethers()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Expected O, but got Unknown
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = handPosition;
		float breakDistanceSqr = maxTetherDistance * maxTetherDistance;
		List<GameObject> list = new List<GameObject>();
		BullseyeSearch val2 = new BullseyeSearch();
		val2.searchOrigin = val;
		val2.maxDistanceFilter = maxTetherDistance;
		val2.teamMaskFilter = TeamMask.allButNeutral;
		val2.sortMode = (SortMode)1;
		val2.filterByLoS = true;
		val2.searchDirection = Vector3.up;
		val2.RefreshCandidates();
		val2.FilterOutGameObject(((EntityState)this).gameObject);
		List<HurtBox> list2 = val2.GetResults().ToList();
		for (int i = 0; i < list2.Count; i++)
		{
			if (list.Count + tetherControllers.Count >= 20)
			{
				break;
			}
			if (list2[i].healthComponent.body.teamComponent.teamIndex != ((EntityState)this).characterBody.teamComponent.teamIndex && !affectedIndividuals.Contains(list2[i]))
			{
				GameObject gameObject = ((Component)list2[i].healthComponent).gameObject;
				if (Object.op_Implicit((Object)(object)gameObject))
				{
					list.Add(gameObject);
					affectedIndividuals.Add(list2[i]);
				}
			}
		}
		float tickInterval = 1f / damageTickFrequency * (1f / ((EntityState)this).characterBody.attackSpeed);
		float damageCoefficientPerTick = damagePerSecond / damageTickFrequency;
		float mulchDistanceSqr = mulchDistance * mulchDistance;
		GameObject val3 = Resources.Load<GameObject>("Prefabs/NetworkedObjects/TarTether");
		for (int j = 0; j < list.Count; j++)
		{
			GameObject val4 = Object.Instantiate<GameObject>(val3, val, Quaternion.identity);
			TarTetherController component = val4.GetComponent<TarTetherController>();
			component.NetworkownerRoot = ((EntityState)this).gameObject;
			component.NetworktargetRoot = list[j];
			component.breakDistanceSqr = breakDistanceSqr;
			component.damageCoefficientPerTick = damageCoefficientPerTick;
			component.tickInterval = tickInterval;
			component.tickTimer = (float)j * 0.1f;
			component.mulchDistanceSqr = mulchDistanceSqr;
			component.mulchDamageScale = mulchDamageScale;
			component.mulchTickIntervalScale = mulchTickFrequencyScale;
			component.reelSpeed = 0f;
			tetherControllers.Add(component);
			NetworkServer.Spawn(val4);
		}
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

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		return (InterruptPriority)3;
	}
}
