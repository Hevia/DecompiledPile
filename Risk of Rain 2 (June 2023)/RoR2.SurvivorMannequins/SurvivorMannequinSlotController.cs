using HG;
using UnityEngine;

namespace RoR2.SurvivorMannequins;

public class SurvivorMannequinSlotController : MonoBehaviour
{
	public GameObject toggleableEffect;

	private NetworkUser _networkUser;

	private SurvivorDef _currentSurvivorDef;

	private Loadout currentLoadout;

	private bool loadoutDirty;

	private Transform mannequinInstanceTransform;

	private bool mannequinInstanceDirty;

	public NetworkUser networkUser
	{
		get
		{
			return _networkUser;
		}
		set
		{
			if (_networkUser != value)
			{
				_networkUser = value;
				mannequinInstanceDirty = true;
				loadoutDirty = true;
			}
		}
	}

	private SurvivorDef currentSurvivorDef
	{
		get
		{
			return _currentSurvivorDef;
		}
		set
		{
			if (_currentSurvivorDef != value)
			{
				_currentSurvivorDef = value;
				mannequinInstanceDirty = true;
			}
		}
	}

	private void Awake()
	{
		currentLoadout = new Loadout();
	}

	private void OnEnable()
	{
		NetworkUser.onLoadoutChangedGlobal += OnLoadoutChangedGlobal;
	}

	private void OnDisable()
	{
		NetworkUser.onLoadoutChangedGlobal -= OnLoadoutChangedGlobal;
	}

	private void Update()
	{
		SurvivorDef survivorDef = null;
		if (Object.op_Implicit((Object)(object)networkUser))
		{
			survivorDef = networkUser.GetSurvivorPreference();
		}
		currentSurvivorDef = survivorDef;
		if (mannequinInstanceDirty)
		{
			mannequinInstanceDirty = false;
			RebuildMannequinInstance();
		}
		if (loadoutDirty)
		{
			loadoutDirty = false;
			if (Object.op_Implicit((Object)(object)networkUser))
			{
				networkUser.networkLoadout.CopyLoadout(currentLoadout);
			}
			ApplyLoadoutToMannequinInstance();
		}
		if (Object.op_Implicit((Object)(object)toggleableEffect))
		{
			toggleableEffect.SetActive(Object.op_Implicit((Object)(object)networkUser));
		}
	}

	private void OnLoadoutChangedGlobal(NetworkUser networkUser)
	{
		if (this.networkUser == networkUser)
		{
			loadoutDirty = true;
		}
	}

	private void ClearMannequinInstance()
	{
		if (Object.op_Implicit((Object)(object)mannequinInstanceTransform))
		{
			Object.Destroy((Object)(object)((Component)mannequinInstanceTransform).gameObject);
		}
		mannequinInstanceTransform = null;
	}

	private void RebuildMannequinInstance()
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		ClearMannequinInstance();
		if (Object.op_Implicit((Object)(object)currentSurvivorDef) && Object.op_Implicit((Object)(object)currentSurvivorDef.displayPrefab))
		{
			mannequinInstanceTransform = Object.Instantiate<GameObject>(currentSurvivorDef.displayPrefab, ((Component)this).transform.position, ((Component)this).transform.rotation, ((Component)this).transform).transform;
			CharacterSelectSurvivorPreviewDisplayController component = ((Component)mannequinInstanceTransform).GetComponent<CharacterSelectSurvivorPreviewDisplayController>();
			if (Object.op_Implicit((Object)(object)component))
			{
				component.networkUser = networkUser;
			}
			ApplyLoadoutToMannequinInstance();
		}
	}

	private void ApplyLoadoutToMannequinInstance()
	{
		if (!Object.op_Implicit((Object)(object)mannequinInstanceTransform))
		{
			return;
		}
		BodyIndex bodyIndexFromSurvivorIndex = SurvivorCatalog.GetBodyIndexFromSurvivorIndex(currentSurvivorDef.survivorIndex);
		int skinIndex = (int)currentLoadout.bodyLoadoutManager.GetSkinIndex(bodyIndexFromSurvivorIndex);
		SkinDef safe = ArrayUtils.GetSafe<SkinDef>(BodyCatalog.GetBodySkins(bodyIndexFromSurvivorIndex), skinIndex);
		if (Object.op_Implicit((Object)(object)safe))
		{
			CharacterModel componentInChildren = ((Component)mannequinInstanceTransform).GetComponentInChildren<CharacterModel>();
			if (Object.op_Implicit((Object)(object)componentInChildren))
			{
				safe.Apply(((Component)componentInChildren).gameObject);
			}
		}
	}

	public static void Swap(SurvivorMannequinSlotController a, SurvivorMannequinSlotController b)
	{
		if (Object.op_Implicit((Object)(object)a.mannequinInstanceTransform))
		{
			a.mannequinInstanceTransform.SetParent(((Component)b).transform, false);
		}
		if (Object.op_Implicit((Object)(object)b.mannequinInstanceTransform))
		{
			b.mannequinInstanceTransform.SetParent(((Component)a).transform, false);
		}
		Util.Swap(ref a._networkUser, ref b._networkUser);
		Util.Swap(ref a._currentSurvivorDef, ref b._currentSurvivorDef);
		Util.Swap(ref a.currentLoadout, ref b.currentLoadout);
		Util.Swap(ref a.loadoutDirty, ref b.loadoutDirty);
		Util.Swap(ref a.mannequinInstanceDirty, ref b.mannequinInstanceDirty);
		Util.Swap(ref a.mannequinInstanceTransform, ref b.mannequinInstanceTransform);
	}
}
