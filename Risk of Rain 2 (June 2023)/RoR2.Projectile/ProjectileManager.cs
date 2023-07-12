using System.Collections.Generic;
using RoR2.Networking;
using Unity;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2.Projectile;

public class ProjectileManager : MonoBehaviour
{
	private class PlayerFireProjectileMessage : MessageBase
	{
		public double sendTime;

		public uint prefabIndexPlusOne;

		public Vector3 position;

		public Quaternion rotation;

		public GameObject owner;

		public HurtBoxReference target;

		public float damage;

		public float force;

		public bool crit;

		public ushort predictionId;

		public DamageColorIndex damageColorIndex;

		public float speedOverride;

		public float fuseOverride;

		public bool useDamageTypeOverride;

		public DamageType damageTypeOverride;

		public override void Serialize(NetworkWriter writer)
		{
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			writer.Write(sendTime);
			writer.WritePackedUInt32(prefabIndexPlusOne);
			writer.Write(position);
			writer.Write(rotation);
			writer.Write(owner);
			GeneratedNetworkCode._WriteHurtBoxReference_None(writer, target);
			writer.Write(damage);
			writer.Write(force);
			writer.Write(crit);
			writer.WritePackedUInt32((uint)predictionId);
			writer.Write((int)damageColorIndex);
			writer.Write(speedOverride);
			writer.Write(fuseOverride);
			writer.Write(useDamageTypeOverride);
			writer.Write((int)damageTypeOverride);
		}

		public override void Deserialize(NetworkReader reader)
		{
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			sendTime = reader.ReadDouble();
			prefabIndexPlusOne = reader.ReadPackedUInt32();
			position = reader.ReadVector3();
			rotation = reader.ReadQuaternion();
			owner = reader.ReadGameObject();
			target = GeneratedNetworkCode._ReadHurtBoxReference_None(reader);
			damage = reader.ReadSingle();
			force = reader.ReadSingle();
			crit = reader.ReadBoolean();
			predictionId = (ushort)reader.ReadPackedUInt32();
			damageColorIndex = (DamageColorIndex)reader.ReadInt32();
			speedOverride = reader.ReadSingle();
			fuseOverride = reader.ReadSingle();
			useDamageTypeOverride = reader.ReadBoolean();
			damageTypeOverride = (DamageType)reader.ReadInt32();
		}
	}

	private class ReleasePredictionIdMessage : MessageBase
	{
		public ushort predictionId;

		public override void Serialize(NetworkWriter writer)
		{
			writer.WritePackedUInt32((uint)predictionId);
		}

		public override void Deserialize(NetworkReader reader)
		{
			predictionId = (ushort)reader.ReadPackedUInt32();
		}
	}

	private class PredictionManager
	{
		private Dictionary<ushort, ProjectileController> predictions = new Dictionary<ushort, ProjectileController>();

		public ProjectileController FindPredictedProjectileController(ushort predictionId)
		{
			return predictions[predictionId];
		}

		public void OnAuthorityProjectileReceived(ProjectileController authoritativeProjectile)
		{
			if (((NetworkBehaviour)authoritativeProjectile).hasAuthority && authoritativeProjectile.predictionId != 0 && predictions.TryGetValue(authoritativeProjectile.predictionId, out var value))
			{
				authoritativeProjectile.ghost = value.ghost;
				if (Object.op_Implicit((Object)(object)authoritativeProjectile.ghost))
				{
					authoritativeProjectile.ghost.authorityTransform = ((Component)authoritativeProjectile).transform;
				}
			}
		}

		public void ReleasePredictionId(ushort predictionId)
		{
			ProjectileController projectileController = predictions[predictionId];
			predictions.Remove(predictionId);
			if (Object.op_Implicit((Object)(object)projectileController) && Object.op_Implicit((Object)(object)((Component)projectileController).gameObject))
			{
				Object.Destroy((Object)(object)((Component)projectileController).gameObject);
			}
		}

