using System.Collections.Generic;
using RoR2.Audio;
using RoR2.Networking;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public class OverlapAttack
{
	private struct OverlapInfo
	{
		public HurtBox hurtBox;

		public Vector3 hitPosition;

		public Vector3 pushDirection;
	}

	public struct AttackInfo
	{
		public GameObject attacker;

		public GameObject inflictor;

		public float damage;

		public bool isCrit;

		public float procCoefficient;

		public DamageColorIndex damageColorIndex;

		public DamageType damageType;

		public Vector3 forceVector;
	}

	private class OverlapAttackMessage : MessageBase
	{
		public GameObject attacker;

		public GameObject inflictor;

		public float damage;

		public bool isCrit;

		public ProcChainMask procChainMask;

		public float procCoefficient;

		public DamageColorIndex damageColorIndex;

		public DamageType damageType;

		public Vector3 forceVector;

		public float pushAwayForce;

		public readonly List<OverlapInfo> overlapInfoList = new List<OverlapInfo>();

		public override void Serialize(NetworkWriter writer)
		{
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
			((MessageBase)this).Serialize(writer);
			writer.Write(attacker);
			writer.Write(inflictor);
			writer.Write(damage);
			writer.Write(isCrit);
			writer.Write(procChainMask);
			writer.Write(procCoefficient);
			writer.Write(damageColorIndex);
			writer.Write(damageType);
			writer.Write(forceVector);
			writer.Write(pushAwayForce);
			writer.WritePackedUInt32((uint)overlapInfoList.Count);
			foreach (OverlapInfo overlapInfo in overlapInfoList)
			{
				writer.Write(HurtBoxReference.FromHurtBox(overlapInfo.hurtBox));
				writer.Write(overlapInfo.hitPosition);
				writer.Write(overlapInfo.pushDirection);
			}
		}

		public override void Deserialize(NetworkReader reader)
		{
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
			((MessageBase)this).Deserialize(reader);
			attacker = reader.ReadGameObject();
			inflictor = reader.ReadGameObject();
			damage = reader.ReadSingle();
			isCrit = reader.ReadBoolean();
			procChainMask = reader.ReadProcChainMask();
			procCoefficient = reader.ReadSingle();
			damageColorIndex = reader.ReadDamageColorIndex();
			damageType = reader.ReadDamageType();
			forceVector = reader.ReadVector3();
			pushAwayForce = reader.ReadSingle();
			overlapInfoList.Clear();
			int i = 0;
			for (int num = (int)reader.ReadPackedUInt32(); i < num; i++)
			{
				OverlapInfo item = default(OverlapInfo);
				GameObject obj = reader.ReadHurtBoxReference().ResolveGameObject();
				item.hurtBox = ((obj != null) ? obj.GetComponent<HurtBox>() : null);
				item.hitPosition = reader.ReadVector3();
				item.pushDirection = reader.ReadVector3();
				overlapInfoList.Add(item);
			}
		}
	}

	public GameObject attacker;

	public GameObject inflictor;

	public TeamIndex teamIndex;

	public AttackerFiltering attackerFiltering = AttackerFiltering.NeverHitSelf;

	public Vector3 forceVector = Vector3.zero;

	public float pushAwayForce;

	public float damage = 1f;

	public bool isCrit;

	public ProcChainMask procChainMask;

	public float procCoefficient = 1f;

	public HitBoxGroup hitBoxGroup;

	public GameObject hitEffectPrefab;

	public NetworkSoundEventIndex impactSound = NetworkSoundEventIndex.Invalid;

	public DamageColorIndex damageColorIndex;

	public DamageType damageType;

	public int maximumOverlapTargets = 100;

	private readonly List<HealthComponent> ignoredHealthComponentList = new List<HealthComponent>();

	private readonly List<OverlapInfo> overlapList = new List<OverlapInfo>();

	private static readonly OverlapAttackMessage incomingMessage = new OverlapAttackMessage();

	private static readonly OverlapAttackMessage outgoingMessage = new OverlapAttackMessage();

	public Vector3 lastFireAverageHitPosition { get; private set; }

	private bool HurtBoxPassesFilter(HurtBox hurtBox)
	{
		if (!Object.op_Implicit((Object)(object)hurtBox.healthComponent))
		{
			return true;
		}
		if ((Object)(object)((Component)hurtBox.healthComponent).gameObject == (Object)(object)attacker && attackerFiltering == AttackerFiltering.NeverHitSelf)
		{
			return false;
		}
		if ((Object)(object)attacker == (Object)null && (Object)(object)((Component)hurtBox.healthComponent).gameObject.GetComponent<MaulingRock>() != (Object)null)
		{
			return false;
		}
		if (ignoredHealthComponentList.Contains(hurtBox.healthComponent))
		{
			return false;
		}
		if (!FriendlyFireManager.ShouldDirectHitProceed(hurtBox.healthComponent, teamIndex))
		{
			return false;
		}
		return true;
	}

	public bool Fire(List<HurtBox> hitResults = null)
	{
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0274: Unknown result type (might be due to invalid IL or missing references)
		//IL_0279: Unknown result type (might be due to invalid IL or missing references)
		//IL_0294: Unknown result type (might be due to invalid IL or missing references)
		//IL_0296: Unknown result type (might be due to invalid IL or missing references)
		//IL_029d: Unknown result type (might be due to invalid IL or missing references)
		//IL_029f: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ae: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)hitBoxGroup))
		{
			return false;
		}
		HitBox[] hitBoxes = hitBoxGroup.hitBoxes;
		foreach (HitBox hitBox in hitBoxes)
		{
			if (!Object.op_Implicit((Object)(object)hitBox) || !((Behaviour)hitBox).enabled || !Object.op_Implicit((Object)(object)((Component)hitBox).gameObject) || !((Component)hitBox).gameObject.activeInHierarchy || !Object.op_Implicit((Object)(object)((Component)hitBox).transform))
			{
				continue;
			}
			Transform transform = ((Component)hitBox).transform;
			Vector3 position = transform.position;
			Vector3 val = transform.lossyScale * 0.5f;
			Quaternion rotation = transform.rotation;
			if (float.IsInfinity(val.x) || float.IsInfinity(val.y) || float.IsInfinity(val.z))
			{
				Chat.AddMessage("Aborting OverlapAttack.Fire: hitBoxHalfExtents are infinite.");
				continue;
			}
			if (float.IsNaN(val.x) || float.IsNaN(val.y) || float.IsNaN(val.z))
			{
				Chat.AddMessage("Aborting OverlapAttack.Fire: hitBoxHalfExtents are NaN.");
				continue;
			}
			if (float.IsInfinity(position.x) || float.IsInfinity(position.y) || float.IsInfinity(position.z))
			{
				Chat.AddMessage("Aborting OverlapAttack.Fire: hitBoxCenter is infinite.");
				continue;
			}
			if (float.IsNaN(position.x) || float.IsNaN(position.y) || float.IsNaN(position.z))
			{
				Chat.AddMessage("Aborting OverlapAttack.Fire: hitBoxCenter is NaN.");
				continue;
			}
			if (float.IsInfinity(rotation.x) || float.IsInfinity(rotation.y) || float.IsInfinity(rotation.z) || float.IsInfinity(rotation.w))
			{
				Chat.AddMessage("Aborting OverlapAttack.Fire: hitBoxRotation is infinite.");
				continue;
			}
			if (float.IsNaN(rotation.x) || float.IsNaN(rotation.y) || float.IsNaN(rotation.z) || float.IsNaN(rotation.w))
			{
				Chat.AddMessage("Aborting OverlapAttack.Fire: hitBoxRotation is NaN.");
				continue;
			}
			Collider[] array = Physics.OverlapBox(position, val, rotation, LayerMask.op_Implicit(LayerIndex.entityPrecise.mask));
			int num = array.Length;
			int num2 = 0;
			for (int j = 0; j < num; j++)
			{
				if (Object.op_Implicit((Object)(object)array[j]))
				{
					HurtBox component = ((Component)array[j]).GetComponent<HurtBox>();
					if (Object.op_Implicit((Object)(object)component) && HurtBoxPassesFilter(component) && Object.op_Implicit((Object)(object)((Component)component).transform))
					{
						Vector3 position2 = ((Component)component).transform.position;
						List<OverlapInfo> list = overlapList;
						OverlapInfo item = new OverlapInfo
						{
							hurtBox = component,
							hitPosition = position2
						};
						Vector3 val2 = position2 - position;
						item.pushDirection = ((Vector3)(ref val2)).normalized;
						list.Add(item);
						ignoredHealthComponentList.Add(component.healthComponent);
						hitResults?.Add(component);
						num2++;
					}
					if (num2 >= maximumOverlapTargets)
					{
						break;
					}
				}
			}
		}
		ProcessHits(overlapList);
		bool result = overlapList.Count > 0;
		overlapList.Clear();
		return result;
	}

	[NetworkMessageHandler(msgType = 71, client = false, server = true)]
	public static void HandleOverlapAttackHits(NetworkMessage netMsg)
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		netMsg.ReadMessage<OverlapAttackMessage>(incomingMessage);
		PerformDamage(incomingMessage.attacker, incomingMessage.inflictor, incomingMessage.damage, incomingMessage.isCrit, incomingMessage.procChainMask, incomingMessage.procCoefficient, incomingMessage.damageColorIndex, incomingMessage.damageType, incomingMessage.forceVector, incomingMessage.pushAwayForce, incomingMessage.overlapInfoList);
	}

	private void ProcessHits(List<OverlapInfo> hitList)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_028e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0293: Unknown result type (might be due to invalid IL or missing references)
		if (hitList.Count == 0)
		{
			return;
		}
		Vector3 val = Vector3.zero;
		float num = 1f / (float)hitList.Count;
		for (int i = 0; i < hitList.Count; i++)
		{
			OverlapInfo overlapInfo = hitList[i];
			if (Object.op_Implicit((Object)(object)hitEffectPrefab))
			{
				Vector3 forward = -hitList[i].pushDirection;
				EffectManager.SpawnEffect(hitEffectPrefab, new EffectData
				{
					origin = overlapInfo.hitPosition,
					rotation = Util.QuaternionSafeLookRotation(forward),
					networkSoundEventIndex = impactSound
				}, transmit: true);
			}
			val += overlapInfo.hitPosition * num;
			SurfaceDefProvider surfaceDefProvider = (Object.op_Implicit((Object)(object)overlapInfo.hurtBox) ? ((Component)overlapInfo.hurtBox).GetComponent<SurfaceDefProvider>() : null);
			if (!Object.op_Implicit((Object)(object)surfaceDefProvider) || !Object.op_Implicit((Object)(object)surfaceDefProvider.surfaceDef))
			{
				continue;
			}
			SurfaceDef objectSurfaceDef = SurfaceDefProvider.GetObjectSurfaceDef(hitList[i].hurtBox.collider, hitList[i].hitPosition);
			if (Object.op_Implicit((Object)(object)objectSurfaceDef))
			{
				if (Object.op_Implicit((Object)(object)objectSurfaceDef.impactEffectPrefab))
				{
					EffectManager.SpawnEffect(objectSurfaceDef.impactEffectPrefab, new EffectData
					{
						origin = overlapInfo.hitPosition,
						rotation = ((overlapInfo.pushDirection == Vector3.zero) ? Quaternion.identity : Util.QuaternionSafeLookRotation(overlapInfo.pushDirection)),
						color = Color32.op_Implicit(objectSurfaceDef.approximateColor),
						scale = 2f
					}, transmit: true);
				}
				if (objectSurfaceDef.impactSoundString != null && objectSurfaceDef.impactSoundString.Length != 0)
				{
					Util.PlaySound(objectSurfaceDef.impactSoundString, ((Component)hitList[i].hurtBox).gameObject);
				}
			}
		}
		lastFireAverageHitPosition = val;
		if (NetworkServer.active)
		{
			PerformDamage(attacker, inflictor, damage, isCrit, procChainMask, procCoefficient, damageColorIndex, damageType, forceVector, pushAwayForce, hitList);
			return;
		}
		outgoingMessage.attacker = attacker;
		outgoingMessage.inflictor = inflictor;
		outgoingMessage.damage = damage;
		outgoingMessage.isCrit = isCrit;
		outgoingMessage.procChainMask = procChainMask;
		outgoingMessage.procCoefficient = procCoefficient;
		outgoingMessage.damageColorIndex = damageColorIndex;
		outgoingMessage.damageType = damageType;
		outgoingMessage.forceVector = forceVector;
		outgoingMessage.pushAwayForce = pushAwayForce;
		Util.CopyList(hitList, outgoingMessage.overlapInfoList);
		((NetworkManager)PlatformSystems.networkManager).client.connection.SendByChannel((short)71, (MessageBase)(object)outgoingMessage, QosChannelIndex.defaultReliable.intVal);
	}

	private static void PerformDamage(GameObject attacker, GameObject inflictor, float damage, bool isCrit, ProcChainMask procChainMask, float procCoefficient, DamageColorIndex damageColorIndex, DamageType damageType, Vector3 forceVector, float pushAwayForce, List<OverlapInfo> hitList)
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < hitList.Count; i++)
		{
			OverlapInfo overlapInfo = hitList[i];
			if (Object.op_Implicit((Object)(object)overlapInfo.hurtBox))
			{
				HealthComponent healthComponent = overlapInfo.hurtBox.healthComponent;
				if (Object.op_Implicit((Object)(object)healthComponent))
				{
					DamageInfo damageInfo = new DamageInfo();
					damageInfo.attacker = attacker;
					damageInfo.inflictor = inflictor;
					damageInfo.force = forceVector + pushAwayForce * overlapInfo.pushDirection;
					damageInfo.damage = damage;
					damageInfo.crit = isCrit;
					damageInfo.position = overlapInfo.hitPosition;
					damageInfo.procChainMask = procChainMask;
					damageInfo.procCoefficient = procCoefficient;
					damageInfo.damageColorIndex = damageColorIndex;
					damageInfo.damageType = damageType;
					damageInfo.ModifyDamageInfo(overlapInfo.hurtBox.damageModifier);
					healthComponent.TakeDamage(damageInfo);
					GlobalEventManager.instance.OnHitEnemy(damageInfo, ((Component)healthComponent).gameObject);
					GlobalEventManager.instance.OnHitAll(damageInfo, ((Component)healthComponent).gameObject);
				}
			}
		}
	}

	public void ResetIgnoredHealthComponents()
	{
		ignoredHealthComponentList.Clear();
	}
}
