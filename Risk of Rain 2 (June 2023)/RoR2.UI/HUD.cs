using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using RoR2.ConVar;
using UnityEngine;

namespace RoR2.UI;

[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(MPEventSystemProvider))]
public class HUD : MonoBehaviour
{
	public delegate void ShouldHudDisplayDelegate(HUD hud, ref bool shouldDisplay);

	private static List<HUD> instancesList;

	private static bool lockInstancesList;

	private static List<GameObject> instancesToReenableList;

	public static readonly ReadOnlyCollection<HUD> readOnlyInstanceList;

	private LocalUser _localUserViewer;

	[Header("Main")]
	[Tooltip("Immediate child of this object which contains all other UI.")]
	public GameObject mainContainer;

	[NonSerialized]
	public CameraRigController cameraRigController;

	public GameObject scoreboardPanel;

	public GameObject mainUIPanel;

	public GameObject cinematicPanel;

	public CombatHealthBarViewer combatHealthBarViewer;

	public ContextManager contextManager;

	public AllyCardManager allyCardManager;

	public Transform gameModeUiRoot;

	[Header("Character")]
	public HealthBar healthBar;

	public ExpBar expBar;

	public LevelText levelText;

	public BuffDisplay buffDisplay;

	public MoneyText moneyText;

	public GameObject lunarCoinContainer;

	public MoneyText lunarCoinText;

	public GameObject voidCoinContainer;

	public MoneyText voidCoinText;

	public SkillIcon[] skillIcons;

	public EquipmentIcon[] equipmentIcons;

	public ItemInventoryDisplay itemInventoryDisplay;

	[Header("Debug")]
	public HUDSpeedometer speedometer;

	private MPEventSystemProvider eventSystemProvider;

	private Canvas canvas;

	private GameObject previousTargetBodyObject;

	public static readonly BoolConVar cvHudEnable;

	public GameObject targetBodyObject
	{
		get
		{
			if (!Object.op_Implicit((Object)(object)targetMaster))
			{
				return null;
			}
			return targetMaster.GetBodyObject();
		}
	}

	public CharacterMaster targetMaster { get; set; }

	public LocalUser localUserViewer
	{
		get
		{
			return _localUserViewer;
		}
		set
		{
			if (_localUserViewer != value)
			{
				_localUserViewer = value;
				eventSystemProvider.eventSystem = _localUserViewer.eventSystem;
			}
		}
	}

	public GameObject gameModeUiInstance { get; private set; }

	public static event Action<HUD> onHudTargetChangedGlobal;

	public static event ShouldHudDisplayDelegate shouldHudDisplay;

	static HUD()
	{
		instancesList = new List<HUD>();
		lockInstancesList = false;
		instancesToReenableList = new List<GameObject>();
		readOnlyInstanceList = instancesList.AsReadOnly();
		cvHudEnable = new BoolConVar("hud_enable", ConVarFlags.Archive, "1", "Enable/disable the HUD.");
	}

	private static void OnUICameraPreRender(UICamera uiCamera)
	{
		CameraRigController cameraRigController = uiCamera.cameraRigController;
		if (!Object.op_Implicit((Object)(object)cameraRigController))
		{
			return;
		}
		LocalUser localUser = (Object.op_Implicit((Object)(object)cameraRigController.viewer) ? cameraRigController.viewer.localUser : null);
		if (localUser == null)
		{
			return;
		}
		lockInstancesList = true;
		for (int i = 0; i < instancesList.Count; i++)
		{
			HUD hUD = instancesList[i];
			if (hUD.localUserViewer == localUser)
			{
				hUD.canvas.worldCamera = uiCamera.camera;
				continue;
			}
			GameObject gameObject = ((Component)hUD).gameObject;
			instancesToReenableList.Add(gameObject);
			gameObject.SetActive(false);
		}
		lockInstancesList = false;
	}

