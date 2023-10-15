using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using KinematicCharacterController;
using RoR2;
using RoR2.Navigation;
using UnityEngine;

namespace EntityStates.GrandParentBoss;

public class SpiritPull : BaseState
{
	private BullseyeSearch enemyFinder;

	public static float lockOnAngle;

	private Vector3 teleporterIndicatorInstance;

	private Transform modelTransform;

	public static float pullTimer;

	public static float zoneRadius;

	public static float initialDelay;

	private float duration = 4f;

	public static float maxRange;

	public static string teleportZoneString;

	public static GameObject teleportZoneEffect;

	public static GameObject playerTeleportEffect;

	public static float effectsDuration = 2f;

	public static float playerTeleportEffectsDuration = 1f;

	private bool effectsDone;

	private bool gatheredVictims;

	private bool teleported;

	private float stopwatch;

	private List<Vector3> Startpositions;

	private List<Vector3> Endpositions;

	public static int stacks;

	private List<HurtBox> results = new List<HurtBox>();

	private HurtBox foundBullseye;

	private BullseyeSearch search;

	private GameObject spiritPullLocationObject;

	public static string indicatorOnPlayerSoundLoop;

	public static string indicatorOnPlayerSoundStop;

	public static string teleportedPlayerSound;

	public override void OnEnter()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Expected O, but got Unknown
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		Startpositions = new List<Vector3>();
		Endpositions = new List<Vector3>();
		Ray aimRay = GetAimRay();
		modelTransform = GetModelTransform();
		enemyFinder = new BullseyeSearch();
		enemyFinder.maxDistanceFilter = 2000f;
		enemyFinder.maxAngleFilter = lockOnAngle;
		enemyFinder.searchOrigin = aimRay.origin;
		enemyFinder.searchDirection = aimRay.direction;
		enemyFinder.filterByLoS = false;
		enemyFinder.sortMode = BullseyeSearch.SortMode.Distance;
		enemyFinder.teamMaskFilter = TeamMask.allButNeutral;
		if (Object.op_Implicit((Object)(object)base.teamComponent))
		{
			enemyFinder.teamMaskFilter.RemoveTeam(base.teamComponent.teamIndex);
		}
		enemyFinder.RefreshCandidates();
		foundBullseye = enemyFinder.GetResults().LastOrDefault();
		_ = Vector3.zero;
		if (Object.op_Implicit((Object)(object)foundBullseye))
		{
			teleporterIndicatorInstance = ((Component)foundBullseye).transform.position;
			RaycastHit val = default(RaycastHit);
			if (Physics.Raycast(teleporterIndicatorInstance, Vector3.down, ref val, 500f, LayerMask.op_Implicit(LayerIndex.world.mask)))
			{
				teleporterIndicatorInstance = ((RaycastHit)(ref val)).point;
				_ = teleporterIndicatorInstance;
			}
			EffectData effectData = new EffectData
			{
				origin = teleporterIndicatorInstance,
				rotation = teleportZoneEffect.transform.rotation,
				scale = zoneRadius * 2f
			};
			EffectManager.SpawnEffect(teleportZoneEffect, effectData, transmit: true);
			if ((Object)(object)spiritPullLocationObject == (Object)null)
			{
				spiritPullLocationObject = new GameObject();
			}
			spiritPullLocationObject.transform.position = effectData.origin;
			Util.PlaySound(indicatorOnPlayerSoundLoop, spiritPullLocationObject);
		}
	}

	private Transform FindTargetFarthest(Vector3 point, TeamIndex enemyTeam)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		ReadOnlyCollection<TeamComponent> teamMembers = TeamComponent.GetTeamMembers(enemyTeam);
		float num = 0f;
		Transform result = null;
		for (int i = 0; i < teamMembers.Count; i++)
		{
			float num2 = Vector3.SqrMagnitude(((Component)teamMembers[i]).transform.position - point);
			if (num2 > num && num2 < maxRange)
			{
				num = num2;
				result = ((Component)teamMembers[i]).transform;
			}
		}
		return result;
	}

	public override void FixedUpdate()
	{
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		pullTimer -= Time.deltaTime;
		base.FixedUpdate();
		stopwatch += Time.fixedDeltaTime;
		if (stopwatch >= effectsDuration && !gatheredVictims)
		{
			effectsDone = true;
			gatheredVictims = true;
			GetPlayersInsideTeleportZone();
		}
		if (stopwatch >= effectsDuration + playerTeleportEffectsDuration && !teleported)
		{
			teleported = true;
			TeleportPlayers();
		}
		if (Object.op_Implicit((Object)(object)base.characterMotor) && Object.op_Implicit((Object)(object)base.characterDirection))
		{
			base.characterMotor.velocity = Vector3.zero;
		}
		if (effectsDone)
		{
			SetPositions();
		}
		if (stopwatch >= effectsDuration + playerTeleportEffectsDuration + duration && base.isAuthority)
		{
			effectsDone = false;
			outer.SetNextStateToMain();
		}
	}

	private void GetPlayersInsideTeleportZone()
	{
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		Startpositions.Clear();
		Endpositions.Clear();
		if (!Object.op_Implicit((Object)(object)foundBullseye))
		{
			return;
		}
		for (int i = 0; i < stacks; i++)
		{
			search = new BullseyeSearch();
			search.filterByLoS = false;
			search.maxDistanceFilter = zoneRadius;
			search.searchOrigin = new Vector3(teleporterIndicatorInstance.x, teleporterIndicatorInstance.y + zoneRadius * (float)i, teleporterIndicatorInstance.z);
			search.sortMode = BullseyeSearch.SortMode.Distance;
			search.teamMaskFilter = TeamMask.allButNeutral;
			search.teamMaskFilter.RemoveTeam(base.teamComponent.teamIndex);
			search.RefreshCandidates();
			search.queryTriggerInteraction = (QueryTriggerInteraction)2;
			for (int j = 0; j < search.GetResults().ToList().Count; j++)
			{
				if (!results.Contains(search.GetResults().ToList()[j]))
				{
					results.Add(search.GetResults().ToList()[j]);
				}
			}
		}
		if (results.Count <= 0)
		{
			return;
		}
		for (int k = 0; k < results.Count; k++)
		{
			HurtBox hurtBox = results[k];
			Transform val = hurtBox.healthComponent.body.modelLocator.modelTransform;
			EffectData effectData = new EffectData
			{
				origin = hurtBox.healthComponent.body.footPosition
			};
			EffectManager.SpawnEffect(playerTeleportEffect, effectData, transmit: true);
			if (Object.op_Implicit((Object)(object)val))
			{
				TemporaryOverlay temporaryOverlay = ((Component)val).gameObject.AddComponent<TemporaryOverlay>();
				temporaryOverlay.duration = 0.6f;
				temporaryOverlay.animateShaderAlpha = true;
				temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
				temporaryOverlay.destroyComponentOnEnd = true;
				temporaryOverlay.originalMaterial = LegacyResourcesAPI.Load<Material>("Materials/matGrandparentTeleportFlash");
				temporaryOverlay.AddToCharacerModel(((Component)val).GetComponent<CharacterModel>());
			}
		}
	}

	private void TeleportPlayers()
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		if (results.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < results.Count; i++)
		{
			HurtBox hurtBox = results[i];
			CharacterModel component = ((Component)hurtBox.healthComponent.body.modelLocator.modelTransform).GetComponent<CharacterModel>();
			HurtBoxGroup component2 = ((Component)hurtBox.healthComponent.body.modelLocator.modelTransform).GetComponent<HurtBoxGroup>();
			Startpositions.Add(((Component)hurtBox).transform.position);
			if (Object.op_Implicit((Object)(object)component2))
			{
				int hurtBoxesDeactivatorCounter = component2.hurtBoxesDeactivatorCounter + 1;
				component2.hurtBoxesDeactivatorCounter = hurtBoxesDeactivatorCounter;
			}
			if (Object.op_Implicit((Object)(object)component))
			{
				component.invisibilityCount++;
			}
			CharacterMotor component3 = ((Component)hurtBox.healthComponent).gameObject.GetComponent<CharacterMotor>();
			if (Object.op_Implicit((Object)(object)component3))
			{
				((Behaviour)component3).enabled = false;
			}
			duration = initialDelay;
			GameObject teleportEffectPrefab = Run.instance.GetTeleportEffectPrefab(base.gameObject);
			if (Object.op_Implicit((Object)(object)teleportEffectPrefab))
			{
				Object.Instantiate<GameObject>(teleportEffectPrefab, base.gameObject.transform.position, Quaternion.identity);
			}
			Util.PlaySound(teleportedPlayerSound, ((Component)hurtBox).gameObject);
			base.characterMotor.velocity = Vector3.zero;
			Vector3 position = ((Component)base.characterBody.modelLocator.modelTransform).GetComponent<ChildLocator>().FindChild(teleportZoneString).position;
			NodeGraph groundNodes = SceneInfo.instance.groundNodes;
			NodeGraph.NodeIndex nodeIndex = groundNodes.FindClosestNode(position, base.characterBody.hullClassification);
			groundNodes.GetNodePosition(nodeIndex, out position);
			position += ((Component)hurtBox.healthComponent.body).transform.position - hurtBox.healthComponent.body.footPosition;
			((Vector3)(ref position))._002Ector(position.x, position.y + 0.1f, position.z);
			Endpositions.Add(position);
		}
	}

	private void SetPositions()
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		if (results.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < results.Count; i++)
		{
			CharacterMotor component = ((Component)results[i].healthComponent).gameObject.GetComponent<CharacterMotor>();
			if (Object.op_Implicit((Object)(object)component))
			{
				Vector3 val = Vector3.Lerp(Startpositions[i], Endpositions[i], stopwatch / duration);
				((BaseCharacterController)component).Motor.SetPositionAndRotation(val, Quaternion.identity, true);
			}
		}
	}

	public override void OnExit()
	{
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		base.OnExit();
		Util.PlaySound(indicatorOnPlayerSoundStop, spiritPullLocationObject);
		if (results.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < results.Count; i++)
		{
			HurtBox hurtBox = results[i];
			CharacterModel component = ((Component)hurtBox.healthComponent.body.modelLocator.modelTransform).GetComponent<CharacterModel>();
			HurtBoxGroup component2 = ((Component)hurtBox.healthComponent.body.modelLocator.modelTransform).GetComponent<HurtBoxGroup>();
			Transform val = hurtBox.healthComponent.body.modelLocator.modelTransform;
			if (Object.op_Implicit((Object)(object)val))
			{
				TemporaryOverlay temporaryOverlay = ((Component)val).gameObject.AddComponent<TemporaryOverlay>();
				temporaryOverlay.duration = 0.6f;
				temporaryOverlay.animateShaderAlpha = true;
				temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
				temporaryOverlay.destroyComponentOnEnd = true;
				temporaryOverlay.originalMaterial = LegacyResourcesAPI.Load<Material>("Materials/matGrandparentTeleportFlash");
				temporaryOverlay.AddToCharacerModel(((Component)val).GetComponent<CharacterModel>());
			}
			EffectData effectData = new EffectData
			{
				origin = hurtBox.healthComponent.body.footPosition
			};
			EffectManager.SpawnEffect(playerTeleportEffect, effectData, transmit: true);
			if (Object.op_Implicit((Object)(object)component2))
			{
				int hurtBoxesDeactivatorCounter = component2.hurtBoxesDeactivatorCounter - 1;
				component2.hurtBoxesDeactivatorCounter = hurtBoxesDeactivatorCounter;
			}
			if (Object.op_Implicit((Object)(object)component))
			{
				component.invisibilityCount--;
			}
			CharacterMotor component3 = ((Component)hurtBox.healthComponent).gameObject.GetComponent<CharacterMotor>();
			if (Object.op_Implicit((Object)(object)component3))
			{
				((Behaviour)component3).enabled = true;
			}
		}
	}
}
