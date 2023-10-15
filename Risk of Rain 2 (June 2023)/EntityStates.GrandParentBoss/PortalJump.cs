using System.Linq;
using KinematicCharacterController;
using RoR2;
using RoR2.Navigation;
using UnityEngine;

namespace EntityStates.GrandParentBoss;

public class PortalJump : BaseState
{
	private BullseyeSearch enemyFinder;

	public static float duration = 3f;

	public static float retreatDuration = 2.433f;

	public static float emergeDuration = 2.933f;

	public static float portalScaleDuration = 2f;

	public static float effectsDuration = 2f;

	private bool retreatDone;

	private bool teleported;

	private bool canMoveDuringTeleport;

	private bool hasEmerged;

	private HurtBox foundBullseye;

	public static float telezoneRadius;

	public static float skillDistance = 2000f;

	private float stopwatch;

	private Vector3 destinationPressence = Vector3.zero;

	private Vector3 startPressence = Vector3.zero;

	private Transform modelTransform;

	private Animator animator;

	private CharacterModel characterModel;

	private HurtBoxGroup hurtboxGroup;

	public static GameObject jumpInEffectPrefab;

	public static GameObject jumpOutEffectPrefab;

	public static Vector3 teleportOffset;

	private GrandparentEnergyFXController FXController;

