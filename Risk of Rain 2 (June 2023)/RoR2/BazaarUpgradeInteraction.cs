using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public sealed class BazaarUpgradeInteraction : NetworkBehaviour, IInteractable, IHologramContentProvider, IDisplayNameProvider
{
	[SyncVar]
	public bool available = true;

	public string displayNameToken;

	public int cost;

	public string contextToken;

	public string[] unlockableProgression;

	private UnlockableDef[] unlockableProgressionDefs;

	public float activationCooldownDuration = 1f;

	private float activationTimer;

	public GameObject purchaseEffect;

	private static readonly Color32 lunarCoinColor = new Color32((byte)198, (byte)173, (byte)250, byte.MaxValue);

	private static readonly string lunarCoinColorString = Util.RGBToHex(lunarCoinColor);

	public bool Networkavailable
	{
		get
		{
			return available;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).SetSyncVar<bool>(value, ref available, 1u);
		}
	}

	private void Awake()
	{
		unlockableProgressionDefs = new UnlockableDef[unlockableProgression.Length];
		for (int i = 0; i < unlockableProgressionDefs.Length; i++)
		{
			unlockableProgressionDefs[i] = UnlockableCatalog.GetUnlockableDef(unlockableProgression[i]);
		}
	}

	private void FixedUpdate()
	{
		if (NetworkServer.active && !available)
		{
			activationTimer -= Time.fixedDeltaTime;
			if (activationTimer <= 0f)
			{
				Networkavailable = true;
			}
		}
	}

	public bool ShouldIgnoreSpherecastForInteractibility(Interactor activator)
	{
		return false;
	}

	private UnlockableDef GetInteractorNextUnlockable(GameObject activatorGameObject)
	{
		NetworkUser networkUser = Util.LookUpBodyNetworkUser(activatorGameObject);
		if (Object.op_Implicit((Object)(object)networkUser))
		{
			LocalUser localUser = networkUser.localUser;
			if (localUser != null)
			{
				for (int i = 0; i < unlockableProgressionDefs.Length; i++)
				{
					UnlockableDef unlockableDef = unlockableProgressionDefs[i];
					if (!localUser.userProfile.HasUnlockable(unlockableDef))
					{
						return unlockableDef;
					}
				}
			}
			else
			{
				for (int j = 0; j < unlockableProgressionDefs.Length; j++)
				{
					UnlockableDef unlockableDef2 = unlockableProgressionDefs[j];
					if (!networkUser.unlockables.Contains(unlockableDef2))
					{
						return unlockableDef2;
					}
				}
			}
		}
		return null;
	}

	private static bool ActivatorHasUnlockable(Interactor activator, string unlockableName)
	{
		NetworkUser networkUser = Util.LookUpBodyNetworkUser(((Component)activator).gameObject);
		if (Object.op_Implicit((Object)(object)networkUser))
		{
			return networkUser.localUser?.userProfile.HasUnlockable(unlockableName) ?? networkUser.unlockables.Contains(UnlockableCatalog.GetUnlockableDef(unlockableName));
		}
		return true;
	}

	public string GetDisplayName()
	{
		return Language.GetString(displayNameToken);
	}

	private string GetCostString()
	{
		return string.Format(" (<color=#{1}>{0}</color>)", cost, lunarCoinColorString);
	}

	public string GetContextString(Interactor activator)
	{
		if (!CanBeAffordedByInteractor(activator))
		{
			return null;
		}
		return Language.GetString(contextToken) + GetCostString();
	}

	public Interactability GetInteractability(Interactor activator)
	{
		if ((Object)(object)GetInteractorNextUnlockable(((Component)activator).gameObject) == (Object)null || !available)
		{
			return Interactability.Disabled;
		}
		if (!CanBeAffordedByInteractor(activator))
		{
			return Interactability.ConditionsNotMet;
		}
		return Interactability.Available;
	}

	public void OnInteractionBegin(Interactor activator)
	{
	}

	private int GetCostForInteractor(Interactor activator)
	{
		return cost;
	}

	public bool CanBeAffordedByInteractor(Interactor activator)
	{
		NetworkUser networkUser = Util.LookUpBodyNetworkUser(((Component)activator).gameObject);
		if (Object.op_Implicit((Object)(object)networkUser))
		{
			return networkUser.lunarCoins >= GetCostForInteractor(activator);
		}
		return false;
	}

	public bool ShouldDisplayHologram(GameObject viewer)
	{
		return (Object)(object)GetInteractorNextUnlockable(viewer) != (Object)null;
	}

	public GameObject GetHologramContentPrefab()
	{
		return LegacyResourcesAPI.Load<GameObject>("Prefabs/CostHologramContent");
	}

	public void UpdateHologramContent(GameObject hologramContentObject)
	{
		CostHologramContent component = hologramContentObject.GetComponent<CostHologramContent>();
		if (Object.op_Implicit((Object)(object)component))
		{
			component.displayValue = cost;
			component.costType = CostTypeIndex.LunarCoin;
		}
	}

	private void OnEnable()
	{
		InstanceTracker.Add<BazaarUpgradeInteraction>(this);
	}

	private void OnDisable()
	{
		InstanceTracker.Remove<BazaarUpgradeInteraction>(this);
	}

	public bool ShouldShowOnScanner()
	{
		return available;
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.Write(available);
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
			writer.Write(available);
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
			available = reader.ReadBoolean();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if (((uint)num & (true ? 1u : 0u)) != 0)
		{
			available = reader.ReadBoolean();
		}
	}

	public override void PreStartClient()
	{
	}
}
