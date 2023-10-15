using System.Linq;
using KinematicCharacterController;
using RoR2;
using RoR2.Navigation;
using UnityEngine;

namespace EntityStates.ImpBossMonster;

public class BlinkState : BaseState
{
	private Transform modelTransform;

	[SerializeField]
	public bool disappearWhileBlinking;

	[SerializeField]
	public GameObject blinkPrefab;

	[SerializeField]
	public GameObject blinkDestinationPrefab;

	[SerializeField]
	public Material destealthMaterial;

	private Vector3 blinkDestination = Vector3.zero;

	private Vector3 blinkStart = Vector3.zero;

	[SerializeField]
	public float duration = 0.3f;

	[SerializeField]
	public float exitDuration;

	[SerializeField]
	public float destinationAlertDuration;

	[SerializeField]
	public float blinkDistance = 25f;

	[SerializeField]
	public string beginSoundString;

	[SerializeField]
	public string endSoundString;

	[SerializeField]
	public float blastAttackRadius;

	[SerializeField]
	public float blastAttackDamageCoefficient;

	[SerializeField]
	public float blastAttackForce;

	[SerializeField]
	public float blastAttackProcCoefficient;

	private Animator animator;

	private CharacterModel characterModel;

	private HurtBoxGroup hurtboxGroup;

	private ChildLocator childLocator;

	private GameObject blinkDestinationInstance;

	private bool isExiting;

	private bool hasBlinked;

