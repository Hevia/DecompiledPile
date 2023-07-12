using System;
using System.Globalization;
using RoR2.ConVar;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace RoR2;

public static class SettingsConVars
{
	private class VSyncCountConVar : BaseConVar
	{
		private static VSyncCountConVar instance = new VSyncCountConVar("vsync_count", ConVarFlags.Archive | ConVarFlags.Engine, null, "Vsync count.");

		private VSyncCountConVar(string name, ConVarFlags flags, string defaultValue, string helpText)
			: base(name, flags, defaultValue, helpText)
		{
		}

		public override void SetString(string newValue)
		{
			if (TextSerialization.TryParseInvariant(newValue, out int result))
			{
				QualitySettings.vSyncCount = result;
			}
		}

		public override string GetString()
		{
			return TextSerialization.ToStringInvariant(QualitySettings.vSyncCount);
		}
	}

	private class WindowModeConVar : BaseConVar
	{
		private enum WindowMode
		{
			Fullscreen,
			Window,
			FullscreenExclusive
		}

		private static WindowModeConVar instance = new WindowModeConVar("window_mode", ConVarFlags.Archive | ConVarFlags.Engine, null, "The window mode. Choices are Fullscreen and Window.");

		private WindowModeConVar(string name, ConVarFlags flags, string defaultValue, string helpText)
			: base(name, flags, defaultValue, helpText)
		{
		}

