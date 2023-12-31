using System;

namespace RoR2;

[Flags]
public enum DamageType : uint
{
	Generic = 0u,
	NonLethal = 1u,
	BypassArmor = 2u,
	ResetCooldownsOnKill = 4u,
	SlowOnHit = 8u,
	WeakPointHit = 0x10u,
	Stun1s = 0x20u,
	BypassBlock = 0x40u,
	IgniteOnHit = 0x80u,
	Freeze2s = 0x100u,
	ClayGoo = 0x200u,
	BleedOnHit = 0x400u,
	Silent = 0x800u,
	PoisonOnHit = 0x1000u,
	PercentIgniteOnHit = 0x2000u,
	WeakOnHit = 0x4000u,
	Nullify = 0x8000u,
	VoidDeath = 0x10000u,
	AOE = 0x20000u,
	BypassOneShotProtection = 0x40000u,
	BonusToLowHealth = 0x80000u,
	BlightOnHit = 0x100000u,
	FallDamage = 0x200000u,
	CrippleOnHit = 0x400000u,
	ApplyMercExpose = 0x800000u,
	Shock5s = 0x1000000u,
	LunarSecondaryRootOnHit = 0x2000000u,
	DoT = 0x4000000u,
	SuperBleedOnCrit = 0x8000000u,
	GiveSkullOnKill = 0x10000000u,
	FruitOnHit = 0x20000000u,
	OutOfBounds = 0x40000000u
}
