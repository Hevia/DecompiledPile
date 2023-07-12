using System.Runtime.InteropServices;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

public class MaulingRock : NetworkBehaviour
{
	private HealthComponent myHealthComponent;

	public GameObject deathEffect;

	public float blastRadius = 1f;

	public float damage = 10f;

	public float damageCoefficient = 1f;

	public float scaleVarianceLow = 0.8f;

	public float scaleVarianceHigh = 1.2f;

	public float verticalOffset;

	[SyncVar]
	private float scale;

	public float Networkscale
	{
		get
		{
			return scale;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).SetSyncVar<float>(value, ref scale, 1u);
		}
	}

	public override void OnStartServer()
	{
		((NetworkBehaviour)this).OnStartServer();
		Networkscale = Random.Range(scaleVarianceLow, scaleVarianceHigh);
	}

	private void Start()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		myHealthComponent = ((Component)this).GetComponent<HealthComponent>();
		((Component)this).transform.localScale = new Vector3(scale, scale, scale);
	}

	private void FixedUpdate()
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		if (NetworkServer.active && (Object)(object)myHealthComponent != (Object)null && !myHealthComponent.alive)
		{
			if ((Object)(object)deathEffect != (Object)null)
			{
				EffectManager.SpawnEffect(deathEffect, new EffectData
				{
					origin = ((Component)this).transform.position,
					scale = blastRadius
				}, transmit: true);
			}
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.Write(scale);
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
			writer.Write(scale);
		}
		if (!flag)
		{
			writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
		}
		return flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		if (initialState)
		{
			scale = reader.ReadSingle();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if (((uint)num & (true ? 1u : 0u)) != 0)
		{
			scale = reader.ReadSingle();
		}
	}

	public override void PreStartClient()
	{
	}
}
