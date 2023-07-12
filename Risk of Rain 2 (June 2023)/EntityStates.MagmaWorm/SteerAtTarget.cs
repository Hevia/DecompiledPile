using System;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.MagmaWorm;

public class SteerAtTarget : BaseSkillState
{
	private WormBodyPositionsDriver wormBodyPositionsDriver;

	private Vector3? targetPosition;

	private static readonly float fastTurnThreshold = Mathf.Cos(MathF.PI / 6f);

	private static readonly float slowTurnThreshold = Mathf.Cos(MathF.PI / 3f);

	private static readonly float fastTurnRate = 180f;

	private static readonly float slowTurnRate = 90f;

	public override void OnEnter()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		wormBodyPositionsDriver = GetComponent<WormBodyPositionsDriver>();
		if (base.isAuthority)
		{
			Ray aimRay = GetAimRay();
			if (Util.CharacterRaycast(base.gameObject, aimRay, out var hitInfo, 1000f, LayerIndex.CommonMasks.bullet, (QueryTriggerInteraction)0))
			{
				targetPosition = ((RaycastHit)(ref hitInfo)).point;
			}
		}
	}

	public override void OnExit()
	{
		base.OnExit();
	}

	public override void FixedUpdate()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		if (NetworkServer.active && targetPosition.HasValue)
		{
			Vector3 val = targetPosition.Value - wormBodyPositionsDriver.chaserPosition;
			if (val != Vector3.zero && wormBodyPositionsDriver.chaserVelocity != Vector3.zero)
			{
				Vector3 normalized = ((Vector3)(ref val)).normalized;
				Vector3 chaserVelocity = wormBodyPositionsDriver.chaserVelocity;
				Vector3 normalized2 = ((Vector3)(ref chaserVelocity)).normalized;
				float num = Vector3.Dot(normalized, normalized2);
				float num2 = 0f;
				if (num >= slowTurnThreshold)
				{
					num2 = slowTurnRate;
					if (num >= fastTurnThreshold)
					{
						num2 = fastTurnRate;
					}
				}
				if (num2 != 0f)
				{
					wormBodyPositionsDriver.chaserVelocity = Vector3.RotateTowards(wormBodyPositionsDriver.chaserVelocity, val, MathF.PI / 180f * num2 * Time.fixedDeltaTime, 0f);
				}
			}
		}
		if (base.isAuthority && !IsKeyDownAuthority())
		{
			outer.SetNextStateToMain();
		}
	}

	public override void OnSerialize(NetworkWriter writer)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		base.OnSerialize(writer);
		if (targetPosition.HasValue)
		{
			writer.Write(true);
			writer.Write(targetPosition.Value);
		}
		else
		{
			writer.Write(false);
		}
	}

	public override void OnDeserialize(NetworkReader reader)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		base.OnDeserialize(reader);
		if (reader.ReadBoolean())
		{
			targetPosition = reader.ReadVector3();
		}
		else
		{
			targetPosition = null;
		}
	}
}
