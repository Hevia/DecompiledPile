using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using RoR2.Audio;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2.Projectile;

[RequireComponent(typeof(TeamFilter))]
public class ProjectileController : NetworkBehaviour
{
	[HideInInspector]
	[Tooltip("This is assigned to the prefab automatically by ProjectileCatalog at runtime. Do not set this value manually.")]
	public int catalogIndex = -1;

	[Tooltip("The prefab to instantiate as the visual representation of this projectile. The prefab must have a ProjectileGhostController attached.")]
	public GameObject ghostPrefab;

	[Tooltip("The transform for the ghost to follow. If null, the transform of this object will be used instead.")]
	public Transform ghostTransformAnchor;

	[Tooltip("The sound to play on Start(). Use this field to ensure the sound only plays once when prediction creates two instances.")]
	public string startSound;

	[Tooltip("Prevents this projectile from being deleted by gameplay events, like Captain's defense matrix.")]
	public bool cannotBeDeleted;

	[SerializeField]
	[Tooltip("The sound loop to play while this object exists. Use this field to ensure the sound only plays once when prediction creates two instances.")]
	private LoopSoundDef flightSoundLoop;

	private Rigidbody rigidbody;

	public bool canImpactOnTrigger;

	public bool allowPrediction = true;

	[NonSerialized]
	[SyncVar]
	public ushort predictionId;

	[SyncVar]
	[HideInInspector]
	public GameObject owner;

	public float procCoefficient = 1f;

	private Collider[] myColliders;

	private NetworkInstanceId ___ownerNetId;

	public TeamFilter teamFilter { get; private set; }

	public ProjectileGhostController ghost { get; set; }

	public bool isPrediction { get; set; }

	public bool shouldPlaySounds { get; set; }

	public ProcChainMask procChainMask { get; set; }

	public NetworkConnection clientAuthorityOwner { get; private set; }

	public ushort NetworkpredictionId
	{
		get
		{
			return predictionId;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).SetSyncVar<ushort>(value, ref predictionId, 1u);
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

	public event Action<ProjectileController> onInitialized;

	private void Awake()
	{
		rigidbody = ((Component)this).GetComponent<Rigidbody>();
		teamFilter = ((Component)this).GetComponent<TeamFilter>();
		myColliders = ((Component)this).GetComponents<Collider>();
		for (int i = 0; i < myColliders.Length; i++)
		{
			myColliders[i].enabled = false;
		}
	}

	private void Start()
	{
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < myColliders.Length; i++)
		{
			myColliders[i].enabled = true;
		}
		IgnoreCollisionsWithOwner(shouldIgnore: true);
		if (!isPrediction && !NetworkServer.active)
		{
			ProjectileManager.instance.OnClientProjectileReceived(this);
		}
		GameObject val = ProjectileGhostReplacementManager.FindProjectileGhostPrefab(this);
		shouldPlaySounds = false;
		if (isPrediction || !allowPrediction || !((NetworkBehaviour)this).hasAuthority)
		{
			shouldPlaySounds = true;
			if (Object.op_Implicit((Object)(object)val))
			{
				Transform transform = ((Component)this).transform;
				if (Object.op_Implicit((Object)(object)ghostTransformAnchor))
				{
					transform = ghostTransformAnchor;
				}
				ghost = Object.Instantiate<GameObject>(val, transform.position, transform.rotation).GetComponent<ProjectileGhostController>();
				if (isPrediction)
				{
					ghost.predictionTransform = transform;
				}
				else
				{
					ghost.authorityTransform = transform;
				}
				((Behaviour)ghost).enabled = true;
			}
		}
		clientAuthorityOwner = ((Component)this).GetComponent<NetworkIdentity>().clientAuthorityOwner;
		if (shouldPlaySounds)
		{
			PointSoundManager.EmitSoundLocal((AkEventIdArg)startSound, ((Component)this).transform.position);
			if (Object.op_Implicit((Object)(object)flightSoundLoop))
			{
				Util.PlaySound(flightSoundLoop.startSoundName, ((Component)this).gameObject);
			}
		}
	}

	private void OnDestroy()
	{
		if (NetworkServer.active && (Object)(object)ProjectileManager.instance != (Object)null)
		{
			ProjectileManager.instance.OnServerProjectileDestroyed(this);
		}
		if (shouldPlaySounds && Object.op_Implicit((Object)(object)flightSoundLoop))
		{
			Util.PlaySound(flightSoundLoop.stopSoundName, ((Component)this).gameObject);
		}
		if (Object.op_Implicit((Object)(object)ghost) && isPrediction)
		{
			Object.Destroy((Object)(object)((Component)ghost).gameObject);
		}
	}

	private void OnEnable()
	{
		InstanceTracker.Add<ProjectileController>(this);
		IgnoreCollisionsWithOwner(shouldIgnore: true);
	}

