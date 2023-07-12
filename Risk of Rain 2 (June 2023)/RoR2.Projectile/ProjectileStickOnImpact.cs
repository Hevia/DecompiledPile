using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace RoR2.Projectile;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(ProjectileController))]
[RequireComponent(typeof(ProjectileNetworkTransform))]
public class ProjectileStickOnImpact : NetworkBehaviour, IProjectileImpactBehavior
{
	public string stickSoundString;

	public ParticleSystem[] stickParticleSystem;

	public bool ignoreCharacters;

	public bool ignoreWorld;

	public bool alignNormals = true;

	public UnityEvent stickEvent;

	private ProjectileController projectileController;

	private Rigidbody rigidbody;

	private bool wasEverEnabled;

	private GameObject _victim;

	[SyncVar]
	private GameObject syncVictim;

	[SyncVar]
	private sbyte hitHurtboxIndex = -1;

	[SyncVar]
	private Vector3 localPosition;

	[SyncVar]
	private Quaternion localRotation;

	private NetworkInstanceId ___syncVictimNetId;

	public Transform stuckTransform { get; private set; }

	public CharacterBody stuckBody { get; private set; }

	public GameObject victim
	{
		get
		{
			return _victim;
		}
		private set
		{
			_victim = value;
			NetworksyncVictim = value;
		}
	}

	public bool stuck => hitHurtboxIndex != -1;

