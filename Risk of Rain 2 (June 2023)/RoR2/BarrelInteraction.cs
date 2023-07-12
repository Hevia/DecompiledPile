using System.Runtime.InteropServices;
using EntityStates.Barrel;
using RoR2.Networking;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public sealed class BarrelInteraction : NetworkBehaviour, IInteractable, IDisplayNameProvider
{
	public int goldReward;

	public uint expReward;

	public string displayNameToken = "BARREL1_NAME";

	public string contextToken;

	[SyncVar]
	private bool opened;

	public bool Networkopened
	{
		get
		{
			return opened;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).SetSyncVar<bool>(value, ref opened, 1u);
		}
	}

	public string GetContextString(Interactor activator)
	{
		return Language.GetString(contextToken);
	}

	public override int GetNetworkChannel()
	{
		return QosChannelIndex.defaultReliable.intVal;
	}

	public Interactability GetInteractability(Interactor activator)
	{
		if (opened)
		{
			return Interactability.Disabled;
		}
		return Interactability.Available;
	}

	private void Start()
	{
		if (Object.op_Implicit((Object)(object)Run.instance))
		{
			goldReward = (int)((float)goldReward * Run.instance.difficultyCoefficient);
			expReward = (uint)((float)expReward * Run.instance.difficultyCoefficient);
		}
	}

	[Server]
	public void OnInteractionBegin(Interactor activator)
	{
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.BarrelInteraction::OnInteractionBegin(RoR2.Interactor)' called on client");
		}
		else if (!opened)
		{
			Networkopened = true;
			EntityStateMachine component = ((Component)this).GetComponent<EntityStateMachine>();
			if (Object.op_Implicit((Object)(object)component))
			{
				component.SetNextState(new Opening());
			}
			CharacterBody component2 = ((Component)activator).GetComponent<CharacterBody>();
			if (Object.op_Implicit((Object)(object)component2))
			{
				TeamIndex objectTeam = TeamComponent.GetObjectTeam(((Component)component2).gameObject);
				TeamManager.instance.GiveTeamMoney(objectTeam, (uint)goldReward);
			}
			CoinDrop();
			ExperienceManager.instance.AwardExperience(((Component)this).transform.position, ((Component)activator).GetComponent<CharacterBody>(), expReward);
		}
	}

	[Server]
	private void CoinDrop()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.BarrelInteraction::CoinDrop()' called on client");
			return;
		}
		EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/CoinEmitter"), new EffectData
		{
			origin = ((Component)this).transform.position,
			genericFloat = goldReward
		}, transmit: true);
	}

	public string GetDisplayName()
	{
		return Language.GetString(displayNameToken);
	}

	public bool ShouldIgnoreSpherecastForInteractibility(Interactor activator)
	{
		return false;
	}

	public void OnEnable()
	{
		InstanceTracker.Add<BarrelInteraction>(this);
	}

	public void OnDisable()
	{
		InstanceTracker.Remove<BarrelInteraction>(this);
	}

	public bool ShouldShowOnScanner()
	{
		return !opened;
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.Write(opened);
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
			writer.Write(opened);
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
			opened = reader.ReadBoolean();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if (((uint)num & (true ? 1u : 0u)) != 0)
		{
			opened = reader.ReadBoolean();
		}
	}

	public override void PreStartClient()
	{
	}
}