	private void OnDisable()
	{
		InstanceTracker.Remove<ProjectileController>(this);
	}

	public void IgnoreCollisionsWithOwner(bool shouldIgnore)
	{
		if (!Object.op_Implicit((Object)(object)owner))
		{
			return;
		}
		ModelLocator component = owner.GetComponent<ModelLocator>();
		if (!Object.op_Implicit((Object)(object)component))
		{
			return;
		}
		Transform modelTransform = component.modelTransform;
		if (!Object.op_Implicit((Object)(object)modelTransform))
		{
			return;
		}
		HurtBoxGroup component2 = ((Component)modelTransform).GetComponent<HurtBoxGroup>();
		if (!Object.op_Implicit((Object)(object)component2))
		{
			return;
		}
		HurtBox[] hurtBoxes = component2.hurtBoxes;
		for (int i = 0; i < hurtBoxes.Length; i++)
		{
			List<Collider> gameObjectComponents = GetComponentsCache<Collider>.GetGameObjectComponents(((Component)hurtBoxes[i]).gameObject);
			int j = 0;
			for (int count = gameObjectComponents.Count; j < count; j++)
			{
				Collider val = gameObjectComponents[j];
				for (int k = 0; k < myColliders.Length; k++)
				{
					Collider val2 = myColliders[k];
					Physics.IgnoreCollision(val, val2, shouldIgnore);
				}
			}
			GetComponentsCache<Collider>.ReturnBuffer(gameObjectComponents);
		}
	}

	private static Vector3 EstimateContactPoint(ContactPoint[] contacts, Collider collider)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		if (contacts.Length == 0)
		{
			return ((Component)collider).transform.position;
		}
		return ((ContactPoint)(ref contacts[0])).point;
	}

	private static Vector3 EstimateContactNormal(ContactPoint[] contacts)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		if (contacts.Length == 0)
		{
			return Vector3.zero;
		}
		return ((ContactPoint)(ref contacts[0])).normal;
	}

	public void OnCollisionEnter(Collision collision)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		if (NetworkServer.active || isPrediction)
		{
			ContactPoint[] contacts = collision.contacts;
			ProjectileImpactInfo projectileImpactInfo = default(ProjectileImpactInfo);
			projectileImpactInfo.collider = collision.collider;
			projectileImpactInfo.estimatedPointOfImpact = EstimateContactPoint(contacts, collision.collider);
			projectileImpactInfo.estimatedImpactNormal = EstimateContactNormal(contacts);
			ProjectileImpactInfo impactInfo = projectileImpactInfo;
			IProjectileImpactBehavior[] components = ((Component)this).GetComponents<IProjectileImpactBehavior>();
			for (int i = 0; i < components.Length; i++)
			{
				components[i].OnProjectileImpact(impactInfo);
			}
		}
	}

	public void OnTriggerEnter(Collider collider)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		if (NetworkServer.active && canImpactOnTrigger)
		{
			Vector3 val = Vector3.zero;
			if (Object.op_Implicit((Object)(object)rigidbody))
			{
				val = rigidbody.velocity;
			}
			ProjectileImpactInfo projectileImpactInfo = default(ProjectileImpactInfo);
			projectileImpactInfo.collider = collider;
			projectileImpactInfo.estimatedPointOfImpact = ((Component)this).transform.position;
			projectileImpactInfo.estimatedImpactNormal = -((Vector3)(ref val)).normalized;
			ProjectileImpactInfo impactInfo = projectileImpactInfo;
			IProjectileImpactBehavior[] components = ((Component)this).GetComponents<IProjectileImpactBehavior>();
			for (int i = 0; i < components.Length; i++)
			{
				components[i].OnProjectileImpact(impactInfo);
			}
		}
	}

	public void DispatchOnInitialized()
	{
		this.onInitialized?.Invoke(this);
	}

	private void OnValidate()
	{
		if (!Application.IsPlaying((Object)(object)this))
		{
			bool localPlayerAuthority = ((Component)this).GetComponent<NetworkIdentity>().localPlayerAuthority;
			if (allowPrediction && !localPlayerAuthority)
			{
				Debug.LogWarningFormat((Object)(object)((Component)this).gameObject, "ProjectileController: {0} allows predictions, so it should have localPlayerAuthority=true", new object[1] { ((Component)this).gameObject });
			}
		}
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.WritePackedUInt32((uint)predictionId);
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
			writer.WritePackedUInt32((uint)predictionId);
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
			predictionId = (ushort)reader.ReadPackedUInt32();
			___ownerNetId = reader.ReadNetworkId();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if (((uint)num & (true ? 1u : 0u)) != 0)
		{
			predictionId = (ushort)reader.ReadPackedUInt32();
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