		public override void SetString(string newValue)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				FullScreenMode fullScreenMode = (FullScreenMode)0;
				switch ((WindowMode)Enum.Parse(typeof(WindowMode), newValue, ignoreCase: true))
				{
				case WindowMode.Fullscreen:
					fullScreenMode = (FullScreenMode)1;
					break;
				case WindowMode.Window:
					fullScreenMode = (FullScreenMode)3;
					break;
				case WindowMode.FullscreenExclusive:
					fullScreenMode = (FullScreenMode)0;
					break;
				}
				Screen.fullScreenMode = fullScreenMode;
			}
			catch (ArgumentException)
			{
				Console.ShowHelpText(name);
			}
		}

		public override string GetString()
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Expected I4, but got Unknown
			FullScreenMode fullScreenMode = Screen.fullScreenMode;
			return ((int)fullScreenMode switch
			{
				0 => WindowMode.FullscreenExclusive, 
				1 => WindowMode.Fullscreen, 
				2 => WindowMode.Window, 
				3 => WindowMode.Window, 
				_ => WindowMode.Fullscreen, 
			}).ToString();
		}
	}

	private class ResolutionConVar : BaseConVar
	{
		private static ResolutionConVar instance = new ResolutionConVar("resolution", ConVarFlags.Archive | ConVarFlags.Engine, null, "The resolution of the game window. Format example: 1920x1080x60");

		private ResolutionConVar(string name, ConVarFlags flags, string defaultValue, string helpText)
			: base(name, flags, defaultValue, helpText)
		{
		}

		public override void SetString(string newValue)
		{
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			string[] array = newValue.Split(new char[1] { 'x' });
			if (array.Length < 1 || !TextSerialization.TryParseInvariant(array[0], out int result))
			{
				throw new ConCommandException("Invalid resolution format. No width integer. Example: \"1920x1080x60\".");
			}
			if (array.Length < 2 || !TextSerialization.TryParseInvariant(array[1], out int result2))
			{
				throw new ConCommandException("Invalid resolution format. No height integer. Example: \"1920x1080x60\".");
			}
			if (array.Length < 3 || !TextSerialization.TryParseInvariant(array[2], out int result3))
			{
				throw new ConCommandException("Invalid resolution format. No refresh rate integer. Example: \"1920x1080x60\".");
			}
			Screen.SetResolution(result, result2, Screen.fullScreenMode, result3);
		}

		public override string GetString()
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			Resolution currentResolution = Screen.currentResolution;
			return string.Format(CultureInfo.InvariantCulture, "{0}x{1}x{2}", Screen.width, Screen.height, ((Resolution)(ref currentResolution)).refreshRate);
		}

		[ConCommand(commandName = "resolution_list", flags = ConVarFlags.None, helpText = "Prints a list of all possible resolutions for the current display.")]
		private static void CCResolutionList(ConCommandArgs args)
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			Resolution[] resolutions = Screen.resolutions;
			string[] array = new string[resolutions.Length];
			for (int i = 0; i < resolutions.Length; i++)
			{
				Resolution val = resolutions[i];
				array[i] = $"{((Resolution)(ref val)).width}x{((Resolution)(ref val)).height}x{((Resolution)(ref val)).refreshRate}";
			}
			Debug.Log((object)string.Join("\n", array));
		}
	}

	private class FpsMaxConVar : BaseConVar
	{
		private static FpsMaxConVar instance = new FpsMaxConVar("fps_max", ConVarFlags.Archive, "60", "Maximum FPS. -1 is unlimited.");

		private FpsMaxConVar(string name, ConVarFlags flags, string defaultValue, string helpText)
			: base(name, flags, defaultValue, helpText)
		{
		}

		public override void SetString(string newValue)
		{
			if (TextSerialization.TryParseInvariant(newValue, out int result))
			{
				Application.targetFrameRate = result;
			}
		}

		public override string GetString()
		{
			return TextSerialization.ToStringInvariant(Application.targetFrameRate);
		}
	}

	private class ShadowsConVar : BaseConVar
	{
		private static ShadowsConVar instance = new ShadowsConVar("r_shadows", ConVarFlags.Archive | ConVarFlags.Engine, null, "Shadow quality. Can be \"All\" \"HardOnly\" or \"Disable\"");

		private ShadowsConVar(string name, ConVarFlags flags, string defaultValue, string helpText)
			: base(name, flags, defaultValue, helpText)
		{
		}

		public override void SetString(string newValue)
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				QualitySettings.shadows = (ShadowQuality)Enum.Parse(typeof(ShadowQuality), newValue, ignoreCase: true);
			}
			catch (ArgumentException)
			{
				Console.ShowHelpText(name);
			}
		}

		public override string GetString()
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			ShadowQuality shadows = QualitySettings.shadows;
			return ((object)(ShadowQuality)(ref shadows)).ToString();
		}
	}

	private class SoftParticlesConVar : BaseConVar
	{
		private static SoftParticlesConVar instance = new SoftParticlesConVar("r_softparticles", ConVarFlags.Archive | ConVarFlags.Engine, null, "Whether or not soft particles are enabled.");

		private SoftParticlesConVar(string name, ConVarFlags flags, string defaultValue, string helpText)
			: base(name, flags, defaultValue, helpText)
		{
		}

		public override void SetString(string newValue)
		{
			if (TextSerialization.TryParseInvariant(newValue, out int result))
			{
				QualitySettings.softParticles = result != 0;
			}
		}

		public override string GetString()
		{
			if (!QualitySettings.softParticles)
			{
				return "0";
			}
			return "1";
		}
	}

	private class FoliageWindConVar : BaseConVar
	{
		private static FoliageWindConVar instance = new FoliageWindConVar("r_foliagewind", ConVarFlags.Archive, "1", "Whether or not foliage has wind.");

		private FoliageWindConVar(string name, ConVarFlags flags, string defaultValue, string helpText)
			: base(name, flags, defaultValue, helpText)
		{
		}

		public override void SetString(string newValue)
		{
			if (TextSerialization.TryParseInvariant(newValue, out int result))
			{
				if (result >= 1)
				{
					Shader.EnableKeyword("ENABLE_WIND");
				}
				else
				{
					Shader.DisableKeyword("ENABLE_WIND");
				}
			}
		}

		public override string GetString()
		{
			if (!Shader.IsKeywordEnabled("ENABLE_WIND"))
			{
				return "0";
			}
			return "1";
		}
	}

	private class LodBiasConVar : BaseConVar
	{
		private static LodBiasConVar instance = new LodBiasConVar("r_lod_bias", ConVarFlags.Archive | ConVarFlags.Engine, null, "LOD bias.");

		private LodBiasConVar(string name, ConVarFlags flags, string defaultValue, string helpText)
			: base(name, flags, defaultValue, helpText)
		{
		}

		public override void SetString(string newValue)
		{
			if (TextSerialization.TryParseInvariant(newValue, out float result))
			{
				QualitySettings.lodBias = result;
			}
		}

		public override string GetString()
		{
			return TextSerialization.ToStringInvariant(QualitySettings.lodBias);
		}
	}

	private class MaximumLodConVar : BaseConVar
	{
		private static MaximumLodConVar instance = new MaximumLodConVar("r_lod_max", ConVarFlags.Archive | ConVarFlags.Engine, null, "The maximum allowed LOD level.");

		private MaximumLodConVar(string name, ConVarFlags flags, string defaultValue, string helpText)
			: base(name, flags, defaultValue, helpText)
		{
		}

		public override void SetString(string newValue)
		{
			if (TextSerialization.TryParseInvariant(newValue, out int result))
			{
				QualitySettings.maximumLODLevel = result;
			}
		}

		public override string GetString()
		{
			return TextSerialization.ToStringInvariant(QualitySettings.maximumLODLevel);
		}
	}

	private class MasterTextureLimitConVar : BaseConVar
	{
		private static MasterTextureLimitConVar instance = new MasterTextureLimitConVar("master_texture_limit", ConVarFlags.Archive | ConVarFlags.Engine, null, "Reduction in texture quality. 0 is highest quality textures, 1 is half, 2 is quarter, etc.");

		private MasterTextureLimitConVar(string name, ConVarFlags flags, string defaultValue, string helpText)
			: base(name, flags, defaultValue, helpText)
		{
		}

		public override void SetString(string newValue)
		{
			if (TextSerialization.TryParseInvariant(newValue, out int result))
			{
				QualitySettings.masterTextureLimit = result;
			}
		}

		public override string GetString()
		{
			return TextSerialization.ToStringInvariant(QualitySettings.masterTextureLimit);
		}
	}

	private class AnisotropicFilteringConVar : BaseConVar
	{
		private static AnisotropicFilteringConVar instance = new AnisotropicFilteringConVar("anisotropic_filtering", ConVarFlags.Archive, "Disable", "The anisotropic filtering mode. Can be \"Disable\", \"Enable\" or \"ForceEnable\".");

		private AnisotropicFilteringConVar(string name, ConVarFlags flags, string defaultValue, string helpText)
			: base(name, flags, defaultValue, helpText)
		{
		}

		public override void SetString(string newValue)
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				QualitySettings.anisotropicFiltering = (AnisotropicFiltering)Enum.Parse(typeof(AnisotropicFiltering), newValue, ignoreCase: true);
			}
			catch (ArgumentException)
			{
				Console.ShowHelpText(name);
			}
		}

		public override string GetString()
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			AnisotropicFiltering anisotropicFiltering = QualitySettings.anisotropicFiltering;
			return ((object)(AnisotropicFiltering)(ref anisotropicFiltering)).ToString();
		}
	}

	private class ShadowResolutionConVar : BaseConVar
	{
		private static ShadowResolutionConVar instance = new ShadowResolutionConVar("shadow_resolution", ConVarFlags.Archive | ConVarFlags.Engine, "Medium", "Default shadow resolution. Can be \"Low\", \"Medium\", \"High\" or \"VeryHigh\".");

		private ShadowResolutionConVar(string name, ConVarFlags flags, string defaultValue, string helpText)
			: base(name, flags, defaultValue, helpText)
		{
		}

		public override void SetString(string newValue)
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				QualitySettings.shadowResolution = (ShadowResolution)Enum.Parse(typeof(ShadowResolution), newValue, ignoreCase: true);
			}
			catch (ArgumentException)
			{
				Console.ShowHelpText(name);
			}
		}

		public override string GetString()
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			ShadowResolution shadowResolution = QualitySettings.shadowResolution;
			return ((object)(ShadowResolution)(ref shadowResolution)).ToString();
		}
	}

	private class ShadowCascadesConVar : BaseConVar
	{
		private static ShadowCascadesConVar instance = new ShadowCascadesConVar("shadow_cascades", ConVarFlags.Archive | ConVarFlags.Engine, null, "The number of cascades to use for directional light shadows. low=0 high=4");

		private ShadowCascadesConVar(string name, ConVarFlags flags, string defaultValue, string helpText)
			: base(name, flags, defaultValue, helpText)
		{
		}

		public override void SetString(string newValue)
		{
			if (TextSerialization.TryParseInvariant(newValue, out int result))
			{
				QualitySettings.shadowCascades = result;
			}
		}

		public override string GetString()
		{
			return TextSerialization.ToStringInvariant(QualitySettings.shadowCascades);
		}
	}

	private class ShadowDistanceConvar : BaseConVar
	{
		private static ShadowDistanceConvar instance = new ShadowDistanceConvar("shadow_distance", ConVarFlags.Archive, "200", "The distance in meters to draw shadows.");

		private ShadowDistanceConvar(string name, ConVarFlags flags, string defaultValue, string helpText)
			: base(name, flags, defaultValue, helpText)
		{
		}

		public override void SetString(string newValue)
		{
			if (TextSerialization.TryParseInvariant(newValue, out float result))
			{
				QualitySettings.shadowDistance = result;
			}
		}

		public override string GetString()
		{
			return TextSerialization.ToStringInvariant(QualitySettings.shadowDistance);
		}
	}

	private class MSAAConvar : BaseConVar
	{
		private static MSAAConvar instance = new MSAAConvar("r_ui_msaa", ConVarFlags.Archive, "0", "Whether or not MSAA for the UI is enabled.");

		private MSAAConvar(string name, ConVarFlags flags, string defaultValue, string helpText)
			: base(name, flags, defaultValue, helpText)
		{
		}

		public override void SetString(string newValue)
		{
			if (TextSerialization.TryParseInvariant(newValue, out int result))
			{
				QualitySettings.antiAliasing = result;
			}
		}

		public override string GetString()
		{
			return QualitySettings.antiAliasing.ToString();
		}
	}

	private class PpMotionBlurConVar : BaseConVar
	{
		private static MotionBlur settings;

		private static PpMotionBlurConVar instance = new PpMotionBlurConVar("pp_motionblur", ConVarFlags.Archive, "0", "Motion blur. 0 = disabled 1 = enabled");

		private PpMotionBlurConVar(string name, ConVarFlags flags, string defaultValue, string helpText)
			: base(name, flags, defaultValue, helpText)
		{
			RoR2Application.instance.postProcessSettingsController.sharedProfile.TryGetSettings<MotionBlur>(ref settings);
		}

		public override void SetString(string newValue)
		{
			if (TextSerialization.TryParseInvariant(newValue, out int result) && Object.op_Implicit((Object)(object)settings))
			{
				((PostProcessEffectSettings)settings).active = result == 0;
			}
		}

		public override string GetString()
		{
			if (!Object.op_Implicit((Object)(object)settings))
			{
				return "1";
			}
			if (!((PostProcessEffectSettings)settings).active)
			{
				return "1";
			}
			return "0";
		}
	}

	private class PpSobelOutlineConVar : BaseConVar
	{
		private static SobelOutline sobelOutlineSettings;

		private static PpSobelOutlineConVar instance = new PpSobelOutlineConVar("pp_sobel_outline", ConVarFlags.Archive, "1", "Whether or not to use the sobel rim light effect.");

		private PpSobelOutlineConVar(string name, ConVarFlags flags, string defaultValue, string helpText)
			: base(name, flags, defaultValue, helpText)
		{
			RoR2Application.instance.postProcessSettingsController.sharedProfile.TryGetSettings<SobelOutline>(ref sobelOutlineSettings);
		}

		public override void SetString(string newValue)
		{
			if (TextSerialization.TryParseInvariant(newValue, out int result) && Object.op_Implicit((Object)(object)sobelOutlineSettings))
			{
				((PostProcessEffectSettings)sobelOutlineSettings).active = result == 0;
			}
		}

		public override string GetString()
		{
			if (!Object.op_Implicit((Object)(object)sobelOutlineSettings))
			{
				return "1";
			}
			if (!((PostProcessEffectSettings)sobelOutlineSettings).active)
			{
				return "1";
			}
			return "0";
		}
	}

	private class PpBloomConVar : BaseConVar
	{
		private static Bloom settings;

		private static PpBloomConVar instance = new PpBloomConVar("pp_bloom", ConVarFlags.Archive | ConVarFlags.Engine, null, "Bloom postprocessing. 0 = disabled 1 = enabled");

		private PpBloomConVar(string name, ConVarFlags flags, string defaultValue, string helpText)
			: base(name, flags, defaultValue, helpText)
		{
			RoR2Application.instance.postProcessSettingsController.sharedProfile.TryGetSettings<Bloom>(ref settings);
		}

		public override void SetString(string newValue)
		{
			if (TextSerialization.TryParseInvariant(newValue, out int result) && Object.op_Implicit((Object)(object)settings))
			{
				((PostProcessEffectSettings)settings).active = result == 0;
			}
		}

		public override string GetString()
		{
			if (!Object.op_Implicit((Object)(object)settings))
			{
				return "1";
			}
			if (!((PostProcessEffectSettings)settings).active)
			{
				return "1";
			}
			return "0";
		}
	}

	private class PpAOConVar : BaseConVar
	{
		private static AmbientOcclusion settings;

		private static PpAOConVar instance = new PpAOConVar("pp_ao", ConVarFlags.Archive | ConVarFlags.Engine, null, "SSAO postprocessing. 0 = disabled 1 = enabled");

		private PpAOConVar(string name, ConVarFlags flags, string defaultValue, string helpText)
			: base(name, flags, defaultValue, helpText)
		{
			RoR2Application.instance.postProcessSettingsController.sharedProfile.TryGetSettings<AmbientOcclusion>(ref settings);
		}

		public override void SetString(string newValue)
		{
			if (TextSerialization.TryParseInvariant(newValue, out int result) && Object.op_Implicit((Object)(object)settings))
			{
				((PostProcessEffectSettings)settings).active = result == 0;
			}
		}

		public override string GetString()
		{
			if (!Object.op_Implicit((Object)(object)settings))
			{
				return "1";
			}
			if (!((PostProcessEffectSettings)settings).active)
			{
				return "1";
			}
			return "0";
		}
	}

	private class PpScreenDistortionConVar : BaseConVar
	{
		private static LensDistortion settings;

		private static PpScreenDistortionConVar instance = new PpScreenDistortionConVar("pp_screendistortion", ConVarFlags.Archive | ConVarFlags.Engine, null, "Screen distortion, like from Spinel Tonic. 0 = disabled 1 = enabled");

		private PpScreenDistortionConVar(string name, ConVarFlags flags, string defaultValue, string helpText)
			: base(name, flags, defaultValue, helpText)
		{
			RoR2Application.instance.postProcessSettingsController.sharedProfile.TryGetSettings<LensDistortion>(ref settings);
		}

		public override void SetString(string newValue)
		{
			if (TextSerialization.TryParseInvariant(newValue, out int result) && Object.op_Implicit((Object)(object)settings))
			{
				((PostProcessEffectSettings)settings).active = result == 0;
			}
		}

		public override string GetString()
		{
			if (!Object.op_Implicit((Object)(object)settings))
			{
				return "1";
			}
			if (!((PostProcessEffectSettings)settings).active)
			{
				return "1";
			}
			return "0";
		}
	}

	private class PpGammaConVar : BaseConVar
	{
		private static ColorGrading colorGradingSettings;

		private static PpGammaConVar instance = new PpGammaConVar("gamma", ConVarFlags.Archive, "0", "Gamma boost, from -inf to inf.");

		private PpGammaConVar(string name, ConVarFlags flags, string defaultValue, string helpText)
			: base(name, flags, defaultValue, helpText)
		{
			RoR2Application.instance.postProcessSettingsController.sharedProfile.TryGetSettings<ColorGrading>(ref colorGradingSettings);
		}

		public override void SetString(string newValue)
		{
			if (TextSerialization.TryParseInvariant(newValue, out float result) && Object.op_Implicit((Object)(object)colorGradingSettings))
			{
				((ParameterOverride<Vector4>)(object)colorGradingSettings.gamma).value.w = result;
			}
		}

		public override string GetString()
		{
			if (!Object.op_Implicit((Object)(object)colorGradingSettings))
			{
				return "0";
			}
			return TextSerialization.ToStringInvariant(((ParameterOverride<Vector4>)(object)colorGradingSettings.gamma).value.w);
		}
	}

	public static readonly BoolConVar cvExpAndMoneyEffects = new BoolConVar("exp_and_money_effects", ConVarFlags.Archive, "1", "Whether or not to create effects for experience and money from defeating monsters.");

	public static BoolConVar enableDamageNumbers = new BoolConVar("enable_damage_numbers", ConVarFlags.Archive, "1", "Whether or not damage and healing numbers spawn.");
}
