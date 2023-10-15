using System.Collections.Generic;
using System.Linq;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.RoboBallBoss.Weapon;

public class FireDelayKnockup : BaseState
{
	[SerializeField]
	public int knockupCount;

	[SerializeField]
	public float randomPositionRadius;

	public static float baseDuration;

	public static GameObject projectilePrefab;

	public static GameObject muzzleEffectPrefab;

	public static float maxDistance;

	private float duration;

	public override void OnEnter()
	{
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		PlayCrossfade("Gesture, Additive", "FireDelayKnockup", 0.1f);
		if (Object.op_Implicit((Object)(object)muzzleEffectPrefab))
		{
			EffectManager.SimpleMuzzleFlash(muzzleEffectPrefab, base.gameObject, "EyeballMuzzle1", transmit: false);
			EffectManager.SimpleMuzzleFlash(muzzleEffectPrefab, base.gameObject, "EyeballMuzzle2", transmit: false);
			EffectManager.SimpleMuzzleFlash(muzzleEffectPrefab, base.gameObject, "EyeballMuzzle3", transmit: false);
		}
		if (!NetworkServer.active)
		{
			return;
		}
		BullseyeSearch bullseyeSearch = new BullseyeSearch();
		bullseyeSearch.teamMaskFilter = TeamMask.allButNeutral;
		if (Object.op_Implicit((Object)(object)base.teamComponent))
		{
			bullseyeSearch.teamMaskFilter.RemoveTeam(base.teamComponent.teamIndex);
		}
		bullseyeSearch.maxDistanceFilter = maxDistance;
		bullseyeSearch.maxAngleFilter = 360f;
		Ray aimRay = GetAimRay();
		bullseyeSearch.searchOrigin = aimRay.origin;
		bullseyeSearch.searchDirection = aimRay.direction;
		bullseyeSearch.filterByLoS = false;
		bullseyeSearch.sortMode = BullseyeSearch.SortMode.Angle;
		bullseyeSearch.RefreshCandidates();
		List<HurtBox> list = bullseyeSearch.GetResults().ToList();
		int num = 0;
		RaycastHit val3 = default(RaycastHit);
		for (int i = 0; i < knockupCount; i++)
		{
			if (num >= list.Count)
			{
				num = 0;
			}
			HurtBox hurtBox = list[num];
			if (Object.op_Implicit((Object)(object)hurtBox))
			{
				Vector2 val = Random.insideUnitCircle * randomPositionRadius;
				Vector3 val2 = ((Component)hurtBox).transform.position + new Vector3(val.x, 0f, val.y);
				if (Physics.Raycast(new Ray(val2 + Vector3.up * 1f, Vector3.down), ref val3, 200f, LayerMask.op_Implicit(LayerIndex.world.mask), (QueryTriggerInteraction)1))
				{
					val2 = ((RaycastHit)(ref val3)).point;
				}
				ProjectileManager.instance.FireProjectile(projectilePrefab, val2, Quaternion.identity, base.gameObject, damageStat, 0f, Util.CheckRoll(critStat, base.characterBody.master));
			}
			num++;
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.fixedAge >= duration)
		{
			outer.SetNextStateToMain();
		}
	}
}
