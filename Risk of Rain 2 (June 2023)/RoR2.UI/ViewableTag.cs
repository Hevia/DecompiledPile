using System.Collections.Generic;
using RoR2.ConVar;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace RoR2.UI;

public class ViewableTag : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler
{
	public enum ViewableVisualStyle
	{
		Button,
		Icon
	}

	private static readonly List<ViewableTag> instancesList = new List<ViewableTag>();

	[SerializeField]
	[FormerlySerializedAs("viewableName")]
	[Tooltip("The path of the viewable that determines whether or not the \"NEW\" tag is activated.")]
	private string _viewableName;

	[Tooltip("Marks the named viewable as viewed when this component is disabled.")]
	public bool markAsViewedOnDisable;

	public bool markAsViewedOnHover;

	public ViewableVisualStyle viewableVisualStyle;

	public static readonly BoolConVar viewablesWarnUndefined = new BoolConVar("viewables_warn_undefined", ConVarFlags.None, "0", "Issues a warning in the console if a viewable is not defined.");

	private static GameObject tagPrefab;

	private GameObject tagInstance;

	private static bool pendingRefreshAll = false;

	public string viewableName
	{
		get
		{
			return _viewableName;
		}
		set
		{
			if (!(_viewableName == value))
			{
				_viewableName = value;
				Refresh();
			}
		}
	}

	private bool Check()
	{
		if (LocalUserManager.readOnlyLocalUsersList.Count == 0)
		{
			return false;
		}
		UserProfile userProfile = LocalUserManager.readOnlyLocalUsersList[0].userProfile;
		ViewablesCatalog.Node node = ViewablesCatalog.FindNode(viewableName ?? "");
		if (node == null)
		{
			if (viewablesWarnUndefined.value)
			{
				Debug.LogWarningFormat("Viewable {0} is not defined.", new object[1] { viewableName });
			}
			return false;
		}
		return node.shouldShowUnviewed(userProfile);
	}

	private void OnEnable()
	{
		instancesList.Add(this);
		RoR2Application.onNextUpdate += CallRefreshIfStillValid;
	}

	private void CallRefreshIfStillValid()
	{
		if (Object.op_Implicit((Object)(object)this))
		{
			Refresh();
		}
	}

	public void Refresh()
	{
		bool flag = ((Behaviour)this).enabled && Check();
		if (Object.op_Implicit((Object)(object)tagInstance) != flag)
		{
			if (Object.op_Implicit((Object)(object)tagInstance))
			{
				Object.Destroy((Object)(object)tagInstance);
				tagInstance = null;
			}
			else
			{
				string childName = viewableVisualStyle.ToString();
				tagInstance = Object.Instantiate<GameObject>(tagPrefab, ((Component)this).transform);
				((Component)tagInstance.GetComponent<ChildLocator>().FindChild(childName)).gameObject.SetActive(true);
			}
		}
	}

	private void OnDisable()
	{
		instancesList.Remove(this);
		Refresh();
		if (markAsViewedOnDisable)
		{
			TriggerView();
		}
	}

	private void TriggerView()
	{
		ViewableTrigger.TriggerView(viewableName);
	}

	[RuntimeInitializeOnLoadMethod]
	private static void Init()
	{
		tagPrefab = LegacyResourcesAPI.Load<GameObject>("Prefabs/UI/NewViewableTag");
		UserProfile.onUserProfileViewedViewablesChanged += delegate
		{
			if (!pendingRefreshAll)
			{
				pendingRefreshAll = true;
				RoR2Application.onNextUpdate += delegate
				{
					foreach (ViewableTag instances in instancesList)
					{
						instances.Refresh();
					}
					pendingRefreshAll = false;
				};
			}
		};
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (markAsViewedOnHover)
		{
			TriggerView();
		}
	}
}
