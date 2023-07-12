using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using RoR2.UI;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public class VoidStageMissionController : NetworkBehaviour
{
	public class FogRequest : IDisposable
	{
		public readonly IZone safeZone;

		private Action<FogRequest> disposeCallback;

		public FogRequest(IZone zone, Action<FogRequest> onDispose)
		{
			safeZone = zone;
			disposeCallback = onDispose;
		}

		public void Dispose()
		{
			disposeCallback?.Invoke(this);
			disposeCallback = null;
		}
	}

	[SerializeField]
	private InteractableSpawnCard batterySpawnCard;

	[SerializeField]
	private int batteryCount;

	[SerializeField]
	private Transform portalRoot;

	[SerializeField]
	private string portalOpenChatToken;

	[SerializeField]
	private GenericObjectiveProvider deepVoidPortalObjectiveProvider;

	[SerializeField]
	private InteractableSpawnCard deepVoidPortalSpawnCard;

	[SerializeField]
	private InteractableSpawnCard voidPortalSpawnCard;

	[SerializeField]
	public string batteryObjectiveToken;

	[SerializeField]
	private FogDamageController fogDamageController;

	private Xoroshiro128Plus rng;

	private GameObject voidPortal;

	private int fogRefCount;

	[SyncVar]
	private int _numBatteriesSpawned;

	[SyncVar]
	private int _numBatteriesActivated;

	public int numBatteriesSpawned => _numBatteriesSpawned;

	public int numBatteriesActivated => _numBatteriesActivated;

	public static VoidStageMissionController instance { get; private set; }

	public int Network_numBatteriesSpawned
	{
		get
		{
			return _numBatteriesSpawned;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).SetSyncVar<int>(value, ref _numBatteriesSpawned, 1u);
		}
	}

	public int Network_numBatteriesActivated
	{
		get
		{
			return _numBatteriesActivated;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).SetSyncVar<int>(value, ref _numBatteriesActivated, 2u);
		}
	}

	public FogRequest RequestFog(IZone zone)
	{
		if (Object.op_Implicit((Object)(object)fogDamageController))
		{
			if (!((Behaviour)fogDamageController).enabled)
			{
				((Behaviour)fogDamageController).enabled = true;
			}
			fogRefCount++;
			fogDamageController.AddSafeZone(zone);
			return new FogRequest(zone, OnFogUnrequested);
		}
		return null;
	}

	private void OnFogUnrequested(FogRequest request)
	{
		if (Object.op_Implicit((Object)(object)fogDamageController))
		{
			fogDamageController.RemoveSafeZone(request.safeZone);
			((Behaviour)fogDamageController).enabled = --fogRefCount > 0;
		}
	}

	private void Start()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Expected O, but got Unknown
		if (Object.op_Implicit((Object)(object)deepVoidPortalObjectiveProvider))
		{
			((Behaviour)deepVoidPortalObjectiveProvider).enabled = false;
		}
		if (!NetworkServer.active)
		{
			return;
		}
		rng = new Xoroshiro128Plus(Run.instance.stageRng.nextUlong);
		Network_numBatteriesSpawned = 0;
		for (int i = 0; i < batteryCount; i++)
		{
			DirectorPlacementRule placementRule = new DirectorPlacementRule
			{
				placementMode = DirectorPlacementRule.PlacementMode.Random
			};
			DirectorSpawnRequest directorSpawnRequest = new DirectorSpawnRequest(batterySpawnCard, placementRule, rng);
			GameObject val = DirectorCore.instance.TrySpawnObject(directorSpawnRequest);
			if (Object.op_Implicit((Object)(object)val) && Object.op_Implicit((Object)(object)val.GetComponent<PurchaseInteraction>()))
			{
				Network_numBatteriesSpawned = _numBatteriesSpawned + 1;
			}
		}
	}

	private void FixedUpdate()
	{
		if (Object.op_Implicit((Object)(object)deepVoidPortalObjectiveProvider) && numBatteriesActivated >= numBatteriesSpawned && numBatteriesSpawned > 0)
		{
			((Behaviour)deepVoidPortalObjectiveProvider).enabled = true;
		}
	}

	public void OnBatteryActivated()
	{
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		Network_numBatteriesActivated = _numBatteriesActivated + 1;
		if (_numBatteriesActivated >= _numBatteriesSpawned && Object.op_Implicit((Object)(object)portalRoot))
		{
			CombatDirector[] array = new CombatDirector[CombatDirector.instancesList.Count];
			CombatDirector.instancesList.CopyTo(array);
			CombatDirector[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				((Behaviour)array2[i]).enabled = false;
			}
			DirectorPlacementRule placementRule = new DirectorPlacementRule
			{
				placementMode = DirectorPlacementRule.PlacementMode.Direct,
				spawnOnTarget = portalRoot
			};
			DirectorSpawnRequest directorSpawnRequest = new DirectorSpawnRequest(deepVoidPortalSpawnCard, placementRule, rng);
			voidPortal = DirectorCore.instance.TrySpawnObject(directorSpawnRequest);
			voidPortal.transform.rotation = portalRoot.rotation;
			NetworkServer.Spawn(voidPortal);
		}
	}

	private void OnEnable()
	{
		instance = SingletonHelper.Assign<VoidStageMissionController>(instance, this);
		ObjectivePanelController.collectObjectiveSources += OnCollectObjectiveSources;
	}

	private void OnDisable()
	{
		instance = SingletonHelper.Unassign<VoidStageMissionController>(instance, this);
		ObjectivePanelController.collectObjectiveSources -= OnCollectObjectiveSources;
	}

	private void OnCollectObjectiveSources(CharacterMaster master, List<ObjectivePanelController.ObjectiveSourceDescriptor> objectiveSourcesList)
	{
		if (_numBatteriesSpawned > 0 && _numBatteriesActivated < _numBatteriesSpawned)
		{
			objectiveSourcesList.Add(new ObjectivePanelController.ObjectiveSourceDescriptor
			{
				master = master,
				objectiveType = typeof(VoidStageBatteryMissionObjectiveTracker),
				source = (Object)(object)this
			});
		}
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.WritePackedUInt32((uint)_numBatteriesSpawned);
			writer.WritePackedUInt32((uint)_numBatteriesActivated);
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
			writer.WritePackedUInt32((uint)_numBatteriesSpawned);
		}
		if ((((NetworkBehaviour)this).syncVarDirtyBits & 2u) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
				flag = true;
			}
			writer.WritePackedUInt32((uint)_numBatteriesActivated);
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
			_numBatteriesSpawned = (int)reader.ReadPackedUInt32();
			_numBatteriesActivated = (int)reader.ReadPackedUInt32();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if (((uint)num & (true ? 1u : 0u)) != 0)
		{
			_numBatteriesSpawned = (int)reader.ReadPackedUInt32();
		}
		if (((uint)num & 2u) != 0)
		{
			_numBatteriesActivated = (int)reader.ReadPackedUInt32();
		}
	}

	public override void PreStartClient()
	{
	}
}
