using RoR2.VoidRaidCrab;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.VoidRaidCrab;

public class TurnState : GenericCharacterMain
{
	[SerializeField]
	public float duration = 1f;

	[SerializeField]
	public float turnDegrees = 22.5f;

	[SerializeField]
	public int maxNumConsecutiveTurns;

	[SerializeField]
	public string animationLayerName;

	[SerializeField]
	public string clockwiseAnimationStateName;

	[SerializeField]
	public string counterClockwiseAnimationStateName;

	[SerializeField]
	public string animationPlaybackRateParam;

	private CentralLegController centralLegController;

	private CentralLegController.SuppressBreaksRequest suppressBreaksRequest;

	private int turnCount = 1;

	private bool isClockwiseTurn;

	private bool canTurn;

	public override void OnEnter()
	{
		base.OnEnter();
		if (base.isAuthority)
		{
			centralLegController = GetComponent<CentralLegController>();
			if (Object.op_Implicit((Object)(object)centralLegController))
			{
				suppressBreaksRequest = centralLegController.SuppressBreaks();
			}
			float dirToAimDegrees = GetDirToAimDegrees();
			if (dirToAimDegrees >= turnDegrees)
			{
				isClockwiseTurn = true;
				canTurn = true;
			}
			else if (dirToAimDegrees <= 0f - turnDegrees)
			{
				isClockwiseTurn = false;
				canTurn = true;
			}
		}
		if (canTurn)
		{
			if (isClockwiseTurn)
			{
				PlayAnimation(animationLayerName, clockwiseAnimationStateName, animationPlaybackRateParam, duration);
			}
			else
			{
				PlayAnimation(animationLayerName, counterClockwiseAnimationStateName, animationPlaybackRateParam, duration);
			}
		}
		else if (base.isAuthority)
		{
			outer.SetNextStateToMain();
		}
	}

	public override void OnExit()
	{
		base.OnExit();
		suppressBreaksRequest?.Dispose();
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.isAuthority && base.fixedAge > duration)
		{
			if (Mathf.Abs(GetDirToAimDegrees()) >= turnDegrees && Object.op_Implicit((Object)(object)centralLegController) && !centralLegController.AreAnyBreaksPending() && turnCount < maxNumConsecutiveTurns)
			{
				TurnState turnState = new TurnState();
				turnState.turnCount = turnCount + 1;
				outer.SetNextState(turnState);
			}
			else
			{
				outer.SetNextStateToMain();
			}
		}
	}

	public float GetDirToAimDegrees()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		float num = 0f;
		if (Object.op_Implicit((Object)(object)base.inputBank) && Object.op_Implicit((Object)(object)base.characterDirection))
		{
			Vector3 val = base.inputBank.aimDirection;
			val.y = 0f;
			((Vector3)(ref val)).Normalize();
			Vector3 forward = base.characterDirection.forward;
			forward.y = 0f;
			((Vector3)(ref forward)).Normalize();
			num = Mathf.Acos(Vector3.Dot(forward, val)) * 57.29578f;
			if (Vector3.Dot(Vector3.Cross(forward, val), Vector3.up) < 0f)
			{
				num *= -1f;
			}
		}
		return num;
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Frozen;
	}

	public override void OnSerialize(NetworkWriter writer)
	{
		base.OnSerialize(writer);
		writer.Write(isClockwiseTurn);
		writer.Write(canTurn);
	}

	public override void OnDeserialize(NetworkReader reader)
	{
		base.OnDeserialize(reader);
		isClockwiseTurn = reader.ReadBoolean();
		canTurn = reader.ReadBoolean();
	}
}
