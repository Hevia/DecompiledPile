using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public class RadiotowerTerminal : NetworkBehaviour
{
	[SyncVar(hook = "SetHasBeenPurchased")]
	private bool hasBeenPurchased;

	private UnlockableDef unlockableDef;

	public string unlockSoundString;

	public GameObject unlockEffect;

	public bool NetworkhasBeenPurchased
	{
		get
		{
			return hasBeenPurchased;
		}
		[param: In]
		set
		{
			if (NetworkServer.localClientActive && !((NetworkBehaviour)this).syncVarHookGuard)
			{
				((NetworkBehaviour)this).syncVarHookGuard = true;
				SetHasBeenPurchased(value);
				((NetworkBehaviour)this).syncVarHookGuard = false;
			}
			((NetworkBehaviour)this).SetSyncVar<bool>(value, ref hasBeenPurchased, 1u);
		}
	}

	private void SetHasBeenPurchased(bool newHasBeenPurchased)
	{
		if (hasBeenPurchased != newHasBeenPurchased)
		{
			NetworkhasBeenPurchased = newHasBeenPurchased;
		}
	}

	public void Start()
	{
		if (NetworkServer.active)
		{
			FindStageLogUnlockable();
		}
		_ = NetworkClient.active;
	}

	private void FindStageLogUnlockable()
	{
		SceneDef mostRecentSceneDef = SceneCatalog.mostRecentSceneDef;
		if (Object.op_Implicit((Object)(object)mostRecentSceneDef))
		{
			unlockableDef = SceneCatalog.GetUnlockableLogFromBaseSceneName(mostRecentSceneDef.baseSceneName);
		}
	}

	[Server]
	public void GrantUnlock(Interactor interactor)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.RadiotowerTerminal::GrantUnlock(RoR2.Interactor)' called on client");
			return;
		}
		EffectManager.SpawnEffect(unlockEffect, new EffectData
		{
			origin = ((Component)this).transform.position
		}, transmit: true);
		SetHasBeenPurchased(newHasBeenPurchased: true);
		if (Object.op_Implicit((Object)(object)Run.instance))
		{
			Util.PlaySound(unlockSoundString, ((Component)interactor).gameObject);
			Run.instance.GrantUnlockToAllParticipatingPlayers(unlockableDef);
			string pickupToken = "???";
			if (Object.op_Implicit((Object)(object)unlockableDef))
			{
				pickupToken = unlockableDef.nameToken;
			}
			Chat.SendBroadcastChat(new Chat.PlayerPickupChatMessage
			{
				subjectAsCharacterBody = ((Component)interactor).GetComponent<CharacterBody>(),
				baseToken = "PLAYER_PICKUP",
				pickupToken = pickupToken,
				pickupColor = ColorCatalog.GetColor(ColorCatalog.ColorIndex.Unlockable),
				pickupQuantity = 1u
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
			writer.Write(hasBeenPurchased);
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
			writer.Write(hasBeenPurchased);
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
			hasBeenPurchased = reader.ReadBoolean();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if (((uint)num & (true ? 1u : 0u)) != 0)
		{
			SetHasBeenPurchased(reader.ReadBoolean());
		}
	}

	public override void PreStartClient()
	{
	}
}
