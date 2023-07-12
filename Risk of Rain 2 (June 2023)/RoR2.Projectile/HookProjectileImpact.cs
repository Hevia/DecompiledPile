using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2.Projectile;

[RequireComponent(typeof(ProjectileController))]
public class HookProjectileImpact : NetworkBehaviour, IProjectileImpactBehavior
{
	private enum HookState
	{
		Flying,
		HitDelay,
		Reel,
		ReelFail
	}

	private ProjectileController projectileController;

	public float reelDelayTime;

	public float reelSpeed = 40f;

	public string attachmentString;

	public float victimPullFactor = 1f;

	public float pullMinimumDistance = 10f;

	public GameObject impactSpark;

	public GameObject impactSuccess;

	[SyncVar]
	private HookState hookState;

	[SyncVar]
	private GameObject victim;

	private Transform ownerTransform;

	private ProjectileDamage projectileDamage;

	private Rigidbody rigidbody;

	public float liveTimer;

	private float delayTimer;

	private float flyTimer;

	private NetworkInstanceId ___victimNetId;

	public HookState NetworkhookState
	{
		get
		{
			return hookState;
		}
		[param: In]
		set
		{
			ulong num = (ulong)value;
			ulong num2 = (ulong)hookState;
			((NetworkBehaviour)this).SetSyncVarEnum<HookState>(value, num, ref hookState, num2, 1u);
		}
	}