	public GameObject NetworksyncVictim
	{
		get
		{
			return syncVictim;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).SetSyncVarGameObject(value, ref syncVictim, 1u, ref ___syncVictimNetId);
		}
	}

	public sbyte NetworkhitHurtboxIndex
	{
		get
		{
			return hitHurtboxIndex;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).SetSyncVar<sbyte>(value, ref hitHurtboxIndex, 2u);
		}
	}

	public Vector3 NetworklocalPosition
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return localPosition;
		}
		[param: In]
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			((NetworkBehaviour)this).SetSyncVar<Vector3>(value, ref localPosition, 4u);
		}
	}

	public Quaternion NetworklocalRotation
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return localRotation;
		}
		[param: In]
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			((NetworkBehaviour)this).SetSyncVar<Quaternion>(value, ref localRotation, 8u);
		}
	}

	private void Awake()
	{
		projectileController = ((Component)this).GetComponent<ProjectileController>();
		rigidbody = ((Component)this).GetComponent<Rigidbody>();
	}

	public void FixedUpdate()
	{
		UpdateSticking();
	}

	private void OnEnable()
	{
		if (wasEverEnabled)
		{
			Collider component = ((Component)this).GetComponent<Collider>();
			component.enabled = false;
			component.enabled = true;
		}
		wasEverEnabled = true;
	}

	private void OnDisable()
	{
		if (NetworkServer.active)
		{
			Detach();
		}
	}

	[Server]
	public void Detach()
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.Projectile.ProjectileStickOnImpact::Detach()' called on client");
			return;
		}
		victim = null;
		stuckTransform = null;
		NetworkhitHurtboxIndex = -1;
		UpdateSticking();
	}

	public void OnProjectileImpact(ProjectileImpactInfo impactInfo)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		if (((Behaviour)this).enabled)
		{
			TrySticking(impactInfo.collider, impactInfo.estimatedImpactNormal);
		}
	}

	private bool TrySticking(Collider hitCollider, Vector3 impactNormal)
	{
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)victim))
		{
			return false;
		}
		GameObject val = null;
		sbyte networkhitHurtboxIndex = -1;
		HurtBox component = ((Component)hitCollider).GetComponent<HurtBox>();
		if (Object.op_Implicit((Object)(object)component))
		{
			HealthComponent healthComponent = component.healthComponent;
			if (Object.op_Implicit((Object)(object)healthComponent))
			{
				val = ((Component)healthComponent).gameObject;
			}
			networkhitHurtboxIndex = (sbyte)component.indexInGroup;
		}
		if (!Object.op_Implicit((Object)(object)val) && !ignoreWorld)
		{
			val = ((Component)hitCollider).gameObject;
			networkhitHurtboxIndex = -2;
		}
		if ((Object)(object)val == (Object)(object)projectileController.owner || (ignoreCharacters && Object.op_Implicit((Object)(object)component)))
		{
			val = null;
			networkhitHurtboxIndex = -1;
		}
		if (Object.op_Implicit((Object)(object)val))
		{
			stickEvent.Invoke();
			ParticleSystem[] array = stickParticleSystem;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Play();
			}
			if (stickSoundString.Length > 0)
			{
				Util.PlaySound(stickSoundString, ((Component)this).gameObject);
			}
			if (alignNormals && impactNormal != Vector3.zero)
			{
				((Component)this).transform.rotation = Util.QuaternionSafeLookRotation(impactNormal, ((Component)this).transform.up);
			}
			Transform transform = ((Component)hitCollider).transform;
			NetworklocalPosition = transform.InverseTransformPoint(((Component)this).transform.position);
			NetworklocalRotation = Quaternion.Inverse(transform.rotation) * ((Component)this).transform.rotation;
			victim = val;
			NetworkhitHurtboxIndex = networkhitHurtboxIndex;
			return true;
		}
		return false;
	}

	private void UpdateSticking()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		bool flag = Object.op_Implicit((Object)(object)stuckTransform);
		if (flag)
		{
			((Component)this).transform.SetPositionAndRotation(stuckTransform.TransformPoint(localPosition), alignNormals ? (stuckTransform.rotation * localRotation) : ((Component)this).transform.rotation);
		}
		else
		{
			GameObject val = (NetworkServer.active ? victim : syncVictim);
			if (Object.op_Implicit((Object)(object)val))
			{
				stuckTransform = val.transform;
				flag = true;
				if (hitHurtboxIndex >= 0)
				{
					stuckBody = ((Component)stuckTransform).GetComponent<CharacterBody>();
					if (Object.op_Implicit((Object)(object)stuckBody) && Object.op_Implicit((Object)(object)stuckBody.hurtBoxGroup))
					{
						HurtBox hurtBox = stuckBody.hurtBoxGroup.hurtBoxes[hitHurtboxIndex];
						stuckTransform = (Object.op_Implicit((Object)(object)hurtBox) ? ((Component)hurtBox).transform : null);
					}
				}
			}
			else if (hitHurtboxIndex == -2 && !NetworkServer.active)
			{
				flag = true;
			}
		}
		if (!NetworkServer.active)
		{
			return;
		}
		if (rigidbody.isKinematic != flag)
		{
			if (flag)
			{
				rigidbody.collisionDetectionMode = (CollisionDetectionMode)0;
				rigidbody.isKinematic = true;
			}
			else
			{
				rigidbody.isKinematic = false;
				rigidbody.collisionDetectionMode = (CollisionDetectionMode)1;
			}
		}
		if (!flag)
		{
			NetworkhitHurtboxIndex = -1;
		}
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		if (forceAll)
		{
			writer.Write(syncVictim);
			writer.WritePackedUInt32((uint)hitHurtboxIndex);
			writer.Write(localPosition);
			writer.Write(localRotation);
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
			writer.Write(syncVictim);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 2u) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
				flag = true;
			}
			writer.WritePackedUInt32((uint)hitHurtboxIndex);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 4u) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
				flag = true;
			}
			writer.Write(localPosition);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 8u) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
				flag = true;
			}
			writer.Write(localRotation);
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
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		if (initialState)
		{
			___syncVictimNetId = reader.ReadNetworkId();
			hitHurtboxIndex = (sbyte)reader.ReadPackedUInt32();
			localPosition = reader.ReadVector3();
			localRotation = reader.ReadQuaternion();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if (((uint)num & (true ? 1u : 0u)) != 0)
		{
			syncVictim = reader.ReadGameObject();
		}
		if (((uint)num & 2u) != 0)
		{
			hitHurtboxIndex = (sbyte)reader.ReadPackedUInt32();
		}
		if (((uint)num & 4u) != 0)
		{
			localPosition = reader.ReadVector3();
		}
		if (((uint)num & 8u) != 0)
		{
			localRotation = reader.ReadQuaternion();
		}
	}

	public override void PreStartClient()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		if (!((NetworkInstanceId)(ref ___syncVictimNetId)).IsEmpty())
		{
			NetworksyncVictim = ClientScene.FindLocalObject(___syncVictimNetId);
		}
	}
}
