using BepInEx;
using BepInEx.Configuration;

namespace VileMod.Modules;

public static class Config
{
	public static void ReadConfig()
	{
	}

	public static ConfigEntry<bool> CharacterEnableConfig(string characterName, string description = "Set to false to disable this character", bool enabledDefault = true)
	{
		return ((BaseUnityPlugin)VilePlugin.instance).Config.Bind<bool>("General", "Enable " + characterName, enabledDefault, description);
	}
}
