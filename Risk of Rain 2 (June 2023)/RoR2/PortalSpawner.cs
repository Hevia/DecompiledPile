using System.Runtime.InteropServices;
using RoR2.ExpansionManagement;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public class PortalSpawner : NetworkBehaviour
{
	[SerializeField]
	private InteractableSpawnCard portalSpawnCard;

	[SerializeField]
	[Range(0f, 1f)]
	private float spawnChance;

	[Tooltip("The portal is spawned relative to this transform.  If null, it uses this object's transform for reference.")]
	[SerializeField]
	private Transform spawnReferenceLocation;

	[SerializeField]
	private float minSpawnDistance;

	[Tooltip("The maximum spawn distance for the portal relative to the spawnReferenceLocation.  If 0, it will be spawned at exactly the referenced location.")]
	[SerializeField]
	private float maxSpawnDistance;

	[SerializeField]
	private string spawnPreviewMessageToken;

	[SerializeField]
	private string spawnMessageToken;

	[SerializeField]
	private ChildLocator modelChildLocator;

	[SerializeField]
	private string previewChildName;

	[SerializeField]
	private ExpansionDef requiredExpansion;

	[SerializeField]
	private int minStagesCleared;

	[SerializeField]
	private string bannedEventFlag;

	private Xoroshiro128Plus rng;

	private GameObject previewChild;

	[SyncVar(hook = "OnWillSpawnUpdated")]
	private bool willSpawn;

	public bool NetworkwillSpawn
	{
		get
		{
			return willSpawn;
		}
		[param: In]
		set
		{
			if (NetworkServer.localClientActive && !((NetworkBehaviour)this).syncVarHookGuard)
			{
				((NetworkBehaviour)this).syncVarHookGuard = true;
				OnWillSpawnUpdated(value);
				((NetworkBehaviour)this).syncVarHookGuard = false;
			}
			((NetworkBehaviour)this).SetSyncVar<bool>(value, ref willSpawn, 1u);
		}
	}

	private void Start()
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Expected O, but got Unknown
		if (Object.op_Implicit((Object)(object)modelChildLocator))
		{
			Transform val = modelChildLocator.FindChild(previewChildName);
			if (Object.op_Implicit((Object)(object)val))
			{
				previewChild = ((Component)val).gameObject;
			}
		}
		if (!NetworkServer.active)
		{
			return;
		}
		rng = new Xoroshiro128Plus(Run.instance.stageRng.nextUlong);
		bool flag = !Object.op_Implicit((Object)(object)requiredExpansion) || Run.instance.IsExpansionEnabled(requiredExpansion);
		bool flag2 = Run.instance.stageClearCount >= minStagesCleared;
		if ((string.IsNullOrEmpty(bannedEventFlag) || !Run.instance.GetEventFlag(bannedEventFlag)) && rng.nextNormalizedFloat <= spawnChance && flag && flag2)
		{
			NetworkwillSpawn = true;
			if (!string.IsNullOrEmpty(spawnPreviewMessageToken))
			{
				Chat.SendBroadcastChat(new Chat.SimpleChatMessage
				{
					baseToken = spawnPreviewMessageToken
				});
			}
			if (Object.op_Implicit((Object)(object)previewChild))
			{
				previewChild.SetActive(true);
			}
		}
	}

	[Server]
	public bool AttemptSpawnPortalServer()
	{
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Boolean RoR2.PortalSpawner::AttemptSpawnPortalServer()' called on client");
			return false;
		}
		if (willSpawn)
		{
			if (Object.op_Implicit((Object)(object)previewChild))
			{
				previewChild.SetActive(false);
			}
			NetworkwillSpawn = false;
			DirectorPlacementRule.PlacementMode placementMode = DirectorPlacementRule.PlacementMode.Approximate;
			if (maxSpawnDistance <= 0f)
			{
				placementMode = DirectorPlacementRule.PlacementMode.Direct;
			}
			Transform transform = spawnReferenceLocation;
			if (!Object.op_Implicit((Object)(object)transform))
			{
				transform = ((Component)this).transform;
			}
			GameObject obj = DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(portalSpawnCard, new DirectorPlacementRule
			{
				minDistance = minSpawnDistance,
				maxDistance = maxSpawnDistance,
				placementMode = placementMode,
				position = ((Component)this).transform.position,
				spawnOnTarget = transform
			}, rng));
			if (Object.op_Implicit((Object)(object)obj) && !string.IsNullOrEmpty(spawnMessageToken))
			{
				Chat.SendBroadcastChat(new Chat.SimpleChatMessage
				{
					baseToken = spawnMessageToken
				});
			}
			return Object.op_Implicit((Object)(object)obj);
		}
		return false;
	}

	private void OnWillSpawnUpdated(bool newValue)
	{
		NetworkwillSpawn = newValue;
		if (Object.op_Implicit((Object)(object)previewChild))
		{
			previewChild.SetActive(newValue);
		}
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.Write(willSpawn);
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
			writer.Write(willSpawn);
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
			willSpawn = reader.ReadBoolean();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if (((uint)num & (true ? 1u : 0u)) != 0)
		{
			OnWillSpawnUpdated(reader.ReadBoolean());
		}
	}

	public override void PreStartClient()
	{
	}
}
