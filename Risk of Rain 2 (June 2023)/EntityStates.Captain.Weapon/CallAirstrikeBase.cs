using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace EntityStates.Captain.Weapon;

public class CallAirstrikeBase : AimThrowableBase
{
	[SerializeField]
	public float airstrikeRadius;

	[SerializeField]
	public float bloom;

	public static GameObject muzzleFlashEffect;

	public static string muzzleString;

	public static string fireAirstrikeSoundString;

	public override void OnEnter()
	{
		base.OnEnter();
		base.characterBody.SetSpreadBloom(bloom);
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		base.characterBody.SetAimTimer(4f);
	}

	public override void OnExit()
	{
		Util.PlaySound(fireAirstrikeSoundString, base.gameObject);
		base.OnExit();
	}

	protected override void ModifyProjectile(ref FireProjectileInfo fireProjectileInfo)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		base.ModifyProjectile(ref fireProjectileInfo);
		fireProjectileInfo.position = currentTrajectoryInfo.hitPoint;
		fireProjectileInfo.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
		fireProjectileInfo.speedOverride = 0f;
	}

	protected override bool KeyIsDown()
	{
		return base.inputBank.skill1.down;
	}

	protected override EntityState PickNextState()
	{
		return new Idle();
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.PrioritySkill;
	}
}
