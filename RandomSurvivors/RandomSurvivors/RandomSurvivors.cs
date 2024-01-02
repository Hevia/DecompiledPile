using System;
using System.Runtime.CompilerServices;
using BepInEx;
using BepInEx.Configuration;
using EntityStates;
using EntityStates.Bandit2;
using EntityStates.Bandit2.Weapon;
using EntityStates.BeetleQueenMonster;
using EntityStates.Bison;
using EntityStates.Captain.Weapon;
using EntityStates.Commando;
using EntityStates.Commando.CommandoWeapon;
using EntityStates.Croco;
using EntityStates.Drone.DroneWeapon;
using EntityStates.Engi.EngiWeapon;
using EntityStates.Huntress;
using EntityStates.Huntress.Weapon;
using EntityStates.ImpMonster;
using EntityStates.LemurianBruiserMonster;
using EntityStates.Loader;
using EntityStates.Mage.Weapon;
using EntityStates.Merc;
using EntityStates.Merc.Weapon;
using EntityStates.MiniMushroom;
using EntityStates.NullifierMonster;
using EntityStates.Paladin;
using EntityStates.ParentMonster;
using EntityStates.RoboBallBoss.Weapon;
using EntityStates.ScavMonster;
using EntityStates.TitanMonster;
using EntityStates.Toolbot;
using EntityStates.Treebot.Weapon;
using KinematicCharacterController;
using On.RoR2;
using R2API;
using R2API.Utils;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.UI;

namespace RandomSurvivors;

[R2APISubmoduleDependency(new string[] { "SurvivorAPI" })]
[R2APISubmoduleDependency(new string[] { "LoadoutAPI" })]
[R2APISubmoduleDependency(new string[] { "PrefabAPI" })]
[BepInDependency(/*Could not decode attribute arguments.*/)]
[BepInPlugin("com.Fubuki.RandomSurvivors", "Random Survivors", "0.4.1")]
[NetworkCompatibility(/*Could not decode attribute arguments.*/)]
public class RandomSurvivors : BaseUnityPlugin
{
	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static hook_Build _003C_003E9__17_0;

