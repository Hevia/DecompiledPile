using System.Linq;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace EntityStates.HermitCrab;

public class FireMortar : BaseState
{
	public static GameObject mortarProjectilePrefab;

	public static GameObject mortarMuzzleflashEffect;

	public static int mortarCount;

	public static string mortarMuzzleName;

	public static string mortarSoundString;

	public static float mortarDamageCoefficient;

	public static float baseDuration;

	public static float timeToTarget = 3f;

	public static float projectileVelocity = 55f;

	public static float minimumDistance;

	private float stopwatch;

	private float duration;

	public override void OnEnter()
	{
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		PlayCrossfade("Gesture, Additive", "FireMortar", 0f);
		Util.PlaySound(mortarSoundString, base.gameObject);
		EffectManager.SimpleMuzzleFlash(mortarMuzzleflashEffect, base.gameObject, mortarMuzzleName, transmit: false);
		if (base.isAuthority)
		{
			Fire();
		}
	}

	private void Fire()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_018b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		Ray aimRay = GetAimRay();
		Ray val = default(Ray);
		((Ray)(ref val))._002Ector(((Ray)(ref aimRay)).origin, Vector3.up);
		BullseyeSearch bullseyeSearch = new BullseyeSearch();
		bullseyeSearch.searchOrigin = ((Ray)(ref aimRay)).origin;
		bullseyeSearch.searchDirection = ((Ray)(ref aimRay)).direction;
		bullseyeSearch.filterByLoS = false;
		bullseyeSearch.teamMaskFilter = TeamMask.allButNeutral;
		if (Object.op_Implicit((Object)(object)base.teamComponent))
		{
			bullseyeSearch.teamMaskFilter.RemoveTeam(base.teamComponent.teamIndex);
		}
		bullseyeSearch.sortMode = BullseyeSearch.SortMode.Angle;
		bullseyeSearch.RefreshCandidates();
		HurtBox hurtBox = bullseyeSearch.GetResults().FirstOrDefault();
		bool flag = false;
		Vector3 val2 = Vector3.zero;
		RaycastHit val3 = default(RaycastHit);
		if (Object.op_Implicit((Object)(object)hurtBox))
		{
			val2 = ((Component)hurtBox).transform.position;
			flag = true;
		}
		else if (Physics.Raycast(aimRay, ref val3, 1000f, LayerMask.op_Implicit(LayerIndex.world.mask) | LayerMask.op_Implicit(LayerIndex.entityPrecise.mask), (QueryTriggerInteraction)1))
		{
			val2 = ((RaycastHit)(ref val3)).point;
			flag = true;
		}
		float magnitude = projectileVelocity;
		if (flag)
		{
			Vector3 val4 = val2 - ((Ray)(ref val)).origin;
			Vector2 val5 = default(Vector2);
			((Vector2)(ref val5))._002Ector(val4.x, val4.z);
			float magnitude2 = ((Vector2)(ref val5)).magnitude;
			Vector2 val6 = val5 / magnitude2;
			if (magnitude2 < minimumDistance)
			{
				magnitude2 = minimumDistance;
			}
			float num = Trajectory.CalculateInitialYSpeed(timeToTarget, val4.y);
			float num2 = magnitude2 / timeToTarget;
			Vector3 direction = default(Vector3);
			((Vector3)(ref direction))._002Ector(val6.x * num2, num, val6.y * num2);
			magnitude = ((Vector3)(ref direction)).magnitude;
			((Ray)(ref val)).direction = direction;
		}
		for (int i = 0; i < mortarCount; i++)
		{
			Quaternion rotation = Util.QuaternionSafeLookRotation(((Ray)(ref val)).direction + ((i != 0) ? (Random.insideUnitSphere * 0.05f) : Vector3.zero));
			ProjectileManager.instance.FireProjectile(mortarProjectilePrefab, ((Ray)(ref val)).origin, rotation, base.gameObject, damageStat * mortarDamageCoefficient, 0f, Util.CheckRoll(critStat, base.characterBody.master), DamageColorIndex.Default, null, magnitude);
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		stopwatch += Time.fixedDeltaTime;
		if (stopwatch > duration)
		{
			Burrowed burrowed = new Burrowed();
			burrowed.duration = Burrowed.mortarCooldown;
			outer.SetNextState(burrowed);
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Skill;
	}
}
