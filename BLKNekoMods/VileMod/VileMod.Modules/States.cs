using VileMod.SkillStates;
using VileMod.SkillStates.BaseStates;

namespace VileMod.Modules;

public static class States
{
	internal static void RegisterStates()
	{
		Content.AddEntityState(typeof(BaseMeleeAttack));
		Content.AddEntityState(typeof(CherryBlast));
		Content.AddEntityState(typeof(BumpityBoom));
		Content.AddEntityState(typeof(BumpityBoom2));
		Content.AddEntityState(typeof(FrontRunner));
		Content.AddEntityState(typeof(CerberusPhantom));
		Content.AddEntityState(typeof(BurningDrive));
		Content.AddEntityState(typeof(EletricSpark));
		Content.AddEntityState(typeof(ShotgunIce));
		Content.AddEntityState(typeof(Fury));
		Content.AddEntityState(typeof(DeathState));
	}
}
