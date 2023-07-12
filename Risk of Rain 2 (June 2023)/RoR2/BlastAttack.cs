using System;
using HG;
using RoR2.Networking;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public class BlastAttack
{
	public enum FalloffModel
	{
		None,
		Linear,
		SweetSpot
	}

	public enum LoSType
	{
		None,
		NearestHit
	}

	public struct HitPoint
	{
		public HurtBox hurtBox;

		public Vector3 hitPosition;

		public Vector3 hitNormal;

		public float distanceSqr;
	}

	public struct Result
	{
		public int hitCount;

		public HitPoint[] hitPoints;
	}

	private struct BlastAttackDamageInfo
	{
		public GameObject attacker;

		public GameObject inflictor;

		public bool crit;

		public float damage;

		public DamageColorIndex damageColorIndex;

		public HurtBox.DamageModifier damageModifier;

		public DamageType damageType;

		public Vector3 force;

		public Vector3 position;

		public ProcChainMask procChainMask;

		public float procCoefficient;

		public HealthComponent hitHealthComponent;

		public bool canRejectForce;

		public void Write(NetworkWriter writer)
		{
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_008e: Unknown result type (might be due to invalid IL or missing references)
			writer.Write(attacker);
			writer.Write(inflictor);
			writer.Write(crit);
			writer.Write(damage);
			writer.Write(damageColorIndex);
			writer.Write((byte)damageModifier);
			writer.Write(damageType);
			writer.Write(force);
			writer.Write(position);
			writer.Write(procChainMask);
			writer.Write(procCoefficient);
			writer.Write(((NetworkBehaviour)hitHealthComponent).netId);
			writer.Write(canRejectForce);
		}

		public void Read(NetworkReader reader)
		{
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			attacker = reader.ReadGameObject();
			inflictor = reader.ReadGameObject();
			crit = reader.ReadBoolean();
			damage = reader.ReadSingle();
			damageColorIndex = reader.ReadDamageColorIndex();
			damageModifier = (HurtBox.DamageModifier)reader.ReadByte();
			damageType = reader.ReadDamageType();
			force = reader.ReadVector3();
			position = reader.ReadVector3();
			procChainMask = reader.ReadProcChainMask();
			procCoefficient = reader.ReadSingle();
			GameObject val = reader.ReadGameObject();
			hitHealthComponent = (Object.op_Implicit((Object)(object)val) ? val.GetComponent<HealthComponent>() : null);
			canRejectForce = reader.ReadBoolean();
		}
	}

	public GameObject attacker;

	public GameObject inflictor;

	public TeamIndex teamIndex;

	public AttackerFiltering attackerFiltering;

	public Vector3 position;

	public float radius;

	public FalloffModel falloffModel = FalloffModel.Linear;

	public float baseDamage;

	public float baseForce;

	public Vector3 bonusForce;

	public bool crit;

	public DamageType damageType;

	public DamageColorIndex damageColorIndex;

	public LoSType losType;

	public EffectIndex impactEffect = EffectIndex.Invalid;

	public bool canRejectForce = true;

	public ProcChainMask procChainMask;

	public float procCoefficient = 1f;

	private static readonly int initialBufferSize = 256;

	private static HitPoint[] hitPointsBuffer = new HitPoint[initialBufferSize];

	private static int[] hitOrderBuffer = new int[initialBufferSize];

	private static HealthComponent[] encounteredHealthComponentsBuffer = new HealthComponent[initialBufferSize];

	public Result Fire()
	{
		HitPoint[] array = CollectHits();
		HandleHits(array);
		Result result = default(Result);
		result.hitCount = array.Length;
		result.hitPoints = array;
		return result;
	}

	[NetworkMessageHandler(msgType = 75, client = false, server = true)]
	private static void HandleReportBlastAttackDamage(NetworkMessage netMsg)
	{
		NetworkReader reader = netMsg.reader;
		BlastAttackDamageInfo blastAttackDamageInfo = default(BlastAttackDamageInfo);
		blastAttackDamageInfo.Read(reader);
		PerformDamageServer(in blastAttackDamageInfo);
	}

	private HitPoint[] CollectHits()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0208: Unknown result type (might be due to invalid IL or missing references)
		//IL_020d: Unknown result type (might be due to invalid IL or missing references)
		//IL_039b: Unknown result type (might be due to invalid IL or missing references)
		//IL_039e: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03be: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0402: Unknown result type (might be due to invalid IL or missing references)
		//IL_0442: Unknown result type (might be due to invalid IL or missing references)
		//IL_0447: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = position;
		Collider[] array = Physics.OverlapSphere(val, radius, LayerMask.op_Implicit(LayerIndex.entityPrecise.mask));
		int num = array.Length;
		int num2 = 0;
		int encounteredHealthComponentsLength = 0;
		int hitOrderBufferLength = 0;
		ArrayUtils.EnsureCapacity<HitPoint>(ref hitPointsBuffer, num);
		ArrayUtils.EnsureCapacity<int>(ref hitOrderBuffer, num);
		ArrayUtils.EnsureCapacity<HealthComponent>(ref encounteredHealthComponentsBuffer, num);
		for (int i = 0; i < num; i++)
		{
			Collider val2 = array[i];
			HurtBox component = ((Component)val2).GetComponent<HurtBox>();
			if (!Object.op_Implicit((Object)(object)component))
			{
				continue;
			}
			HealthComponent healthComponent2 = component.healthComponent;
			if (!Object.op_Implicit((Object)(object)healthComponent2))
			{
				continue;
			}
			bool flag = true;
			switch (attackerFiltering)
			{
			case AttackerFiltering.Default:
				flag = true;
				break;
			case AttackerFiltering.AlwaysHitSelf:
				flag = true;
				if ((Object)(object)((Component)healthComponent2).gameObject == (Object)(object)attacker)
				{
					flag = false;
				}
				break;
			case AttackerFiltering.AlwaysHit:
				flag = false;
				break;
			case AttackerFiltering.NeverHitSelf:
				flag = true;
				if ((Object)(object)((Component)healthComponent2).gameObject == (Object)(object)attacker)
				{
					continue;
				}
				break;
			}
			if (!flag || FriendlyFireManager.ShouldSplashHitProceed(healthComponent2, teamIndex))
			{
				Vector3 val3 = ((Component)val2).transform.position;
				Vector3 hitNormal = val - val3;
				float sqrMagnitude = ((Vector3)(ref hitNormal)).sqrMagnitude;
				hitPointsBuffer[num2++] = new HitPoint
				{
					hurtBox = component,
					hitPosition = val3,
					hitNormal = hitNormal,
					distanceSqr = sqrMagnitude
				};
			}
		}
		if (true)
		{
			RaycastHit val4 = default(RaycastHit);
			for (int j = 0; j < num2; j++)
			{
				ref HitPoint reference = ref hitPointsBuffer[j];
				if (reference.hurtBox != null && reference.distanceSqr > 0f && reference.hurtBox.collider.Raycast(new Ray(val, -reference.hitNormal), ref val4, radius))
				{
					Vector3 val5 = val - ((RaycastHit)(ref val4)).point;
					reference.distanceSqr = ((Vector3)(ref val5)).sqrMagnitude;
					reference.hitPosition = ((RaycastHit)(ref val4)).point;
					reference.hitNormal = ((RaycastHit)(ref val4)).normal;
				}
			}
		}
		hitOrderBufferLength = num2;
		for (int k = 0; k < num2; k++)
		{
			hitOrderBuffer[k] = k;
		}
		bool flag2 = true;
		while (flag2)
		{
			flag2 = false;
			for (int l = 1; l < hitOrderBufferLength; l++)
			{
				int num3 = l - 1;
				if (hitPointsBuffer[hitOrderBuffer[num3]].distanceSqr > hitPointsBuffer[hitOrderBuffer[l]].distanceSqr)
				{
					Util.Swap(ref hitOrderBuffer[num3], ref hitOrderBuffer[l]);
					flag2 = true;
				}
			}
		}
		bool flag3 = losType == LoSType.None || losType == LoSType.NearestHit;
		for (int m = 0; m < hitOrderBufferLength; m++)
		{
			int num4 = hitOrderBuffer[m];
			ref HitPoint reference2 = ref hitPointsBuffer[num4];
			HealthComponent healthComponent3 = reference2.hurtBox.healthComponent;
			if (!EntityIsMarkedEncountered(healthComponent3))
			{
				MarkEntityAsEncountered(healthComponent3);
			}
			else if (flag3)
			{
				reference2.hurtBox = null;
			}
		}
		ClearEncounteredEntities();
		CondenseHitOrderBuffer();
		LoSType loSType = losType;
		if (loSType != 0 && loSType == LoSType.NearestHit)
		{
			NativeArray<RaycastCommand> val6 = default(NativeArray<RaycastCommand>);
			val6._002Ector(hitOrderBufferLength, (Allocator)3, (NativeArrayOptions)1);
			NativeArray<RaycastHit> val7 = default(NativeArray<RaycastHit>);
			val7._002Ector(hitOrderBufferLength, (Allocator)3, (NativeArrayOptions)1);
			int n = 0;
			int num5 = 0;
			for (; n < hitOrderBufferLength; n++)
			{
				int num6 = hitOrderBuffer[n];
				ref HitPoint reference3 = ref hitPointsBuffer[num6];
				val6[num5++] = new RaycastCommand(val, reference3.hitPosition - val, Mathf.Sqrt(reference3.distanceSqr), LayerMask.op_Implicit(LayerIndex.world.mask), 1);
			}
			bool queriesHitTriggers = Physics.queriesHitTriggers;
			Physics.queriesHitTriggers = true;
			JobHandle val8 = RaycastCommand.ScheduleBatch(val6, val7, 1, default(JobHandle));
			((JobHandle)(ref val8)).Complete();
			Physics.queriesHitTriggers = queriesHitTriggers;
			int num7 = 0;
			int num8 = 0;
			for (; num7 < hitOrderBufferLength; num7++)
			{
				int num9 = hitOrderBuffer[num7];
				ref HitPoint reference4 = ref hitPointsBuffer[num9];
				if (reference4.hurtBox != null)
				{
					RaycastHit val9 = val7[num8++];
					if (Object.op_Implicit((Object)(object)((RaycastHit)(ref val9)).collider))
					{
						reference4.hurtBox = null;
					}
				}
			}
			val7.Dispose();
			val6.Dispose();
			CondenseHitOrderBuffer();
		}
		HitPoint[] array2 = new HitPoint[hitOrderBufferLength];
		for (int num10 = 0; num10 < hitOrderBufferLength; num10++)
		{
			int num11 = hitOrderBuffer[num10];
			array2[num10] = hitPointsBuffer[num11];
		}
		ArrayUtils.Clear<HitPoint>(hitPointsBuffer, ref num2);
		ClearEncounteredEntities();
		return array2;
		void ClearEncounteredEntities()
		{
			Array.Clear(encounteredHealthComponentsBuffer, 0, encounteredHealthComponentsLength);
			encounteredHealthComponentsLength = 0;
		}
		void CondenseHitOrderBuffer()
		{
			for (int num12 = 0; num12 < hitOrderBufferLength; num12++)
			{
				int num13 = 0;
				for (int num14 = num12; num14 < hitOrderBufferLength; num14++)
				{
					int num15 = hitOrderBuffer[num14];
					if (hitPointsBuffer[num15].hurtBox != null)
					{
						break;
					}
					num13++;
				}
				if (num13 > 0)
				{
					ArrayUtils.ArrayRemoveAt<int>(hitOrderBuffer, ref hitOrderBufferLength, num12, num13);
				}
			}
		}
		bool EntityIsMarkedEncountered(HealthComponent healthComponent)
		{
			for (int num16 = 0; num16 < encounteredHealthComponentsLength; num16++)
			{
				if (encounteredHealthComponentsBuffer[num16] == healthComponent)
				{
					return true;
				}
			}
			return false;
		}
		void MarkEntityAsEncountered(HealthComponent healthComponent)
		{
			encounteredHealthComponentsBuffer[encounteredHealthComponentsLength++] = healthComponent;
		}
	}

	private void HandleHits(HitPoint[] hitPoints)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = position;
		for (int i = 0; i < hitPoints.Length; i++)
		{
			HitPoint hitPoint = hitPoints[i];
			float num = Mathf.Sqrt(hitPoint.distanceSqr);
			float num2 = 0f;
			Vector3 val2 = ((num > 0f) ? ((hitPoint.hitPosition - val) / num) : Vector3.zero);
			HealthComponent healthComponent = (Object.op_Implicit((Object)(object)hitPoint.hurtBox) ? hitPoint.hurtBox.healthComponent : null);
			if (Object.op_Implicit((Object)(object)healthComponent))
			{
				switch (falloffModel)
				{
				case FalloffModel.None:
					num2 = 1f;
					break;
				case FalloffModel.Linear:
					num2 = 1f - Mathf.Clamp01(num / radius);
					break;
				case FalloffModel.SweetSpot:
					num2 = 1f - ((num > radius / 2f) ? 0.75f : 0f);
					break;
				}
				BlastAttackDamageInfo blastAttackDamageInfo = default(BlastAttackDamageInfo);
				blastAttackDamageInfo.attacker = attacker;
				blastAttackDamageInfo.inflictor = inflictor;
				blastAttackDamageInfo.crit = crit;
				blastAttackDamageInfo.damage = baseDamage * num2;
				blastAttackDamageInfo.damageColorIndex = damageColorIndex;
				blastAttackDamageInfo.damageModifier = hitPoint.hurtBox.damageModifier;
				blastAttackDamageInfo.damageType = damageType | DamageType.AOE;
				blastAttackDamageInfo.force = bonusForce * num2 + baseForce * num2 * val2;
				blastAttackDamageInfo.position = hitPoint.hitPosition;
				blastAttackDamageInfo.procChainMask = procChainMask;
				blastAttackDamageInfo.procCoefficient = procCoefficient;
				blastAttackDamageInfo.hitHealthComponent = healthComponent;
				blastAttackDamageInfo.canRejectForce = canRejectForce;
				BlastAttackDamageInfo blastAttackDamageInfo2 = blastAttackDamageInfo;
				if (NetworkServer.active)
				{
					PerformDamageServer(in blastAttackDamageInfo2);
				}
				else
				{
					ClientReportDamage(in blastAttackDamageInfo2);
				}
				if (impactEffect != EffectIndex.Invalid)
				{
					EffectData effectData = new EffectData();
					effectData.origin = hitPoint.hitPosition;
					effectData.rotation = Quaternion.LookRotation(-val2);
					EffectManager.SpawnEffect(impactEffect, effectData, transmit: true);
				}
			}
		}
	}

	private static void ClientReportDamage(in BlastAttackDamageInfo blastAttackDamageInfo)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		NetworkWriter val = new NetworkWriter();
		val.StartMessage((short)75);
		blastAttackDamageInfo.Write(val);
		val.FinishMessage();
		((NetworkManager)PlatformSystems.networkManager).client.connection.SendWriter(val, QosChannelIndex.defaultReliable.intVal);
	}

	private static void PerformDamageServer(in BlastAttackDamageInfo blastAttackDamageInfo)
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)blastAttackDamageInfo.hitHealthComponent))
		{
			DamageInfo damageInfo = new DamageInfo();
			damageInfo.attacker = blastAttackDamageInfo.attacker;
			damageInfo.inflictor = blastAttackDamageInfo.inflictor;
			damageInfo.damage = blastAttackDamageInfo.damage;
			damageInfo.crit = blastAttackDamageInfo.crit;
			damageInfo.force = blastAttackDamageInfo.force;
			damageInfo.procChainMask = blastAttackDamageInfo.procChainMask;
			damageInfo.procCoefficient = blastAttackDamageInfo.procCoefficient;
			damageInfo.damageType = blastAttackDamageInfo.damageType;
			damageInfo.damageColorIndex = blastAttackDamageInfo.damageColorIndex;
			damageInfo.position = blastAttackDamageInfo.position;
			damageInfo.canRejectForce = blastAttackDamageInfo.canRejectForce;
			damageInfo.ModifyDamageInfo(blastAttackDamageInfo.damageModifier);
			blastAttackDamageInfo.hitHealthComponent.TakeDamage(damageInfo);
			GlobalEventManager.instance.OnHitEnemy(damageInfo, ((Component)blastAttackDamageInfo.hitHealthComponent).gameObject);
			GlobalEventManager.instance.OnHitAll(damageInfo, ((Component)blastAttackDamageInfo.hitHealthComponent).gameObject);
		}
	}
}