		public void RegisterPrediction(ProjectileController predictedProjectile)
		{
			predictedProjectile.NetworkpredictionId = RequestPredictionId();
			predictions[predictedProjectile.predictionId] = predictedProjectile;
			predictedProjectile.isPrediction = true;
		}

		private ushort RequestPredictionId()
		{
			for (ushort num = 1; num < 32767; num = (ushort)(num + 1))
			{
				if (!predictions.ContainsKey(num))
				{
					return num;
				}
			}
			return 0;
		}
	}

	private PredictionManager predictionManager;

	private PlayerFireProjectileMessage fireMsg = new PlayerFireProjectileMessage();

	private ReleasePredictionIdMessage releasePredictionIdMsg = new ReleasePredictionIdMessage();

	public static ProjectileManager instance { get; private set; }

	private void Awake()
	{
		predictionManager = new PredictionManager();
	}

	private void OnEnable()
	{
		instance = SingletonHelper.Assign<ProjectileManager>(instance, this);
	}

	private void OnDisable()
	{
		instance = SingletonHelper.Unassign<ProjectileManager>(instance, this);
	}

	[NetworkMessageHandler(msgType = 49, server = true)]
	private static void HandlePlayerFireProjectile(NetworkMessage netMsg)
	{
		if (Object.op_Implicit((Object)(object)instance))
		{
			instance.HandlePlayerFireProjectileInternal(netMsg);
		}
	}

	[NetworkMessageHandler(msgType = 50, client = true)]
	private static void HandleReleaseProjectilePredictionId(NetworkMessage netMsg)
	{
		if (Object.op_Implicit((Object)(object)instance))
		{
			instance.HandleReleaseProjectilePredictionIdInternal(netMsg);
		}
	}

