using UnityEngine;

namespace RoR2.Orbs;

public class HuntressArrowOrb : GenericDamageOrb
{
	public override void Begin()
	{
		speed = 120f;
		base.Begin();
	}

	protected override GameObject GetOrbEffect()
	{
		return LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/OrbEffects/ArrowOrbEffect");
	}
}
