using BepInEx;
using On.RoR2;
using R2API.Utils;
using RoR2;
using UnityEngine;
using VileMod.Modules;
using VileMod.Modules.Survivors;

namespace VileMod;

[BepInDependency(/*Could not decode attribute arguments.*/)]
[NetworkCompatibility(/*Could not decode attribute arguments.*/)]
[BepInPlugin("com.BLKNeko.VileModV3", "VileModV3", "3.0.0")]
[R2APISubmoduleDependency(new string[] { "PrefabAPI", "LanguageAPI", "SoundAPI", "UnlockableAPI" })]
public class VilePlugin : BaseUnityPlugin
{
	public const string MODUID = "com.BLKNeko.VileModV3";

	public const string MODNAME = "VileModV3";

	public const string MODVERSION = "3.0.0";

	public const string DEVELOPER_PREFIX = "BLKNeko";

	public static VilePlugin instance;

	private void Awake()
	{
		instance = this;
		Log.Init(((BaseUnityPlugin)this).Logger);
		Assets.Initialize();
		Config.ReadConfig();
		States.RegisterStates();
		Buffs.RegisterBuffs();
		Projectiles.RegisterProjectiles();
		Tokens.AddTokens();
		ItemDisplays.PopulateDisplays();
		new MyCharacter().Initialize();
		new ContentPacks().Initialize();
		Hook();
	}

	private void Hook()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected O, but got Unknown
		CharacterBody.RecalculateStats += new hook_RecalculateStats(CharacterBody_RecalculateStats);
	}

	private void CharacterBody_RecalculateStats(orig_RecalculateStats orig, CharacterBody self)
	{
		orig.Invoke(self);
		if (Object.op_Implicit((Object)(object)self) && self.HasBuff(Buffs.armorBuff))
		{
			self.armor += 300f;
		}
		if (Object.op_Implicit((Object)(object)self) && self.HasBuff(Buffs.VileFuryBuff))
		{
			self.moveSpeed *= 1.3f;
			self.attackSpeed *= 1.25f;
			self.damage *= 1.4f;
			self.regen *= 1.4f;
		}
	}
}
