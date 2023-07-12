using System;
using System.Runtime.CompilerServices;
using RoR2.ConVar;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace RoR2.UI;

[RequireComponent(typeof(MPEventSystemLocator))]
public class BaseSettingsControl : MonoBehaviour
{
	public enum SettingSource
	{
		ConVar,
		UserProfilePref
	}

	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static UnityAction _003C_003E9__18_2;

		internal void _003CSubmitSettingTemporary_003Eb__18_2()
		{
		}
	}

	public SettingSource settingSource;

	[FormerlySerializedAs("convarName")]
	public string settingName;

	public string nameToken;

	public LanguageTextMeshController nameLabel;

	[Tooltip("Whether or not this setting requires a confirmation dialog. This is mainly for video options.")]
	public bool useConfirmationDialog;

	[Tooltip("Whether or not this updates every frame. Should be disabled unless the setting is being modified from some other source.")]
	public bool updateControlsInUpdate;

	private MPEventSystemLocator eventSystemLocator;

	private string originalValue;

	public bool hasBeenChanged => originalValue != null;

	protected bool inUpdateControls { get; private set; }

	protected void Awake()
	{
		eventSystemLocator = ((Component)this).GetComponent<MPEventSystemLocator>();
		if (Object.op_Implicit((Object)(object)nameLabel) && !string.IsNullOrEmpty(nameToken))
		{
			nameLabel.token = nameToken;
		}
		if (settingSource == SettingSource.ConVar && GetConVar() == null)
		{
			Debug.LogErrorFormat("Null convar {0} detected in options", new object[1] { settingName });
		}
	}

	protected void Start()
	{
		Initialize();
		UpdateControls();
	}

	protected void OnEnable()
	{
		UpdateControls();
	}

	protected virtual void Update()
	{
		if (updateControlsInUpdate)
		{
			UpdateControls();
		}
	}

	public virtual void Initialize()
	{
	}

	public void SubmitSetting(string newValue)
	{
		if (useConfirmationDialog)
		{
			SubmitSettingTemporary(newValue);
		}
		else
		{
			SubmitSettingInternal(newValue);
		}
	}

	private void SubmitSettingInternal(string newValue)
	{
		if (originalValue == null)
		{
			originalValue = GetCurrentValue();
		}
		if (originalValue == newValue)
		{
			originalValue = null;
		}
		switch (settingSource)
		{
		case SettingSource.ConVar:
			GetConVar()?.AttemptSetString(newValue);
			break;
		case SettingSource.UserProfilePref:
			GetCurrentUserProfile()?.SetSaveFieldString(settingName, newValue);
			GetCurrentUserProfile()?.RequestEventualSave();
			break;
		}
		RoR2Application.onNextUpdate += UpdateControls;
	}

	private void SubmitSettingTemporary(string newValue)
	{
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Expected O, but got Unknown
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Expected O, but got Unknown
		string oldValue = GetCurrentValue();
		if (newValue == oldValue)
		{
			return;
		}
		SubmitSettingInternal(newValue);
		SimpleDialogBox dialogBox = SimpleDialogBox.Create();
		Action revertFunction = delegate
		{
			if (Object.op_Implicit((Object)(object)dialogBox))
			{
				SubmitSettingInternal(oldValue);
			}
		};
		float num = 10f;
		float timeEnd = Time.unscaledTime + num;
		MPButton revertButton = dialogBox.AddActionButton((UnityAction)delegate
		{
			revertFunction();
		}, "OPTION_REVERT", true);
		SimpleDialogBox simpleDialogBox = dialogBox;
		object obj = _003C_003Ec._003C_003E9__18_2;
		if (obj == null)
		{
			UnityAction val = delegate
			{
			};
			obj = (object)val;
			_003C_003Ec._003C_003E9__18_2 = val;
		}
		simpleDialogBox.AddActionButton((UnityAction)obj, "OPTION_ACCEPT", true);
		Action updateText = null;
		updateText = delegate
		{
			if (Object.op_Implicit((Object)(object)dialogBox))
			{
				int num2 = Mathf.FloorToInt(timeEnd - Time.unscaledTime);
				if (num2 < 0)
				{
					num2 = 0;
				}
				dialogBox.descriptionToken = new SimpleDialogBox.TokenParamsPair
				{
					token = "OPTION_AUTOREVERT_DIALOG_DESCRIPTION",
					formatParams = new object[1] { num2 }
				};
				if (num2 > 0)
				{
					RoR2Application.unscaledTimeTimers.CreateTimer(1f, updateText);
				}
			}
		};
		updateText();
		dialogBox.headerToken = new SimpleDialogBox.TokenParamsPair
		{
			token = "OPTION_AUTOREVERT_DIALOG_TITLE"
		};
		RoR2Application.unscaledTimeTimers.CreateTimer(num, delegate
		{
			if (Object.op_Implicit((Object)(object)revertButton))
			{
				((UnityEvent)((Button)revertButton).onClick).Invoke();
			}
		});
	}

	public string GetCurrentValue()
	{
		return settingSource switch
		{
			SettingSource.ConVar => GetConVar()?.GetString(), 
			SettingSource.UserProfilePref => GetCurrentUserProfile()?.GetSaveFieldString(settingName) ?? "", 
			_ => "", 
		};
	}

	protected BaseConVar GetConVar()
	{
		return Console.instance.FindConVar(settingName);
	}

	public UserProfile GetCurrentUserProfile()
	{
		return eventSystemLocator.eventSystem?.localUser?.userProfile;
	}

	public void Revert()
	{
		if (hasBeenChanged)
		{
			SubmitSetting(originalValue);
			originalValue = null;
		}
	}

	protected void UpdateControls()
	{
		if (Object.op_Implicit((Object)(object)this) && !inUpdateControls)
		{
			inUpdateControls = true;
			OnUpdateControls();
			inUpdateControls = false;
		}
	}

	protected virtual void OnUpdateControls()
	{
	}
}
