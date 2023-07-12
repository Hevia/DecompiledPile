using KinematicCharacterController;
using RoR2;
using RoR2.Navigation;
using UnityEngine;

namespace EntityStates.ImpMonster;

public class BlinkState : BaseState
{
	private Transform modelTransform;

	public static GameObject blinkPrefab;

	public static Material destealthMaterial;

	private float stopwatch;

	private Vector3 blinkDestination = Vector3.zero;

	private Vector3 blinkStart = Vector3.zero;

	public static float duration = 0.3f;

	public static float blinkDistance = 25f;

	public static string beginSoundString;

	public static string endSoundString;

	private Animator animator;

	private CharacterModel characterModel;

	private HurtBoxGroup hurtboxGroup;

	public override void OnEnter()
	{
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		Util.PlaySound(beginSoundString, base.gameObject);
		modelTransform = GetModelTransform();
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			animator = ((Component)modelTransform).GetComponent<Animator>();
			characterModel = ((Component)modelTransform).GetComponent<CharacterModel>();
			hurtboxGroup = ((Component)modelTransform).GetComponent<HurtBoxGroup>();
		}
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
		Vector3 val = base.inputBank.moveVector * blinkDistance;
		blinkDestination = base.transform.position;
		blinkStart = base.transform.position;
		NodeGraph groundNodes = SceneInfo.instance.groundNodes;
		NodeGraph.NodeIndex nodeIndex = groundNodes.FindClosestNode(base.transform.position + val, base.characterBody.hullClassification);
		groundNodes.GetNodePosition(nodeIndex, out blinkDestination);
		blinkDestination += base.transform.position - base.characterBody.footPosition;
		CreateBlinkEffect(Util.GetCorePosition(base.gameObject));
	}

	private void CreateBlinkEffect(Vector3 origin)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		EffectData effectData = new EffectData();
		effectData.rotation = Util.QuaternionSafeLookRotation(blinkDestination - blinkStart);
		effectData.origin = origin;
		EffectManager.SpawnEffect(blinkPrefab, effectData, transmit: false);
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
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		stopwatch += Time.fixedDeltaTime;
		if (Object.op_Implicit((Object)(object)base.characterMotor) && Object.op_Implicit((Object)(object)base.characterDirection))
		{
			base.characterMotor.velocity = Vector3.zero;
		}
		SetPosition(Vector3.Lerp(blinkStart, blinkDestination, stopwatch / duration));
		if (stopwatch >= duration && base.isAuthority)
		{
			outer.SetNextStateToMain();
		}
	}

	public override void OnExit()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		Util.PlaySound(endSoundString, base.gameObject);
		CreateBlinkEffect(Util.GetCorePosition(base.gameObject));
		modelTransform = GetModelTransform();
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
		if (Object.op_Implicit((Object)(object)base.characterMotor))
		{
			((Behaviour)base.characterMotor).enabled = true;
		}
		PlayAnimation("Gesture, Additive", "BlinkEnd");
		base.OnExit();
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.PrioritySkill;
	}
}
