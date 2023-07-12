using System.Runtime.InteropServices;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public class TitanRockController : NetworkBehaviour
{
	[Tooltip("The child transform from which projectiles will be fired.")]
	public Transform fireTransform;

	[Tooltip("How long it takes to start firing.")]
	public float startDelay = 4f;

	[Tooltip("Firing interval.")]
	public float fireInterval = 1f;

	[Tooltip("The prefab to fire as a projectile.")]
	public GameObject projectilePrefab;

	[Tooltip("The damage coefficient to multiply by the owner's damage stat for the projectile's final damage value.")]
	public float damageCoefficient;

	[Tooltip("The force of the projectile's damage.")]
	public float damageForce;

	[SyncVar(hook = "SetOwner")]
	private GameObject owner;

	private Transform targetTransform;

	private Vector3 velocity;

	private static readonly Vector3 targetLocalPosition = new Vector3(0f, 12f, -3f);

	private float fireTimer;

	private InputBankTest ownerInputBank;

	private CharacterBody ownerCharacterBody;

	private bool isCrit;

	private bool foundOwner;

	private NetworkInstanceId ___ownerNetId;

	public GameObject Networkowner
	{
		get
		{
			return owner;
		}
		[param: In]
		set
		{
			if (NetworkServer.localClientActive && !((NetworkBehaviour)this).syncVarHookGuard)
			{
				((NetworkBehaviour)this).syncVarHookGuard = true;
				SetOwner(value);
				((NetworkBehaviour)this).syncVarHookGuard = false;
			}
			((NetworkBehaviour)this).SetSyncVarGameObject(value, ref owner, 1u, ref ___ownerNetId);
		}
	}

	private void Start()
	{
		if (!NetworkServer.active)
		{
			SetOwner(owner);
		}
		else
		{
			fireTimer = startDelay;
		}
	}

	public void SetOwner(GameObject newOwner)
	{
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		ownerInputBank = null;
		ownerCharacterBody = null;
		isCrit = false;
		Networkowner = newOwner;
		if (!Object.op_Implicit((Object)(object)owner))
		{
			return;
		}
		ownerInputBank = owner.GetComponent<InputBankTest>();
		ownerCharacterBody = owner.GetComponent<CharacterBody>();
		ModelLocator component = owner.GetComponent<ModelLocator>();
		if (Object.op_Implicit((Object)(object)component))
		{
			Transform modelTransform = component.modelTransform;
			if (Object.op_Implicit((Object)(object)modelTransform))
			{
				ChildLocator component2 = ((Component)modelTransform).GetComponent<ChildLocator>();
				if (Object.op_Implicit((Object)(object)component2))
				{
					targetTransform = component2.FindChild("Chest");
					if (Object.op_Implicit((Object)(object)targetTransform))
					{
						((Component)this).transform.rotation = targetTransform.rotation;
					}
				}
			}
		}
		((Component)this).transform.position = owner.transform.position + Vector3.down * 20f;
		if (NetworkServer.active && Object.op_Implicit((Object)(object)ownerCharacterBody))
		{
			CharacterMaster master = ownerCharacterBody.master;
			if (Object.op_Implicit((Object)(object)master))
			{
				isCrit = Util.CheckRoll(ownerCharacterBody.crit, master);
			}
		}
	}

	public void FixedUpdate()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)targetTransform))
		{
			foundOwner = true;
			((Component)this).transform.position = Vector3.SmoothDamp(((Component)this).transform.position, targetTransform.TransformPoint(targetLocalPosition), ref velocity, 1f);
			((Component)this).transform.rotation = targetTransform.rotation;
		}
		else if (foundOwner)
		{
			foundOwner = false;
			ParticleSystem[] componentsInChildren = ((Component)this).GetComponentsInChildren<ParticleSystem>();
			foreach (ParticleSystem obj in componentsInChildren)
			{
				MainModule main = obj.main;
				((MainModule)(ref main)).gravityModifier = MinMaxCurve.op_Implicit(1f);
				EmissionModule emission = obj.emission;
				((EmissionModule)(ref emission)).enabled = false;
				NoiseModule noise = obj.noise;
				((NoiseModule)(ref noise)).enabled = false;
				LimitVelocityOverLifetimeModule limitVelocityOverLifetime = obj.limitVelocityOverLifetime;
				((LimitVelocityOverLifetimeModule)(ref limitVelocityOverLifetime)).enabled = false;
			}
			CollisionModule collision = ((Component)((Component)this).transform.Find("Debris")).GetComponent<ParticleSystem>().collision;
			((CollisionModule)(ref collision)).enabled = true;
			Light[] componentsInChildren2 = ((Component)this).GetComponentsInChildren<Light>();
			for (int i = 0; i < componentsInChildren2.Length; i++)
			{
				((Behaviour)componentsInChildren2[i]).enabled = false;
			}
			Util.PlaySound("Stop_titanboss_shift_loop", ((Component)this).gameObject);
		}
		if (NetworkServer.active)
		{
			FixedUpdateServer();
		}
	}

	[Server]
	private void FixedUpdateServer()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.TitanRockController::FixedUpdateServer()' called on client");
			return;
		}
		fireTimer -= Time.fixedDeltaTime;
		if (fireTimer <= 0f)
		{
			Fire();
			fireTimer += fireInterval;
		}
	}

	[Server]
	private void Fire()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.TitanRockController::Fire()' called on client");
		}
		else if (Object.op_Implicit((Object)(object)ownerInputBank))
		{
			Vector3 position = fireTransform.position;
			Vector3 forward = ownerInputBank.aimDirection;
			if (Util.CharacterRaycast(owner, new Ray(ownerInputBank.aimOrigin, ownerInputBank.aimDirection), out var hitInfo, float.PositiveInfinity, LayerMask.op_Implicit(LayerMask.op_Implicit(LayerIndex.world.mask) | LayerMask.op_Implicit(LayerIndex.entityPrecise.mask)), (QueryTriggerInteraction)0))
			{
				forward = ((RaycastHit)(ref hitInfo)).point - position;
			}
			float num = (Object.op_Implicit((Object)(object)ownerCharacterBody) ? ownerCharacterBody.damage : 1f);
			ProjectileManager.instance.FireProjectile(projectilePrefab, position, Util.QuaternionSafeLookRotation(forward), owner, damageCoefficient * num, damageForce, isCrit);
		}
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
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
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		if (initialState)
		{
			___ownerNetId = reader.ReadNetworkId();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if (((uint)num & (true ? 1u : 0u)) != 0)
		{
			SetOwner(reader.ReadGameObject());
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
