using System.Collections.Generic;
using System.Globalization;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RoR2.UI;

public class PingIndicator : MonoBehaviour
{
	public enum PingType
	{
		Default,
		Enemy,
		Interactable,
		Count
	}

	public PositionIndicator positionIndicator;

	public TextMeshPro pingText;

	public Highlight pingHighlight;

	public ObjectScaleCurve pingObjectScaleCurve;

	public GameObject positionIndicatorRoot;

	public Color textBaseColor;

	public GameObject[] defaultPingGameObjects;

	public Color defaultPingColor;

	public float defaultPingDuration;

	public GameObject[] enemyPingGameObjects;

	public Color enemyPingColor;

	public float enemyPingDuration;

	public GameObject[] interactablePingGameObjects;

	public Color interactablePingColor;

	public float interactablePingDuration;

	public static List<PingIndicator> instancesList = new List<PingIndicator>();

	private PingType pingType;

	private Color pingColor;

	private float pingDuration;

	private PurchaseInteraction pingTargetPurchaseInteraction;

	private Transform targetTransformToFollow;

	private float fixedTimer;

	private static readonly StringBuilder sharedStringBuilder = new StringBuilder();

	public Vector3 pingOrigin { get; set; }

	public Vector3 pingNormal { get; set; }

	public GameObject pingOwner { get; set; }

	public GameObject pingTarget { get; set; }

	public static Sprite GetInteractableIcon(GameObject gameObject)
	{
		PingInfoProvider component = gameObject.GetComponent<PingInfoProvider>();
		if (Object.op_Implicit((Object)(object)component) && Object.op_Implicit((Object)(object)component.pingIconOverride))
		{
			return component.pingIconOverride;
		}
		string text = "Textures/MiscIcons/texMysteryIcon";
		if (Object.op_Implicit((Object)(object)gameObject.GetComponent<BarrelInteraction>()))
		{
			text = "Textures/MiscIcons/texBarrelIcon";
		}
		else if (((Object)gameObject).name.Contains("Shrine"))
		{
			text = "Textures/MiscIcons/texShrineIconOutlined";
		}
		else if (Object.op_Implicit((Object)(object)gameObject.GetComponent<GenericPickupController>()) || Object.op_Implicit((Object)(object)gameObject.GetComponent<PickupPickerController>()))
		{
			text = "Textures/MiscIcons/texLootIconOutlined";
		}
		else if (Object.op_Implicit((Object)(object)gameObject.GetComponent<SummonMasterBehavior>()))
		{
			text = "Textures/MiscIcons/texDroneIconOutlined";
		}
		else if (Object.op_Implicit((Object)(object)gameObject.GetComponent<TeleporterInteraction>()))
		{
			text = "Textures/MiscIcons/texTeleporterIconOutlined";
		}
		else
		{
			PortalStatueBehavior component2 = gameObject.GetComponent<PortalStatueBehavior>();
			if (Object.op_Implicit((Object)(object)component2) && component2.portalType == PortalStatueBehavior.PortalType.Shop)
			{
				text = "Textures/MiscIcons/texMysteryIcon";
			}
			else
			{
				PurchaseInteraction component3 = gameObject.GetComponent<PurchaseInteraction>();
				if (Object.op_Implicit((Object)(object)component3) && component3.costType == CostTypeIndex.LunarCoin)
				{
					text = "Textures/MiscIcons/texMysteryIcon";
				}
				else if (Object.op_Implicit((Object)(object)component3) || Object.op_Implicit((Object)(object)gameObject.GetComponent<TimedChestController>()))
				{
					text = "Textures/MiscIcons/texInventoryIconOutlined";
				}
			}
		}
		return LegacyResourcesAPI.Load<Sprite>(text);
	}

	private void OnEnable()
	{
		instancesList.Add(this);
	}

	private void OnDisable()
	{
		instancesList.Remove(this);
	}

