using System;
using System.Collections.Generic;
using RoR2.Networking;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public class ExperienceManager : MonoBehaviour
{
	[Serializable]
	private struct TimedExpAward
	{
		public float awardTime;

		public ulong awardAmount;

		public TeamIndex recipient;
	}

	private class CreateExpEffectMessage : MessageBase
	{
		public Vector3 origin;

		public GameObject targetBody;

		public ulong awardAmount;

		public override void Serialize(NetworkWriter writer)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			writer.Write(origin);
			writer.Write(targetBody);
			writer.WritePackedUInt64(awardAmount);
		}

		public override void Deserialize(NetworkReader reader)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			origin = reader.ReadVector3();
			targetBody = reader.ReadGameObject();
			awardAmount = reader.ReadPackedUInt64();
		}
	}

	private float localTime;

	private List<TimedExpAward> pendingAwards = new List<TimedExpAward>();

	private float nextAward;

	private const float minOrbTravelTime = 0.5f;

	public const float maxOrbTravelTime = 2f;

	public static readonly float[] orbTimeOffsetSequence = new float[16]
	{
		0.841f, 0.394f, 0.783f, 0.799f, 0.912f, 0.197f, 0.335f, 0.768f, 0.278f, 0.554f,
		0.477f, 0.629f, 0.365f, 0.513f, 0.953f, 0.917f
	};

	private static CreateExpEffectMessage currentOutgoingCreateExpEffectMessage = new CreateExpEffectMessage();

	private static CreateExpEffectMessage currentIncomingCreateExpEffectMessage = new CreateExpEffectMessage();

	public static ExperienceManager instance { get; private set; }

	private static float CalcOrbTravelTime(float timeOffset)
	{
		return 0.5f + 1.5f * timeOffset;
	}

	private void OnEnable()
	{
		if (Object.op_Implicit((Object)(object)instance) && (Object)(object)instance != (Object)(object)this)
		{
			Debug.LogError((object)"Only one ExperienceManager can exist at a time.");
		}
		else
		{
			instance = this;
		}
	}

	private void OnDisable()
	{
		if ((Object)(object)instance == (Object)(object)this)
		{
			instance = null;
		}
	}

	private void Start()
	{
		localTime = 0f;
		nextAward = float.PositiveInfinity;
	}

	private void FixedUpdate()
	{
		localTime += Time.fixedDeltaTime;
		if (pendingAwards.Count <= 0 || !(nextAward <= localTime))
		{
			return;
		}
		nextAward = float.PositiveInfinity;
		for (int num = pendingAwards.Count - 1; num >= 0; num--)
		{
			if (pendingAwards[num].awardTime <= localTime)
			{
				if (Object.op_Implicit((Object)(object)TeamManager.instance))
				{
					TeamManager.instance.GiveTeamExperience(pendingAwards[num].recipient, pendingAwards[num].awardAmount);
				}
				pendingAwards.RemoveAt(num);
			}
			else if (pendingAwards[num].awardTime < nextAward)
			{
				nextAward = pendingAwards[num].awardTime;
			}
		}
	}

	public void AwardExperience(Vector3 origin, CharacterBody body, ulong amount)
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		CharacterMaster master = body.master;
		if (!Object.op_Implicit((Object)(object)master))
		{
			return;
		}
		TeamIndex teamIndex = master.teamIndex;
		List<ulong> list = CalculateDenominations(amount);
		uint num = 0u;
		for (int i = 0; i < list.Count; i++)
		{
			AddPendingAward(localTime + 0.5f + 1.5f * orbTimeOffsetSequence[num], teamIndex, list[i]);
			num++;
			if (num >= orbTimeOffsetSequence.Length)
			{
				num = 0u;
			}
		}
		currentOutgoingCreateExpEffectMessage.awardAmount = amount;
		currentOutgoingCreateExpEffectMessage.origin = origin;
		currentOutgoingCreateExpEffectMessage.targetBody = ((Component)body).gameObject;
		NetworkServer.SendToAll((short)55, (MessageBase)(object)currentOutgoingCreateExpEffectMessage);
	}

	private void AddPendingAward(float awardTime, TeamIndex recipient, ulong awardAmount)
	{
		pendingAwards.Add(new TimedExpAward
		{
			awardTime = awardTime,
			recipient = recipient,
			awardAmount = awardAmount
		});
		if (nextAward > awardTime)
		{
			nextAward = awardTime;
		}
	}

	public List<ulong> CalculateDenominations(ulong total)
	{
		List<ulong> list = new List<ulong>();
		while (total != 0)
		{
			ulong num = (ulong)Math.Pow(6.0, Mathf.Floor(Mathf.Log((float)total, 6f)));
			total = Math.Max(total - num, 0uL);
			list.Add(num);
		}
		return list;
	}

	[NetworkMessageHandler(msgType = 55, client = true)]
	private static void HandleCreateExpEffect(NetworkMessage netMsg)
	{
		if (Object.op_Implicit((Object)(object)instance))
		{
			instance.HandleCreateExpEffectInternal(netMsg);
		}
	}

	private void HandleCreateExpEffectInternal(NetworkMessage netMsg)
	{
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		netMsg.ReadMessage<CreateExpEffectMessage>(currentIncomingCreateExpEffectMessage);
		if (!SettingsConVars.cvExpAndMoneyEffects.value)
		{
			return;
		}
		GameObject targetBody = currentIncomingCreateExpEffectMessage.targetBody;
		if (!Object.op_Implicit((Object)(object)targetBody))
		{
			return;
		}
		HurtBox hurtBox = Util.FindBodyMainHurtBox(targetBody);
		Transform targetTransform = (Object.op_Implicit((Object)(object)hurtBox) ? ((Component)hurtBox).transform : targetBody.transform);
		List<ulong> list = CalculateDenominations(currentIncomingCreateExpEffectMessage.awardAmount);
		uint num = 0u;
		for (int i = 0; i < list.Count; i++)
		{
			ExperienceOrbBehavior component = Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/ExpOrb"), currentIncomingCreateExpEffectMessage.origin, Quaternion.identity).GetComponent<ExperienceOrbBehavior>();
			component.targetTransform = targetTransform;
			component.travelTime = CalcOrbTravelTime(orbTimeOffsetSequence[num]);
			component.exp = list[i];
			num++;
			if (num >= orbTimeOffsetSequence.Length)
			{
				num = 0u;
			}
		}
	}
}
