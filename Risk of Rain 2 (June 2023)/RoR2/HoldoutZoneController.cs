using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using RoR2.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace RoR2;

public class HoldoutZoneController : BaseZoneBehavior
{
	public enum HoldoutZoneShape
	{
		Sphere,
		VerticalTube,
		Count
	}

	public delegate void CalcRadiusDelegate(ref float radius);

	public delegate void CalcChargeRateDelegate(ref float rate);

	public delegate void CalcAccumulatedChargeDelegate(ref float charge);

	public delegate void CalcColorDelegate(ref Color color);

	[Serializable]
	public class HoldoutZoneControllerChargedUnityEvent : UnityEvent<HoldoutZoneController>
	{
	}

	private class ChargeHoldoutZoneObjectiveTracker : ObjectivePanelController.ObjectiveTracker
	{
		private int lastPercent = -1;

		private HoldoutZoneController holdoutZoneController => (HoldoutZoneController)(object)sourceDescriptor.source;

		private bool ShouldBeFlashing()
		{
			bool flag = true;
			if (Object.op_Implicit((Object)(object)sourceDescriptor.master) && Object.op_Implicit((Object)(object)holdoutZoneController))
			{
				flag = holdoutZoneController.IsBodyInChargingRadius(sourceDescriptor.master.GetBody());
			}
			return !flag;
		}

		protected override string GenerateString()
		{
			lastPercent = holdoutZoneController.displayChargePercent;
			string text = string.Format(Language.GetString(holdoutZoneController.inBoundsObjectiveToken), lastPercent);
			if (ShouldBeFlashing())
			{
				text = string.Format(Language.GetString(holdoutZoneController.outOfBoundsObjectiveToken), lastPercent);
				if ((int)(Time.time * 12f) % 2 == 0)
				{
					text = $"<style=cDeath>{text}</style>";
				}
			}
			return text;
		}

		protected override bool IsDirty()
		{
			return true;
		}
	}

	private class FocusConvergenceController : MonoBehaviour
	{
		private static readonly float convergenceRadiusDivisor = 2f;

		private static readonly float convergenceChargeRateBonus = 0.3f;

		private static readonly Color convergenceMaterialColor = new Color(0f, 3.9411764f, 5f, 1f);

		private static readonly float rampUpTime = 5f;

		private static readonly float startupDelay = 3f;

		private static readonly int cap = 3;

		private float currentValue;

		private HoldoutZoneController holdoutZoneController;

		private int currentFocusConvergenceCount;

		private Run.FixedTimeStamp enabledTime;

		private static readonly AnimationCurve colorCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

		private void Awake()
		{
			holdoutZoneController = ((Component)this).GetComponent<HoldoutZoneController>();
		}

		private void OnEnable()
		{
			enabledTime = Run.FixedTimeStamp.now;
			holdoutZoneController.calcRadius += ApplyRadius;
			holdoutZoneController.calcChargeRate += ApplyRate;
			holdoutZoneController.calcColor += ApplyColor;
		}

		private void OnDisable()
		{
			holdoutZoneController.calcColor -= ApplyColor;
			holdoutZoneController.calcChargeRate -= ApplyRate;
			holdoutZoneController.calcRadius -= ApplyRadius;
		}

		private void ApplyRadius(ref float radius)
		{
			if (currentFocusConvergenceCount > 0)
			{
				radius /= convergenceRadiusDivisor * (float)currentFocusConvergenceCount;
			}
		}

		private void ApplyColor(ref Color color)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			color = Color.Lerp(color, convergenceMaterialColor, colorCurve.Evaluate(currentValue));
		}

		private void ApplyRate(ref float rate)
		{
			if (currentFocusConvergenceCount > 0)
			{
				rate *= 1f + convergenceChargeRateBonus * (float)currentFocusConvergenceCount;
			}
		}

