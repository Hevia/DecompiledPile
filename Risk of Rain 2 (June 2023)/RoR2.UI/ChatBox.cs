using System.Collections.ObjectModel;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RoR2.UI;

[RequireComponent(typeof(MPEventSystemLocator))]
public class ChatBox : MonoBehaviour
{
	[Header("Cached Components")]
	public TMP_InputField messagesText;

	public TMP_InputField inputField;

	public Button sendButton;

	public Graphic[] gameplayHiddenGraphics;

	public RectTransform standardChatboxRect;

	public RectTransform expandedChatboxRect;

	public ScrollRect scrollRect;

	[Tooltip("The canvas group to use to fade this chat box. Leave empty for no fading behavior.")]
	public CanvasGroup fadeGroup;

	[Header("Parameters")]
	public bool allowExpandedChatbox;

	public bool deselectAfterSubmitChat;

	public bool deactivateInputFieldIfInactive;

	public float fadeWait = 5f;

	public float fadeDuration = 5f;

	private float fadeTimer;

	private MPEventSystemLocator eventSystemLocator;

	private bool _showInput;

	private bool showKeybindTipOnStart => Chat.readOnlyLog.Count == 0;

	private bool showInput
	{
		get
		{
			return _showInput;
		}
		set
		{
			if (_showInput != value)
			{
				_showInput = value;
				RebuildChatRects();
				if (Object.op_Implicit((Object)(object)inputField) && deactivateInputFieldIfInactive)
				{
					((Component)inputField).gameObject.SetActive(_showInput);
				}
				if (Object.op_Implicit((Object)(object)sendButton))
				{
					((Component)sendButton).gameObject.SetActive(_showInput);
				}
				for (int i = 0; i < gameplayHiddenGraphics.Length; i++)
				{
					((Behaviour)gameplayHiddenGraphics[i]).enabled = _showInput;
				}
				if (_showInput)
				{
					FocusInputField();
				}
				else
				{
					UnfocusInputField();
				}
			}
		}
	}

	private void UpdateFade(float deltaTime)
	{
		fadeTimer -= deltaTime;
		if (Object.op_Implicit((Object)(object)fadeGroup))
		{
			float alpha;
			if (!showInput)
			{
				alpha = ((fadeTimer < 0f) ? 0f : ((!(fadeTimer < fadeDuration)) ? 1f : Mathf.Sqrt(Util.Remap(fadeTimer, fadeDuration, 0f, 1f, 0f))));
			}
			else
			{
				alpha = 1f;
				ResetFadeTimer();
			}
			fadeGroup.alpha = alpha;
		}
	}

	private void ResetFadeTimer()
	{
		fadeTimer = fadeDuration + fadeWait;
	}

	public void SetShowInput(bool value)
	{
		showInput = value;
	}

	public void SubmitChat()
	{
		string text = inputField.text;
		if (text != "")
		{
			inputField.text = "";
			ReadOnlyCollection<NetworkUser> readOnlyLocalPlayersList = NetworkUser.readOnlyLocalPlayersList;
			if (readOnlyLocalPlayersList.Count > 0)
			{
				string text2 = text;
				text2 = text2.Replace("\\", "\\\\");
				text2 = text2.Replace("\"", "\\\"");
				Console.instance.SubmitCmd(readOnlyLocalPlayersList[0], "say \"" + text2 + "\"");
				Debug.Log((object)"Submitting say cmd.");
			}
		}
		Debug.LogFormat("SubmitChat() submittedText={0}", new object[1] { text });
		if (deselectAfterSubmitChat)
		{
			showInput = false;
		}
		else
		{
			FocusInputField();
		}
	}

	public void OnInputFieldEndEdit()
	{
	}

	private void Awake()
	{
		eventSystemLocator = ((Component)this).GetComponent<MPEventSystemLocator>();
		showInput = true;
		showInput = false;
		Chat.onChatChanged += OnChatChangedHandler;
	}

	private void OnDestroy()
	{
		Chat.onChatChanged -= OnChatChangedHandler;
	}

	private void Start()
	{
		if (showKeybindTipOnStart && !RoR2Application.isInSinglePlayer)
		{
			Chat.AddMessage(Language.GetString("CHAT_KEYBIND_TIP"));
		}
		BuildChat();
		ScrollToBottom();
		inputField.resetOnDeActivation = true;
	}

	private void OnEnable()
	{
		BuildChat();
		ScrollToBottom();
		((MonoBehaviour)this).Invoke("ScrollToBottom", 0f);
	}

	private void OnDisable()
	{
	}

	private void OnChatChangedHandler()
	{
		ResetFadeTimer();
		if (((Behaviour)this).enabled)
		{
			BuildChat();
			ScrollToBottom();
		}
	}

	public void ScrollToBottom()
	{
		messagesText.verticalScrollbar.value = 0f;
		messagesText.verticalScrollbar.value = 1f;
	}

	private void BuildChat()
	{
		ReadOnlyCollection<string> readOnlyLog = Chat.readOnlyLog;
		string[] array = new string[readOnlyLog.Count];
		readOnlyLog.CopyTo(array, 0);
		messagesText.text = string.Join("\n", array);
		RebuildChatRects();
	}

	private void Update()
	{
		UpdateFade(Time.deltaTime);
		MPEventSystem eventSystem = eventSystemLocator.eventSystem;
		GameObject val = (Object.op_Implicit((Object)(object)eventSystem) ? ((EventSystem)eventSystem).currentSelectedGameObject : null);
		bool flag = false;
		flag = Input.GetKeyDown((KeyCode)13) || Input.GetKeyDown((KeyCode)271);
		if (!showInput && flag && !((Object)(object)ConsoleWindow.instance != (Object)null))
		{
			showInput = true;
		}
		else if ((Object)(object)val == (Object)(object)((Component)inputField).gameObject)
		{
			if (flag)
			{
				if (showInput)
				{
					SubmitChat();
				}
				else if (!Object.op_Implicit((Object)(object)val))
				{
					showInput = true;
				}
			}
			if (Input.GetKeyDown((KeyCode)27))
			{
				showInput = false;
			}
		}
		else
		{
			showInput = false;
		}
	}

	private void RebuildChatRects()
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		RectTransform component = ((Component)scrollRect).GetComponent<RectTransform>();
		((Transform)component).SetParent((Transform)(object)((showInput && allowExpandedChatbox) ? expandedChatboxRect : standardChatboxRect));
		component.offsetMin = Vector2.zero;
		component.offsetMax = Vector2.zero;
		ScrollToBottom();
	}

	private void FocusInputField()
	{
		MPEventSystem eventSystem = eventSystemLocator.eventSystem;
		if (Object.op_Implicit((Object)(object)eventSystem))
		{
			((EventSystem)eventSystem).SetSelectedGameObject(((Component)inputField).gameObject);
		}
		inputField.ActivateInputField();
		inputField.text = "";
	}

	private void UnfocusInputField()
	{
		MPEventSystem eventSystem = eventSystemLocator.eventSystem;
		if (Object.op_Implicit((Object)(object)eventSystem) && (Object)(object)((EventSystem)eventSystem).currentSelectedGameObject == (Object)(object)((Component)inputField).gameObject)
		{
			((EventSystem)eventSystem).SetSelectedGameObject((GameObject)null);
		}
		inputField.DeactivateInputField(false);
	}
}
