using System.Collections.Generic;
using System.Text;
using Rewired;
using RoR2.ConVar;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace RoR2.UI;

[RequireComponent(typeof(MPEventSystemProvider))]
public class ConsoleWindow : MonoBehaviour
{
	private enum InputFieldState
	{
		Neutral,
		History,
		AutoComplete
	}

	public TMP_InputField inputField;

	public TMP_InputField outputField;

	private TMP_Dropdown autoCompleteDropdown;

	private Console.AutoComplete autoComplete;

	private bool preventAutoCompleteUpdate;

	private bool preventHistoryReset;

	private int historyIndex = -1;

	private readonly StringBuilder stringBuilder = new StringBuilder();

	private const string consoleEnabledDefaultValue = "0";

	private static BoolConVar cvConsoleEnabled = new BoolConVar("console_enabled", ConVarFlags.None, "0", "Enables/Disables the console.");

	public static ConsoleWindow instance { get; private set; }

	private bool autoCompleteInUse
	{
		get
		{
			if (Object.op_Implicit((Object)(object)autoCompleteDropdown))
			{
				return autoCompleteDropdown.IsExpanded;
			}
			return false;
		}
	}

	public void Start()
	{
		((Component)this).GetComponent<MPEventSystemProvider>().eventSystem = MPEventSystemManager.kbmEventSystem;
		if (Object.op_Implicit((Object)(object)outputField.verticalScrollbar))
		{
			outputField.verticalScrollbar.value = 1f;
		}
		((Component)outputField.textComponent).gameObject.AddComponent<RectTransformDimensionsChangeEvent>().onRectTransformDimensionsChange += OnOutputFieldRectTransformDimensionsChange;
	}

	private void OnOutputFieldRectTransformDimensionsChange()
	{
		if (Object.op_Implicit((Object)(object)outputField.verticalScrollbar))
		{
			outputField.verticalScrollbar.value = 0f;
			outputField.verticalScrollbar.value = 1f;
		}
	}

	public void OnEnable()
	{
		Console.onLogReceived += OnLogReceived;
		Console.onClear += OnClear;
		RebuildOutput();
		((UnityEvent<string>)(object)inputField.onSubmit).AddListener((UnityAction<string>)Submit);
		((UnityEvent<string>)(object)inputField.onValueChanged).AddListener((UnityAction<string>)OnInputFieldValueChanged);
		instance = this;
	}

	public void SubmitInputField()
	{
		((UnityEvent<string>)(object)inputField.onSubmit).Invoke(inputField.text);
	}

	public void Submit(string text)
	{
		if (!(inputField.text == ""))
		{
			if (Object.op_Implicit((Object)(object)autoCompleteDropdown))
			{
				autoCompleteDropdown.Hide();
			}
			inputField.text = "";
			Console.instance.SubmitCmd(LocalUserManager.GetFirstLocalUser(), text, recordSubmit: true);
			if (Object.op_Implicit((Object)(object)inputField) && ((UIBehaviour)inputField).IsActive())
			{
				inputField.ActivateInputField();
			}
		}
	}

	private void OnInputFieldValueChanged(string text)
	{
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Expected O, but got Unknown
		if (!preventHistoryReset)
		{
			historyIndex = -1;
		}
		if (preventAutoCompleteUpdate)
		{
			return;
		}
		if (text.Length > 0 != (autoComplete != null))
		{
			if (autoComplete != null)
			{
				Object.Destroy((Object)(object)((Component)autoCompleteDropdown).gameObject);
				autoComplete = null;
			}
			else
			{
				GameObject val = Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/UI/ConsoleAutoCompleteDropdown"), ((Component)inputField).transform);
				autoCompleteDropdown = val.GetComponent<TMP_Dropdown>();
				autoComplete = new Console.AutoComplete(Console.instance);
				((UnityEvent<int>)(object)autoCompleteDropdown.onValueChanged).AddListener((UnityAction<int>)ApplyAutoComplete);
			}
		}
		if (autoComplete != null && autoComplete.SetSearchString(text))
		{
			List<OptionData> list = new List<OptionData>();
			List<string> resultsList = autoComplete.resultsList;
			for (int i = 0; i < resultsList.Count; i++)
			{
				list.Add(new OptionData(resultsList[i]));
			}
			autoCompleteDropdown.options = list;
		}
	}