		private void FixedUpdate()
		{
			currentFocusConvergenceCount = Util.GetItemCountForTeam(holdoutZoneController.chargingTeam, RoR2Content.Items.FocusConvergence.itemIndex, requiresAlive: true, requiresConnected: false);
			if (enabledTime.timeSince < startupDelay)
			{
				currentFocusConvergenceCount = 0;
			}
			currentFocusConvergenceCount = Mathf.Min(currentFocusConvergenceCount, cap);
			float num = (((float)currentFocusConvergenceCount > 0f) ? 1f : 0f);
			float num2 = Mathf.MoveTowards(currentValue, num, rampUpTime * Time.fixedDeltaTime);
			if (currentValue <= 0f && num2 > 0f)
			{
				Util.PlaySound("Play_item_lunar_focusedConvergence", ((Component)this).gameObject);
			}
			currentValue = num2;
		}
	}

	public HoldoutZoneShape holdoutZoneShape;

	[Tooltip("The base radius of this charging sphere. Players must be within this radius to charge this zone.")]
	public float baseRadius;

	[Tooltip("No modifiers can reduce the radius below this size")]
	public float minimumRadius;

	[Tooltip("The overall change to the radius from 0% charge to 100% charge.")]
	public float chargeRadiusDelta;

	[Tooltip("How long it takes for this zone to finish charging without any modifiers.")]
	public float baseChargeDuration;

	[Tooltip("Approximately how long it should take to change from any given radius to the desired one.")]
	public float radiusSmoothTime;

	[Tooltip("An object instance which will be used to represent the clear radius.")]
	public Renderer radiusIndicator;

	[Tooltip("The child object to enable when healing nova should be active.")]
	public GameObject healingNovaItemEffect;

	public Transform healingNovaRoot;

	public string inBoundsObjectiveToken = "OBJECTIVE_CHARGE_TELEPORTER";

	public string outOfBoundsObjectiveToken = "OBJECTIVE_CHARGE_TELEPORTER_OOB";

	public bool showObjective = true;

	public bool applyFocusConvergence;

	public bool applyHealingNova = true;

	[Range(0f, float.MaxValue)]
	public float playerCountScaling = 1f;

	[Tooltip("If the zone is empty, this is the rate at which the charge decreases (a negative value will increase charge)")]
	public float dischargeRate;

	public HoldoutZoneControllerChargedUnityEvent onCharged;

	private BuffWard buffWard;

	private static MaterialPropertyBlock sharedColorPropertyBlock;

	private Color baseIndicatorColor;

	private float radiusVelocity;

	private bool wasCharged;

	private GameObject[] healingNovaGeneratorsByTeam = (GameObject[])(object)new GameObject[5];

	[SyncVar]
	private float _charge;

	public float currentRadius { get; private set; }

	public bool isAnyoneCharging { get; private set; }

	public TeamIndex chargingTeam { get; set; } = TeamIndex.Player;


	public int displayChargePercent => Mathf.Clamp(Mathf.FloorToInt(charge * 99f), 0, 99);

	public float charge
	{
		get
		{
			return _charge;
		}
		private set
		{
			Network_charge = value;
		}
	}

	public float Network_charge
	{
		get
		{
			return _charge;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).SetSyncVar<float>(value, ref _charge, 1u);
		}
	}

	public event CalcRadiusDelegate calcRadius;

	public event CalcChargeRateDelegate calcChargeRate;

	public event CalcAccumulatedChargeDelegate calcAccumulatedCharge;

	public event CalcColorDelegate calcColor;

	[RuntimeInitializeOnLoadMethod]
	private static void Init()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Expected O, but got Unknown
		sharedColorPropertyBlock = new MaterialPropertyBlock();
		ObjectivePanelController.collectObjectiveSources += OnCollectObjectiveSources;
	}

	private void Awake()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)radiusIndicator))
		{
			baseIndicatorColor = radiusIndicator.sharedMaterial.GetColor("_TintColor");
		}
		buffWard = ((Component)this).GetComponent<BuffWard>();
	}

	private void Start()
	{
		if (applyFocusConvergence)
		{
			((Component)this).gameObject.AddComponent<FocusConvergenceController>();
		}
	}

	private void OnEnable()
	{
		if (Object.op_Implicit((Object)(object)radiusIndicator))
		{
			radiusIndicator.enabled = true;
			((Component)radiusIndicator).gameObject.SetActive(true);
		}
		currentRadius = 0f;
		InstanceTracker.Add<HoldoutZoneController>(this);
	}

	private void OnDisable()
	{
		InstanceTracker.Remove<HoldoutZoneController>(this);
		currentRadius = 0f;
		if (Object.op_Implicit((Object)(object)radiusIndicator))
		{
			radiusIndicator.enabled = false;
			((Component)radiusIndicator).gameObject.SetActive(false);
		}
	}

	private void UpdateHealingNovas(bool isCharging)
	{
		if (!applyHealingNova)
		{
			return;
		}
		bool flag = false;
		for (TeamIndex teamIndex = TeamIndex.Neutral; teamIndex < TeamIndex.Count; teamIndex++)
		{
			bool flag2 = Util.GetItemCountForTeam(teamIndex, RoR2Content.Items.TPHealingNova.itemIndex, requiresAlive: false) > 0 && isCharging;
			flag = flag || flag2;
			if (NetworkServer.active)
			{
				ref GameObject reference = ref healingNovaGeneratorsByTeam[(int)teamIndex];
				if (flag2 != Object.op_Implicit((Object)(object)reference))
				{
					if (flag2)
					{
						reference = Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/TeleporterHealNovaGenerator"), healingNovaRoot ?? ((Component)this).transform);
						reference.GetComponent<TeamFilter>().teamIndex = teamIndex;
						NetworkServer.Spawn(reference);
					}
					else
					{
						Object.Destroy((Object)(object)reference);
						reference = null;
					}
				}
			}
		}
		if (Object.op_Implicit((Object)(object)healingNovaItemEffect))
		{
			healingNovaItemEffect.SetActive(flag);
		}
	}

	private void FixedUpdate()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		int num = CountLivingPlayers(chargingTeam);
		int num2 = CountPlayersInRadius(this, ((Component)this).transform.position, currentRadius * currentRadius, chargingTeam);
		isAnyoneCharging = num2 > 0;
		if (Object.op_Implicit((Object)(object)Run.instance))
		{
			float radius = baseRadius + charge * chargeRadiusDelta;
			if (Run.instance.selectedDifficulty >= DifficultyIndex.Eclipse2)
			{
				radius *= 0.5f;
			}
			this.calcRadius?.Invoke(ref radius);
			currentRadius = Mathf.Max(Mathf.SmoothDamp(currentRadius, radius, ref radiusVelocity, radiusSmoothTime, float.PositiveInfinity, Time.fixedDeltaTime), minimumRadius);
		}
		if (Object.op_Implicit((Object)(object)radiusIndicator))
		{
			float num3 = 2f * currentRadius;
			((Component)radiusIndicator).transform.localScale = new Vector3(num3, num3, num3);
		}
		if (NetworkServer.active && Object.op_Implicit((Object)(object)buffWard))
		{
			buffWard.Networkradius = currentRadius;
		}
		if (NetworkServer.active)
		{
			float num4 = baseChargeDuration;
			float rate = ((!isAnyoneCharging || num <= 0) ? (0f - dischargeRate) : (Mathf.Pow((float)num2 / (float)num, playerCountScaling) / num4));
			this.calcChargeRate?.Invoke(ref rate);
			charge = Mathf.Clamp01(charge + rate * Time.fixedDeltaTime);
			float num5 = charge;
			this.calcAccumulatedCharge?.Invoke(ref num5);
			charge = num5;
		}
		Color color = baseIndicatorColor;
		this.calcColor?.Invoke(ref color);
		sharedColorPropertyBlock.SetColor("_TintColor", color);
		if (Object.op_Implicit((Object)(object)radiusIndicator))
		{
			radiusIndicator.SetPropertyBlock(sharedColorPropertyBlock);
		}
		bool flag = charge >= 1f;
		if (wasCharged != flag)
		{
			wasCharged = flag;
			if (flag)
			{
				((UnityEvent<HoldoutZoneController>)onCharged)?.Invoke(this);
			}
		}
		UpdateHealingNovas(isAnyoneCharging);
	}

	private void OnDrawGizmos()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		Matrix4x4 matrix = Gizmos.matrix;
		Color color = Gizmos.color;
		Gizmos.matrix = ((Component)this).transform.localToWorldMatrix;
		Gizmos.color = new Color(0.75f, 0f, 0f, 0.5f);
		Gizmos.DrawWireSphere(Vector3.zero, baseRadius);
		Gizmos.color = color;
		Gizmos.matrix = matrix;
	}

	private static bool IsPointInChargingRadius(HoldoutZoneController holdoutZoneController, Vector3 origin, float chargingRadiusSqr, Vector3 point)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val;
		switch (holdoutZoneController.holdoutZoneShape)
		{
		case HoldoutZoneShape.Sphere:
			val = point - origin;
			if (((Vector3)(ref val)).sqrMagnitude <= chargingRadiusSqr)
			{
				return true;
			}
			break;
		case HoldoutZoneShape.VerticalTube:
			point.y = 0f;
			origin.y = 0f;
			val = point - origin;
			if (((Vector3)(ref val)).sqrMagnitude <= chargingRadiusSqr)
			{
				return true;
			}
			break;
		}
		return false;
	}

	private static bool IsBodyInChargingRadius(HoldoutZoneController holdoutZoneController, Vector3 origin, float chargingRadiusSqr, CharacterBody characterBody)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		return IsPointInChargingRadius(holdoutZoneController, origin, chargingRadiusSqr, characterBody.corePosition);
	}

	private static int CountLivingPlayers(TeamIndex teamIndex)
	{
		int num = 0;
		ReadOnlyCollection<TeamComponent> teamMembers = TeamComponent.GetTeamMembers(teamIndex);
		for (int i = 0; i < teamMembers.Count; i++)
		{
			if (teamMembers[i].body.isPlayerControlled)
			{
				num++;
			}
		}
		return num;
	}

	private static int CountPlayersInRadius(HoldoutZoneController holdoutZoneController, Vector3 origin, float chargingRadiusSqr, TeamIndex teamIndex)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		ReadOnlyCollection<TeamComponent> teamMembers = TeamComponent.GetTeamMembers(teamIndex);
		for (int i = 0; i < teamMembers.Count; i++)
		{
			TeamComponent teamComponent = teamMembers[i];
			if (teamComponent.body.isPlayerControlled && IsBodyInChargingRadius(holdoutZoneController, origin, chargingRadiusSqr, teamComponent.body))
			{
				num++;
			}
		}
		return num;
	}

	public bool IsBodyInChargingRadius(CharacterBody body)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)body))
		{
			return false;
		}
		return IsBodyInChargingRadius(this, ((Component)this).transform.position, currentRadius * currentRadius, body);
	}

	[Server]
	public void FullyChargeHoldoutZone()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.HoldoutZoneController::FullyChargeHoldoutZone()' called on client");
		}
		else
		{
			charge = 1f;
		}
	}

	private static void OnCollectObjectiveSources(CharacterMaster master, List<ObjectivePanelController.ObjectiveSourceDescriptor> objectiveSourcesList)
	{
		List<HoldoutZoneController> instancesList = InstanceTracker.GetInstancesList<HoldoutZoneController>();
		int i = 0;
		for (int count = instancesList.Count; i < count; i++)
		{
			HoldoutZoneController holdoutZoneController = instancesList[i];
			if (holdoutZoneController.showObjective && holdoutZoneController.chargingTeam == master.teamIndex)
			{
				objectiveSourcesList.Add(new ObjectivePanelController.ObjectiveSourceDescriptor
				{
					master = master,
					objectiveType = typeof(ChargeHoldoutZoneObjectiveTracker),
					source = (Object)(object)holdoutZoneController
				});
			}
		}
	}

	public override bool IsInBounds(Vector3 position)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		return IsPointInChargingRadius(this, ((Component)this).transform.position, currentRadius * currentRadius, position);
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool flag = base.OnSerialize(writer, forceAll);
		if (forceAll)
		{
			writer.Write(_charge);
			return true;
		}
		bool flag2 = false;
		if ((((NetworkBehaviour)this).syncVarDirtyBits & (true ? 1u : 0u)) != 0)
		{
			if (!flag2)
			{
				writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
				flag2 = true;
			}
			writer.Write(_charge);
		}
		if (!flag2)
		{
			writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
		}
		return flag2 || flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		base.OnDeserialize(reader, initialState);
		if (initialState)
		{
			_charge = reader.ReadSingle();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if (((uint)num & (true ? 1u : 0u)) != 0)
		{
			_charge = reader.ReadSingle();
		}
	}

	public override void PreStartClient()
	{
		base.PreStartClient();
	}
}
