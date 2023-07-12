using RoR2;
using RoR2.Navigation;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Vulture;

public class FlyToLand : BaseSkillState
{
	private float duration;

	private Vector3 targetPosition;

	public static float speedMultiplier;

	private ICharacterGravityParameterProvider characterGravityParameterProvider;

	private ICharacterFlightParameterProvider characterFlightParameterProvider;

	public override void OnEnter()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		characterGravityParameterProvider = base.gameObject.GetComponent<ICharacterGravityParameterProvider>();
		characterFlightParameterProvider = base.gameObject.GetComponent<ICharacterFlightParameterProvider>();
		Vector3 footPosition = GetFootPosition();
		if (base.isAuthority)
		{
			bool flag = false;
			NodeGraph groundNodes = SceneInfo.instance.groundNodes;
			if (Object.op_Implicit((Object)(object)groundNodes))
			{
				NodeGraph.NodeIndex nodeIndex = groundNodes.FindClosestNodeWithFlagConditions(base.transform.position, base.characterBody.hullClassification, NodeFlags.None, NodeFlags.None, preventOverhead: false);
				flag = nodeIndex != NodeGraph.NodeIndex.invalid && groundNodes.GetNodePosition(nodeIndex, out targetPosition);
			}
			if (!flag)
			{
				outer.SetNextState(new Fly
				{
					activatorSkillSlot = base.activatorSkillSlot
				});
				duration = 0f;
				targetPosition = footPosition;
				return;
			}
		}
		Vector3 val = targetPosition - footPosition;
		float num = moveSpeedStat * speedMultiplier;
		duration = ((Vector3)(ref val)).magnitude / num;
		if (characterGravityParameterProvider != null)
		{
			CharacterGravityParameters gravityParameters = characterGravityParameterProvider.gravityParameters;
			gravityParameters.channeledAntiGravityGranterCount++;
			characterGravityParameterProvider.gravityParameters = gravityParameters;
		}
		if (characterFlightParameterProvider != null)
		{
			CharacterFlightParameters flightParameters = characterFlightParameterProvider.flightParameters;
			flightParameters.channeledFlightGranterCount++;
			characterFlightParameterProvider.flightParameters = flightParameters;
		}
	}

	private Vector3 GetFootPosition()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)base.characterBody))
		{
			return base.characterBody.footPosition;
		}
		return base.transform.position;
	}

	public override void FixedUpdate()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		Vector3 footPosition = GetFootPosition();
		CharacterMotor obj = base.characterMotor;
		Vector3 val = targetPosition - footPosition;
		obj.moveDirection = ((Vector3)(ref val)).normalized * speedMultiplier;
		if (base.isAuthority && (base.characterMotor.isGrounded || duration <= base.fixedAge))
		{
			outer.SetNextStateToMain();
		}
	}

	public override void OnExit()
	{
		if (characterFlightParameterProvider != null)
		{
			CharacterFlightParameters flightParameters = characterFlightParameterProvider.flightParameters;
			flightParameters.channeledFlightGranterCount--;
			characterFlightParameterProvider.flightParameters = flightParameters;
		}
		if (characterGravityParameterProvider != null)
		{
			CharacterGravityParameters gravityParameters = characterGravityParameterProvider.gravityParameters;
			gravityParameters.channeledAntiGravityGranterCount--;
			characterGravityParameterProvider.gravityParameters = gravityParameters;
		}
		Animator modelAnimator = GetModelAnimator();
		if (Object.op_Implicit((Object)(object)modelAnimator))
		{
			modelAnimator.SetFloat("Flying", 0f);
		}
		base.OnExit();
	}

	public override void OnSerialize(NetworkWriter writer)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		base.OnSerialize(writer);
		writer.Write(targetPosition);
	}

	public override void OnDeserialize(NetworkReader reader)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		base.OnDeserialize(reader);
		targetPosition = reader.ReadVector3();
	}
}