	public override void OnEnter()
	{
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		modelTransform = GetModelTransform();
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			animator = ((Component)modelTransform).GetComponent<Animator>();
			characterModel = ((Component)modelTransform).GetComponent<CharacterModel>();
			hurtboxGroup = ((Component)modelTransform).GetComponent<HurtBoxGroup>();
			PlayAnimation("Body", "Retreat", "retreat.playbackRate", retreatDuration);
			EffectData effectData = new EffectData
			{
				origin = ((Component)base.characterBody.modelLocator.modelTransform).GetComponent<ChildLocator>().FindChild("Portal").position
			};
			EffectManager.SpawnEffect(jumpInEffectPrefab, effectData, transmit: true);
		}
		FXController = ((Component)base.characterBody).GetComponent<GrandparentEnergyFXController>();
		if (Object.op_Implicit((Object)(object)FXController))
		{
			FXController.portalObject = ((Component)((Component)((Component)base.characterBody.modelLocator.modelTransform).GetComponent<ChildLocator>().FindChild("Portal")).GetComponentInChildren<EffectComponent>()).gameObject;
		}
	}

	public override void FixedUpdate()
	{
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		stopwatch += Time.fixedDeltaTime;
		if (stopwatch >= retreatDuration && !retreatDone)
		{
			retreatDone = true;
			if (Object.op_Implicit((Object)(object)FXController))
			{
				ScaleObject(FXController.portalObject, scaleUp: false);
			}
		}
		if (stopwatch >= retreatDuration + portalScaleDuration && !teleported)
		{
			teleported = true;
			canMoveDuringTeleport = true;
			if (Object.op_Implicit((Object)(object)FXController))
			{
				((Behaviour)FXController.portalObject.GetComponent<ObjectScaleCurve>()).enabled = false;
			}
			DoTeleport();
		}
		if (Object.op_Implicit((Object)(object)base.characterMotor) && Object.op_Implicit((Object)(object)base.characterDirection))
		{
			base.characterMotor.velocity = Vector3.zero;
		}
		if (canMoveDuringTeleport)
		{
			SetPosition(Vector3.Lerp(startPressence, destinationPressence, stopwatch / duration));
		}
		if (stopwatch >= retreatDuration + portalScaleDuration + duration && canMoveDuringTeleport)
		{
			canMoveDuringTeleport = false;
			if (Object.op_Implicit((Object)(object)FXController))
			{
				FXController.portalObject.transform.position = ((Component)base.characterBody.modelLocator.modelTransform).GetComponent<ChildLocator>().FindChild("Portal").position;
				ScaleObject(FXController.portalObject, scaleUp: true);
			}
		}
		if (stopwatch >= retreatDuration + portalScaleDuration * 2f + duration && !hasEmerged)
		{
			hasEmerged = true;
			if (Object.op_Implicit((Object)(object)FXController))
			{
				((Behaviour)FXController.portalObject.GetComponent<ObjectScaleCurve>()).enabled = false;
			}
			modelTransform = GetModelTransform();
			if (Object.op_Implicit((Object)(object)modelTransform))
			{
				PlayAnimation("Body", "Emerge", "emerge.playbackRate", duration);
				EffectData effectData = new EffectData
				{
					origin = ((Component)base.characterBody.modelLocator.modelTransform).GetComponent<ChildLocator>().FindChild("Portal").position
				};
				EffectManager.SpawnEffect(jumpOutEffectPrefab, effectData, transmit: true);
				if (Object.op_Implicit((Object)(object)characterModel))
				{
					characterModel.invisibilityCount--;
				}
				if (Object.op_Implicit((Object)(object)hurtboxGroup))
				{
					HurtBoxGroup hurtBoxGroup = hurtboxGroup;
					int hurtBoxesDeactivatorCounter = hurtBoxGroup.hurtBoxesDeactivatorCounter - 1;
					hurtBoxGroup.hurtBoxesDeactivatorCounter = hurtBoxesDeactivatorCounter;
				}
				if (Object.op_Implicit((Object)(object)base.characterMotor))
				{
					((Behaviour)base.characterMotor).enabled = true;
				}
			}
		}
		if (stopwatch >= retreatDuration + portalScaleDuration * 2f + duration + emergeDuration && base.isAuthority)
		{
			outer.SetNextStateToMain();
		}
	}

	private void DoTeleport()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		Ray aimRay = GetAimRay();
		enemyFinder = new BullseyeSearch();
		enemyFinder.maxDistanceFilter = skillDistance;
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
		modelTransform = GetModelTransform();
		if (Object.op_Implicit((Object)(object)characterModel))
		{
			characterModel.invisibilityCount++;
		}
		if (Object.op_Implicit((Object)(object)hurtboxGroup))
		{
			HurtBoxGroup hurtBoxGroup = hurtboxGroup;
			int hurtBoxesDeactivatorCounter = hurtBoxGroup.hurtBoxesDeactivatorCounter + 1;
			hurtBoxGroup.hurtBoxesDeactivatorCounter = hurtBoxesDeactivatorCounter;
		}
		if (Object.op_Implicit((Object)(object)base.characterMotor))
		{
			((Behaviour)base.characterMotor).enabled = false;
		}
		Vector3 val = base.inputBank.moveVector * skillDistance;
		destinationPressence = base.transform.position;
		startPressence = base.transform.position;
		NodeGraph groundNodes = SceneInfo.instance.groundNodes;
		Vector3 position = startPressence + val;
		if (Object.op_Implicit((Object)(object)foundBullseye))
		{
			position = ((Component)foundBullseye).transform.position;
		}
		NodeGraph.NodeIndex nodeIndex = groundNodes.FindClosestNode(position, base.characterBody.hullClassification);
		groundNodes.GetNodePosition(nodeIndex, out destinationPressence);
		destinationPressence += base.transform.position - base.characterBody.footPosition;
	}

	private void SetPosition(Vector3 newPosition)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)base.characterMotor))
		{
			((BaseCharacterController)base.characterMotor).Motor.SetPositionAndRotation(newPosition, Quaternion.identity, true);
		}
	}

	private void ScaleObject(GameObject objectToScaleDown, bool scaleUp)
	{
		float num = (scaleUp ? 1f : 0f);
		float num2 = (scaleUp ? 0f : 1f);
		ObjectScaleCurve component = objectToScaleDown.GetComponent<ObjectScaleCurve>();
		component.timeMax = portalScaleDuration;
		component.curveX = AnimationCurve.Linear(0f, num2, 1f, num);
		component.curveY = AnimationCurve.Linear(0f, num2, 1f, num);
		component.curveZ = AnimationCurve.Linear(0f, num2, 1f, num);
		component.overallCurve = AnimationCurve.EaseInOut(0f, num2, 1f, num);
		((Behaviour)component).enabled = true;
	}

	public override void OnExit()
	{
		base.OnExit();
	}
}
