using System;
using R2API;

namespace VileMod.Modules;

internal static class Tokens
{
	internal static void AddTokens()
	{
		string text = "BLKNeko_VILEV3_BODY_";
		string text2 = "Vile, the EX-Maverick Hunter.<color=#CCD3E0>" + Environment.NewLine + Environment.NewLine;
		text2 = text2 + "< ! > Vile's Cherry Blast has a low start so use it after any skill for a momentary buff and faster start" + Environment.NewLine + Environment.NewLine;
		text2 = text2 + "< ! > Vile is very powerfull, but is pretty slow" + Environment.NewLine + Environment.NewLine;
		text2 = text2 + "< ! > When activated, Vile's Fury give him a life steal buff for 6 seconds, so try to cause the maximum damage possible" + Environment.NewLine + Environment.NewLine;
		text2 = text2 + "< ! > Vile's tough body make him immune to fall damage" + Environment.NewLine + Environment.NewLine;
		string text3 = "Back to the Hunter Base.";
		string text4 = "I...Failed....";
		LanguageAPI.Add(text + "NAME", "Vile");
		LanguageAPI.Add(text + "DESCRIPTION", text2);
		LanguageAPI.Add(text + "SUBTITLE", "Vile, EX-Maverick Hunter");
		LanguageAPI.Add(text + "LORE", "EX-Maverick Hunter");
		LanguageAPI.Add(text + "OUTRO_FLAVOR", text3);
		LanguageAPI.Add(text + "OUTRO_FAILURE", text4);
		LanguageAPI.Add(text + "DEFAULT_SKIN_NAME", "Default");
		LanguageAPI.Add(text + "MASTERY_SKIN_NAME", "Alternate");
		LanguageAPI.Add(text + "PASSIVE_NAME", "Rage");
		LanguageAPI.Add(text + "PASSIVE_DESCRIPTION", "<style=cIsUtility>Vile won't give up that easily from a fight, moved by his anger he get stronger in a critical state.</style> <style=cIsHealing>When in low health vile gain 10 seconds of buffs</style>.");
		LanguageAPI.Add(text + "CHERRYBLAST_NAME", "CherryBlast");
		LanguageAPI.Add(text + "CHERRYBLAST_DESCRIPTION", "Vile's gatling can fire super fast bullets after completely heated, dealing <style=cIsDamage>25% damage</style>.");
		LanguageAPI.Add(text + "TRIPLE7_NAME", "Triple 7");
		LanguageAPI.Add(text + "TRIPLE7_DESCRIPTION", "Vile's gatling can fire fast bullets after completely heated, this mode is innacurated but deals <style=cIsDamage>50% damage</style>.");
		LanguageAPI.Add(text + "BUMPITYBOOM_NAME", "BumpityBoom");
		LanguageAPI.Add(text + "BUMPITYBOOM_DESCRIPTION", "Vile throws two granades, dealing <style=cIsDamage>250% damage</style>.");
		LanguageAPI.Add(text + "FRONTRUNNER_NAME", "Front Runner");
		LanguageAPI.Add(text + "FRONTRUNNER_DESCRIPTION", "A cannon shot that explodes on impact, dealing <style=cIsDamage>300% damage</style>.");
		LanguageAPI.Add(text + "NAPALMBOMB_NAME", "Napalm Bomb");
		LanguageAPI.Add(text + "NAPALMBOMB_DESCRIPTION", "Vile throws a granade that explodes on impact, dealing <style=cIsDamage>500% damage</style> and spreading smaller granades to cause even more damage.");
		LanguageAPI.Add(text + "ELETRICSPARK_NAME", "Electric Shock Round");
		LanguageAPI.Add(text + "ELETRICSPARK_DESCRIPTION", "Fire an eletric bomb, dealing <style=cIsDamage>1000% damage</style> and paralize enemies for 5s.");
		LanguageAPI.Add(text + "SHOTGUNICE_NAME", "Shotgun Ice");
		LanguageAPI.Add(text + "SHOTGUNICE_DESCRIPTION", "A powerful ice shot that cause <style=cIsDamage>400% damage</style> and freezing the enemies.");
		LanguageAPI.Add(text + "BURNINGDRIVE_NAME", "Burning Drive");
		LanguageAPI.Add(text + "BURNINGDRIVE_DESCRIPTION", "Create a powerful ball of flame using nearby oxygen as fuel, dealing <style=cIsDamage>1000% damage</style>.");
		LanguageAPI.Add(text + "CERBERUSPHANTOM_NAME", "Cerberus Phantom");
		LanguageAPI.Add(text + "CERBERUSPHANTOM_DESCRIPTION", "Shoot a spread of 3 lasers, dealing <style=cIsDamage>250% damage</style>.");
		LanguageAPI.Add(text + "MASTERYUNLOCKABLE_ACHIEVEMENT_NAME", "VileV2: Mastery");
		LanguageAPI.Add(text + "MASTERYUNLOCKABLE_ACHIEVEMENT_DESC", "As VileV2, beat the game or obliterate on Monsoon.");
		LanguageAPI.Add(text + "MASTERYUNLOCKABLE_UNLOCKABLE_NAME", "VileV2: Mastery");
	}
}