	public void Update()
	{
		EventSystem val = (EventSystem)(object)MPEventSystemManager.FindEventSystem(ReInput.players.GetPlayer(0));
		if (!Object.op_Implicit((Object)(object)val) || !((Object)(object)val.currentSelectedGameObject == (Object)(object)((Component)inputField).gameObject))
		{
			return;
		}
		InputFieldState inputFieldState = InputFieldState.Neutral;
		if (Object.op_Implicit((Object)(object)autoCompleteDropdown) && autoCompleteInUse)
		{
			inputFieldState = InputFieldState.AutoComplete;
		}
		else if (historyIndex != -1)
		{
			inputFieldState = InputFieldState.History;
		}
		bool keyDown = Input.GetKeyDown((KeyCode)273);
		bool keyDown2 = Input.GetKeyDown((KeyCode)274);
		switch (inputFieldState)
		{
		case InputFieldState.Neutral:
			if (keyDown)
			{
				if (Console.userCmdHistory.Count > 0)
				{
					historyIndex = Console.userCmdHistory.Count - 1;
					preventHistoryReset = true;
					inputField.text = Console.userCmdHistory[historyIndex];
					inputField.MoveToEndOfLine(false, false);
					preventHistoryReset = false;
				}
			}
			else if (keyDown2 && Object.op_Implicit((Object)(object)autoCompleteDropdown))
			{
				autoCompleteDropdown.Show();
				autoCompleteDropdown.value = 0;
				((UnityEvent<int>)(object)autoCompleteDropdown.onValueChanged).Invoke(autoCompleteDropdown.value);
			}
			break;
		case InputFieldState.History:
		{
			int num = 0;
			if (keyDown)
			{
				num--;
			}
			if (keyDown2)
			{
				num++;
			}
			if (num != 0)
			{
				historyIndex += num;
				if (historyIndex < 0)
				{
					historyIndex = 0;
				}
				if (historyIndex >= Console.userCmdHistory.Count)
				{
					historyIndex = -1;
					break;
				}
				preventHistoryReset = true;
				inputField.text = Console.userCmdHistory[historyIndex];
				inputField.MoveToEndOfLine(false, false);
				preventHistoryReset = false;
			}
			break;
		}
		case InputFieldState.AutoComplete:
			if (keyDown2)
			{
				TMP_Dropdown obj = autoCompleteDropdown;
				int value = obj.value + 1;
				obj.value = value;
			}
			if (keyDown)
			{
				if (autoCompleteDropdown.value > 0)
				{
					TMP_Dropdown obj2 = autoCompleteDropdown;
					int value = obj2.value - 1;
					obj2.value = value;
				}
				else
				{
					autoCompleteDropdown.Hide();
				}
			}
			break;
		}
		val.SetSelectedGameObject(((Component)inputField).gameObject);
	}

	private void ApplyAutoComplete(int optionIndex)
	{
		if (Object.op_Implicit((Object)(object)autoCompleteDropdown) && autoCompleteDropdown.options.Count > optionIndex)
		{
			preventAutoCompleteUpdate = true;
			inputField.text = autoCompleteDropdown.options[optionIndex].text;
			inputField.MoveToEndOfLine(false, false);
			preventAutoCompleteUpdate = false;
		}
	}

	public void OnDisable()
	{
		Console.onLogReceived -= OnLogReceived;
		Console.onClear -= OnClear;
		((UnityEvent<string>)(object)inputField.onSubmit).RemoveListener((UnityAction<string>)Submit);
		((UnityEvent<string>)(object)inputField.onValueChanged).RemoveListener((UnityAction<string>)OnInputFieldValueChanged);
		if ((Object)(object)instance == (Object)(object)this)
		{
			instance = null;
		}
	}

	private void OnLogReceived(Console.Log log)
	{
		RebuildOutput();
	}

	private void OnClear()
	{
		RebuildOutput();
	}

	private void RebuildOutput()
	{
		float value = 0f;
		if (Object.op_Implicit((Object)(object)outputField.verticalScrollbar))
		{
			value = outputField.verticalScrollbar.value;
		}
		string[] array = new string[Console.logs.Count];
		stringBuilder.Clear();
		for (int i = 0; i < array.Length; i++)
		{
			stringBuilder.AppendLine(Console.logs[i].message);
		}
		outputField.text = stringBuilder.ToString();
		if (Object.op_Implicit((Object)(object)outputField.verticalScrollbar))
		{
			outputField.verticalScrollbar.value = 0f;
			outputField.verticalScrollbar.value = 1f;
			outputField.verticalScrollbar.value = value;
		}
	}

	private static void CheckConsoleKey()
	{
		bool keyDown = Input.GetKeyDown((KeyCode)96);
		if (Input.GetKey((KeyCode)306) && Input.GetKey((KeyCode)308) && keyDown)
		{
			cvConsoleEnabled.SetBool(!cvConsoleEnabled.value);
		}
		if (cvConsoleEnabled.value && keyDown)
		{
			if (Object.op_Implicit((Object)(object)instance))
			{
				Object.Destroy((Object)(object)((Component)instance).gameObject);
			}
			else
			{
				Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/UI/ConsoleWindow")).GetComponent<ConsoleWindow>().inputField.ActivateInputField();
			}
		}
		if (Input.GetKeyDown((KeyCode)27) && Object.op_Implicit((Object)(object)instance))
		{
			Object.Destroy((Object)(object)((Component)instance).gameObject);
		}
	}

	[RuntimeInitializeOnLoadMethod]
	public static void Init()
	{
		RoR2Application.onUpdate += CheckConsoleKey;
	}
}
