using System;
using HG;
using RoR2.Audio;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2.Projectile;

[RequireComponent(typeof(ProjectileController))]
public class ProjectileImpactExplosion : ProjectileExplosion, IProjectileImpactBehavior
{
	public enum TransformSpace
	{
		World,
		Local,
		Normal
	}

	private Vector3 impactNormal = Vector3.up;

	[Header("Impact & Lifetime Properties")]
	public GameObject impactEffect;

	[Tooltip("This sound will not play over the network. Use lifetimeExpiredSound instead.")]
	[ShowFieldObsolete]
	[Obsolete("This sound will not play over the network. Use lifetimeExpiredSound instead.", false)]
	public string lifetimeExpiredSoundString;

	public NetworkSoundEventDef lifetimeExpiredSound;

	public float offsetForLifetimeExpiredSound;

	public bool destroyOnEnemy = true;

	public bool destroyOnWorld;

	public bool impactOnWorld = true;

	public bool timerAfterImpact;

	public float lifetime;

	public float lifetimeAfterImpact;

	public float lifetimeRandomOffset;

	private float stopwatch;

	private float stopwatchAfterImpact;

	private bool hasImpact;

	private bool hasPlayedLifetimeExpiredSound;

	public TransformSpace transformSpace;

	protected override void Awake()
	{
		base.Awake();
		lifetime += Random.Range(0f, lifetimeRandomOffset);
	}

	protected void FixedUpdate()
	{
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		stopwatch += Time.fixedDeltaTime;
		if (!NetworkServer.active && !projectileController.isPrediction)
		{
			return;
		}
		if (timerAfterImpact && hasImpact)
		{
			stopwatchAfterImpact += Time.fixedDeltaTime;
		}
		bool num = stopwatch >= lifetime;
		bool flag = timerAfterImpact && stopwatchAfterImpact > lifetimeAfterImpact;
		bool flag2 = Object.op_Implicit((Object)(object)projectileHealthComponent) && !projectileHealthComponent.alive;
		if (num || flag || flag2)
		{
			alive = false;
		}
		if (alive && !hasPlayedLifetimeExpiredSound)
		{
			bool flag3 = stopwatch > lifetime - offsetForLifetimeExpiredSound;
			if (timerAfterImpact)
			{
				flag3 |= stopwatchAfterImpact > lifetimeAfterImpact - offsetForLifetimeExpiredSound;
			}
			if (flag3)
			{
				hasPlayedLifetimeExpiredSound = true;
				if (NetworkServer.active && Object.op_Implicit((Object)(object)lifetimeExpiredSound))
				{
					PointSoundManager.EmitSoundServer(lifetimeExpiredSound.index, ((Component)this).transform.position);
				}
			}
		}
		if (!alive)
		{
			explosionEffect = impactEffect ?? explosionEffect;
			Detonate();
		}
	}

	protected override Quaternion GetRandomDirectionForChild()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		Quaternion randomChildRollPitch = GetRandomChildRollPitch();
		return (Quaternion)(transformSpace switch
		{
			TransformSpace.Local => ((Component)this).transform.rotation * randomChildRollPitch, 
			TransformSpace.Normal => Quaternion.FromToRotation(Vector3.forward, impactNormal) * randomChildRollPitch, 
			_ => randomChildRollPitch, 
		});
	}

	public void OnProjectileImpact(ProjectileImpactInfo impactInfo)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		if (!alive)
		{
			return;
		}
		Collider collider = impactInfo.collider;
		impactNormal = impactInfo.estimatedImpactNormal;
		if (!Object.op_Implicit((Object)(object)collider))
		{
			return;
		}
		DamageInfo damageInfo = new DamageInfo();
		if (Object.op_Implicit((Object)(object)projectileDamage))
		{
			damageInfo.damage = projectileDamage.damage;
			damageInfo.crit = projectileDamage.crit;
			damageInfo.attacker = (Object.op_Implicit((Object)(object)projectileController.owner) ? projectileController.owner.gameObject : null);
			damageInfo.inflictor = ((Component)this).gameObject;
			damageInfo.position = impactInfo.estimatedPointOfImpact;
			damageInfo.force = projectileDamage.force * ((Component)this).transform.forward;
			damageInfo.procChainMask = projectileController.procChainMask;
			damageInfo.procCoefficient = projectileController.procCoefficient;
		}
		else
		{
			Debug.Log((object)"No projectile damage component!");
		}
		HurtBox component = ((Component)collider).GetComponent<HurtBox>();
		if (Object.op_Implicit((Object)(object)component))
		{
			if (destroyOnEnemy)
			{
				HealthComponent healthComponent = component.healthComponent;
				if (Object.op_Implicit((Object)(object)healthComponent))
				{
					if ((Object)(object)((Component)healthComponent).gameObject == (Object)(object)projectileController.owner || (Object.op_Implicit((Object)(object)projectileHealthComponent) && (Object)(object)healthComponent == (Object)(object)projectileHealthComponent))
					{
						return;
					}
					alive = false;
				}
			}
		}
		else if (destroyOnWorld)
		{
			alive = false;
		}
		hasImpact = Object.op_Implicit((Object)(object)component) || impactOnWorld;
		if (NetworkServer.active && hasImpact)
		{
			GlobalEventManager.instance.OnHitAll(damageInfo, ((Component)collider).gameObject);
		}
	}

	protected override void OnValidate()
	{
		if (!Application.IsPlaying((Object)(object)this))
		{
			base.OnValidate();
			if (!string.IsNullOrEmpty(lifetimeExpiredSoundString))
			{
				Debug.LogWarningFormat((Object)(object)((Component)this).gameObject, "{0} ProjectileImpactExplosion component supplies a value in the lifetimeExpiredSoundString field. This will not play correctly over the network. Please use lifetimeExpiredSound instead.", new object[1] { Util.GetGameObjectHierarchyName(((Component)this).gameObject) });
			}
		}
	}
}
