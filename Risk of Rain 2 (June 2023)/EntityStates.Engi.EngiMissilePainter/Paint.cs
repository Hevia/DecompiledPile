using System.Collections.Generic;
using RoR2;
using RoR2.Skills;
using RoR2.UI;
using UnityEngine;

namespace EntityStates.Engi.EngiMissilePainter;

public class Paint : BaseEngiMissilePainterState
{
	private struct IndicatorInfo
	{
		public int refCount;

		public EngiMissileIndicator indicator;
	}

	private class EngiMissileIndicator : Indicator
	{
		public int missileCount;

		public override void UpdateVisualizer()
		{
			//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
			base.UpdateVisualizer();
			Transform val = base.visualizerTransform.Find("DotOrigin");
			for (int num = val.childCount - 1; num >= missileCount; num--)
			{
				EntityState.Destroy((Object)(object)val.GetChild(num));
			}
			for (int i = val.childCount; i < missileCount; i++)
			{
				Object.Instantiate<GameObject>(((Component)base.visualizerPrefab.transform.Find("DotOrigin/DotTemplate")).gameObject, val);
			}
			if (val.childCount > 0)
			{
				float num2 = 360f / (float)val.childCount;
				float num3 = (float)(val.childCount - 1) * 90f;
				for (int j = 0; j < val.childCount; j++)
				{
					Transform child = val.GetChild(j);
					((Component)child).gameObject.SetActive(true);
					child.localRotation = Quaternion.Euler(0f, 0f, num3 + (float)j * num2);
				}
			}
		}

		public EngiMissileIndicator(GameObject owner, GameObject visualizerPrefab)
			: base(owner, visualizerPrefab)
		{
		}
	}

	public static GameObject crosshairOverridePrefab;

	public static GameObject stickyTargetIndicatorPrefab;

	public static float stackInterval;

	public static string enterSoundString;

	public static string exitSoundString;

	public static string loopSoundString;

	public static string lockOnSoundString;

	public static string stopLoopSoundString;

	public static float maxAngle;

	public static float maxDistance;

	private List<HurtBox> targetsList;

	private Dictionary<HurtBox, IndicatorInfo> targetIndicators;

	private Indicator stickyTargetIndicator;

	private SkillDef engiConfirmTargetDummySkillDef;

	private SkillDef engiCancelTargetingDummySkillDef;

	private bool releasedKeyOnce;

	private float stackStopwatch;

	private CrosshairUtils.OverrideRequest crosshairOverrideRequest;

	private BullseyeSearch search;

	private bool queuedFiringState;

	private uint loopSoundID;

	private HealthComponent previousHighlightTargetHealthComponent;

	private HurtBox previousHighlightTargetHurtBox;

	public override void OnEnter()
	{
		base.OnEnter();
		if (base.isAuthority)
		{
			targetsList = new List<HurtBox>();
			targetIndicators = new Dictionary<HurtBox, IndicatorInfo>();
			stickyTargetIndicator = new Indicator(base.gameObject, stickyTargetIndicatorPrefab);
			search = new BullseyeSearch();
		}
		PlayCrossfade("Gesture, Additive", "PrepHarpoons", 0.1f);
		Util.PlaySound(enterSoundString, base.gameObject);
		loopSoundID = Util.PlaySound(loopSoundString, base.gameObject);
		if (Object.op_Implicit((Object)(object)crosshairOverridePrefab))
		{
			crosshairOverrideRequest = CrosshairUtils.RequestOverrideForBody(base.characterBody, crosshairOverridePrefab, CrosshairUtils.OverridePriority.Skill);
		}
		engiConfirmTargetDummySkillDef = SkillCatalog.GetSkillDef(SkillCatalog.FindSkillIndexByName("EngiConfirmTargetDummy"));
		engiCancelTargetingDummySkillDef = SkillCatalog.GetSkillDef(SkillCatalog.FindSkillIndexByName("EngiCancelTargetingDummy"));
		base.skillLocator.primary.SetSkillOverride(this, engiConfirmTargetDummySkillDef, GenericSkill.SkillOverridePriority.Contextual);
		base.skillLocator.secondary.SetSkillOverride(this, engiCancelTargetingDummySkillDef, GenericSkill.SkillOverridePriority.Contextual);
	}

	public override void OnExit()
	{
		if (base.isAuthority && !outer.destroying && !queuedFiringState)
		{
			for (int i = 0; i < targetsList.Count; i++)
			{
				base.activatorSkillSlot.AddOneStock();
			}
		}
		base.skillLocator.secondary.UnsetSkillOverride(this, engiCancelTargetingDummySkillDef, GenericSkill.SkillOverridePriority.Contextual);
		base.skillLocator.primary.UnsetSkillOverride(this, engiConfirmTargetDummySkillDef, GenericSkill.SkillOverridePriority.Contextual);
		if (targetIndicators != null)
		{
			foreach (KeyValuePair<HurtBox, IndicatorInfo> targetIndicator in targetIndicators)
			{
				targetIndicator.Value.indicator.active = false;
			}
		}
		if (stickyTargetIndicator != null)
		{
			stickyTargetIndicator.active = false;
		}
		crosshairOverrideRequest?.Dispose();
		PlayCrossfade("Gesture, Additive", "ExitHarpoons", 0.1f);
		Util.PlaySound(exitSoundString, base.gameObject);
		Util.PlaySound(stopLoopSoundString, base.gameObject);
		base.OnExit();
	}