	public void RebuildPing()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Expected O, but got Unknown
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_0219: Unknown result type (might be due to invalid IL or missing references)
		//IL_021e: Unknown result type (might be due to invalid IL or missing references)
		//IL_027d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0282: Unknown result type (might be due to invalid IL or missing references)
		//IL_037c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0381: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_05bb: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.rotation = Util.QuaternionSafeLookRotation(pingNormal);
		((Component)this).transform.position = (Object.op_Implicit((Object)(object)pingTarget) ? pingTarget.transform.position : pingOrigin);
		((Component)this).transform.localScale = Vector3.one;
		positionIndicator.targetTransform = (Object.op_Implicit((Object)(object)pingTarget) ? pingTarget.transform : null);
		positionIndicator.defaultPosition = ((Component)this).transform.position;
		IDisplayNameProvider displayNameProvider = (Object.op_Implicit((Object)(object)pingTarget) ? pingTarget.GetComponentInParent<IDisplayNameProvider>() : null);
		CharacterBody characterBody = null;
		ModelLocator modelLocator = null;
		pingType = PingType.Default;
		((Behaviour)pingObjectScaleCurve).enabled = false;
		((Behaviour)pingObjectScaleCurve).enabled = true;
		GameObject[] array = defaultPingGameObjects;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(false);
		}
		array = enemyPingGameObjects;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(false);
		}
		array = interactablePingGameObjects;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(false);
		}
		if (Object.op_Implicit((Object)(object)pingTarget))
		{
			Debug.LogFormat("Ping target {0}", new object[1] { pingTarget });
			modelLocator = pingTarget.GetComponent<ModelLocator>();
			if (displayNameProvider != null)
			{
				characterBody = pingTarget.GetComponent<CharacterBody>();
				if (Object.op_Implicit((Object)(object)characterBody))
				{
					pingType = PingType.Enemy;
					targetTransformToFollow = characterBody.coreTransform;
				}
				else
				{
					pingType = PingType.Interactable;
				}
			}
		}
		string bestMasterName = Util.GetBestMasterName(pingOwner.GetComponent<CharacterMaster>());
		string text = (Object.op_Implicit((Object)(MonoBehaviour)displayNameProvider) ? Util.GetBestBodyName(((Component)(MonoBehaviour)displayNameProvider).gameObject) : "");
		((Behaviour)pingText).enabled = true;
		((TMP_Text)pingText).text = bestMasterName;
		switch (pingType)
		{
		case PingType.Default:
		{
			pingColor = defaultPingColor;
			pingDuration = defaultPingDuration;
			pingHighlight.isOn = false;
			array = defaultPingGameObjects;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActive(true);
			}
			Chat.AddMessage(string.Format(Language.GetString("PLAYER_PING_DEFAULT"), bestMasterName));
			break;
		}
		case PingType.Enemy:
		{
			pingColor = enemyPingColor;
			pingDuration = enemyPingDuration;
			array = enemyPingGameObjects;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActive(true);
			}
			if (!Object.op_Implicit((Object)(object)modelLocator))
			{
				break;
			}
			Transform modelTransform = modelLocator.modelTransform;
			if (Object.op_Implicit((Object)(object)modelTransform))
			{
				CharacterModel component3 = ((Component)modelTransform).GetComponent<CharacterModel>();
				if (Object.op_Implicit((Object)(object)component3))
				{
					bool flag = false;
					CharacterModel.RendererInfo[] baseRendererInfos = component3.baseRendererInfos;
					for (int i = 0; i < baseRendererInfos.Length; i++)
					{
						CharacterModel.RendererInfo rendererInfo = baseRendererInfos[i];
						if (!rendererInfo.ignoreOverlays && !flag)
						{
							pingHighlight.highlightColor = Highlight.HighlightColor.teleporter;
							pingHighlight.targetRenderer = rendererInfo.renderer;
							pingHighlight.strength = 1f;
							pingHighlight.isOn = true;
							flag = true;
						}
					}
				}
			}
			Chat.AddMessage(string.Format(Language.GetString("PLAYER_PING_ENEMY"), bestMasterName, text));
			break;
		}
		case PingType.Interactable:
		{
			pingColor = interactablePingColor;
			pingDuration = interactablePingDuration;
			pingTargetPurchaseInteraction = pingTarget.GetComponent<PurchaseInteraction>();
			Sprite interactableIcon = GetInteractableIcon(pingTarget);
			SpriteRenderer component = interactablePingGameObjects[0].GetComponent<SpriteRenderer>();
			ShopTerminalBehavior component2 = pingTarget.GetComponent<ShopTerminalBehavior>();
			if (Object.op_Implicit((Object)(object)component2))
			{
				PickupIndex pickupIndex = component2.CurrentPickupIndex();
				text = string.Format(CultureInfo.InvariantCulture, "{0} ({1})", text, (component2.pickupIndexIsHidden || !Object.op_Implicit((Object)(object)component2.pickupDisplay)) ? "?" : Language.GetString(PickupCatalog.GetPickupDef(pickupIndex)?.nameToken ?? PickupCatalog.invalidPickupToken));
			}
			else if (!((Object)pingTarget.gameObject).name.Contains("Shrine") && (Object.op_Implicit((Object)(object)pingTarget.GetComponent<GenericPickupController>()) || Object.op_Implicit((Object)(object)pingTarget.GetComponent<PickupPickerController>()) || Object.op_Implicit((Object)(object)pingTarget.GetComponent<TeleporterInteraction>())))
			{
				pingDuration = 60f;
			}
			array = interactablePingGameObjects;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActive(true);
			}
			Renderer val = null;
			val = ((!Object.op_Implicit((Object)(object)modelLocator)) ? pingTarget.GetComponentInChildren<Renderer>() : ((Component)modelLocator.modelTransform).GetComponentInChildren<Renderer>());
			if (Object.op_Implicit((Object)(object)val))
			{
				pingHighlight.highlightColor = Highlight.HighlightColor.interactive;
				pingHighlight.targetRenderer = val;
				pingHighlight.strength = 1f;
				pingHighlight.isOn = true;
			}
			component.sprite = interactableIcon;
			if (Object.op_Implicit((Object)(object)pingTargetPurchaseInteraction) && pingTargetPurchaseInteraction.costType != 0)
			{
				sharedStringBuilder.Clear();
				CostTypeCatalog.GetCostTypeDef(pingTargetPurchaseInteraction.costType).BuildCostStringStyled(pingTargetPurchaseInteraction.cost, sharedStringBuilder, forWorldDisplay: false);
				Chat.AddMessage(string.Format(Language.GetString("PLAYER_PING_INTERACTABLE_WITH_COST"), bestMasterName, text, sharedStringBuilder.ToString()));
			}
			else
			{
				Chat.AddMessage(string.Format(Language.GetString("PLAYER_PING_INTERACTABLE"), bestMasterName, text));
			}
			break;
		}
		}
		((Graphic)pingText).color = textBaseColor * pingColor;
		fixedTimer = pingDuration;
	}

	private void Update()
	{
		if (pingType == PingType.Interactable && Object.op_Implicit((Object)(object)pingTargetPurchaseInteraction) && !pingTargetPurchaseInteraction.available)
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
		fixedTimer -= Time.deltaTime;
		if (fixedTimer <= 0f)
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
	}

	private void LateUpdate()
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)pingTarget))
		{
			if (pingTarget != null)
			{
				Object.Destroy((Object)(object)((Component)this).gameObject);
			}
		}
		else if (Object.op_Implicit((Object)(object)targetTransformToFollow))
		{
			((Component)this).transform.SetPositionAndRotation(targetTransformToFollow.position, targetTransformToFollow.rotation);
		}
	}
}