	public GameObject Networkvictim
	{
		get
		{
			return victim;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).SetSyncVarGameObject(value, ref victim, 2u, ref ___victimNetId);
		}
	}

	private void Start()
	{
		rigidbody = ((Component)this).GetComponent<Rigidbody>();
		projectileController = ((Component)this).GetComponent<ProjectileController>();
		projectileDamage = ((Component)this).GetComponent<ProjectileDamage>();
		ownerTransform = projectileController.owner.transform;
		if (!Object.op_Implicit((Object)(object)ownerTransform))
		{
			return;
		}
		ModelLocator component = ((Component)ownerTransform).GetComponent<ModelLocator>();
		if (!Object.op_Implicit((Object)(object)component))
		{
			return;
		}
		Transform modelTransform = component.modelTransform;
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			ChildLocator component2 = ((Component)modelTransform).GetComponent<ChildLocator>();
			if (Object.op_Implicit((Object)(object)component2))
			{
				ownerTransform = component2.FindChild(attachmentString);
			}
		}
	}

	public void OnProjectileImpact(ProjectileImpactInfo impactInfo)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		EffectManager.SimpleImpactEffect(impactSpark, impactInfo.estimatedPointOfImpact, -((Component)this).transform.forward, transmit: true);
		if (hookState != 0)
		{
			return;
		}
		HurtBox component = ((Component)impactInfo.collider).GetComponent<HurtBox>();
		if (Object.op_Implicit((Object)(object)component))
		{
			HealthComponent healthComponent = component.healthComponent;
			if (Object.op_Implicit((Object)(object)healthComponent))
			{
				TeamIndex teamIndex = projectileController.teamFilter.teamIndex;
				if (!FriendlyFireManager.ShouldDirectHitProceed(healthComponent, teamIndex))
				{
					return;
				}
				Networkvictim = ((Component)healthComponent).gameObject;
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
					damageInfo.damageColorIndex = projectileDamage.damageColorIndex;
				}
				healthComponent.TakeDamage(damageInfo);
				GlobalEventManager.instance.OnHitEnemy(damageInfo, ((Component)healthComponent).gameObject);
				NetworkhookState = HookState.HitDelay;
				EffectManager.SimpleImpactEffect(impactSuccess, impactInfo.estimatedPointOfImpact, -((Component)this).transform.forward, transmit: true);
				((Component)this).gameObject.layer = LayerIndex.noCollision.intVal;
			}
		}
		if (!Object.op_Implicit((Object)(object)victim))
		{
			NetworkhookState = HookState.ReelFail;
		}
	}

	private bool Reel()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = ownerTransform.position - victim.transform.position;
		Vector3 normalized = ((Vector3)(ref val)).normalized;
		float num = ((Vector3)(ref val)).magnitude;
		Collider component = projectileController.owner.GetComponent<Collider>();
		Collider component2 = victim.GetComponent<Collider>();
		if (Object.op_Implicit((Object)(object)component) && Object.op_Implicit((Object)(object)component2))
		{
			num = Util.EstimateSurfaceDistance(component, component2);
		}
		bool flag = num <= pullMinimumDistance;
		CharacterMotor characterMotor = null;
		Rigidbody val2 = null;
		float num2 = -1f;
		characterMotor = projectileController.owner.GetComponent<CharacterMotor>();
		if (Object.op_Implicit((Object)(object)characterMotor))
		{
			num2 = characterMotor.mass;
		}
		else
		{
			val2 = projectileController.owner.GetComponent<Rigidbody>();
			if (Object.op_Implicit((Object)(object)val2))
			{
				num2 = val2.mass;
			}
		}
		CharacterMotor characterMotor2 = null;
		Rigidbody val3 = null;
		float num3 = -1f;
		characterMotor2 = victim.GetComponent<CharacterMotor>();
		if (Object.op_Implicit((Object)(object)characterMotor2))
		{
			num3 = characterMotor2.mass;
		}
		else
		{
			val3 = victim.GetComponent<Rigidbody>();
			if (Object.op_Implicit((Object)(object)val3))
			{
				num3 = val3.mass;
			}
		}
		float num4 = 0f;
		float num5 = 0f;
		if (num2 > 0f && num3 > 0f)
		{
			num4 = 1f - num2 / (num2 + num3);
			num5 = 1f - num4;
		}
		else if (num2 > 0f)
		{
			num4 = 1f;
		}
		else if (num3 > 0f)
		{
			num5 = 1f;
		}
		else
		{
			flag = true;
		}
		if (flag)
		{
			num4 = 0f;
			num5 = 0f;
		}
		Vector3 velocity = normalized * (num5 * victimPullFactor * reelSpeed);
		if (Object.op_Implicit((Object)(object)characterMotor2))
		{
			characterMotor2.velocity = velocity;
		}
		if (Object.op_Implicit((Object)(object)val3))
		{
			val3.velocity = velocity;
		}
		return flag;
	}

	public void FixedUpdate()
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		if (NetworkServer.active && !Object.op_Implicit((Object)(object)projectileController.owner))
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
			return;
		}
		if (Object.op_Implicit((Object)(object)victim))
		{
			rigidbody.MovePosition(victim.transform.position);
		}
		switch (hookState)
		{
		case HookState.Flying:
			if (NetworkServer.active)
			{
				flyTimer += Time.fixedDeltaTime;
				if (flyTimer >= liveTimer)
				{
					NetworkhookState = HookState.ReelFail;
				}
			}
			break;
		case HookState.HitDelay:
			if (!NetworkServer.active)
			{
				break;
			}
			if (Object.op_Implicit((Object)(object)victim))
			{
				delayTimer += Time.fixedDeltaTime;
				if (delayTimer >= reelDelayTime)
				{
					NetworkhookState = HookState.Reel;
				}
			}
			else
			{
				NetworkhookState = HookState.Reel;
			}
			break;
		case HookState.Reel:
		{
			bool flag = true;
			if (Object.op_Implicit((Object)(object)victim))
			{
				flag = Reel();
			}
			if (NetworkServer.active)
			{
				if (!Object.op_Implicit((Object)(object)victim))
				{
					NetworkhookState = HookState.ReelFail;
				}
				if (flag)
				{
					Object.Destroy((Object)(object)((Component)this).gameObject);
				}
			}
			break;
		}
		case HookState.ReelFail:
			if (!NetworkServer.active)
			{
				break;
			}
			if (Object.op_Implicit((Object)(object)rigidbody))
			{
				rigidbody.collisionDetectionMode = (CollisionDetectionMode)0;
				rigidbody.isKinematic = true;
			}
			if (Object.op_Implicit((Object)(object)ownerTransform))
			{
				rigidbody.MovePosition(Vector3.MoveTowards(((Component)this).transform.position, ownerTransform.position, reelSpeed * Time.fixedDeltaTime));
				if (((Component)this).transform.position == ownerTransform.position)
				{
					Object.Destroy((Object)(object)((Component)this).gameObject);
				}
			}
			break;
		}
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.Write((int)hookState);
			writer.Write(victim);
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
			writer.Write((int)hookState);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 2u) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
				flag = true;
			}
			writer.Write(victim);
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
			hookState = (HookState)reader.ReadInt32();
			___victimNetId = reader.ReadNetworkId();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if (((uint)num & (true ? 1u : 0u)) != 0)
		{
			hookState = (HookState)reader.ReadInt32();
		}
		if (((uint)num & 2u) != 0)
		{
			victim = reader.ReadGameObject();
		}
	}

	public override void PreStartClient()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		if (!((NetworkInstanceId)(ref ___victimNetId)).IsEmpty())
		{
			Networkvictim = ClientScene.FindLocalObject(___victimNetId);
		}
	}
}