	public override void OnEnter()
	{
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		Util.PlaySound(beginSoundString, base.gameObject);
		modelTransform = GetModelTransform();
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			animator = ((Component)modelTransform).GetComponent<Animator>();
			characterModel = ((Component)modelTransform).GetComponent<CharacterModel>();
			hurtboxGroup = ((Component)modelTransform).GetComponent<HurtBoxGroup>();
			childLocator = ((Component)modelTransform).GetComponent<ChildLocator>();
		}
		if (disappearWhileBlinking)
		{
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
			if (Object.op_Implicit((Object)(object)childLocator))
			{
				((Component)childLocator.FindChild("DustCenter")).gameObject.SetActive(false);
			}
		}
		if (Object.op_Implicit((Object)(object)base.characterMotor))
		{
			((Behaviour)base.characterMotor).enabled = false;
		}
		base.gameObject.layer = LayerIndex.fakeActor.intVal;
		((BaseCharacterController)base.characterMotor).Motor.RebuildCollidableLayers();
		CalculateBlinkDestination();
		CreateBlinkEffect(Util.GetCorePosition(base.gameObject));
	}

	private void CalculateBlinkDestination()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector3.zero;
		Ray aimRay = GetAimRay();
		BullseyeSearch bullseyeSearch = new BullseyeSearch();
		bullseyeSearch.searchOrigin = aimRay.origin;
		bullseyeSearch.searchDirection = aimRay.direction;
		bullseyeSearch.maxDistanceFilter = blinkDistance;
		bullseyeSearch.teamMaskFilter = TeamMask.allButNeutral;
		bullseyeSearch.filterByLoS = false;
		bullseyeSearch.teamMaskFilter.RemoveTeam(TeamComponent.GetObjectTeam(base.gameObject));
		bullseyeSearch.sortMode = BullseyeSearch.SortMode.Angle;
		bullseyeSearch.RefreshCandidates();
		HurtBox hurtBox = bullseyeSearch.GetResults().FirstOrDefault();
		if (Object.op_Implicit((Object)(object)hurtBox))
		{
			val = ((Component)hurtBox).transform.position - base.transform.position;
		}
		blinkDestination = base.transform.position;
		blinkStart = base.transform.position;
		NodeGraph groundNodes = SceneInfo.instance.groundNodes;
		NodeGraph.NodeIndex nodeIndex = groundNodes.FindClosestNode(base.transform.position + val, base.characterBody.hullClassification);
		groundNodes.GetNodePosition(nodeIndex, out blinkDestination);
		blinkDestination += base.transform.position - base.characterBody.footPosition;
		base.characterDirection.forward = val;
	}

	private void CreateBlinkEffect(Vector3 origin)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)blinkPrefab))
		{
			EffectData effectData = new EffectData();
			effectData.rotation = Util.QuaternionSafeLookRotation(blinkDestination - blinkStart);
			effectData.origin = origin;
			EffectManager.SpawnEffect(blinkPrefab, effectData, transmit: false);
		}
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

	public override void FixedUpdate()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		if (Object.op_Implicit((Object)(object)base.characterMotor))
		{
			base.characterMotor.velocity = Vector3.zero;
		}
		if (!hasBlinked)
		{
			SetPosition(Vector3.Lerp(blinkStart, blinkDestination, base.fixedAge / duration));
		}
		if (base.fixedAge >= duration - destinationAlertDuration && !hasBlinked)
		{
			hasBlinked = true;
			if (Object.op_Implicit((Object)(object)blinkDestinationPrefab))
			{
				blinkDestinationInstance = Object.Instantiate<GameObject>(blinkDestinationPrefab, blinkDestination, Quaternion.identity);
				blinkDestinationInstance.GetComponent<ScaleParticleSystemDuration>().newDuration = destinationAlertDuration;
			}
			SetPosition(blinkDestination);
		}
		if (base.fixedAge >= duration)
		{
			ExitCleanup();
		}
		if (base.fixedAge >= duration + exitDuration && base.isAuthority)
		{
			outer.SetNextStateToMain();
		}
	}

	private void ExitCleanup()
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		if (isExiting)
		{
			return;
		}
		isExiting = true;
		base.gameObject.layer = LayerIndex.defaultLayer.intVal;
		((BaseCharacterController)base.characterMotor).Motor.RebuildCollidableLayers();
		Util.PlaySound(endSoundString, base.gameObject);
		CreateBlinkEffect(Util.GetCorePosition(base.gameObject));
		modelTransform = GetModelTransform();
		if (blastAttackDamageCoefficient > 0f)
		{
			BlastAttack blastAttack = new BlastAttack();
			blastAttack.attacker = base.gameObject;
			blastAttack.inflictor = base.gameObject;
			blastAttack.teamIndex = TeamComponent.GetObjectTeam(base.gameObject);
			blastAttack.baseDamage = damageStat * blastAttackDamageCoefficient;
			blastAttack.baseForce = blastAttackForce;
			blastAttack.position = blinkDestination;
			blastAttack.radius = blastAttackRadius;
			blastAttack.falloffModel = BlastAttack.FalloffModel.Linear;
			blastAttack.attackerFiltering = AttackerFiltering.NeverHitSelf;
			blastAttack.Fire();
		}
		if (disappearWhileBlinking)
		{
			if (Object.op_Implicit((Object)(object)modelTransform) && Object.op_Implicit((Object)(object)destealthMaterial))
			{
				TemporaryOverlay temporaryOverlay = ((Component)animator).gameObject.AddComponent<TemporaryOverlay>();
				temporaryOverlay.duration = 1f;
				temporaryOverlay.destroyComponentOnEnd = true;
				temporaryOverlay.originalMaterial = destealthMaterial;
				temporaryOverlay.inspectorCharacterModel = ((Component)animator).gameObject.GetComponent<CharacterModel>();
				temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
				temporaryOverlay.animateShaderAlpha = true;
			}
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
			if (Object.op_Implicit((Object)(object)childLocator))
			{
				((Component)childLocator.FindChild("DustCenter")).gameObject.SetActive(true);
			}
			PlayAnimation("Gesture, Additive", "BlinkEnd", "BlinkEnd.playbackRate", exitDuration);
		}
		if (Object.op_Implicit((Object)(object)blinkDestinationInstance))
		{
			EntityState.Destroy((Object)(object)blinkDestinationInstance);
		}
		if (Object.op_Implicit((Object)(object)base.characterMotor))
		{
			((Behaviour)base.characterMotor).enabled = true;
		}
	}

	public override void OnExit()
	{
		base.OnExit();
		ExitCleanup();
	}
}
