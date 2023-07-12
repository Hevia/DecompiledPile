using RoR2.Projectile;
using UnityEngine;

namespace EntityStates.Captain.Weapon;

public class CallAirstrikeAlt : CallAirstrikeBase
{
	[SerializeField]
	public float projectileForce;

	public override void OnExit()
	{
		PlayAnimation("Gesture, Override", "CallAirstrike3");
		PlayAnimation("Gesture, Additive", "CallAirstrike3");
		AddRecoil(-2f, -2f, -0.5f, 0.5f);
		base.OnExit();
	}

	protected override void ModifyProjectile(ref FireProjectileInfo fireProjectileInfo)
	{
		base.ModifyProjectile(ref fireProjectileInfo);
		fireProjectileInfo.force = projectileForce;
	}
}
