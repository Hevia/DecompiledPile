using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RoR2.UI;

[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(RectTransform))]
public class CombatHealthBarViewer : MonoBehaviour, ILayoutGroup, ILayoutController
{
	private class HealthBarInfo
	{
		public HealthComponent sourceHealthComponent;

		public Transform sourceTransform;

		public GameObject healthBarRootObject;

		public Transform healthBarRootObjectTransform;

		public HealthBar healthBar;

		public float verticalOffset;

		public float endTime = float.NegativeInfinity;
	}

	private static readonly List<CombatHealthBarViewer> instancesList;

	public RectTransform container;

	public GameObject healthBarPrefab;

	public float healthBarDuration;

	private const float hoverHealthBarDuration = 1f;

	private RectTransform rectTransform;

	private Canvas canvas;

	private UICamera uiCamera;

	private List<HealthComponent> trackedVictims = new List<HealthComponent>();

	private Dictionary<HealthComponent, HealthBarInfo> victimToHealthBarInfo = new Dictionary<HealthComponent, HealthBarInfo>();

	public float zPosition;

	private const float overheadOffset = 1f;

	public HealthComponent crosshairTarget { get; set; }

	public GameObject viewerBodyObject { get; set; }

	public CharacterBody viewerBody { get; set; }

	public TeamIndex viewerTeamIndex { get; set; }

	static CombatHealthBarViewer()
	{
		instancesList = new List<CombatHealthBarViewer>();
		GlobalEventManager.onClientDamageNotified += delegate(DamageDealtMessage msg)
		{
			if (Object.op_Implicit((Object)(object)msg.victim) && !msg.isSilent)
			{
				HealthComponent component = msg.victim.GetComponent<HealthComponent>();
				if (Object.op_Implicit((Object)(object)component) && !component.dontShowHealthbar)
				{
					TeamIndex objectTeam = TeamComponent.GetObjectTeam(((Component)component).gameObject);
					foreach (CombatHealthBarViewer instances in instancesList)
					{
						if ((Object)(object)msg.attacker == (Object)(object)instances.viewerBodyObject && Object.op_Implicit((Object)(object)instances.viewerBodyObject))
						{
							instances.HandleDamage(component, objectTeam);
						}
					}
				}
			}
		};
	}

	private void Update()
	{
		if (Object.op_Implicit((Object)(object)crosshairTarget))
		{
			HealthBarInfo healthBarInfo = GetHealthBarInfo(crosshairTarget);
			healthBarInfo.endTime = Mathf.Max(healthBarInfo.endTime, Time.time + 1f);
		}
		SetDirty();
	}

