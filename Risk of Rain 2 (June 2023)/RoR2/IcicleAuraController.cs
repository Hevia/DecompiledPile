using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public class IcicleAuraController : NetworkBehaviour
{
	private struct OwnerInfo
	{
		public readonly GameObject gameObject;

		public readonly Transform transform;

		public readonly CharacterBody characterBody;

		public readonly CameraTargetParams cameraTargetParams;

		public OwnerInfo(GameObject gameObject)
		{
			this.gameObject = gameObject;
			if (Object.op_Implicit((Object)(object)gameObject))
			{
				transform = gameObject.transform;
				characterBody = gameObject.GetComponent<CharacterBody>();
				cameraTargetParams = gameObject.GetComponent<CameraTargetParams>();
			}
			else
			{
				transform = null;
				characterBody = null;
				cameraTargetParams = null;
			}
		}
	}

	public float baseIcicleAttackInterval = 0.25f;

	public float icicleBaseRadius = 10f;

	public float icicleRadiusPerIcicle = 2f;

	public float icicleDamageCoefficientPerTick = 2f;

	public float icicleDamageCoefficientPerTickPerIcicle = 1f;

	public float icicleDuration = 5f;

	public float icicleProcCoefficientPerTick = 0.2f;

	public int icicleCountOnFirstKill = 2;

	public int baseIcicleMax = 6;

	public int icicleMaxPerStack = 3;

	public BuffWard buffWard;

	private CameraTargetParams.AimRequest aimRequest;

	private List<float> icicleLifetimes = new List<float>();

	private float attackStopwatch;

	private int lastIcicleCount;

	[SyncVar]
	private int finalIcicleCount;

	[SyncVar]
	public GameObject owner;

	private OwnerInfo cachedOwnerInfo;

	public ParticleSystem[] auraParticles;

	public ParticleSystem[] procParticles;

	private Transform transform;

	private float actualRadius;

	private float scaleVelocity;

	private NetworkInstanceId ___ownerNetId;

	private int maxIcicleCount
	{
		get
		{
			int num = 1;
			if (Object.op_Implicit((Object)(object)cachedOwnerInfo.characterBody.inventory))
			{
				num = cachedOwnerInfo.characterBody.inventory.GetItemCount(RoR2Content.Items.Icicle);
			}
			return baseIcicleMax + (num - 1) * icicleMaxPerStack;
		}
	}

	public int NetworkfinalIcicleCount
	{
		get
		{
			return finalIcicleCount;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).SetSyncVar<int>(value, ref finalIcicleCount, 1u);
		}
	}

	public GameObject Networkowner
	{
		get
		{
			return owner;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).SetSyncVarGameObject(value, ref owner, 2u, ref ___ownerNetId);
		}
	}

	private void Awake()
	{
		transform = ((Component)this).transform;
		if (Object.op_Implicit((Object)(object)buffWard))
		{
			buffWard.interval = baseIcicleAttackInterval;
		}
	}

	private void FixedUpdate()
	{
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)cachedOwnerInfo.gameObject != (Object)(object)owner)
		{
			cachedOwnerInfo = new OwnerInfo(owner);
		}
		UpdateRadius();
		if (NetworkServer.active)
		{
			if (!Object.op_Implicit((Object)(object)owner))
			{
				Object.Destroy((Object)(object)((Component)this).gameObject);
				return;
			}
			for (int num = icicleLifetimes.Count - 1; num >= 0; num--)
			{
				icicleLifetimes[num] -= Time.fixedDeltaTime;
				if (icicleLifetimes[num] <= 0f)
				{
					icicleLifetimes.RemoveAt(num);
				}
			}
			NetworkfinalIcicleCount = Mathf.Min((icicleLifetimes.Count > 0) ? (icicleCountOnFirstKill + icicleLifetimes.Count) : 0, maxIcicleCount);
			attackStopwatch += Time.fixedDeltaTime;
		}
		if (Object.op_Implicit((Object)(object)cachedOwnerInfo.characterBody))
		{
			if (finalIcicleCount > 0)
			{
				if (lastIcicleCount == 0)
				{
					OnIciclesActivated();
				}
				if (lastIcicleCount < finalIcicleCount)
				{
					OnIcicleGained();
				}
			}
			else if (lastIcicleCount > 0)
			{
				OnIciclesDeactivated();
			}
			lastIcicleCount = finalIcicleCount;
		}
		if (NetworkServer.active && Object.op_Implicit((Object)(object)cachedOwnerInfo.characterBody) && finalIcicleCount > 0 && attackStopwatch >= baseIcicleAttackInterval)
		{
			attackStopwatch = 0f;
			BlastAttack blastAttack = new BlastAttack();
			blastAttack.attacker = owner;
			blastAttack.inflictor = ((Component)this).gameObject;
			blastAttack.teamIndex = cachedOwnerInfo.characterBody.teamComponent.teamIndex;
			blastAttack.position = transform.position;
			blastAttack.procCoefficient = icicleProcCoefficientPerTick;
			blastAttack.radius = actualRadius;
			blastAttack.baseForce = 0f;
			blastAttack.baseDamage = cachedOwnerInfo.characterBody.damage * (icicleDamageCoefficientPerTick + icicleDamageCoefficientPerTickPerIcicle * (float)finalIcicleCount);
			blastAttack.bonusForce = Vector3.zero;
			blastAttack.crit = false;
			blastAttack.damageType = DamageType.Generic;
			blastAttack.falloffModel = BlastAttack.FalloffModel.None;
			blastAttack.damageColorIndex = DamageColorIndex.Item;
			blastAttack.attackerFiltering = AttackerFiltering.NeverHitSelf;
			blastAttack.Fire();
		}
		if (Object.op_Implicit((Object)(object)buffWard))
		{
			buffWard.Networkradius = actualRadius;
		}
	}

	private void UpdateRadius()
	{
		if (Object.op_Implicit((Object)(object)owner))
		{
			if (finalIcicleCount > 0)
			{
				actualRadius = (Object.op_Implicit((Object)(object)cachedOwnerInfo.characterBody) ? (cachedOwnerInfo.characterBody.radius + icicleBaseRadius + icicleRadiusPerIcicle * (float)finalIcicleCount) : 0f);
			}
			else
			{
				actualRadius = 0f;
			}
		}
	}

	private void UpdateVisuals()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)cachedOwnerInfo.gameObject))
		{
			transform.position = (Object.op_Implicit((Object)(object)cachedOwnerInfo.characterBody) ? cachedOwnerInfo.characterBody.corePosition : cachedOwnerInfo.transform.position);
		}
		float num = Mathf.SmoothDamp(transform.localScale.x, actualRadius, ref scaleVelocity, 0.5f);
		transform.localScale = new Vector3(num, num, num);
	}

	private void OnIciclesDeactivated()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		Util.PlaySound("Stop_item_proc_icicle", ((Component)this).gameObject);
		ParticleSystem[] array = auraParticles;
		for (int i = 0; i < array.Length; i++)
		{
			MainModule main = array[i].main;
			((MainModule)(ref main)).loop = false;
		}
		aimRequest?.Dispose();
	}

	private void OnIciclesActivated()
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		Util.PlaySound("Play_item_proc_icicle", ((Component)this).gameObject);
		if (Object.op_Implicit((Object)(object)cachedOwnerInfo.cameraTargetParams))
		{
			aimRequest = cachedOwnerInfo.cameraTargetParams.RequestAimType(CameraTargetParams.AimType.Aura);
		}
		ParticleSystem[] array = auraParticles;
		foreach (ParticleSystem obj in array)
		{
			MainModule main = obj.main;
			((MainModule)(ref main)).loop = true;
			obj.Play();
		}
	}

	private void OnIcicleGained()
	{
		ParticleSystem[] array = procParticles;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Play();
		}
	}

	private void LateUpdate()
	{
		UpdateVisuals();
	}

	public void OnOwnerKillOther()
	{
		icicleLifetimes.Add(icicleDuration);
	}

	public void OnDestroy()
	{
		OnIciclesDeactivated();
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.WritePackedUInt32((uint)finalIcicleCount);
			writer.Write(owner);
			return true;
		}
		bool flag = false;
		if ((((NetworkBehaviour)this).syncVarDirtyBits & (true ? 1u : 0u)) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
				flag = true;
			}
			writer.WritePackedUInt32((uint)finalIcicleCount);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 2u) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
				flag = true;
			}
			writer.Write(owner);
		}
		if (!flag)
		{
			writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
		}
		return flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		if (initialState)
		{
			finalIcicleCount = (int)reader.ReadPackedUInt32();
			___ownerNetId = reader.ReadNetworkId();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if (((uint)num & (true ? 1u : 0u)) != 0)
		{
			finalIcicleCount = (int)reader.ReadPackedUInt32();
		}
		if (((uint)num & 2u) != 0)
		{
			owner = reader.ReadGameObject();
		}
	}

	public override void PreStartClient()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		if (!((NetworkInstanceId)(ref ___ownerNetId)).IsEmpty())
		{
			Networkowner = ClientScene.FindLocalObject(___ownerNetId);
		}
	}
}