		internal void _003CAwake_003Eb__17_0(orig_Build orig, CharacterSelectBarController self)
		{
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			if (configCharSelect.Value)
			{
				self.gridLayoutGroup.cellSize = new Vector2(40f, 40f);
				self.gridLayoutGroup.spacing = new Vector2(-5f, -5f);
				self.gridLayoutGroup.constraintCount = 38;
				RectOffset padding = ((LayoutGroup)self.gridLayoutGroup).padding;
				padding.right += -850;
			}
			orig.Invoke(self);
		}
	}

	private int randomSeed;

	public static ConfigEntry<int> configSeed { get; set; }

	public static ConfigEntry<int> configBodies { get; set; }

	public static ConfigEntry<bool> configFliers { get; set; }

	public static ConfigEntry<bool> configCharSelect { get; set; }

	public void Awake()
	{
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Expected O, but got Unknown
		//IL_38fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_3908: Expected O, but got Unknown
		//IL_0bac: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bb1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0cba: Unknown result type (might be due to invalid IL or missing references)
		//IL_0cc4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ce5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d0b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d8d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d92: Unknown result type (might be due to invalid IL or missing references)
		//IL_0def: Unknown result type (might be due to invalid IL or missing references)
		//IL_0df9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e1a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e40: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f13: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f1d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f3e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f64: Unknown result type (might be due to invalid IL or missing references)
		//IL_1048: Unknown result type (might be due to invalid IL or missing references)
		//IL_1052: Unknown result type (might be due to invalid IL or missing references)
		//IL_1073: Unknown result type (might be due to invalid IL or missing references)
		//IL_1099: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ec2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ec7: Unknown result type (might be due to invalid IL or missing references)
		//IL_116c: Unknown result type (might be due to invalid IL or missing references)
		//IL_1176: Unknown result type (might be due to invalid IL or missing references)
		//IL_1197: Unknown result type (might be due to invalid IL or missing references)
		//IL_11bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ff7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ffc: Unknown result type (might be due to invalid IL or missing references)
		//IL_1290: Unknown result type (might be due to invalid IL or missing references)
		//IL_129a: Unknown result type (might be due to invalid IL or missing references)
		//IL_12bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_12e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_111b: Unknown result type (might be due to invalid IL or missing references)
		//IL_1120: Unknown result type (might be due to invalid IL or missing references)
		//IL_13b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_13be: Unknown result type (might be due to invalid IL or missing references)
		//IL_13df: Unknown result type (might be due to invalid IL or missing references)
		//IL_1405: Unknown result type (might be due to invalid IL or missing references)
		//IL_123f: Unknown result type (might be due to invalid IL or missing references)
		//IL_1244: Unknown result type (might be due to invalid IL or missing references)
		//IL_1363: Unknown result type (might be due to invalid IL or missing references)
		//IL_1368: Unknown result type (might be due to invalid IL or missing references)
		//IL_1487: Unknown result type (might be due to invalid IL or missing references)
		//IL_148c: Unknown result type (might be due to invalid IL or missing references)
		//IL_14e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_14f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_1514: Unknown result type (might be due to invalid IL or missing references)
		//IL_153a: Unknown result type (might be due to invalid IL or missing references)
		//IL_161e: Unknown result type (might be due to invalid IL or missing references)
		//IL_1628: Unknown result type (might be due to invalid IL or missing references)
		//IL_1649: Unknown result type (might be due to invalid IL or missing references)
		//IL_166f: Unknown result type (might be due to invalid IL or missing references)
		//IL_1742: Unknown result type (might be due to invalid IL or missing references)
		//IL_174c: Unknown result type (might be due to invalid IL or missing references)
		//IL_176d: Unknown result type (might be due to invalid IL or missing references)
		//IL_1793: Unknown result type (might be due to invalid IL or missing references)
		//IL_15cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_15d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_1866: Unknown result type (might be due to invalid IL or missing references)
		//IL_1870: Unknown result type (might be due to invalid IL or missing references)
		//IL_1891: Unknown result type (might be due to invalid IL or missing references)
		//IL_18b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_16f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_16f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_198a: Unknown result type (might be due to invalid IL or missing references)
		//IL_1994: Unknown result type (might be due to invalid IL or missing references)
		//IL_19b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_19db: Unknown result type (might be due to invalid IL or missing references)
		//IL_1815: Unknown result type (might be due to invalid IL or missing references)
		//IL_181a: Unknown result type (might be due to invalid IL or missing references)
		//IL_1abb: Unknown result type (might be due to invalid IL or missing references)
		//IL_1ac5: Unknown result type (might be due to invalid IL or missing references)
		//IL_1939: Unknown result type (might be due to invalid IL or missing references)
		//IL_193e: Unknown result type (might be due to invalid IL or missing references)
		//IL_1b9d: Unknown result type (might be due to invalid IL or missing references)
		//IL_1ba7: Unknown result type (might be due to invalid IL or missing references)
		//IL_1a6a: Unknown result type (might be due to invalid IL or missing references)
		//IL_1a6f: Unknown result type (might be due to invalid IL or missing references)
		//IL_1b47: Unknown result type (might be due to invalid IL or missing references)
		//IL_1b4c: Unknown result type (might be due to invalid IL or missing references)
		//IL_1d84: Unknown result type (might be due to invalid IL or missing references)
		//IL_1d89: Unknown result type (might be due to invalid IL or missing references)
		//IL_1d8e: Unknown result type (might be due to invalid IL or missing references)
		//IL_1da5: Unknown result type (might be due to invalid IL or missing references)
		//IL_1daa: Unknown result type (might be due to invalid IL or missing references)
		//IL_1daf: Unknown result type (might be due to invalid IL or missing references)
		//IL_1dbb: Unknown result type (might be due to invalid IL or missing references)
		//IL_1dc0: Unknown result type (might be due to invalid IL or missing references)
		//IL_1df5: Unknown result type (might be due to invalid IL or missing references)
		//IL_1df7: Unknown result type (might be due to invalid IL or missing references)
		//IL_1dfc: Unknown result type (might be due to invalid IL or missing references)
		//IL_1e0b: Unknown result type (might be due to invalid IL or missing references)
		//IL_1e10: Unknown result type (might be due to invalid IL or missing references)
		//IL_1e15: Unknown result type (might be due to invalid IL or missing references)
		//IL_1e2c: Unknown result type (might be due to invalid IL or missing references)
		//IL_1e31: Unknown result type (might be due to invalid IL or missing references)
		//IL_1e36: Unknown result type (might be due to invalid IL or missing references)
		//IL_1e4d: Unknown result type (might be due to invalid IL or missing references)
		//IL_1e52: Unknown result type (might be due to invalid IL or missing references)
		//IL_1e57: Unknown result type (might be due to invalid IL or missing references)
		//IL_1e6e: Unknown result type (might be due to invalid IL or missing references)
		//IL_1e73: Unknown result type (might be due to invalid IL or missing references)
		//IL_1e78: Unknown result type (might be due to invalid IL or missing references)
		//IL_1e8a: Unknown result type (might be due to invalid IL or missing references)
		//IL_1e91: Expected O, but got Unknown
		//IL_1ed4: Unknown result type (might be due to invalid IL or missing references)
		//IL_1ed9: Unknown result type (might be due to invalid IL or missing references)
		//IL_2273: Unknown result type (might be due to invalid IL or missing references)
		//IL_2294: Unknown result type (might be due to invalid IL or missing references)
		//IL_22ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_22e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_2306: Unknown result type (might be due to invalid IL or missing references)
		//IL_232c: Unknown result type (might be due to invalid IL or missing references)
		//IL_2352: Unknown result type (might be due to invalid IL or missing references)
		//IL_2378: Unknown result type (might be due to invalid IL or missing references)
		//IL_23a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_23f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_243c: Unknown result type (might be due to invalid IL or missing references)
		//IL_2441: Unknown result type (might be due to invalid IL or missing references)
		//IL_247a: Unknown result type (might be due to invalid IL or missing references)
		//IL_247f: Unknown result type (might be due to invalid IL or missing references)
		//IL_2483: Unknown result type (might be due to invalid IL or missing references)
		//IL_249d: Unknown result type (might be due to invalid IL or missing references)
		//IL_24b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_24d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_24d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_24d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_24f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_24f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_24fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_2502: Unknown result type (might be due to invalid IL or missing references)
		//IL_2504: Unknown result type (might be due to invalid IL or missing references)
		//IL_2509: Unknown result type (might be due to invalid IL or missing references)
		//IL_2518: Unknown result type (might be due to invalid IL or missing references)
		//IL_251d: Unknown result type (might be due to invalid IL or missing references)
		//IL_2522: Unknown result type (might be due to invalid IL or missing references)
		//IL_2539: Unknown result type (might be due to invalid IL or missing references)
		//IL_253e: Unknown result type (might be due to invalid IL or missing references)
		//IL_2543: Unknown result type (might be due to invalid IL or missing references)
		//IL_255a: Unknown result type (might be due to invalid IL or missing references)
		//IL_255f: Unknown result type (might be due to invalid IL or missing references)
		//IL_2564: Unknown result type (might be due to invalid IL or missing references)
		//IL_257b: Unknown result type (might be due to invalid IL or missing references)
		//IL_2580: Unknown result type (might be due to invalid IL or missing references)
		//IL_2585: Unknown result type (might be due to invalid IL or missing references)
		//IL_25be: Unknown result type (might be due to invalid IL or missing references)
		//IL_25c5: Expected O, but got Unknown
		//IL_25cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_25d2: Expected O, but got Unknown
		//IL_25d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_25df: Expected O, but got Unknown
		//IL_25e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_25ec: Expected O, but got Unknown
		//IL_25f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_25f9: Expected O, but got Unknown
		//IL_25ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_2606: Expected O, but got Unknown
		//IL_260c: Unknown result type (might be due to invalid IL or missing references)
		//IL_2613: Expected O, but got Unknown
		//IL_2422: Unknown result type (might be due to invalid IL or missing references)
		//IL_283a: Unknown result type (might be due to invalid IL or missing references)
		//IL_2849: Unknown result type (might be due to invalid IL or missing references)
		//IL_2870: Unknown result type (might be due to invalid IL or missing references)
		//IL_287f: Unknown result type (might be due to invalid IL or missing references)
		//IL_28a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_28b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_28dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_28eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_2912: Unknown result type (might be due to invalid IL or missing references)
		//IL_2921: Unknown result type (might be due to invalid IL or missing references)
		//IL_2948: Unknown result type (might be due to invalid IL or missing references)
		//IL_2957: Unknown result type (might be due to invalid IL or missing references)
		//IL_297e: Unknown result type (might be due to invalid IL or missing references)
		//IL_298d: Unknown result type (might be due to invalid IL or missing references)
		//IL_29fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_2a2b: Unknown result type (might be due to invalid IL or missing references)
		//IL_2a2d: Unknown result type (might be due to invalid IL or missing references)
		//IL_2a3a: Unknown result type (might be due to invalid IL or missing references)
		//IL_2a3f: Unknown result type (might be due to invalid IL or missing references)
		//IL_2a41: Unknown result type (might be due to invalid IL or missing references)
		//IL_2a42: Unknown result type (might be due to invalid IL or missing references)
		//IL_29b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_29d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_29db: Unknown result type (might be due to invalid IL or missing references)
		//IL_29e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_29ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_29ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_29f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_26c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_26c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_26e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_26ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_270e: Unknown result type (might be due to invalid IL or missing references)
		//IL_2713: Unknown result type (might be due to invalid IL or missing references)
		//IL_2735: Unknown result type (might be due to invalid IL or missing references)
		//IL_273a: Unknown result type (might be due to invalid IL or missing references)
		//IL_275c: Unknown result type (might be due to invalid IL or missing references)
		//IL_2761: Unknown result type (might be due to invalid IL or missing references)
		//IL_2783: Unknown result type (might be due to invalid IL or missing references)
		//IL_2788: Unknown result type (might be due to invalid IL or missing references)
		//IL_27aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_27af: Unknown result type (might be due to invalid IL or missing references)
		//IL_2ae3: Unknown result type (might be due to invalid IL or missing references)
		//IL_2ae8: Unknown result type (might be due to invalid IL or missing references)
		//IL_2b24: Unknown result type (might be due to invalid IL or missing references)
		//IL_2b29: Unknown result type (might be due to invalid IL or missing references)
		//IL_2b36: Unknown result type (might be due to invalid IL or missing references)
		//IL_2b3b: Unknown result type (might be due to invalid IL or missing references)
		//IL_2b4c: Unknown result type (might be due to invalid IL or missing references)
		//IL_2b51: Unknown result type (might be due to invalid IL or missing references)
		//IL_2bad: Unknown result type (might be due to invalid IL or missing references)
		//IL_2bb2: Unknown result type (might be due to invalid IL or missing references)
		//IL_2ec0: Unknown result type (might be due to invalid IL or missing references)
		//IL_2ec5: Unknown result type (might be due to invalid IL or missing references)
		//IL_2f14: Unknown result type (might be due to invalid IL or missing references)
		//IL_2d30: Unknown result type (might be due to invalid IL or missing references)
		//IL_2d35: Unknown result type (might be due to invalid IL or missing references)
		//IL_2f94: Unknown result type (might be due to invalid IL or missing references)
		//IL_2f99: Unknown result type (might be due to invalid IL or missing references)
		//IL_2fe9: Unknown result type (might be due to invalid IL or missing references)
		//IL_2ca8: Unknown result type (might be due to invalid IL or missing references)
		//IL_2e50: Unknown result type (might be due to invalid IL or missing references)
		//IL_35be: Unknown result type (might be due to invalid IL or missing references)
		//IL_35c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_3613: Unknown result type (might be due to invalid IL or missing references)
		//IL_36dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_36e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_372c: Unknown result type (might be due to invalid IL or missing references)
		//IL_3817: Unknown result type (might be due to invalid IL or missing references)
		//IL_3831: Unknown result type (might be due to invalid IL or missing references)
		//IL_383b: Expected O, but got Unknown
		//IL_383c: Unknown result type (might be due to invalid IL or missing references)
		//IL_383e: Unknown result type (might be due to invalid IL or missing references)
		//IL_384d: Unknown result type (might be due to invalid IL or missing references)
		//IL_3867: Unknown result type (might be due to invalid IL or missing references)
		//IL_3871: Expected O, but got Unknown
		//IL_3872: Unknown result type (might be due to invalid IL or missing references)
		//IL_3874: Unknown result type (might be due to invalid IL or missing references)
		//IL_3883: Unknown result type (might be due to invalid IL or missing references)
		//IL_389d: Unknown result type (might be due to invalid IL or missing references)
		//IL_38a7: Expected O, but got Unknown
		//IL_38a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_38aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_38b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_38d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_38dd: Expected O, but got Unknown
		//IL_38de: Unknown result type (might be due to invalid IL or missing references)
		//IL_38e0: Unknown result type (might be due to invalid IL or missing references)
		configSeed = ((BaseUnityPlugin)this).Config.Bind<int>("Randomized Survivors", "RandomSeed", 0, "Random Seed to be used when generating survivors. (Must be an integer, such as 1234. 0 will generate a random seed)");
		if (configSeed.Value != 0)
		{
			randomSeed = configSeed.Value;
			Random.InitState(randomSeed);
			MonoBehaviour.print((object)("Random Survivors Seed: " + configSeed.Value));
		}
		else
		{
			randomSeed = Random.Range(1, 999999);
			Random.InitState(randomSeed);
			MonoBehaviour.print((object)("Random Survivors Seed: " + randomSeed));
		}
		configBodies = ((BaseUnityPlugin)this).Config.Bind<int>("Randomized Survivors", "NumberOfSurvivors", 141, "The number of Random Survivors to generate.");
		configFliers = ((BaseUnityPlugin)this).Config.Bind<bool>("Randomized Survivors", "EnableFliers", false, "Whether or not to enable flying bodies.");
		configCharSelect = ((BaseUnityPlugin)this).Config.Bind<bool>("Randomized Survivors", "EnableExpandedCharacterSelect", true, "Shrinks down Survivor select icons, and expands it across the top area of the screen. Disable if using any mods that change the Survivor selection screen!");
		object obj = _003C_003Ec._003C_003E9__17_0;
		if (obj == null)
		{
			hook_Build val = delegate(orig_Build orig, CharacterSelectBarController self)
			{
				//IL_0020: Unknown result type (might be due to invalid IL or missing references)
				//IL_003b: Unknown result type (might be due to invalid IL or missing references)
				if (configCharSelect.Value)
				{
					self.gridLayoutGroup.cellSize = new Vector2(40f, 40f);
					self.gridLayoutGroup.spacing = new Vector2(-5f, -5f);
					self.gridLayoutGroup.constraintCount = 38;
					RectOffset padding = ((LayoutGroup)self.gridLayoutGroup).padding;
					padding.right += -850;
				}
				orig.Invoke(self);
			};
			_003C_003Ec._003C_003E9__17_0 = val;
			obj = (object)val;
		}
		CharacterSelectBarController.Build += (hook_Build)obj;
		int i = 0;
		int num = 21;
		if (configBodies.Value > 0 && configBodies.Value <= 2500)
		{
			num = configBodies.Value;
		}
		for (; i < num; i++)
		{
			string[] array = (configFliers.Value ? new string[52]
			{
				"Bandit", "ClayBruiser", "Commando", "Croco", "Engi", "EngiWalkerTurret", "Huntress", "Imp", "Lemurian", "Loader",
				"Mage", "Merc", "Treebot", "ClayBoss", "Golem", "Gravekeeper", "ImpBoss", "LemurianBruiser", "Nullifier", "Scav",
				"ScavLunar1", "Titan", "TitanGold", "Shopkeeper", "Beetle", "Bison", "HermitCrab", "Vulture", "MiniMushroom", "Parent",
				"Assassin", "Bandit2", "Captain", "Heretic", "LunarExploder", "LunarGolem", "BackupDrone", "Bell", "Drone1", "Drone2",
				"BackupDroneOld", "EmergencyDrone", "EquipmentDrone", "FlameDrone", "Jellyfish", "RoboBallMini", "MissileDrone", "Wisp", "WispSoul", "GreaterWisp",
				"RoboBallGreenBuddy", "RoboBallRedBuddy"
			} : new string[36]
			{
				"Bandit", "ClayBruiser", "Commando", "Croco", "Engi", "EngiWalkerTurret", "Huntress", "Imp", "Lemurian", "Loader",
				"Mage", "Merc", "Treebot", "ClayBoss", "Golem", "Gravekeeper", "ImpBoss", "LemurianBruiser", "Nullifier", "Scav",
				"ScavLunar1", "Titan", "TitanGold", "Shopkeeper", "Beetle", "Bison", "HermitCrab", "Vulture", "MiniMushroom", "Parent",
				"Assassin", "Bandit2", "Captain", "Heretic", "LunarExploder", "LunarGolem"
			});
			string[] array2 = new string[92]
			{
				"Arch", "Ancient", "Bellowing", "Rotting", "Screaming", "Undead", "Holy", "Ghastly", "Mysterious", "Cooked",
				"Uncooked", "Wet", "Glowing", "Oily", "Punished", "Greater", "Lesser", "Lively", "Friendly", "Toxic",
				"Void", "Terrible", "Destructive", "Patient", "Clay", "Leaking", "Goopy", "Nasty", "Exiled", "Lord",
				"Killer", "Coy", "Baby", "Wild", "Turbo", "Captain", "Pulsing", "Strange", "Uncomfortable", "Discomforting",
				"Sticky", "Slimy", "Hard", "Solid", "Soft", "Dry", "Liquid", "Chilled", "Inferno", "Rancid",
				"Ugly", "Curious", "False", "True", "Feral", "Infantile", "Rickety", "Unstable", "Silent", "The",
				"Fearful", "Blessed", "Cursed", "Evil", "Good", "Righteous", "Radical", "Beautiful", "Metalic", "Pained",
				"Distinguished", "Royal", "Dark", "Light", "Crazy", "Lightning", "Fire", "Water", "Earth", "Frost",
				"Overloading", "Plasma", "Jelly", "Explosive", "Stinky", "Dynamic", "Alpha", "Beta", "Gamma", "Chaos",
				"Death", "Perfect"
			};
			string[] array3 = new string[74]
			{
				"Spirit", "Hog", "Creature", "Ghost", "Trinity", "Captain", "Spawn", "Demon", "Gremlin", "Architect",
				"Egg", "Insect", "Mimic", "Being", "Strider", "Denizen", "Stronghold", "Walker", "Glider", "Killer",
				"Reaper", "Baby", "Queen", "King", "Underling", "Servant", "Entity", "Ratlord", "Gunner", "Smithy",
				"Mage", "Snake", "Robot", "Swordsman", "Kicker", "Flailer", "Prodder", "Failure", "Monster", "Slayer",
				"Animal", "Humanoid", "End", "Orb", "Hunter", "Challenger", "Boss", "Runt", "Protagonist", "Hero",
				"Voidling", "Knight", "Paladin", "Rider", "Soul", "Idiot", "Pyromancer", "Slime", "Psycho", "Wraith",
				"Replica", "Shield", "Blade", "Spear", "Explosion", "Wolf", "Artifact", "Dissonance", "Enigma", "Kin",
				"Sacrifice", "Vengeance", "Cannon", "Heretic"
			};
			string text = array[Random.Range(0, array.Length)];
			string text2 = array2[Random.Range(0, array2.Length)];
			string text3 = array3[Random.Range(0, array3.Length)];
			string text4 = text2 + " " + text3;
			MonoBehaviour.print((object)("Creating Survivor: " + text4 + ", Using: [" + text + "Body]"));
			string text5 = text2 + text3;
			string text6 = Convert.ToString(i);
			GameObject val2 = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/CharacterBodies/" + text + "Body"), "Prefabs/CharacterBodies/Random" + text5 + "Body_" + text6, true, "C:\\Visual Studio Projects\\RoR2\\RoguelikeSurvivor\\FirstMod\\Class1.cs", "Awake", 390);
			GameObject val3 = Resources.Load<GameObject>("Prefabs/CharacterBodies/CrocoBody");
			val2.AddComponent<RandomManager>();
			val2.GetComponent<RandomManager>().mySeed = Random.Range(0, 99999);
			val2.GetComponent<RandomManager>().myColor = new Color(Random.Range(0.15f, 1f), Random.Range(0.15f, 1f), Random.Range(0.15f, 1f));
			val2.GetComponent<RandomManager>().myPassive = Mathf.Clamp(Random.Range(-8, 10), 0, 9);
			CharacterMotor component = val2.GetComponent<CharacterMotor>();
			CharacterMotor component2 = val3.GetComponent<CharacterMotor>();
			val2.GetComponent<CharacterBody>().spreadBloomDecayTime = val3.GetComponent<CharacterBody>().spreadBloomDecayTime;
			val2.GetComponent<CharacterBody>().spreadBloomCurve = val3.GetComponent<CharacterBody>().spreadBloomCurve;
			if (!Object.op_Implicit((Object)(object)val2.GetComponent<Interactor>()))
			{
				val2.AddComponent<Interactor>();
			}
			if (!Object.op_Implicit((Object)(object)val2.GetComponent<InteractionDriver>()))
			{
				val2.AddComponent<InteractionDriver>();
			}
			if ((double)Random.value < 0.7)
			{
				val2.GetComponent<ModelLocator>().normalizeToFloor = false;
			}
			else
			{
				val2.GetComponent<ModelLocator>().normalizeToFloor = true;
			}
			if (text == "LunarGolem")
			{
				GameObject gameObject = ((Component)val2.GetComponent<ModelLocator>().modelBaseTransform).gameObject;
				gameObject.transform.localScale = Vector3.one * 0.65f;
				gameObject.transform.Translate(new Vector3(0f, 1f, 0f));
				val2.GetComponent<CharacterBody>().aimOriginTransform.Translate(new Vector3(0f, 1.5f, 0f));
				KinematicCharacterMotor[] componentsInChildren = val2.GetComponentsInChildren<KinematicCharacterMotor>();
				foreach (KinematicCharacterMotor val4 in componentsInChildren)
				{
					val4.SetCapsuleDimensions(val4.Capsule.radius * 0.4f, val4.Capsule.height * 0.4f, -0.25f);
				}
				val2.GetComponent<CameraTargetParams>().cameraParams.standardLocalCameraPos = Resources.Load<GameObject>("Prefabs/CharacterBodies/CrocoBody").GetComponent<CameraTargetParams>().cameraParams.standardLocalCameraPos;
				val2.GetComponent<CameraTargetParams>().cameraParams.pivotVerticalOffset = 2.5f;
			}
			if (text == "Scav" || text == "ScavLunar1")
			{
				GameObject gameObject2 = ((Component)val2.GetComponent<ModelLocator>().modelBaseTransform).gameObject;
				gameObject2.transform.localScale = Vector3.one / 5f;
				gameObject2.transform.Translate(new Vector3(0f, 4f, 0f));
				val2.GetComponent<CharacterBody>().aimOriginTransform.Translate(new Vector3(0f, -3f, 0f));
				KinematicCharacterMotor[] componentsInChildren2 = val2.GetComponentsInChildren<KinematicCharacterMotor>();
				foreach (KinematicCharacterMotor val5 in componentsInChildren2)
				{
					val5.SetCapsuleDimensions(val5.Capsule.radius / 5f, val5.Capsule.height / 5f, 0f);
				}
				val2.GetComponent<CameraTargetParams>().cameraParams.standardLocalCameraPos = Resources.Load<GameObject>("Prefabs/CharacterBodies/CrocoBody").GetComponent<CameraTargetParams>().cameraParams.standardLocalCameraPos;
				val2.GetComponent<CameraTargetParams>().cameraParams.pivotVerticalOffset = 2f;
			}
			if (text == "ClayBoss")
			{
				GameObject gameObject3 = ((Component)val2.GetComponent<ModelLocator>().modelBaseTransform).gameObject;
				gameObject3.transform.localScale = Vector3.one / 5f;
				gameObject3.transform.Translate(new Vector3(0f, 3f, 0f));
				val2.GetComponent<CharacterBody>().aimOriginTransform.Translate(new Vector3(0f, -11f, 0f));
				val2.GetComponent<Interactor>().maxInteractionDistance = 6f;
				KinematicCharacterMotor[] componentsInChildren3 = val2.GetComponentsInChildren<KinematicCharacterMotor>();
				foreach (KinematicCharacterMotor val6 in componentsInChildren3)
				{
					val6.SetCapsuleDimensions(val6.Capsule.radius / 5f, val6.Capsule.height / 5f, 0f);
				}
				val2.GetComponent<CameraTargetParams>().cameraParams.standardLocalCameraPos = Resources.Load<GameObject>("Prefabs/CharacterBodies/CrocoBody").GetComponent<CameraTargetParams>().cameraParams.standardLocalCameraPos;
				val2.GetComponent<CameraTargetParams>().cameraParams.pivotVerticalOffset = 2f;
			}
			if (text == "Gravekeeper")
			{
				GameObject gameObject4 = ((Component)val2.GetComponent<ModelLocator>().modelBaseTransform).gameObject;
				gameObject4.transform.localScale = Vector3.one / 4f;
				gameObject4.transform.Translate(new Vector3(0f, 5.5f, 0f));
				val2.GetComponent<CharacterBody>().aimOriginTransform.Translate(new Vector3(0f, -4f, 0f));
				KinematicCharacterMotor[] componentsInChildren4 = val2.GetComponentsInChildren<KinematicCharacterMotor>();
				foreach (KinematicCharacterMotor val7 in componentsInChildren4)
				{
					val7.SetCapsuleDimensions(val7.Capsule.radius / 4f, val7.Capsule.height / 4f, 0f);
				}
				val2.GetComponent<CameraTargetParams>().cameraParams.standardLocalCameraPos = Resources.Load<GameObject>("Prefabs/CharacterBodies/CrocoBody").GetComponent<CameraTargetParams>().cameraParams.standardLocalCameraPos;
				val2.GetComponent<CameraTargetParams>().cameraParams.pivotVerticalOffset = 2f;
			}
			if (text == "ImpBoss")
			{
				GameObject gameObject5 = ((Component)val2.GetComponent<ModelLocator>().modelBaseTransform).gameObject;
				gameObject5.transform.localScale = Vector3.one / 2f;
				gameObject5.transform.Translate(new Vector3(0f, 4f, 0f));
				val2.GetComponent<CharacterBody>().aimOriginTransform.Translate(new Vector3(0f, -3f, 0f));
				KinematicCharacterMotor[] componentsInChildren5 = val2.GetComponentsInChildren<KinematicCharacterMotor>();
				foreach (KinematicCharacterMotor val8 in componentsInChildren5)
				{
					val8.SetCapsuleDimensions(val8.Capsule.radius / 2f, val8.Capsule.height / 2f, 0f);
				}
				val2.GetComponent<CameraTargetParams>().cameraParams.standardLocalCameraPos = Resources.Load<GameObject>("Prefabs/CharacterBodies/CrocoBody").GetComponent<CameraTargetParams>().cameraParams.standardLocalCameraPos;
				val2.GetComponent<CameraTargetParams>().cameraParams.pivotVerticalOffset = 2f;
			}
			if (text == "Imp")
			{
				GameObject gameObject6 = ((Component)val2.GetComponent<ModelLocator>().modelBaseTransform).gameObject;
				gameObject6.transform.localScale = Vector3.one * 0.95f;
				gameObject6.transform.Translate(new Vector3(0f, 0.15f, 0f));
				val2.GetComponent<CharacterBody>().aimOriginTransform.Translate(new Vector3(0f, -0.15f, 0f));
				KinematicCharacterMotor[] componentsInChildren6 = val2.GetComponentsInChildren<KinematicCharacterMotor>();
				foreach (KinematicCharacterMotor val9 in componentsInChildren6)
				{
					val9.SetCapsuleDimensions(val9.Capsule.radius * 0.95f, val9.Capsule.height * 0.95f, 0f);
				}
				val2.GetComponent<CameraTargetParams>().cameraParams.standardLocalCameraPos = Resources.Load<GameObject>("Prefabs/CharacterBodies/CommandoBody").GetComponent<CameraTargetParams>().cameraParams.standardLocalCameraPos;
				val2.GetComponent<CameraTargetParams>().cameraParams.pivotVerticalOffset = 2f;
			}
			if (text == "Nullifier")
			{
				GameObject gameObject7 = ((Component)val2.GetComponent<ModelLocator>().modelBaseTransform).gameObject;
				gameObject7.transform.localScale = Vector3.one / 2f;
				gameObject7.transform.Translate(new Vector3(0f, 4f, 0f));
				val2.GetComponent<CharacterBody>().aimOriginTransform.Translate(new Vector3(0f, 0f, 0f));
				KinematicCharacterMotor[] componentsInChildren7 = val2.GetComponentsInChildren<KinematicCharacterMotor>();
				foreach (KinematicCharacterMotor val10 in componentsInChildren7)
				{
					val10.SetCapsuleDimensions(val10.Capsule.radius / 2f, val10.Capsule.height / 2f, 2f);
				}
				val2.GetComponent<CameraTargetParams>().cameraParams.standardLocalCameraPos = Resources.Load<GameObject>("Prefabs/CharacterBodies/CrocoBody").GetComponent<CameraTargetParams>().cameraParams.standardLocalCameraPos;
				val2.GetComponent<CameraTargetParams>().cameraParams.pivotVerticalOffset = 2f;
			}
			if (text == "Titan" || text == "TitanGold")
			{
				GameObject gameObject8 = ((Component)val2.GetComponent<ModelLocator>().modelBaseTransform).gameObject;
				gameObject8.transform.localScale = Vector3.one / 3.5f;
				gameObject8.transform.Translate(new Vector3(0f, 4f, 0f));
				val2.GetComponent<CharacterBody>().aimOriginTransform.Translate(new Vector3(0f, -8f, 0f));
				val2.GetComponent<Interactor>().maxInteractionDistance = 6f;
				KinematicCharacterMotor[] componentsInChildren8 = val2.GetComponentsInChildren<KinematicCharacterMotor>();
				foreach (KinematicCharacterMotor val11 in componentsInChildren8)
				{
					val11.SetCapsuleDimensions(val11.Capsule.radius / 3.5f, val11.Capsule.height / 3.5f, 0f);
				}
				val2.GetComponent<CameraTargetParams>().cameraParams.standardLocalCameraPos = Resources.Load<GameObject>("Prefabs/CharacterBodies/CrocoBody").GetComponent<CameraTargetParams>().cameraParams.standardLocalCameraPos;
				val2.GetComponent<CameraTargetParams>().cameraParams.pivotVerticalOffset = 2f;
			}
			if (text == "LemurianBruiser")
			{
				GameObject gameObject9 = ((Component)val2.GetComponent<ModelLocator>().modelBaseTransform).gameObject;
				gameObject9.transform.localScale = Vector3.one / 3f;
				gameObject9.transform.Translate(new Vector3(0f, 3f, 0f));
				val2.GetComponent<CharacterBody>().aimOriginTransform.Translate(new Vector3(0f, -3f, 0f));
				KinematicCharacterMotor[] componentsInChildren9 = val2.GetComponentsInChildren<KinematicCharacterMotor>();
				foreach (KinematicCharacterMotor val12 in componentsInChildren9)
				{
					val12.SetCapsuleDimensions(val12.Capsule.radius / 3f, val12.Capsule.height / 3f, 0f);
				}
				val2.GetComponent<CameraTargetParams>().cameraParams.standardLocalCameraPos = Resources.Load<GameObject>("Prefabs/CharacterBodies/CrocoBody").GetComponent<CameraTargetParams>().cameraParams.standardLocalCameraPos;
				val2.GetComponent<CameraTargetParams>().cameraParams.pivotVerticalOffset = 2f;
			}
			if (text == "Parent")
			{
				GameObject gameObject10 = ((Component)val2.GetComponent<ModelLocator>().modelBaseTransform).gameObject;
				gameObject10.transform.localScale = Vector3.one / 3f;
				gameObject10.transform.Translate(new Vector3(0f, 3f, 0f));
				val2.GetComponent<CharacterBody>().aimOriginTransform.Translate(new Vector3(0f, -4f, 0f));
				KinematicCharacterMotor[] componentsInChildren10 = val2.GetComponentsInChildren<KinematicCharacterMotor>();
				foreach (KinematicCharacterMotor val13 in componentsInChildren10)
				{
					val13.SetCapsuleDimensions(val13.Capsule.radius / 3f, val13.Capsule.height / 3f, 0f);
				}
				val2.GetComponent<CameraTargetParams>().cameraParams.standardLocalCameraPos = Resources.Load<GameObject>("Prefabs/CharacterBodies/CrocoBody").GetComponent<CameraTargetParams>().cameraParams.standardLocalCameraPos;
				val2.GetComponent<CameraTargetParams>().cameraParams.pivotVerticalOffset = 2f;
			}
			if (text == "Golem")
			{
				GameObject gameObject11 = ((Component)val2.GetComponent<ModelLocator>().modelBaseTransform).gameObject;
				gameObject11.transform.localScale = Vector3.one / 2f;
				gameObject11.transform.Translate(new Vector3(0f, 2f, 0f));
				val2.GetComponent<CharacterBody>().aimOriginTransform.Translate(new Vector3(0f, -1f, 0f));
				KinematicCharacterMotor[] componentsInChildren11 = val2.GetComponentsInChildren<KinematicCharacterMotor>();
				foreach (KinematicCharacterMotor val14 in componentsInChildren11)
				{
					val14.SetCapsuleDimensions(val14.Capsule.radius / 2f, val14.Capsule.height / 2f, 0f);
				}
				val2.GetComponent<CameraTargetParams>().cameraParams.standardLocalCameraPos = Resources.Load<GameObject>("Prefabs/CharacterBodies/CrocoBody").GetComponent<CameraTargetParams>().cameraParams.standardLocalCameraPos;
				val2.GetComponent<CameraTargetParams>().cameraParams.pivotVerticalOffset = 2f;
			}
			if (text == "Shopkeeper")
			{
				GameObject gameObject12 = ((Component)val2.GetComponent<ModelLocator>().modelBaseTransform).gameObject;
				gameObject12.transform.localScale = Vector3.one / 2f;
				gameObject12.transform.Translate(new Vector3(0f, 2f, 0f));
				val2.GetComponent<CharacterBody>().aimOriginTransform.Translate(new Vector3(0f, -1f, 0f));
				val2.GetComponent<CharacterBody>().rootMotionInMainState = false;
				KinematicCharacterMotor[] componentsInChildren12 = val2.GetComponentsInChildren<KinematicCharacterMotor>();
				foreach (KinematicCharacterMotor val15 in componentsInChildren12)
				{
					val15.SetCapsuleDimensions(val15.Capsule.radius / 2f, val15.Capsule.height / 2f, 0f);
				}
				val2.GetComponent<CameraTargetParams>().cameraParams.standardLocalCameraPos = Resources.Load<GameObject>("Prefabs/CharacterBodies/CrocoBody").GetComponent<CameraTargetParams>().cameraParams.standardLocalCameraPos;
				val2.GetComponent<CameraTargetParams>().cameraParams.pivotVerticalOffset = 2f;
			}
			if (text == "Vagrant")
			{
				GameObject gameObject13 = ((Component)val2.GetComponent<ModelLocator>().modelBaseTransform).gameObject;
				gameObject13.transform.localScale = Vector3.one / 2f;
				KinematicCharacterMotor[] componentsInChildren13 = val2.GetComponentsInChildren<KinematicCharacterMotor>();
				foreach (KinematicCharacterMotor val16 in componentsInChildren13)
				{
					val16.SetCapsuleDimensions(val16.Capsule.radius / 2f, val16.Capsule.height / 2f, 0f);
				}
				val2.GetComponent<CameraTargetParams>().cameraParams.standardLocalCameraPos = Resources.Load<GameObject>("Prefabs/CharacterBodies/GreaterWispBody").GetComponent<CameraTargetParams>().cameraParams.standardLocalCameraPos;
				val2.GetComponent<CameraTargetParams>().cameraParams.standardLocalCameraPos.y = 0f;
			}
			if (text == "GreaterWisp")
			{
				GameObject gameObject14 = ((Component)val2.GetComponent<ModelLocator>().modelBaseTransform).gameObject;
				gameObject14.transform.localScale = Vector3.one / 2f;
				KinematicCharacterMotor[] componentsInChildren14 = val2.GetComponentsInChildren<KinematicCharacterMotor>();
				foreach (KinematicCharacterMotor val17 in componentsInChildren14)
				{
					val17.SetCapsuleDimensions(val17.Capsule.radius / 2f, val17.Capsule.height / 2f, 0f);
				}
			}
			SetStateOnHurt component3 = val2.GetComponent<SetStateOnHurt>();
			if (Object.op_Implicit((Object)(object)component3))
			{
				component3.hitThreshold = 1f;
			}
			GenericSkill[] componentsInChildren15 = val2.GetComponentsInChildren<GenericSkill>();
			foreach (GenericSkill val18 in componentsInChildren15)
			{
				Object.DestroyImmediate((Object)(object)val18);
			}
			SkillLocator component4 = val2.GetComponent<SkillLocator>();
			Reflection.SetFieldValue<GenericSkill[]>((object)component4, "allSkills", (GenericSkill[])(object)new GenericSkill[0]);
			component4.primary = val2.AddComponent<GenericSkill>();
			SkillFamily val19 = ScriptableObject.CreateInstance<SkillFamily>();
			val19.variants = (Variant[])(object)new Variant[1];
			LoadoutAPI.AddSkillFamily(val19);
			Reflection.SetFieldValue<SkillFamily>((object)component4.primary, "_skillFamily", val19);
			component4.secondary = val2.AddComponent<GenericSkill>();
			SkillFamily val20 = ScriptableObject.CreateInstance<SkillFamily>();
			val20.variants = (Variant[])(object)new Variant[1];
			LoadoutAPI.AddSkillFamily(val20);
			Reflection.SetFieldValue<SkillFamily>((object)component4.secondary, "_skillFamily", val20);
			component4.utility = val2.AddComponent<GenericSkill>();
			SkillFamily val21 = ScriptableObject.CreateInstance<SkillFamily>();
			val21.variants = (Variant[])(object)new Variant[1];
			LoadoutAPI.AddSkillFamily(val21);
			Reflection.SetFieldValue<SkillFamily>((object)component4.utility, "_skillFamily", val21);
			component4.special = val2.AddComponent<GenericSkill>();
			SkillFamily val22 = ScriptableObject.CreateInstance<SkillFamily>();
			val22.variants = (Variant[])(object)new Variant[1];
			LoadoutAPI.AddSkillFamily(val22);
			Reflection.SetFieldValue<SkillFamily>((object)component4.special, "_skillFamily", val22);
			Color32[] array4 = (Color32[])(object)new Color32[7]
			{
				Color32.op_Implicit(Color.clear),
				Color32.op_Implicit(new Color(0f, 0f, 0f)),
				default(Color32),
				default(Color32),
				default(Color32),
				default(Color32),
				default(Color32)
			};
			Color myColor = val2.GetComponent<RandomManager>().myColor;
			myColor.r *= 0.4f;
			myColor.b *= 0.6f;
			myColor.g *= 0.5f;
			array4[2] = Color32.op_Implicit(myColor);
			array4[3] = Color32.op_Implicit(val2.GetComponent<RandomManager>().myColor);
			array4[4] = Color32.op_Implicit(new Color(0.29f, 0.33f, 0.36f));
			array4[5] = Color32.op_Implicit(new Color(0.64f, 0.66f, 0.68f));
			array4[6] = Color32.op_Implicit(new Color(1f, 1f, 1f));
			string text7 = "0000000000000000000000111100000000001123321100000001236666321000001236666663210000133663366331000123333336633210013333336633331001333336633333100123333663333210001333333333310000123336633321000001233663321000000011233211000000000011110000000000000000000000";
			Texture2D val23 = new Texture2D(16, 16, (TextureFormat)4, false);
			((Texture)val23).filterMode = (FilterMode)0;
			((Texture)val23).wrapMode = (TextureWrapMode)1;
			int num12 = 0;
			int num13 = 0;
			int num14 = 0;
			for (; num13 < 16; num13++)
			{
				for (num12 = 0; num12 < 16; num12++)
				{
					val23.SetPixel(num12, Mathf.Abs(num13 - 15), Color32.op_Implicit(array4[int.Parse(text7.Substring(num14, 1))]));
					num14++;
				}
			}
			val23.Apply();
			Texture portraitIcon = (Texture)(object)val23;
			CharacterBody component5 = val2.GetComponent<CharacterBody>();
			component5.baseDamage = 10f;
			component5.baseCrit = 1f;
			component5.levelCrit = 0f;
			component5.baseMaxHealth = Random.Range(80f, 200f);
			component5.levelMaxHealth = component5.baseMaxHealth * 0.3f;
			component5.baseArmor = 20f;
			component5.baseRegen = Random.Range(0.5f, 2.5f);
			component5.levelRegen = component5.baseRegen * 0.2f;
			component5.baseMoveSpeed = Random.Range(6f, 8f);
			component5.levelMoveSpeed = 0f;
			component5.baseAttackSpeed = 1f;
			component5.baseAcceleration = Random.Range(60f, 100f);
			component5.baseJumpPower = Random.Range(8f, 25f);
			component5.baseJumpCount = 1;
			((Object)component5).name = text4 + " " + text6;
			component5.portraitIcon = portraitIcon;
			component5.baseNameToken = text4;
			component5.subtitleNameToken = text4;
			component5.crosshairPrefab = Resources.Load<GameObject>("Prefabs/CharacterBodies/CrocoBody").GetComponent<CharacterBody>().crosshairPrefab;
			component5.preferredPodPrefab = Resources.Load<GameObject>("Prefabs/CharacterBodies/ToolbotBody").GetComponent<CharacterBody>().preferredPodPrefab;
			if (Object.op_Implicit((Object)(object)val2.GetComponent<CharacterDirection>()))
			{
				val2.GetComponent<CharacterDirection>().turnSpeed = Random.Range(100f, 1000f);
				val2.GetComponent<CharacterDirection>().driveFromRootRotation = false;
			}
			if (text == "Titan" || text == "TitanGold" || text == "Vulture")
			{
				val2.GetComponent<RandomManager>().canSprint = false;
				component5.baseMoveSpeed *= 1.5f;
			}
			else
			{
				val2.GetComponent<RandomManager>().canSprint = true;
			}
			switch (text)
			{
			default:
				if (!(text == "GreaterWisp"))
				{
					val2.GetComponent<RandomManager>().isFlier = false;
					break;
				}
				goto case "BackupDrone";
			case "BackupDrone":
			case "Bell":
			case "Drone1":
			case "Drone2":
			case "BackupDroneOld":
			case "EmergencyDrone":
			case "EquipmentDrone":
			case "FlameDrone":
			case "Jellyfish":
			case "RoboBallMini":
			case "RoboBallGreenBuddy":
			case "RoboBallRedBuddy":
			case "MissileDrone":
			case "Wisp":
			case "WispSoul":
				val2.GetComponent<RandomManager>().isFlier = true;
				component5.baseAcceleration *= 0.2f;
				component5.baseMoveSpeed *= 2f;
				break;
			}
			int myPassive = val2.GetComponent<RandomManager>().myPassive;
			string skillNameToken = "";
			string skillDescriptionToken = "";
			BodyFlags val24 = (BodyFlags)16;
			string text8 = null;
			if (myPassive == 0)
			{
				skillNameToken = "Blessing of Strong Will";
				skillDescriptionToken = "Even if you start from nothing, with enough hard work you can surpass the mightiest of adversaries.";
				val24 = (BodyFlags)16;
				text8 = "0000000000000000000000000000000000000000000000000000000000000000000000111100000000000114551000000000114556510000000011455551000000001144554100000000111444110000000001111110000000000011110000000000000000000000000000000000000000000000000000000000000000000000";
			}
			if (myPassive == 1)
			{
				skillNameToken = "Blessing of Slippery Oil";
				skillDescriptionToken = "Unnefected by the <style=cIsUtility>slowdown effect of Tar</style>.";
				val24 = (BodyFlags)24;
				text8 = "0000000000000000000000000000000000000000000000000000000000000000000000111100000000000144551000000000144556510000000014144511000000001111114100000000114111110000000011111111000000000111111100000000011101100000000001010110000000000100001000000000000100100000";
			}
			if (myPassive == 2)
			{
				skillNameToken = "Blessing of Thick Moss";
				skillDescriptionToken = "<style=cIsUtility>Immune</style> to <style=cIsUtility>fall damage</style>.";
				val24 = (BodyFlags)17;
				text8 = "0000000000000000000000000000000000000000000000000000000001100000000000111121000000000114551100000000114556510000000131455511000000132144513100000012111144131000000123131112100000011232332110000000112221121000000000111011000000000000000000000000000000000000";
			}
			if (myPassive == 3)
			{
				skillNameToken = "Blessing of Robotics";
				skillDescriptionToken = "May look organic on the surface, but...";
				val24 = (BodyFlags)18;
				text8 = "0000000000000000000000000000000000000000000000000000000000000000000000111100000000000145651000000000121231310000000014455651000000001144554100000000121231210000000001144410000000000011110000000000000000000000000000000000000000000000000000000000000000000000";
			}
			if (myPassive == 4)
			{
				skillNameToken = "Blessing of Hidden Eyes";
				skillDescriptionToken = "<style=cIsUtility>Sprint</style> in <style=cIsUtility>any direction</style>.";
				val24 = (BodyFlags)48;
				text8 = "0000000000000000000000000000000000000000000000000000000000000000000000111100000000000145651000000000111111110000000231311313200000001111111100000000144455510000000011444551000000001111111100000000000000000000000000000000000000000000000000000000000000000000";
			}
			if (myPassive == 5)
			{
				skillNameToken = "Blessing of Drones";
				skillDescriptionToken = "Take reduced <style=cIsUtility>area-of-effect damage</style>.";
				val24 = (BodyFlags)80;
				text8 = "0000000000000000000000000000000001010111111010100000000110000000000011111111000000001145666100000000114551110000000011451163100000001144113310000000111441110000000001111110000000000011110000000000000110000000000000111111100000000000000000000000000000000000";
			}
			if (myPassive == 6)
			{
				skillNameToken = "Blessing of Hopoo";
				skillDescriptionToken = "Can <style=cIsUtility>double jump</style>.";
				val24 = (BodyFlags)16;
				text8 = "0000000000000000000000000000000000000000000000000000000000000000000000111100000000000114551000000000114556510000000011455551000000321144554123000300111444110030003301111110330000023333333320000030000000000300000333333333300000000200002000000000003333000000";
				component5.baseJumpCount = 2;
			}
			if (myPassive == 7)
			{
				skillNameToken = "Blessing of Transcendence";
				skillDescriptionToken = "Start with a <style=cIsUtility>shield</style>.";
				val24 = (BodyFlags)16;
				text8 = "0000000000000000000000000000000000000000000000000000003333000000000003211230000000003114551300000003214556523000000311455551300000031144554130000003211444123000000031111113000000000321123000000000003333000000000000000000000000000000000000000000000000000000";
				component5.baseMaxShield = component5.baseMaxHealth * 0.2f;
				component5.levelMaxShield = component5.levelMaxHealth * 0.2f;
			}
			if (myPassive == 8)
			{
				skillNameToken = "Poor Eyesight";
				skillDescriptionToken = "Can't see your <style=cIsHealth>crosshair</style>.";
				val24 = (BodyFlags)16;
				text8 = "0000000000000000000000000000000000000000000000000000000011110000000011111100000000000111111111000011111111110000000011233211100011111123321000000000111111111110000111111110000000000011111110000000111100000000000000000000000000000000000000000000000000000000";
				component5.hideCrosshair = true;
			}
			if (myPassive == 9)
			{
				skillNameToken = "Monster's Curse";
				skillDescriptionToken = "No protection from <style=cIsHealth>execution</style>.";
				val24 = (BodyFlags)0;
				text8 = "0000000000000000000000000000000000000000000000000000000000000000000000111100000000000214551000000000214541110000000021411651000000003144154200000000321424130000000003213123000000000332320300000000003030000000000000003000000000000000000000000000000030000000";
			}
			Color32[] array5 = (Color32[])(object)new Color32[7];
			Color myColor2 = val2.GetComponent<RandomManager>().myColor;
			myColor2.r *= 0.4f;
			myColor2.b *= 0.6f;
			myColor2.g *= 0.5f;
			Color myColor3 = val2.GetComponent<RandomManager>().myColor;
			myColor3.r = (myColor3.r + 0.2f) / 2f;
			myColor3.b = (myColor3.g + 0.2f) / 2f;
			myColor3.g = (myColor3.b + 0.2f) / 2f;
			array5[0] = Color32.op_Implicit(myColor3);
			array5[1] = Color32.op_Implicit(new Color(0f, 0f, 0f));
			array5[2] = Color32.op_Implicit(myColor2);
			array5[3] = Color32.op_Implicit(val2.GetComponent<RandomManager>().myColor);
			array5[4] = Color32.op_Implicit(new Color(0.29f, 0.33f, 0.36f));
			array5[5] = Color32.op_Implicit(new Color(0.64f, 0.66f, 0.68f));
			array5[6] = Color32.op_Implicit(new Color(1f, 1f, 1f));
			string text9 = text8;
			string text10 = "2222222222222222211111111111111221111111111111122111111112233322211111122333663221222333366666622111111223336632211111111223332221223332111111122233366321111112336666662111111222333663211111122122333211111112211111111111111221111111111111122222222222222222";
			string text11 = "2222222222222223211111111113136221111111111636322111111115166312211111111156663221111111165511122111111165511112211111165511111221115555511111122115144541111112215114411411111221511451111111122111145111111112211114451111111221111444111111122222222222222222";
			string text12 = "2222222222222222211111111111111221111111166661122111111166336612211111161233661221111111113366122111111111366112211111111336611221121111236611122121111133661112212211123661111221322233661111122143335551111112211445511111111221111111111111122222222222222222";
			string text13 = "2222222222222222211111111111111221111112111111122111111131111112211133116112111221121163633111122111123666311112211336666666311221211366663112122111236663611112211136336213111221123321311311122123211131211112223211121111111223211111111111123222222222222222";
			string text14 = "2222222222222222211111111111111221111114411111122111154441111112211155411111111221165111111111122116111111111112211641116111111221156111561111122111665146611112211115666666111221111115666664122111111166641112211111466411111221111111111111122222222222222222";
			string text15 = "2222222222232222211111111113111221111111111611122111111111363112211212233666663321131111113631122116111111161112236663211213111221161111121311122113111123221112211211223632211221211111232111122131111112111112236321111211111221311111111111122222222222222222";
			Texture2D val25 = new Texture2D(16, 16, (TextureFormat)4, false);
			Texture2D val26 = new Texture2D(16, 16, (TextureFormat)4, false);
			Texture2D val27 = new Texture2D(16, 16, (TextureFormat)4, false);
			Texture2D val28 = new Texture2D(16, 16, (TextureFormat)4, false);
			Texture2D val29 = new Texture2D(16, 16, (TextureFormat)4, false);
			Texture2D val30 = new Texture2D(16, 16, (TextureFormat)4, false);
			Texture2D val31 = new Texture2D(16, 16, (TextureFormat)4, false);
			((Texture)val25).filterMode = (FilterMode)0;
			((Texture)val26).filterMode = (FilterMode)0;
			((Texture)val27).filterMode = (FilterMode)0;
			((Texture)val28).filterMode = (FilterMode)0;
			((Texture)val29).filterMode = (FilterMode)0;
			((Texture)val30).filterMode = (FilterMode)0;
			((Texture)val31).filterMode = (FilterMode)0;
			((Texture)val25).wrapMode = (TextureWrapMode)1;
			((Texture)val26).wrapMode = (TextureWrapMode)1;
			((Texture)val27).wrapMode = (TextureWrapMode)1;
			((Texture)val28).wrapMode = (TextureWrapMode)1;
			((Texture)val29).wrapMode = (TextureWrapMode)1;
			((Texture)val30).wrapMode = (TextureWrapMode)1;
			((Texture)val31).wrapMode = (TextureWrapMode)1;
			int num15 = 0;
			int num16 = 0;
			int num17 = 0;
			for (; num16 < 16; num16++)
			{
				for (num15 = 0; num15 < 16; num15++)
				{
					val25.SetPixel(num15, num16, Color32.op_Implicit(array5[int.Parse(text9.Substring(num17, 1))]));
					val26.SetPixel(num15, num16, Color32.op_Implicit(array5[int.Parse(text10.Substring(num17, 1))]));
					val27.SetPixel(num15, num16, Color32.op_Implicit(array5[int.Parse(text11.Substring(num17, 1))]));
					val28.SetPixel(num15, num16, Color32.op_Implicit(array5[int.Parse(text12.Substring(num17, 1))]));
					val29.SetPixel(num15, num16, Color32.op_Implicit(array5[int.Parse(text13.Substring(num17, 1))]));
					val30.SetPixel(num15, num16, Color32.op_Implicit(array5[int.Parse(text14.Substring(num17, 1))]));
					val31.SetPixel(num15, num16, Color32.op_Implicit(array5[int.Parse(text15.Substring(num17, 1))]));
					num17++;
				}
			}
			val25.Apply();
			val26.Apply();
			val27.Apply();
			val28.Apply();
			val29.Apply();
			val30.Apply();
			val31.Apply();
			Sprite icon = Sprite.Create(val25, new Rect(0f, 16f, 16f, -16f), new Vector2(8f, 8f), 16f);
			Sprite icon2 = Sprite.Create(val26, new Rect(0f, 16f, 16f, -16f), new Vector2(8f, 8f), 16f);
			Sprite icon3 = Sprite.Create(val27, new Rect(0f, 16f, 16f, -16f), new Vector2(8f, 8f), 16f);
			Sprite icon4 = Sprite.Create(val28, new Rect(0f, 16f, 16f, -16f), new Vector2(8f, 8f), 16f);
			Sprite icon5 = Sprite.Create(val29, new Rect(0f, 16f, 16f, -16f), new Vector2(8f, 8f), 16f);
			Sprite icon6 = Sprite.Create(val30, new Rect(0f, 16f, 16f, -16f), new Vector2(8f, 8f), 16f);
			Sprite icon7 = Sprite.Create(val31, new Rect(0f, 16f, 16f, -16f), new Vector2(8f, 8f), 16f);
			if (myPassive > 0)
			{
				component4.passiveSkill = new PassiveSkill
				{
					enabled = true,
					icon = icon,
					skillNameToken = skillNameToken,
					skillDescriptionToken = skillDescriptionToken
				};
				CharacterBody component6 = val2.GetComponent<CharacterBody>();
				component6.bodyFlags |= val24;
			}
			else
			{
				component4.passiveSkill = new PassiveSkill
				{
					enabled = false,
					icon = null,
					skillNameToken = "",
					skillDescriptionToken = ""
				};
				CharacterBody component7 = val2.GetComponent<CharacterBody>();
				component7.bodyFlags |= val24;
			}
			GameObject gameObject15 = ((Component)val2.GetComponent<ModelLocator>().modelBaseTransform).gameObject;
			string[] array6 = new string[9] { "Commando", "Engi", "Huntress", "Mage", "Merc", "Toolbot", "TreeBot", "Loader", "Croco" };
			SurvivorDef val32 = ScriptableObject.CreateInstance<SurvivorDef>();
			val32.bodyPrefab = val2;
			val32.descriptionToken = text4 + Environment.NewLine;
			val32.displayPrefab = gameObject15;
			val32.primaryColor = val2.GetComponent<RandomManager>().myColor;
			val32.displayNameToken = text4;
			val32.desiredSortPosition = 512f;
			RandomStateMachine randomStateMachine = val2.AddComponent<RandomStateMachine>();
			((EntityStateMachine)randomStateMachine).customName = "CustomWeapon";
			((EntityStateMachine)randomStateMachine).mainStateType = new SerializableEntityStateType(typeof(Idle));
			((EntityStateMachine)randomStateMachine).commonComponents = new CommonComponentCache(((Component)this).gameObject);
			((EntityStateMachine)randomStateMachine).initialStateType = new SerializableEntityStateType(typeof(Idle));
			SurvivorAPI.AddSurvivor(val32);
			LoadoutAPI.AddSkill(typeof(RandomPrimary));
			SkillDef val33 = ScriptableObject.CreateInstance<SkillDef>();
			val2.GetComponent<RandomManager>().primaryType = Random.Range(0, 3);
			if (val2.GetComponent<RandomManager>().primaryType == 0)
			{
				val33.activationState = new SerializableEntityStateType(typeof(RandomProjectilePrimary));
				val33.activationStateMachineName = "CustomWeapon";
				val33.baseRechargeInterval = Mathf.Clamp(Random.Range(-2f, 4f), 0f, 4f);
				if (val33.baseRechargeInterval > 0f)
				{
					val33.baseMaxStock = Random.Range(1, 33);
					val33.stockToConsume = 1;
					val33.resetCooldownTimerOnUse = true;
					val33.skillDescriptionToken = "Holds <style=cIsDamage>" + val33.baseMaxStock + " charge(s)</style>, and takes <style=cIsDamage>" + Mathf.RoundToInt(val33.baseRechargeInterval) + " seconds</style> to recharge.";
				}
				else
				{
					val33.baseMaxStock = 1;
					val33.stockToConsume = 0;
					val33.skillDescriptionToken = "Holds <style=cIsDamage>infinite charges</style>, but has <style=cIsHealth>reduced damage bonus</style>.";
				}
				val33.beginSkillCooldownOnSkillEnd = true;
				val33.canceledFromSprinting = false;
				val33.fullRestockOnAssign = true;
				val33.interruptPriority = (InterruptPriority)1;
				val33.isCombatSkill = true;
				val33.mustKeyPress = false;
				val33.cancelSprintingOnActivation = true;
				val33.rechargeStock = 32;
				val33.requiredStock = 1;
				val33.skillName = text5 + "_PRIMARY";
				val33.skillNameToken = text4 + "'s Projectile Skill";
				val33.icon = icon2;
			}
			else if (val2.GetComponent<RandomManager>().primaryType == 1)
			{
				val33.activationState = new SerializableEntityStateType(typeof(RandomHitscanPrimary));
				val33.activationStateMachineName = "CustomWeapon";
				val33.baseRechargeInterval = Mathf.Clamp(Random.Range(-2f, 4f), 0f, 4f);
				val33.baseRechargeInterval = Mathf.Clamp(Random.Range(-2f, 4f), 0f, 4f);
				if (val33.baseRechargeInterval > 0f)
				{
					val33.baseMaxStock = Random.Range(1, 33);
					val33.stockToConsume = 1;
					val33.resetCooldownTimerOnUse = true;
					val33.skillDescriptionToken = "Holds <style=cIsDamage>" + val33.baseMaxStock + " bullet(s)</style>, and takes <style=cIsDamage>" + Mathf.RoundToInt(val33.baseRechargeInterval) + " seconds</style> to reload.";
				}
				else
				{
					val33.baseMaxStock = 1;
					val33.stockToConsume = 0;
					val33.skillDescriptionToken = "Holds <style=cIsDamage>infinite bullets</style>, but has <style=cIsHealth>reduced damage bonus</style>.";
				}
				val33.beginSkillCooldownOnSkillEnd = true;
				val33.canceledFromSprinting = false;
				val33.fullRestockOnAssign = true;
				val33.interruptPriority = (InterruptPriority)1;
				val33.isCombatSkill = true;
				val33.mustKeyPress = false;
				val33.cancelSprintingOnActivation = true;
				val33.rechargeStock = 32;
				val33.requiredStock = 1;
				val33.skillName = text5 + "_PRIMARY";
				val33.skillNameToken = text4 + "'s Gun";
				val33.icon = icon3;
			}
			else
			{
				val33.activationState = new SerializableEntityStateType(typeof(RandomMeleePrimary));
				val33.activationStateMachineName = "CustomWeapon";
				val33.baseMaxStock = 1;
				val33.stockToConsume = 0;
				val33.skillDescriptionToken = text4 + "'s <style=cIsDamage>trusty blade</style>.";
				val33.beginSkillCooldownOnSkillEnd = true;
				val33.canceledFromSprinting = false;
				val33.fullRestockOnAssign = true;
				val33.interruptPriority = (InterruptPriority)1;
				val33.isCombatSkill = true;
				val33.mustKeyPress = false;
				val33.cancelSprintingOnActivation = true;
				val33.rechargeStock = 1;
				val33.requiredStock = 1;
				val33.skillName = text5 + "_PRIMARY";
				val33.skillNameToken = text4 + "'s Melee Attack";
				val33.icon = icon4;
			}
			LoadoutAPI.AddSkill(typeof(RandomSecondary));
			SkillDef val34 = ScriptableObject.CreateInstance<SkillDef>();
			val34.activationState = new SerializableEntityStateType(typeof(RandomSecondary));
			val34.activationStateMachineName = "CustomWeapon";
			val34.baseMaxStock = Random.Range(1, 4);
			val34.baseRechargeInterval = Random.Range(1f, 8f);
			val34.beginSkillCooldownOnSkillEnd = true;
			val34.canceledFromSprinting = false;
			val34.fullRestockOnAssign = true;
			val34.interruptPriority = (InterruptPriority)1;
			val34.isCombatSkill = true;
			val34.mustKeyPress = false;
			val34.cancelSprintingOnActivation = true;
			val34.rechargeStock = 1;
			val34.requiredStock = 1;
			val34.stockToConsume = 1;
			val34.skillDescriptionToken = "Holds <style=cIsDamage>" + val34.baseMaxStock + " charge(s)</style>, and takes <style=cIsDamage>" + Mathf.RoundToInt(val34.baseRechargeInterval) + " seconds</style> to recharge.";
			val34.skillName = text5 + "_SECONDARY";
			val34.skillNameToken = text4 + "'s Secondary Attack";
			val34.icon = icon5;
			Type[] array7 = (val2.GetComponent<RandomManager>().isFlier ? new Type[9]
			{
				typeof(CastSmokescreenNoDelay),
				typeof(ThrowMineDeployer),
				typeof(ChargeSonicBoom),
				typeof(FireEyeBlast),
				typeof(FireDelayKnockup),
				typeof(FireMegaTurret),
				typeof(FireMissileBarrage),
				typeof(FireTwinRocket),
				typeof(FirePortalBomb)
			} : new Type[25]
			{
				typeof(BlinkState),
				typeof(MiniBlinkState),
				typeof(Charge),
				typeof(ChainableLeap),
				typeof(Leap),
				typeof(BlinkState),
				typeof(PrepWall),
				typeof(ToolbotDash),
				typeof(EvisDash),
				typeof(DodgeState),
				typeof(ChargePlantSonicBoom),
				typeof(IceNova),
				typeof(RUtilEscape),
				typeof(CastSmokescreenNoDelay),
				typeof(ThrowMineDeployer),
				typeof(ChargeSonicBoom),
				typeof(FireEyeBlast),
				typeof(FireDelayKnockup),
				typeof(LoomingPresence),
				typeof(SlideState),
				typeof(FirePortalBomb),
				typeof(DashSlam),
				typeof(FireSonicPull),
				typeof(ThrowSmokebomb),
				typeof(SetupAirstrike)
			});
			Type[] array8 = (val2.GetComponent<RandomManager>().isFlier ? new Type[26]
			{
				typeof(FireSpit),
				typeof(ChargeMegaFireball),
				typeof(FireBarrage),
				typeof(FireShotgunBlast),
				typeof(PlaceTurret),
				typeof(PlaceWalkerTurret),
				typeof(FireFlower2),
				typeof(FireSuperEyeblast),
				typeof(FireSuperDelayKnockup),
				typeof(FireGoldFist),
				typeof(ThrowPylon),
				typeof(AimMortar),
				typeof(AimMortar2),
				typeof(AimMortarRain),
				typeof(FireEnergyCannon),
				typeof(ThrowSack),
				typeof(ThrowEvisProjectile),
				typeof(FireBubbleShield),
				typeof(RSpecialAddBuff),
				typeof(FireArrowSnipe),
				typeof(ArrowRain),
				typeof(ChargeMeteor),
				typeof(FireMegaTurret),
				typeof(FireMissileBarrage),
				typeof(FireTwinRocket),
				typeof(SporeGrenade)
			} : new Type[25]
			{
				typeof(FireSpit),
				typeof(ChargeMegaFireball),
				typeof(FireBarrage),
				typeof(FireShotgunBlast),
				typeof(PlaceTurret),
				typeof(PlaceWalkerTurret),
				typeof(FireSuperEyeblast),
				typeof(FireSuperDelayKnockup),
				typeof(FireGoldFist),
				typeof(ThrowPylon),
				typeof(FireEnergyCannon),
				typeof(ThrowSack),
				typeof(ThrowEvisProjectile),
				typeof(FireBubbleShield),
				typeof(RSpecialAddBuff),
				typeof(FireArrowSnipe),
				typeof(ArrowRain),
				typeof(AimMortar2),
				typeof(SporeGrenade),
				typeof(RChargeMegaNova),
				typeof(SetupAirstrikeAlt),
				typeof(PreGroundSlam),
				typeof(Bandit2FireShiv),
				typeof(PrepSidearmResetRevolver),
				typeof(PrepSidearmSkullRevolver)
			});
			Type type = array7[Random.Range(0, array7.Length)];
			Type type2 = array8[Random.Range(0, array8.Length)];
			LoadoutAPI.AddSkill(typeof(RandomUtility));
			SkillDef val35 = ScriptableObject.CreateInstance<SkillDef>();
			val35.activationState = new SerializableEntityStateType(type);
			val35.activationStateMachineName = "CustomWeapon";
			val35.baseMaxStock = Random.Range(1, 4);
			val35.baseRechargeInterval = Random.Range(1f, 8f);
			val35.beginSkillCooldownOnSkillEnd = true;
			val35.canceledFromSprinting = false;
			val35.fullRestockOnAssign = true;
			val35.interruptPriority = (InterruptPriority)2;
			val35.isCombatSkill = false;
			val35.mustKeyPress = true;
			val35.cancelSprintingOnActivation = false;
			val35.rechargeStock = 1;
			val35.requiredStock = 1;
			val35.stockToConsume = 1;
			val35.skillDescriptionToken = "Holds <style=cIsDamage>" + val35.baseMaxStock + " charge(s)</style>, and takes <style=cIsDamage>" + Mathf.RoundToInt(val35.baseRechargeInterval) + " seconds</style> to recharge";
			val35.skillName = text5 + "_UTILITY";
			val35.skillNameToken = type.FullName;
			val35.icon = icon6;
			LoadoutAPI.AddSkill(typeof(RandomSpecial));
			SkillDef val36 = ScriptableObject.CreateInstance<SkillDef>();
			val36.activationState = new SerializableEntityStateType(type2);
			val36.activationStateMachineName = "CustomWeapon";
			val36.baseMaxStock = 1;
			val36.baseRechargeInterval = Random.Range(4f, 25f);
			val36.beginSkillCooldownOnSkillEnd = true;
			val36.canceledFromSprinting = false;
			val36.fullRestockOnAssign = true;
			val36.interruptPriority = (InterruptPriority)2;
			val36.isCombatSkill = true;
			val36.mustKeyPress = true;
			val36.cancelSprintingOnActivation = true;
			val36.rechargeStock = 1;
			val36.requiredStock = 1;
			val36.stockToConsume = 1;
			val36.skillDescriptionToken = "Takes <style=cIsDamage>" + Mathf.RoundToInt(val36.baseRechargeInterval) + " seconds</style> to recharge";
			val36.skillName = text5 + "_SPECIAL";
			val36.skillNameToken = type2.FullName;
			val36.icon = icon7;
			LoadoutAPI.AddSkillDef(val33);
			LoadoutAPI.AddSkillDef(val34);
			LoadoutAPI.AddSkillDef(val35);
			LoadoutAPI.AddSkillDef(val36);
			SkillFamily skillFamily = component4.primary.skillFamily;
			SkillFamily skillFamily2 = component4.secondary.skillFamily;
			SkillFamily skillFamily3 = component4.utility.skillFamily;
			SkillFamily skillFamily4 = component4.special.skillFamily;
			Variant[] variants = skillFamily.variants;
			Variant val37 = new Variant
			{
				skillDef = val33
			};
			((Variant)(ref val37)).viewableNode = new Node(val33.skillNameToken, false, (Node)null);
			variants[0] = val37;
			Variant[] variants2 = skillFamily2.variants;
			val37 = new Variant
			{
				skillDef = val34
			};
			((Variant)(ref val37)).viewableNode = new Node(val34.skillNameToken, false, (Node)null);
			variants2[0] = val37;
			Variant[] variants3 = skillFamily3.variants;
			val37 = new Variant
			{
				skillDef = val35
			};
			((Variant)(ref val37)).viewableNode = new Node(val35.skillNameToken, false, (Node)null);
			variants3[0] = val37;
			Variant[] variants4 = skillFamily4.variants;
			val37 = new Variant
			{
				skillDef = val36
			};
			((Variant)(ref val37)).viewableNode = new Node(val36.skillNameToken, false, (Node)null);
			variants4[0] = val37;
		}
		CharacterSelectBarController.Start += (hook_Start)delegate(orig_Start orig, CharacterSelectBarController self)
		{
			MonoBehaviour.print((object)("Random Survivors Seed: " + randomSeed));
			orig.Invoke(self);
		};
	}
}