	private void AddTargetAuthority(HurtBox hurtBox)
	{
		if (base.activatorSkillSlot.stock != 0)
		{
			Util.PlaySound(lockOnSoundString, base.gameObject);
			targetsList.Add(hurtBox);
			if (!targetIndicators.TryGetValue(hurtBox, out var value))
			{
				IndicatorInfo indicatorInfo = default(IndicatorInfo);
				indicatorInfo.refCount = 0;
				indicatorInfo.indicator = new EngiMissileIndicator(base.gameObject, LegacyResourcesAPI.Load<GameObject>("Prefabs/EngiMissileTrackingIndicator"));
				value = indicatorInfo;
				value.indicator.targetTransform = ((Component)hurtBox).transform;
				value.indicator.active = true;
			}
			value.refCount++;
			value.indicator.missileCount = value.refCount;
			targetIndicators[hurtBox] = value;
			base.activatorSkillSlot.DeductStock(1);
		}
	}

	private void RemoveTargetAtAuthority(int i)
	{
		HurtBox key = targetsList[i];
		targetsList.RemoveAt(i);
		if (targetIndicators.TryGetValue(key, out var value))
		{
			value.refCount--;
			value.indicator.missileCount = value.refCount;
			targetIndicators[key] = value;
			if (value.refCount == 0)
			{
				value.indicator.active = false;
				targetIndicators.Remove(key);
			}
		}
	}

	private void CleanTargetsList()
	{
		for (int num = targetsList.Count - 1; num >= 0; num--)
		{
			HurtBox hurtBox = targetsList[num];
			if (!Object.op_Implicit((Object)(object)hurtBox.healthComponent) || !hurtBox.healthComponent.alive)
			{
				RemoveTargetAtAuthority(num);
				base.activatorSkillSlot.AddOneStock();
			}
		}
		for (int num2 = targetsList.Count - 1; num2 >= base.activatorSkillSlot.maxStock; num2--)
		{
			RemoveTargetAtAuthority(num2);
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		base.characterBody.SetAimTimer(3f);
		if (base.isAuthority)
		{
			AuthorityFixedUpdate();
		}
	}

	private void GetCurrentTargetInfo(out HurtBox currentTargetHurtBox, out HealthComponent currentTargetHealthComponent)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		Ray aimRay = GetAimRay();
		search.filterByDistinctEntity = true;
		search.filterByLoS = true;
		search.minDistanceFilter = 0f;
		search.maxDistanceFilter = maxDistance;
		search.minAngleFilter = 0f;
		search.maxAngleFilter = maxAngle;
		search.viewer = base.characterBody;
		search.searchOrigin = aimRay.origin;
		search.searchDirection = aimRay.direction;
		search.sortMode = BullseyeSearch.SortMode.DistanceAndAngle;
		search.teamMaskFilter = TeamMask.GetUnprotectedTeams(GetTeam());
		search.RefreshCandidates();
		search.FilterOutGameObject(base.gameObject);
		foreach (HurtBox result in search.GetResults())
		{
			if (Object.op_Implicit((Object)(object)result.healthComponent) && result.healthComponent.alive)
			{
				currentTargetHurtBox = result;
				currentTargetHealthComponent = result.healthComponent;
				return;
			}
		}
		currentTargetHurtBox = null;
		currentTargetHealthComponent = null;
	}

	private void AuthorityFixedUpdate()
	{
		CleanTargetsList();
		bool flag = false;
		GetCurrentTargetInfo(out var currentTargetHurtBox, out var currentTargetHealthComponent);
		if (Object.op_Implicit((Object)(object)currentTargetHurtBox))
		{
			stackStopwatch += Time.fixedDeltaTime;
			if (base.inputBank.skill1.down && ((Object)(object)previousHighlightTargetHealthComponent != (Object)(object)currentTargetHealthComponent || stackStopwatch > stackInterval / attackSpeedStat || base.inputBank.skill1.justPressed))
			{
				stackStopwatch = 0f;
				AddTargetAuthority(currentTargetHurtBox);
			}
		}
		if (base.inputBank.skill1.justReleased)
		{
			flag = true;
		}
		if (base.inputBank.skill2.justReleased)
		{
			outer.SetNextStateToMain();
			return;
		}
		if (base.inputBank.skill3.justReleased)
		{
			if (releasedKeyOnce)
			{
				flag = true;
			}
			releasedKeyOnce = true;
		}
		if (currentTargetHurtBox != previousHighlightTargetHurtBox)
		{
			previousHighlightTargetHurtBox = currentTargetHurtBox;
			previousHighlightTargetHealthComponent = currentTargetHealthComponent;
			stickyTargetIndicator.targetTransform = ((Object.op_Implicit((Object)(object)currentTargetHurtBox) && base.activatorSkillSlot.stock != 0) ? ((Component)currentTargetHurtBox).transform : null);
			stackStopwatch = 0f;
		}
		stickyTargetIndicator.active = Object.op_Implicit((Object)(object)stickyTargetIndicator.targetTransform);
		if (flag)
		{
			queuedFiringState = true;
			outer.SetNextState(new Fire
			{
				targetsList = targetsList,
				activatorSkillSlot = base.activatorSkillSlot
			});
		}
	}
}
