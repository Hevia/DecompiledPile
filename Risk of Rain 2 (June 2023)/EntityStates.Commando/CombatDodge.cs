using System.Collections.Generic;
using System.Linq;
using RoR2;
using UnityEngine;

namespace EntityStates.Commando;

public class CombatDodge : DodgeState
{
	public static float durationToFire;

	public static int bulletCount;

	public static GameObject muzzleEffectPrefab;

	public static GameObject tracerEffectPrefab;

	public static GameObject hitEffectPrefab;

	public static float damageCoefficient;

	public static float force;

	public static string firePistolSoundString;

	public static float recoilAmplitude = 1f;

	public static float range;

	private int bulletsFired;

	private BullseyeSearch search;

	public override void OnEnter()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		search = new BullseyeSearch();
		search.searchDirection = Vector3.zero;
		search.teamMaskFilter = TeamMask.allButNeutral;
		search.teamMaskFilter.RemoveTeam(base.characterBody.teamComponent.teamIndex);
		search.filterByLoS = true;
		search.sortMode = BullseyeSearch.SortMode.Distance;
		search.maxDistanceFilter = range;
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		float num = base.fixedAge / durationToFire;
		if (bulletsFired < bulletCount && num > (float)bulletsFired / (float)bulletCount)
		{
			if (bulletsFired % 2 == 0)
			{
				PlayAnimation("Gesture Additive, Left", "FirePistol, Left");
				FireBullet("MuzzleLeft");
			}
			else
			{
				PlayAnimation("Gesture Additive, Right", "FirePistol, Right");
				FireBullet("MuzzleRight");
			}
		}
	}

	private HurtBox PickNextTarget()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		BullseyeSearch bullseyeSearch = search;
		Ray aimRay = GetAimRay();
		bullseyeSearch.searchOrigin = aimRay.origin;
		search.RefreshCandidates();
		List<HurtBox> list = search.GetResults().ToList();
		if (list.Count <= 0)
		{
			return null;
		}
		return list[Random.Range(0, list.Count)];
	}

	private void FireBullet(string targetMuzzle)
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		bulletsFired++;
		AddRecoil(-0.4f * recoilAmplitude, -0.8f * recoilAmplitude, -0.3f * recoilAmplitude, 0.3f * recoilAmplitude);
		if (base.isAuthority)
		{
			Ray aimRay = GetAimRay();
			aimRay.direction = Random.onUnitSphere;
			HurtBox hurtBox = PickNextTarget();
			if (Object.op_Implicit((Object)(object)hurtBox))
			{
				aimRay.direction = ((Component)hurtBox).transform.position - aimRay.origin;
			}
			Util.PlaySound(firePistolSoundString, base.gameObject);
			if (Object.op_Implicit((Object)(object)muzzleEffectPrefab))
			{
				EffectManager.SimpleMuzzleFlash(muzzleEffectPrefab, base.gameObject, targetMuzzle, transmit: false);
			}
			BulletAttack bulletAttack = new BulletAttack();
			bulletAttack.owner = base.gameObject;
			bulletAttack.weapon = base.gameObject;
			bulletAttack.origin = aimRay.origin;
			bulletAttack.aimVector = aimRay.direction;
			bulletAttack.minSpread = 0f;
			bulletAttack.maxSpread = base.characterBody.spreadBloomAngle;
			bulletAttack.damage = damageCoefficient * damageStat;
			bulletAttack.force = force;
			bulletAttack.tracerEffectPrefab = tracerEffectPrefab;
			bulletAttack.muzzleName = targetMuzzle;
			bulletAttack.hitEffectPrefab = hitEffectPrefab;
			bulletAttack.isCrit = Util.CheckRoll(critStat, base.characterBody.master);
			bulletAttack.radius = 0.1f;
			bulletAttack.smartCollision = true;
			bulletAttack.Fire();
		}
	}
}