	private void Awake()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		rectTransform = (RectTransform)((Component)this).transform;
		canvas = ((Component)this).GetComponent<Canvas>();
	}

	private void Start()
	{
		FindCamera();
	}

	private void FindCamera()
	{
		uiCamera = ((Component)canvas.rootCanvas.worldCamera).GetComponent<UICamera>();
	}

	private void OnEnable()
	{
		instancesList.Add(this);
	}

	private void OnDisable()
	{
		instancesList.Remove(this);
		for (int num = trackedVictims.Count - 1; num >= 0; num--)
		{
			Remove(num);
		}
	}

	private void Remove(int trackedVictimIndex)
	{
		Remove(trackedVictimIndex, victimToHealthBarInfo[trackedVictims[trackedVictimIndex]]);
	}

	private void Remove(int trackedVictimIndex, HealthBarInfo healthBarInfo)
	{
		trackedVictims.RemoveAt(trackedVictimIndex);
		Object.Destroy((Object)(object)healthBarInfo.healthBarRootObject);
		victimToHealthBarInfo.Remove(healthBarInfo.sourceHealthComponent);
	}

	private bool VictimIsValid(HealthComponent victim)
	{
		if (Object.op_Implicit((Object)(object)victim) && victim.alive)
		{
			if (!(victimToHealthBarInfo[victim].endTime > Time.time))
			{
				return (Object)(object)victim == (Object)(object)crosshairTarget;
			}
			return true;
		}
		return false;
	}

	private void LateUpdate()
	{
		CleanUp();
	}

	private void CleanUp()
	{
		for (int num = trackedVictims.Count - 1; num >= 0; num--)
		{
			HealthComponent healthComponent = trackedVictims[num];
			if (!VictimIsValid(healthComponent))
			{
				Remove(num, victimToHealthBarInfo[healthComponent]);
			}
		}
	}

	private void UpdateAllHealthbarPositions(Camera sceneCam, Camera uiCam)
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)sceneCam) || !Object.op_Implicit((Object)(object)uiCam))
		{
			return;
		}
		foreach (HealthBarInfo value in victimToHealthBarInfo.Values)
		{
			if (Object.op_Implicit((Object)(object)value.sourceTransform) && Object.op_Implicit((Object)(object)value.healthBarRootObjectTransform))
			{
				Vector3 position = value.sourceTransform.position;
				position.y += value.verticalOffset;
				Vector3 val = sceneCam.WorldToScreenPoint(position);
				val.z = ((val.z > 0f) ? 1f : (-1f));
				Vector3 position2 = uiCam.ScreenToWorldPoint(val);
				value.healthBarRootObjectTransform.position = position2;
			}
		}
	}

	private void HandleDamage(HealthComponent victimHealthComponent, TeamIndex victimTeam)
	{
		if (viewerTeamIndex != victimTeam && victimTeam != TeamIndex.None)
		{
			CharacterBody body = victimHealthComponent.body;
			if (!Object.op_Implicit((Object)(object)body) || body.GetVisibilityLevel(viewerBody) >= VisibilityLevel.Revealed)
			{
				GetHealthBarInfo(victimHealthComponent).endTime = Time.time + healthBarDuration;
			}
		}
	}

	private HealthBarInfo GetHealthBarInfo(HealthComponent victimHealthComponent)
	{
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		if (!victimToHealthBarInfo.TryGetValue(victimHealthComponent, out var value))
		{
			value = new HealthBarInfo();
			value.healthBarRootObject = Object.Instantiate<GameObject>(healthBarPrefab, (Transform)(object)container);
			value.healthBarRootObjectTransform = value.healthBarRootObject.transform;
			value.healthBar = value.healthBarRootObject.GetComponent<HealthBar>();
			value.healthBar.source = victimHealthComponent;
			value.healthBar.viewerBody = viewerBody;
			value.healthBarRootObject.GetComponentInChildren<BuffDisplay>().source = victimHealthComponent.body;
			value.sourceHealthComponent = victimHealthComponent;
			value.verticalOffset = 0f;
			Collider component = ((Component)victimHealthComponent).GetComponent<Collider>();
			if (Object.op_Implicit((Object)(object)component))
			{
				HealthBarInfo healthBarInfo = value;
				Bounds bounds = component.bounds;
				healthBarInfo.verticalOffset = ((Bounds)(ref bounds)).extents.y;
			}
			value.sourceTransform = victimHealthComponent.body.coreTransform ?? ((Component)victimHealthComponent).transform;
			ModelLocator component2 = ((Component)victimHealthComponent).GetComponent<ModelLocator>();
			if (Object.op_Implicit((Object)(object)component2))
			{
				Transform modelTransform = component2.modelTransform;
				if (Object.op_Implicit((Object)(object)modelTransform))
				{
					ChildLocator component3 = ((Component)modelTransform).GetComponent<ChildLocator>();
					if (Object.op_Implicit((Object)(object)component3))
					{
						Transform val = component3.FindChild("HealthBarOrigin");
						if (Object.op_Implicit((Object)(object)val))
						{
							value.sourceTransform = val;
							value.verticalOffset = 0f;
						}
					}
				}
			}
			victimToHealthBarInfo.Add(victimHealthComponent, value);
			trackedVictims.Add(victimHealthComponent);
		}
		return value;
	}

	private void SetDirty()
	{
		if (((Behaviour)this).isActiveAndEnabled && !CanvasUpdateRegistry.IsRebuildingLayout())
		{
			LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
		}
	}

	private static void LayoutForCamera(UICamera uiCamera)
	{
		Camera camera = uiCamera.camera;
		Camera sceneCam = uiCamera.cameraRigController.sceneCam;
		for (int i = 0; i < instancesList.Count; i++)
		{
			instancesList[i].UpdateAllHealthbarPositions(sceneCam, camera);
		}
	}

	public void SetLayoutHorizontal()
	{
		if (Object.op_Implicit((Object)(object)uiCamera))
		{
			LayoutForCamera(uiCamera);
		}
	}

	public void SetLayoutVertical()
	{
	}
}
