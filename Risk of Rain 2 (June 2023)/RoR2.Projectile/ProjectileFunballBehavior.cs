using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2.Projectile;

[RequireComponent(typeof(TeamFilter))]
[Obsolete("This component is deprecated and will likely be removed from future releases.", false)]
[RequireComponent(typeof(ProjectileController))]
public class ProjectileFunballBehavior : NetworkBehaviour
{
	[Tooltip("The effect to use for the explosion.")]
	public GameObject explosionPrefab;

	[Tooltip("How many seconds until detonation.")]
	public float duration;

	[Tooltip("Radius of blast in meters.")]
	public float blastRadius = 1f;

	[Tooltip("Maximum damage of blast.")]
	public float blastDamage = 1f;

	[Tooltip("Force of blast.")]
	public float blastForce = 1f;

	private ProjectileController projectileController;

	[SyncVar]
	private float timer;

	private bool fuseStarted;

	public float Networktimer
	{
		get
		{
			return timer;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).SetSyncVar<float>(value, ref timer, 1u);
		}
	}

	private void Awake()
	{
		projectileController = ((Component)this).GetComponent<ProjectileController>();
	}

	private void Start()
	{
		Networktimer = -1f;
	}

	private void FixedUpdate()
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		if (NetworkServer.active && fuseStarted)
		{
			Networktimer = timer + Time.fixedDeltaTime;
			if (timer >= duration)
			{
				EffectManager.SpawnEffect(explosionPrefab, new EffectData
				{
					origin = ((Component)this).transform.position,
					scale = blastRadius
				}, transmit: true);
				BlastAttack blastAttack = new BlastAttack();
				blastAttack.attacker = projectileController.owner;
				blastAttack.inflictor = ((Component)this).gameObject;
				blastAttack.teamIndex = projectileController.teamFilter.teamIndex;
				blastAttack.position = ((Component)this).transform.position;
				blastAttack.procChainMask = projectileController.procChainMask;
				blastAttack.procCoefficient = projectileController.procCoefficient;
				blastAttack.radius = blastRadius;
				blastAttack.baseDamage = blastDamage;
				blastAttack.baseForce = blastForce;
				blastAttack.bonusForce = Vector3.zero;
				blastAttack.crit = false;
				blastAttack.damageType = DamageType.Generic;
				blastAttack.Fire();
				Object.Destroy((Object)(object)((Component)this).gameObject);
			}
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		fuseStarted = true;
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.Write(timer);
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
			writer.Write(timer);
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
			timer = reader.ReadSingle();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if (((uint)num & (true ? 1u : 0u)) != 0)
		{
			timer = reader.ReadSingle();
		}
	}

	public override void PreStartClient()
	{
	}
}
