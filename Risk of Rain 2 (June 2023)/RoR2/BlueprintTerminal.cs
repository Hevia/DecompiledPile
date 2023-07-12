using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public class BlueprintTerminal : NetworkBehaviour
{
	[Serializable]
	public struct UnlockableOption
	{
		[Obsolete("'unlockableName' will be discontinued. Use 'unlockableDef' instead.", false)]
		[Tooltip("'unlockableName' will be discontinued. Use 'unlockableDef' instead.")]
		public string unlockableName;

		public UnlockableDef unlockableDef;

		public int cost;

		public float weight;

		public UnlockableDef GetResolvedUnlockableDef()
		{
			string text = unlockableName;
			if (!Object.op_Implicit((Object)(object)unlockableDef) && !string.IsNullOrEmpty(text))
			{
				unlockableDef = UnlockableCatalog.GetUnlockableDef(text);
			}
			return unlockableDef;
		}
	}

	[SyncVar(hook = "SetHasBeenPurchased")]
	private bool hasBeenPurchased;

	public Transform displayBaseTransform;

	[Tooltip("The unlockables to grant")]
	public UnlockableOption[] unlockableOptions;

	private int unlockableChoice;

	public string unlockSoundString;

	public float idealDisplayVolume = 1.5f;

	public GameObject unlockEffect;

	private GameObject displayInstance;

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
			Rebuild();
		}
	}

	public void Start()
	{
		if (NetworkServer.active)
		{
			RollChoice();
		}
		if (NetworkClient.active)
		{
			Rebuild();
		}
	}

	private void RollChoice()
	{
		WeightedSelection<int> weightedSelection = new WeightedSelection<int>();
		for (int i = 0; i < unlockableOptions.Length; i++)
		{
			weightedSelection.AddChoice(i, unlockableOptions[i].weight);
		}
		unlockableChoice = weightedSelection.Evaluate(Random.value);
		Rebuild();
	}

	private void Rebuild()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		UnlockableOption unlockableOption = unlockableOptions[unlockableChoice];
		if (Object.op_Implicit((Object)(object)displayInstance))
		{
			Object.Destroy((Object)(object)displayInstance);
		}
		((Component)displayBaseTransform).gameObject.SetActive(!hasBeenPurchased);
		if (!hasBeenPurchased && Object.op_Implicit((Object)(object)displayBaseTransform))
		{
			Debug.Log((object)"Found base");
			UnlockableDef resolvedUnlockableDef = unlockableOption.GetResolvedUnlockableDef();
			if (Object.op_Implicit((Object)(object)resolvedUnlockableDef))
			{
				Debug.Log((object)"Found unlockable");
				GameObject displayModelPrefab = resolvedUnlockableDef.displayModelPrefab;
				if (Object.op_Implicit((Object)(object)displayModelPrefab))
				{
					Debug.Log((object)"Found prefab");
					displayInstance = Object.Instantiate<GameObject>(displayModelPrefab, displayBaseTransform.position, ((Component)displayBaseTransform).transform.rotation, displayBaseTransform);
					Renderer componentInChildren = displayInstance.GetComponentInChildren<Renderer>();
					float num = 1f;
					if (Object.op_Implicit((Object)(object)componentInChildren))
					{
						displayInstance.transform.rotation = Quaternion.identity;
						Bounds bounds = componentInChildren.bounds;
						Vector3 size = ((Bounds)(ref bounds)).size;
						float num2 = size.x * size.y * size.z;
						num *= Mathf.Pow(idealDisplayVolume, 1f / 3f) / Mathf.Pow(num2, 1f / 3f);
					}
					displayInstance.transform.localScale = new Vector3(num, num, num);
				}
			}
		}
		PurchaseInteraction component = ((Component)this).GetComponent<PurchaseInteraction>();
		if (Object.op_Implicit((Object)(object)component))
		{
			component.Networkcost = unlockableOption.cost;
		}
	}

	[Server]
	public void GrantUnlock(Interactor interactor)
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.BlueprintTerminal::GrantUnlock(RoR2.Interactor)' called on client");
			return;
		}
		SetHasBeenPurchased(newHasBeenPurchased: true);
		UnlockableOption unlockableOption = unlockableOptions[unlockableChoice];
		UnlockableDef resolvedUnlockableDef = unlockableOption.GetResolvedUnlockableDef();
		EffectManager.SpawnEffect(unlockEffect, new EffectData
		{
			origin = ((Component)this).transform.position
		}, transmit: true);
		if (Object.op_Implicit((Object)(object)Run.instance))
		{
			Util.PlaySound(unlockSoundString, ((Component)interactor).gameObject);
			CharacterBody component = ((Component)interactor).GetComponent<CharacterBody>();
			Run.instance.GrantUnlockToSinglePlayer(resolvedUnlockableDef, component);
			string pickupToken = "???";
			if ((Object)(object)resolvedUnlockableDef != (Object)null)
			{
				pickupToken = resolvedUnlockableDef.nameToken;
			}
			Chat.SendBroadcastChat(new Chat.PlayerPickupChatMessage
			{
				subjectAsCharacterBody = component,
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