	private static void OnUICameraPostRender(UICamera uiCamera)
	{
		lockInstancesList = true;
		for (int i = 0; i < instancesToReenableList.Count; i++)
		{
			instancesToReenableList[i].SetActive(true);
		}
		instancesToReenableList.Clear();
		lockInstancesList = false;
	}

	public void OnEnable()
	{
		if (!lockInstancesList)
		{
			instancesList.Add(this);
			UpdateHudVisibility();
		}
	}

	public void OnDisable()
	{
		if (!lockInstancesList)
		{
			instancesList.Remove(this);
		}
	}

	private void Awake()
	{
		eventSystemProvider = ((Component)this).GetComponent<MPEventSystemProvider>();
		canvas = ((Component)this).GetComponent<Canvas>();
		if (Object.op_Implicit((Object)(object)scoreboardPanel))
		{
			scoreboardPanel.SetActive(false);
		}
		mainUIPanel.SetActive(false);
		gameModeUiInstance = Run.instance.InstantiateUi(gameModeUiRoot);
	}

	public void Update()
	{
		NetworkUser networkUser = ((!Object.op_Implicit((Object)(object)targetMaster)) ? null : ((Component)targetMaster).GetComponent<PlayerCharacterMasterController>()?.networkUser);
		PlayerCharacterMasterController playerCharacterMasterController = (Object.op_Implicit((Object)(object)targetMaster) ? ((Component)targetMaster).GetComponent<PlayerCharacterMasterController>() : null);
		Inventory inventory = (Object.op_Implicit((Object)(object)targetMaster) ? targetMaster.inventory : null);
		CharacterBody characterBody = (Object.op_Implicit((Object)(object)targetBodyObject) ? targetBodyObject.GetComponent<CharacterBody>() : null);
		if (Object.op_Implicit((Object)(object)healthBar) && Object.op_Implicit((Object)(object)targetBodyObject))
		{
			healthBar.source = targetBodyObject.GetComponent<HealthComponent>();
		}
		if (Object.op_Implicit((Object)(object)expBar))
		{
			expBar.source = targetMaster;
		}
		if (Object.op_Implicit((Object)(object)levelText))
		{
			levelText.source = characterBody;
		}
		if (Object.op_Implicit((Object)(object)moneyText))
		{
			moneyText.targetValue = (int)(Object.op_Implicit((Object)(object)targetMaster) ? targetMaster.money : 0);
		}
		if (Object.op_Implicit((Object)(object)lunarCoinContainer))
		{
			bool flag = localUserViewer != null && localUserViewer.userProfile.totalCollectedCoins != 0;
			uint targetValue = (Object.op_Implicit((Object)(object)networkUser) ? networkUser.lunarCoins : 0u);
			lunarCoinContainer.SetActive(flag);
			if (flag && Object.op_Implicit((Object)(object)lunarCoinText))
			{
				lunarCoinText.targetValue = (int)targetValue;
			}
		}
		if (Object.op_Implicit((Object)(object)voidCoinContainer))
		{
			int num = (int)(Object.op_Implicit((Object)(object)targetMaster) ? targetMaster.voidCoins : 0);
			bool flag2 = num > 0;
			voidCoinContainer.SetActive(flag2);
			if (flag2 && Object.op_Implicit((Object)(object)voidCoinText))
			{
				voidCoinText.targetValue = num;
			}
		}
		if (Object.op_Implicit((Object)(object)itemInventoryDisplay))
		{
			itemInventoryDisplay.SetSubscribedInventory(inventory);
		}
		if (Object.op_Implicit((Object)(object)targetBodyObject))
		{
			SkillLocator component = targetBodyObject.GetComponent<SkillLocator>();
			if (Object.op_Implicit((Object)(object)component))
			{
				if (skillIcons.Length != 0 && Object.op_Implicit((Object)(object)skillIcons[0]))
				{
					skillIcons[0].targetSkillSlot = SkillSlot.Primary;
					skillIcons[0].targetSkill = component.primary;
					skillIcons[0].playerCharacterMasterController = playerCharacterMasterController;
				}
				if (skillIcons.Length > 1 && Object.op_Implicit((Object)(object)skillIcons[1]))
				{
					skillIcons[1].targetSkillSlot = SkillSlot.Secondary;
					skillIcons[1].targetSkill = component.secondary;
					skillIcons[1].playerCharacterMasterController = playerCharacterMasterController;
				}
				if (skillIcons.Length > 2 && Object.op_Implicit((Object)(object)skillIcons[2]))
				{
					skillIcons[2].targetSkillSlot = SkillSlot.Utility;
					skillIcons[2].targetSkill = component.utility;
					skillIcons[2].playerCharacterMasterController = playerCharacterMasterController;
				}
				if (skillIcons.Length > 3 && Object.op_Implicit((Object)(object)skillIcons[3]))
				{
					skillIcons[3].targetSkillSlot = SkillSlot.Special;
					skillIcons[3].targetSkill = component.special;
					skillIcons[3].playerCharacterMasterController = playerCharacterMasterController;
				}
			}
		}
		EquipmentIcon[] array = equipmentIcons;
		foreach (EquipmentIcon obj in array)
		{
			obj.targetInventory = inventory;
			obj.targetEquipmentSlot = (Object.op_Implicit((Object)(object)targetBodyObject) ? targetBodyObject.GetComponent<EquipmentSlot>() : null);
			obj.playerCharacterMasterController = (Object.op_Implicit((Object)(object)targetMaster) ? ((Component)targetMaster).GetComponent<PlayerCharacterMasterController>() : null);
		}
		if (Object.op_Implicit((Object)(object)buffDisplay))
		{
			buffDisplay.source = characterBody;
		}
		if (Object.op_Implicit((Object)(object)allyCardManager))
		{
			allyCardManager.sourceGameObject = targetBodyObject;
		}
		if (Object.op_Implicit((Object)(object)scoreboardPanel))
		{
			bool active = localUserViewer != null && localUserViewer.inputPlayer != null && localUserViewer.inputPlayer.GetButton("info");
			scoreboardPanel.SetActive(active);
		}
		if (Object.op_Implicit((Object)(object)speedometer))
		{
			speedometer.targetTransform = (Object.op_Implicit((Object)(object)targetBodyObject) ? targetBodyObject.transform : null);
		}
		if (Object.op_Implicit((Object)(object)combatHealthBarViewer))
		{
			combatHealthBarViewer.crosshairTarget = (Object.op_Implicit((Object)(object)cameraRigController.lastCrosshairHurtBox) ? cameraRigController.lastCrosshairHurtBox.healthComponent : null);
			combatHealthBarViewer.viewerBody = characterBody;
			combatHealthBarViewer.viewerBodyObject = targetBodyObject;
			combatHealthBarViewer.viewerTeamIndex = TeamComponent.GetObjectTeam(targetBodyObject);
		}
		if (targetBodyObject != previousTargetBodyObject)
		{
			previousTargetBodyObject = targetBodyObject;
			HUD.onHudTargetChangedGlobal?.Invoke(this);
		}
		int j = 0;
		for (int num2 = ((Component)this).transform.childCount - 1; j < num2; j++)
		{
			((Component)((Component)this).transform.GetChild(j)).gameObject.SetActive(false);
		}
		if (((Component)this).transform.childCount > 0)
		{
			((Component)((Component)this).transform.GetChild(((Component)this).transform.childCount - 1)).gameObject.SetActive(true);
		}
		UpdateHudVisibility();
	}

	private void UpdateHudVisibility()
	{
		if (mainContainer.activeInHierarchy)
		{
			bool shouldDisplay = cameraRigController?.isHudAllowed ?? false;
			HUD.shouldHudDisplay?.Invoke(this, ref shouldDisplay);
			mainUIPanel.SetActive(cvHudEnable.value && shouldDisplay);
		}
	}
}
