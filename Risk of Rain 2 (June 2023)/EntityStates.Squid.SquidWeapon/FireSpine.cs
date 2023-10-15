using System.Linq;
using RoR2;
using RoR2.Orbs;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Squid.SquidWeapon;

public class FireSpine : BaseState
{
	public static GameObject hitEffectPrefab;

	public static GameObject muzzleflashEffectPrefab;

	public static float damageCoefficient;

	public static float baseDuration = 2f;

	public static float procCoefficient = 1f;

	public static float forceScalar = 1f;

	private bool hasFiredArrow;

	private ChildLocator childLocator;

	private BullseyeSearch enemyFinder;

	private const float maxVisionDistance = float.PositiveInfinity;

	public bool fullVision = true;

	private float duration;

	public override void OnEnter()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		GetAimRay();
		PlayAnimation("Gesture", "FireGoo");
		if (base.isAuthority)
		{
			FireOrbArrow();
		}
	}

	private void FireOrbArrow()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		//IL_0227: Unknown result type (might be due to invalid IL or missing references)
		if (hasFiredArrow || !NetworkServer.active)
		{
			return;
		}
		Ray aimRay = GetAimRay();
		enemyFinder = new BullseyeSearch();
		enemyFinder.viewer = base.characterBody;
		enemyFinder.maxDistanceFilter = float.PositiveInfinity;
		enemyFinder.searchOrigin = aimRay.origin;
		enemyFinder.searchDirection = aimRay.direction;
		enemyFinder.sortMode = BullseyeSearch.SortMode.Distance;
		enemyFinder.teamMaskFilter = TeamMask.allButNeutral;
		enemyFinder.minDistanceFilter = 0f;
		enemyFinder.maxAngleFilter = (fullVision ? 180f : 90f);
		enemyFinder.filterByLoS = true;
		if (Object.op_Implicit((Object)(object)base.teamComponent))
		{
			enemyFinder.teamMaskFilter.RemoveTeam(base.teamComponent.teamIndex);
		}
		enemyFinder.RefreshCandidates();
		HurtBox hurtBox = enemyFinder.GetResults().FirstOrDefault();
		if (Object.op_Implicit((Object)(object)hurtBox))
		{
			Vector3 position = ((Component)hurtBox).transform.position;
			Ray aimRay2 = GetAimRay();
			Vector3 val = position - ((Ray)(ref aimRay2)).origin;
			aimRay2 = GetAimRay();
			aimRay.origin = ((Ray)(ref aimRay2)).origin;
			aimRay.direction = val;
			base.inputBank.aimDirection = val;
			StartAimMode(aimRay);
			hasFiredArrow = true;
			SquidOrb squidOrb = new SquidOrb();
			squidOrb.forceScalar = forceScalar;
			squidOrb.damageValue = base.characterBody.damage * damageCoefficient;
			squidOrb.isCrit = Util.CheckRoll(base.characterBody.crit, base.characterBody.master);
			squidOrb.teamIndex = TeamComponent.GetObjectTeam(base.gameObject);
			squidOrb.attacker = base.gameObject;
			squidOrb.procCoefficient = procCoefficient;
			HurtBox hurtBox2 = hurtBox;
			if (Object.op_Implicit((Object)(object)hurtBox2))
			{
				Transform val2 = ((Component)base.characterBody.modelLocator.modelTransform).GetComponent<ChildLocator>().FindChild("Muzzle");
				EffectManager.SimpleMuzzleFlash(muzzleflashEffectPrefab, base.gameObject, "Muzzle", transmit: true);
				squidOrb.origin = val2.position;
				squidOrb.target = hurtBox2;
				OrbManager.instance.AddOrb(squidOrb);
			}
		}
	}

	public override void OnExit()
	{
		base.OnExit();
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.fixedAge >= duration && base.isAuthority)
		{
			outer.SetNextStateToMain();
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Skill;
	}
}