	public void FireProjectile(GameObject prefab, Vector3 position, Quaternion rotation, GameObject owner, float damage, float force, bool crit, DamageColorIndex damageColorIndex = DamageColorIndex.Default, GameObject target = null, float speedOverride = -1f)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		FireProjectileInfo fireProjectileInfo = default(FireProjectileInfo);
		fireProjectileInfo.projectilePrefab = prefab;
		fireProjectileInfo.position = position;
		fireProjectileInfo.rotation = rotation;
		fireProjectileInfo.owner = owner;
		fireProjectileInfo.damage = damage;
		fireProjectileInfo.force = force;
		fireProjectileInfo.crit = crit;
		fireProjectileInfo.damageColorIndex = damageColorIndex;
		fireProjectileInfo.target = target;
		fireProjectileInfo.speedOverride = speedOverride;
		fireProjectileInfo.fuseOverride = -1f;
		fireProjectileInfo.damageTypeOverride = null;
		FireProjectileInfo fireProjectileInfo2 = fireProjectileInfo;
		FireProjectile(fireProjectileInfo2);
	}

	public void FireProjectile(FireProjectileInfo fireProjectileInfo)
	{
		if (NetworkServer.active)
		{
			FireProjectileServer(fireProjectileInfo, null, 0);
		}
		else
		{
			FireProjectileClient(fireProjectileInfo, NetworkManager.singleton.client);
		}
	}

	private void FireProjectileClient(FireProjectileInfo fireProjectileInfo, NetworkClient client)
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Expected O, but got Unknown
		int projectileIndex = ProjectileCatalog.GetProjectileIndex(fireProjectileInfo.projectilePrefab);
		if (projectileIndex == -1)
		{
			Debug.LogErrorFormat((Object)(object)fireProjectileInfo.projectilePrefab, "Prefab {0} is not a registered projectile prefab.", new object[1] { fireProjectileInfo.projectilePrefab });
			return;
		}
		bool allowPrediction = ProjectileCatalog.GetProjectilePrefabProjectileControllerComponent(projectileIndex).allowPrediction;
		ushort predictionId = 0;
		if (allowPrediction)
		{
			ProjectileController component = Object.Instantiate<GameObject>(fireProjectileInfo.projectilePrefab, fireProjectileInfo.position, fireProjectileInfo.rotation).GetComponent<ProjectileController>();
			InitializeProjectile(component, fireProjectileInfo);
			predictionManager.RegisterPrediction(component);
			predictionId = component.predictionId;
		}
		fireMsg.sendTime = Run.instance.time;
		fireMsg.prefabIndexPlusOne = Util.IntToUintPlusOne(projectileIndex);
		fireMsg.position = fireProjectileInfo.position;
		fireMsg.rotation = fireProjectileInfo.rotation;
		fireMsg.owner = fireProjectileInfo.owner;
		fireMsg.predictionId = predictionId;
		fireMsg.damage = fireProjectileInfo.damage;
		fireMsg.force = fireProjectileInfo.force;
		fireMsg.crit = fireProjectileInfo.crit;
		fireMsg.damageColorIndex = fireProjectileInfo.damageColorIndex;
		fireMsg.speedOverride = fireProjectileInfo.speedOverride;
		fireMsg.fuseOverride = fireProjectileInfo.fuseOverride;
		fireMsg.useDamageTypeOverride = fireProjectileInfo.damageTypeOverride.HasValue;
		fireMsg.damageTypeOverride = fireProjectileInfo.damageTypeOverride ?? DamageType.Generic;
		if (Object.op_Implicit((Object)(object)fireProjectileInfo.target))
		{
			HurtBox component2 = fireProjectileInfo.target.GetComponent<HurtBox>();
			fireMsg.target = (Object.op_Implicit((Object)(object)component2) ? HurtBoxReference.FromHurtBox(component2) : HurtBoxReference.FromRootObject(fireProjectileInfo.target));
		}
		NetworkWriter val = new NetworkWriter();
		val.StartMessage((short)49);
		val.Write((MessageBase)(object)fireMsg);
		val.FinishMessage();
		client.SendWriter(val, 0);
	}

	private static void InitializeProjectile(ProjectileController projectileController, FireProjectileInfo fireProjectileInfo)
	{
		GameObject gameObject = ((Component)projectileController).gameObject;
		ProjectileDamage component = gameObject.GetComponent<ProjectileDamage>();
		TeamFilter component2 = gameObject.GetComponent<TeamFilter>();
		ProjectileNetworkTransform component3 = gameObject.GetComponent<ProjectileNetworkTransform>();
		ProjectileTargetComponent component4 = gameObject.GetComponent<ProjectileTargetComponent>();
		ProjectileSimple component5 = gameObject.GetComponent<ProjectileSimple>();
		projectileController.Networkowner = fireProjectileInfo.owner;
		projectileController.procChainMask = fireProjectileInfo.procChainMask;
		if (Object.op_Implicit((Object)(object)component2))
		{
			component2.teamIndex = TeamComponent.GetObjectTeam(fireProjectileInfo.owner);
		}
		if (Object.op_Implicit((Object)(object)component3))
		{
			component3.SetValuesFromTransform();
		}
		if (Object.op_Implicit((Object)(object)component4) && Object.op_Implicit((Object)(object)fireProjectileInfo.target))
		{
			CharacterBody component6 = fireProjectileInfo.target.GetComponent<CharacterBody>();
			component4.target = (Object.op_Implicit((Object)(object)component6) ? component6.coreTransform : fireProjectileInfo.target.transform);
		}
		if (fireProjectileInfo.useSpeedOverride && Object.op_Implicit((Object)(object)component5))
		{
			component5.desiredForwardSpeed = fireProjectileInfo.speedOverride;
		}
		if (fireProjectileInfo.useFuseOverride)
		{
			ProjectileImpactExplosion component7 = gameObject.GetComponent<ProjectileImpactExplosion>();
			if (Object.op_Implicit((Object)(object)component7))
			{
				component7.lifetime = fireProjectileInfo.fuseOverride;
			}
			ProjectileFuse component8 = gameObject.GetComponent<ProjectileFuse>();
			if (Object.op_Implicit((Object)(object)component8))
			{
				component8.fuse = fireProjectileInfo.fuseOverride;
			}
		}
		if (Object.op_Implicit((Object)(object)component))
		{
			component.damage = fireProjectileInfo.damage;
			component.force = fireProjectileInfo.force;
			component.crit = fireProjectileInfo.crit;
			component.damageColorIndex = fireProjectileInfo.damageColorIndex;
			if (fireProjectileInfo.damageTypeOverride.HasValue)
			{
				component.damageType = fireProjectileInfo.damageTypeOverride.Value;
			}
		}
		projectileController.DispatchOnInitialized();
	}

	private void FireProjectileServer(FireProjectileInfo fireProjectileInfo, NetworkConnection clientAuthorityOwner = null, ushort predictionId = 0, double fastForwardTime = 0.0)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = Object.Instantiate<GameObject>(fireProjectileInfo.projectilePrefab, fireProjectileInfo.position, fireProjectileInfo.rotation);
		ProjectileController component = val.GetComponent<ProjectileController>();
		component.NetworkpredictionId = predictionId;
		InitializeProjectile(component, fireProjectileInfo);
		NetworkIdentity component2 = val.GetComponent<NetworkIdentity>();
		if (clientAuthorityOwner != null && component2.localPlayerAuthority)
		{
			NetworkServer.SpawnWithClientAuthority(val, clientAuthorityOwner);
		}
		else
		{
			NetworkServer.Spawn(val);
		}
	}

	public void OnServerProjectileDestroyed(ProjectileController projectile)
	{
		if (projectile.predictionId != 0)
		{
			NetworkConnection clientAuthorityOwner = projectile.clientAuthorityOwner;
			if (clientAuthorityOwner != null)
			{
				ReleasePredictionId(clientAuthorityOwner, projectile.predictionId);
			}
		}
	}

	public void OnClientProjectileReceived(ProjectileController projectile)
	{
		if (projectile.predictionId != 0 && ((NetworkBehaviour)projectile).hasAuthority)
		{
			predictionManager.OnAuthorityProjectileReceived(projectile);
		}
	}

	private void ReleasePredictionId(NetworkConnection owner, ushort predictionId)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected O, but got Unknown
		releasePredictionIdMsg.predictionId = predictionId;
		NetworkWriter val = new NetworkWriter();
		val.StartMessage((short)50);
		val.Write((MessageBase)(object)releasePredictionIdMsg);
		val.FinishMessage();
		owner.SendWriter(val, 0);
	}

	private void HandlePlayerFireProjectileInternal(NetworkMessage netMsg)
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		netMsg.ReadMessage<PlayerFireProjectileMessage>(fireMsg);
		GameObject projectilePrefab = ProjectileCatalog.GetProjectilePrefab(Util.UintToIntMinusOne(fireMsg.prefabIndexPlusOne));
		if ((Object)(object)projectilePrefab == (Object)null)
		{
			ReleasePredictionId(netMsg.conn, fireMsg.predictionId);
			return;
		}
		FireProjectileServer(new FireProjectileInfo
		{
			projectilePrefab = projectilePrefab,
			position = fireMsg.position,
			rotation = fireMsg.rotation,
			owner = fireMsg.owner,
			damage = fireMsg.damage,
			force = fireMsg.force,
			crit = fireMsg.crit,
			target = fireMsg.target.ResolveGameObject(),
			damageColorIndex = fireMsg.damageColorIndex,
			speedOverride = fireMsg.speedOverride,
			fuseOverride = fireMsg.fuseOverride,
			damageTypeOverride = (fireMsg.useDamageTypeOverride ? new DamageType?(fireMsg.damageTypeOverride) : null)
		}, netMsg.conn, fireMsg.predictionId, (double)Run.instance.time - fireMsg.sendTime);
	}

	private void HandleReleaseProjectilePredictionIdInternal(NetworkMessage netMsg)
	{
		netMsg.ReadMessage<ReleasePredictionIdMessage>(releasePredictionIdMsg);
		predictionManager.ReleasePredictionId(releasePredictionIdMsg.predictionId);
	}
}
