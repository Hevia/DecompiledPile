using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RoR2.UI;

[RequireComponent(typeof(MPEventSystemLocator))]
public class SimpleDialogBox : UIBehaviour
{
	public struct TokenParamsPair
	{
		public string token;

		public object[] formatParams;

		public TokenParamsPair(string token, params object[] formatParams)
		{
			this.token = token;
			this.formatParams = formatParams;
		}
	}

	public static readonly List<SimpleDialogBox> instancesList = new List<SimpleDialogBox>();

	public GameObject rootObject;

	public GameObject buttonPrefab;

	public RectTransform buttonContainer;

	public TextMeshProUGUI headerLabel;

	public TextMeshProUGUI descriptionLabel;

	private MPButton defaultChoice;

	public object[] descriptionFormatParameters;

	public TokenParamsPair headerToken
	{
		set
		{
			((TMP_Text)headerLabel).text = GetString(value);
		}
	}

	public TokenParamsPair descriptionToken
	{
		set
		{
			((TMP_Text)descriptionLabel).text = GetString(value);
		}
	}

	protected override void OnEnable()
	{
		((UIBehaviour)this).OnEnable();
		instancesList.Add(this);
	}

	protected override void OnDisable()
	{
		instancesList.Remove(this);
		((UIBehaviour)this).OnDisable();
	}

	private static string GetString(TokenParamsPair pair)
	{
		string text = Language.GetString(pair.token);
		if (pair.formatParams != null && pair.formatParams.Length != 0)
		{
			text = string.Format(text, pair.formatParams);
		}
		return text;
	}

	private MPButton AddButton(UnityAction callback, string displayToken, params object[] formatParams)
	{
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = Object.Instantiate<GameObject>(buttonPrefab, (Transform)(object)buttonContainer);
		MPButton component = val.GetComponent<MPButton>();
		string text = Language.GetString(displayToken);
		if (formatParams != null && formatParams.Length != 0)
		{
			text = string.Format(text, formatParams);
		}
		((TMP_Text)val.GetComponentInChildren<TextMeshProUGUI>()).text = text;
		((UnityEvent)((Button)component).onClick).AddListener(callback);
		val.SetActive(true);
		if (!Object.op_Implicit((Object)(object)defaultChoice))
		{
			defaultChoice = component;
		}
		if (((Transform)buttonContainer).childCount > 1)
		{
			int siblingIndex = val.transform.GetSiblingIndex();
			int num = siblingIndex - 1;
			int num2 = siblingIndex + 1;
			MPButton mPButton = null;
			MPButton mPButton2 = null;
			if (num > 0)
			{
				mPButton = ((Component)((Transform)buttonContainer).GetChild(num)).GetComponent<MPButton>();
			}
			if (num2 < ((Transform)buttonContainer).childCount)
			{
				mPButton2 = ((Component)((Transform)buttonContainer).GetChild(num2)).GetComponent<MPButton>();
			}
			if (Object.op_Implicit((Object)(object)mPButton))
			{
				Navigation navigation = ((Selectable)mPButton).navigation;
				((Navigation)(ref navigation)).mode = (Mode)4;
				((Navigation)(ref navigation)).selectOnRight = (Selectable)(object)component;
				((Selectable)mPButton).navigation = navigation;
				Navigation navigation2 = ((Selectable)component).navigation;
				((Navigation)(ref navigation2)).mode = (Mode)4;
				((Navigation)(ref navigation2)).selectOnLeft = (Selectable)(object)mPButton;
				((Selectable)component).navigation = navigation2;
			}
			if (Object.op_Implicit((Object)(object)mPButton2))
			{
				Navigation navigation3 = ((Selectable)mPButton2).navigation;
				((Navigation)(ref navigation3)).mode = (Mode)4;
				((Navigation)(ref navigation3)).selectOnLeft = (Selectable)(object)mPButton;
				((Selectable)mPButton2).navigation = navigation3;
				Navigation navigation4 = ((Selectable)component).navigation;
				((Navigation)(ref navigation4)).mode = (Mode)4;
				((Navigation)(ref navigation4)).selectOnRight = (Selectable)(object)mPButton2;
				((Selectable)component).navigation = navigation4;
			}
		}
		return component;
	}

	public MPButton AddCommandButton(string consoleString, string displayToken, params object[] formatParams)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		return AddButton((UnityAction)delegate
		{
			if (!string.IsNullOrEmpty(consoleString))
			{
				Console.instance.SubmitCmd(null, consoleString, recordSubmit: true);
			}
			Object.Destroy((Object)(object)rootObject);
		}, displayToken, formatParams);
	}

	public MPButton AddCancelButton(string displayToken, params object[] formatParams)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		return AddButton((UnityAction)delegate
		{
			Object.Destroy((Object)(object)rootObject);
		}, displayToken, formatParams);
	}

	public MPButton AddActionButton(UnityAction action, string displayToken, bool destroyDialog = true, params object[] formatParams)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Expected O, but got Unknown
		return AddButton((UnityAction)delegate
		{
			action.Invoke();
			if (destroyDialog)
			{
				Object.Destroy((Object)(object)rootObject);
			}
		}, displayToken, formatParams);
	}

	protected override void Start()
	{
		((UIBehaviour)this).Start();
		if (Object.op_Implicit((Object)(object)defaultChoice))
		{
			defaultChoice.defaultFallbackButton = true;
		}
	}

	private void Update()
	{
		((Component)buttonContainer).gameObject.SetActive(((Transform)buttonContainer).childCount > 0);
	}

	public static SimpleDialogBox Create(MPEventSystem owner = null)
	{
		GameObject val = Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/UI/SimpleDialogRoot"));
		if (Object.op_Implicit((Object)(object)owner))
		{
			MPEventSystemProvider component = val.GetComponent<MPEventSystemProvider>();
			component.eventSystem = owner;
			component.fallBackToMainEventSystem = false;
			((EventSystem)component.eventSystem).SetSelectedGameObject((GameObject)null);
		}
		return ((Component)val.transform).GetComponentInChildren<SimpleDialogBox>();
	}
}
