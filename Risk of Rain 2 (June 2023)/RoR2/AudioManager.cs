using System;
using System.Reflection;
using RoR2.ConVar;
using UnityEngine;

namespace RoR2;

[RequireComponent(typeof(AkGameObj))]
public class AudioManager : MonoBehaviour
{
	private class VolumeConVar : BaseConVar
	{
		private readonly string rtpcName;

		private string fallbackString;

		public VolumeConVar(string name, ConVarFlags flags, string defaultValue, string helpText, string rtpcName)
			: base(name, flags, defaultValue, helpText)
		{
			this.rtpcName = rtpcName;
		}

		public override void SetString(string newValue)
		{
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			if (AkSoundEngine.IsInitialized())
			{
				fallbackString = newValue;
				if (TextSerialization.TryParseInvariant(newValue, out float result))
				{
					AkSoundEngine.SetRTPCValue(rtpcName, Mathf.Clamp(result, 0f, 100f));
				}
			}
		}

		public override string GetString()
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Invalid comparison between Unknown and I4
			int num = 1;
			float value = default(float);
			if ((int)AkSoundEngine.GetRTPCValue(rtpcName, (GameObject)null, 0u, ref value, ref num) == 1)
			{
				return TextSerialization.ToStringInvariant(value);
			}
			return fallbackString;
		}
	}

	private class AudioFocusedOnlyConVar : BaseConVar
	{
		private class ApplicationFocusListener : MonoBehaviour
		{
			public Action<bool> onApplicationFocus;

			private void OnApplicationFocus(bool focus)
			{
				onApplicationFocus?.Invoke(focus);
			}
		}

		private static AudioFocusedOnlyConVar instance = new AudioFocusedOnlyConVar("audio_focused_only", ConVarFlags.Archive | ConVarFlags.Engine, null, "Whether or not audio should mute when focus is lost.");

		private bool onlyPlayWhenFocused;

		private bool isFocused;

		private static MethodInfo akSoundEngineController_ActivateAudio_methodInfo = typeof(AkSoundEngineController).GetMethod("ActivateAudio", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

		public AudioFocusedOnlyConVar(string name, ConVarFlags flags, string defaultValue, string helpText)
			: base(name, flags, defaultValue, helpText)
		{
			isFocused = Application.isFocused;
			RoR2Application.onUpdate += SearchForAkInitializer;
		}

		public override void SetString(string newValue)
		{
			if (TextSerialization.TryParseInvariant(newValue, out int result))
			{
				onlyPlayWhenFocused = result != 0;
				Refresh();
			}
		}

		public override string GetString()
		{
			if (!onlyPlayWhenFocused)
			{
				return "0";
			}
			return "1";
		}

		private void OnApplicationFocus(bool focus)
		{
			isFocused = focus;
			Refresh();
		}

		private void Refresh()
		{
			bool flag = !isFocused && onlyPlayWhenFocused;
			bool flag2 = false;
			AkSoundEngineController val = AkSoundEngineController.Instance;
			if (val != null)
			{
				AkSoundEngineController_ActivateAudio(val, !flag, !flag2);
			}
		}

		private static void AkSoundEngineController_ActivateAudio(AkSoundEngineController akSoundEngineController, bool activate, bool renderAnyway)
		{
			akSoundEngineController_ActivateAudio_methodInfo.Invoke(akSoundEngineController, new object[2] { activate, renderAnyway });
		}

		private void SearchForAkInitializer()
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Expected O, but got Unknown
			AkInitializer val = (AkInitializer)akInitializerMsInstanceField.GetValue(null);
			if (Object.op_Implicit((Object)(object)val))
			{
				RoR2Application.onUpdate -= SearchForAkInitializer;
				ApplicationFocusListener applicationFocusListener = ((Component)val).gameObject.AddComponent<ApplicationFocusListener>();
				applicationFocusListener.onApplicationFocus = (Action<bool>)Delegate.Combine(applicationFocusListener.onApplicationFocus, new Action<bool>(OnApplicationFocus));
				Refresh();
			}
		}
	}

	private class WwiseLogEnabledConVar : BaseConVar
	{
		private static WwiseLogEnabledConVar instance = new WwiseLogEnabledConVar("wwise_log_enabled", ConVarFlags.Archive | ConVarFlags.Engine, null, "Wwise logging. 0 = disabled 1 = enabled");

		private WwiseLogEnabledConVar(string name, ConVarFlags flags, string defaultValue, string helpText)
			: base(name, flags, defaultValue, helpText)
		{
		}

		public override void SetString(string newValue)
		{
			if (!TextSerialization.TryParseInvariant(newValue, out int result))
			{
				return;
			}
			object? value = akInitializerMsInstanceField.GetValue(null);
			AkInitializer val = (AkInitializer)((value is AkInitializer) ? value : null);
			if (Object.op_Implicit((Object)(object)val))
			{
				AkWwiseInitializationSettings initializationSettings = val.InitializationSettings;
				InitializationSettings val2 = ((initializationSettings != null) ? ((AkBasePlatformSettings)initializationSettings).CallbackManagerInitializationSettings : null);
				if (val2 != null)
				{
					val2.IsLoggingEnabled = result != 0;
				}
				else
				{
					Debug.Log((object)"Cannot set value. callbackManagerInitializationSettings is null.");
				}
			}
		}

		public override string GetString()
		{
			object? value = akInitializerMsInstanceField.GetValue(null);
			AkInitializer val = (AkInitializer)((value is AkInitializer) ? value : null);
			if (Object.op_Implicit((Object)(object)val))
			{
				AkWwiseInitializationSettings initializationSettings = val.InitializationSettings;
				if (((initializationSettings != null) ? ((AkBasePlatformSettings)initializationSettings).CallbackManagerInitializationSettings : null) != null)
				{
					if (!((AkBasePlatformSettings)val.InitializationSettings).CallbackManagerInitializationSettings.IsLoggingEnabled)
					{
						return "0";
					}
					return "1";
				}
				Debug.Log((object)"Cannot retrieve value. callbackManagerInitializationSettings is null.");
			}
			return "1";
		}
	}

	private AkGameObj akGameObj;

	private static VolumeConVar cvVolumeMaster;

	private static VolumeConVar cvVolumeSfx;

	private static VolumeConVar cvVolumeMsx;

	private static readonly FieldInfo akInitializerMsInstanceField;

	public static AudioManager instance { get; private set; }

	public static event Action<AudioManager> onAwakeGlobal;

	private void Awake()
	{
		instance = this;
		akGameObj = ((Component)this).GetComponent<AkGameObj>();
		AudioManager.onAwakeGlobal?.Invoke(this);
	}

	static AudioManager()
	{
		cvVolumeMaster = new VolumeConVar("volume_master", ConVarFlags.Archive | ConVarFlags.Engine, "100", "The master volume of the game audio, from 0 to 100.", "Volume_Master");
		cvVolumeSfx = new VolumeConVar("volume_sfx", ConVarFlags.Archive | ConVarFlags.Engine, "100", "The volume of sound effects, from 0 to 100.", "Volume_SFX");
		cvVolumeMsx = new VolumeConVar("volume_music", ConVarFlags.Archive | ConVarFlags.Engine, "100", "The music volume, from 0 to 100.", "Volume_MSX");
		akInitializerMsInstanceField = typeof(AkInitializer).GetField("ms_Instance", BindingFlags.Static | BindingFlags.NonPublic);
		PauseManager.onPauseStartGlobal = (Action)Delegate.Combine(PauseManager.onPauseStartGlobal, (Action)delegate
		{
			AkSoundEngine.PostEvent("Pause_All", (GameObject)null);
		});
		PauseManager.onPauseEndGlobal = (Action)Delegate.Combine(PauseManager.onPauseEndGlobal, (Action)delegate
		{
			AkSoundEngine.PostEvent("Unpause_All", (GameObject)null);
		});
	}
}
