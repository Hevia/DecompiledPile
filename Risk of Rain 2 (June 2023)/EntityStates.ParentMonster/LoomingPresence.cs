using KinematicCharacterController;
using RoR2;
using RoR2.Navigation;
using UnityEngine;

namespace EntityStates.ParentMonster;

public class LoomingPresence : BaseState
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

	public static float destealthDuration;

	private CharacterModel characterModel;

	private HurtBoxGroup hurtboxGroup;

	public override void OnEnter()
	{
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		Util.PlaySound(beginSoundString, base.gameObject);
		modelTransform = GetModelTransform();
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
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
		if (base.isAuthority)
		{
			Vector3 val = base.inputBank.aimDirection * blinkDistance;
			blinkDestination = base.transform.position;
			blinkStart = base.transform.position;
			NodeGraph groundNodes = SceneInfo.instance.groundNodes;
			NodeGraph.NodeIndex nodeIndex = groundNodes.FindClosestNode(base.transform.position + val, base.characterBody.hullClassification);
			groundNodes.GetNodePosition(nodeIndex, out blinkDestination);
			blinkDestination += base.transform.position - base.characterBody.footPosition;
			val = blinkDestination - blinkStart;
			CreateBlinkEffect(Util.GetCorePosition(base.gameObject), val);
		}
	}

	private void CreateBlinkEffect(Vector3 origin, Vector3 direction)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		EffectData effectData = new EffectData();
		effectData.rotation = Util.QuaternionSafeLookRotation(direction);
		effectData.origin = origin;
		EffectManager.SpawnEffect(blinkPrefab, effectData, transmit: true);
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
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		Util.PlaySound(endSoundString, base.gameObject);
		modelTransform = GetModelTransform();
		if (Object.op_Implicit((Object)(object)base.characterDirection))
		{
			CharacterDirection obj = base.characterDirection;
			Ray aimRay = GetAimRay();
			obj.forward = aimRay.direction;
		}
		if (Object.op_Implicit((Object)(object)modelTransform) && Object.op_Implicit((Object)(object)destealthMaterial))
		{
			TemporaryOverlay temporaryOverlay = ((Component)modelTransform).gameObject.AddComponent<TemporaryOverlay>();
			temporaryOverlay.duration = destealthDuration;
			temporaryOverlay.destroyComponentOnEnd = true;
			temporaryOverlay.originalMaterial = destealthMaterial;
			temporaryOverlay.inspectorCharacterModel = ((Component)modelTransform).gameObject.GetComponent<CharacterModel>();
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
		base.OnExit();
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.PrioritySkill;
	}
}
