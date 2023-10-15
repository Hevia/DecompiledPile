using System.Linq;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.TitanMonster;

public class FireMegaLaser : BaseState
{
	[SerializeField]
	public GameObject effectPrefab;

	[SerializeField]
	public GameObject hitEffectPrefab;

	[SerializeField]
	public GameObject laserPrefab;

	public static string playAttackSoundString;

	public static string playLoopSoundString;

	public static string stopLoopSoundString;

	public static float damageCoefficient;

	public static float force;

	public static float minSpread;

	public static float maxSpread;

	public static int bulletCount;

	public static float fireFrequency;

	public static float maxDistance;

	public static float minimumDuration;

	public static float maximumDuration;

	public static float lockOnAngle;

	public static float procCoefficientPerTick;

	private HurtBox lockedOnHurtBox;

	private float fireStopwatch;

	private float stopwatch;

	private Ray aimRay;

	private Transform modelTransform;

	private GameObject laserEffect;

	private ChildLocator laserChildLocator;

	private Transform laserEffectEnd;

	protected Transform muzzleTransform;

	private BullseyeSearch enemyFinder;

	private bool foundAnyTarget;

	public override void OnEnter()
	{
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		base.characterBody.SetAimTimer(maximumDuration);
		Util.PlaySound(playAttackSoundString, base.gameObject);
		Util.PlaySound(playLoopSoundString, base.gameObject);
		PlayCrossfade("Gesture, Additive", "FireLaserLoop", 0.25f);
		enemyFinder = new BullseyeSearch();
		enemyFinder.viewer = base.characterBody;
		enemyFinder.maxDistanceFilter = maxDistance;
		enemyFinder.maxAngleFilter = lockOnAngle;
		enemyFinder.searchOrigin = aimRay.origin;
		enemyFinder.searchDirection = aimRay.direction;
		enemyFinder.filterByLoS = false;
		enemyFinder.sortMode = BullseyeSearch.SortMode.Angle;
		enemyFinder.teamMaskFilter = TeamMask.allButNeutral;
		if (Object.op_Implicit((Object)(object)base.teamComponent))
		{
			enemyFinder.teamMaskFilter.RemoveTeam(base.teamComponent.teamIndex);
		}
		aimRay = GetAimRay();
		modelTransform = GetModelTransform();
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			ChildLocator component = ((Component)modelTransform).GetComponent<ChildLocator>();
			if (Object.op_Implicit((Object)(object)component))
			{
				muzzleTransform = component.FindChild("MuzzleLaser");
				if (Object.op_Implicit((Object)(object)muzzleTransform) && Object.op_Implicit((Object)(object)laserPrefab))
				{
					laserEffect = Object.Instantiate<GameObject>(laserPrefab, muzzleTransform.position, muzzleTransform.rotation);
					laserEffect.transform.parent = muzzleTransform;
					laserChildLocator = laserEffect.GetComponent<ChildLocator>();
					laserEffectEnd = laserChildLocator.FindChild("LaserEnd");
				}
			}
		}
		UpdateLockOn();
	}

	public override void OnExit()
	{
		if (Object.op_Implicit((Object)(object)laserEffect))
		{
			EntityState.Destroy((Object)(object)laserEffect);
		}
		base.characterBody.SetAimTimer(2f);
		Util.PlaySound(stopLoopSoundString, base.gameObject);
		PlayCrossfade("Gesture, Additive", "FireLaserEnd", 0.25f);
		base.OnExit();
	}

	private void UpdateLockOn()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		if (base.isAuthority)
		{
			enemyFinder.searchOrigin = aimRay.origin;
			enemyFinder.searchDirection = aimRay.direction;
			enemyFinder.RefreshCandidates();
			foundAnyTarget = Object.op_Implicit((Object)(object)(lockedOnHurtBox = enemyFinder.GetResults().FirstOrDefault()));
		}
	}

	public override void FixedUpdate()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Unknown result type (might be due to invalid IL or missing references)
		//IL_023c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0241: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		fireStopwatch += Time.fixedDeltaTime;
		stopwatch += Time.fixedDeltaTime;
		aimRay = GetAimRay();
		Vector3 val = aimRay.origin;
		if (Object.op_Implicit((Object)(object)muzzleTransform))
		{
			val = muzzleTransform.position;
		}
		RaycastHit hitInfo;
		Vector3 val2 = (Object.op_Implicit((Object)(object)lockedOnHurtBox) ? ((Component)lockedOnHurtBox).transform.position : ((!Util.CharacterRaycast(base.gameObject, aimRay, out hitInfo, maxDistance, LayerMask.op_Implicit(LayerMask.op_Implicit(LayerIndex.world.mask) | LayerMask.op_Implicit(LayerIndex.entityPrecise.mask)), (QueryTriggerInteraction)1)) ? aimRay.GetPoint(maxDistance) : ((RaycastHit)(ref hitInfo)).point));
		Ray val3 = default(Ray);
		((Ray)(ref val3))._002Ector(val, val2 - val);
		bool flag = false;
		Vector3 val4;
		if (Object.op_Implicit((Object)(object)laserEffect) && Object.op_Implicit((Object)(object)laserChildLocator))
		{
			GameObject bodyObject = base.gameObject;
			Ray ray = val3;
			val4 = val2 - val;
			if (Util.CharacterRaycast(bodyObject, ray, out var hitInfo2, ((Vector3)(ref val4)).magnitude, LayerMask.op_Implicit(LayerMask.op_Implicit(LayerIndex.world.mask) | LayerMask.op_Implicit(LayerIndex.entityPrecise.mask)), (QueryTriggerInteraction)0))
			{
				val2 = ((RaycastHit)(ref hitInfo2)).point;
				if (Util.CharacterRaycast(base.gameObject, new Ray(val2 - ((Ray)(ref val3)).direction * 0.1f, -((Ray)(ref val3)).direction), out var _, ((RaycastHit)(ref hitInfo2)).distance, LayerMask.op_Implicit(LayerMask.op_Implicit(LayerIndex.world.mask) | LayerMask.op_Implicit(LayerIndex.entityPrecise.mask)), (QueryTriggerInteraction)0))
				{
					val2 = ((Ray)(ref val3)).GetPoint(0.1f);
					flag = true;
				}
			}
			laserEffect.transform.rotation = Util.QuaternionSafeLookRotation(val2 - val);
			((Component)laserEffectEnd).transform.position = val2;
		}
		if (fireStopwatch > 1f / fireFrequency)
		{
			string targetMuzzle = "MuzzleLaser";
			if (!flag)
			{
				Transform obj = modelTransform;
				Ray val5 = val3;
				val4 = val2 - ((Ray)(ref val3)).origin;
				FireBullet(obj, val5, targetMuzzle, ((Vector3)(ref val4)).magnitude + 0.1f);
			}
			UpdateLockOn();
			fireStopwatch -= 1f / fireFrequency;
		}
		if (base.isAuthority && (((!Object.op_Implicit((Object)(object)base.inputBank) || !base.inputBank.skill4.down) && stopwatch > minimumDuration) || stopwatch > maximumDuration))
		{
			outer.SetNextStateToMain();
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Skill;
	}

	private void FireBullet(Transform modelTransform, Ray aimRay, string targetMuzzle, float maxDistance)
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)effectPrefab))
		{
			EffectManager.SimpleMuzzleFlash(effectPrefab, base.gameObject, targetMuzzle, transmit: false);
		}
		if (base.isAuthority)
		{
			BulletAttack bulletAttack = new BulletAttack();
			bulletAttack.owner = base.gameObject;
			bulletAttack.weapon = base.gameObject;
			bulletAttack.origin = aimRay.origin;
			bulletAttack.aimVector = aimRay.direction;
			bulletAttack.minSpread = minSpread;
			bulletAttack.maxSpread = maxSpread;
			bulletAttack.bulletCount = 1u;
			bulletAttack.damage = damageCoefficient * damageStat / fireFrequency;
			bulletAttack.force = force;
			bulletAttack.muzzleName = targetMuzzle;
			bulletAttack.hitEffectPrefab = hitEffectPrefab;
			bulletAttack.isCrit = Util.CheckRoll(critStat, base.characterBody.master);
			bulletAttack.procCoefficient = procCoefficientPerTick;
			bulletAttack.HitEffectNormal = false;
			bulletAttack.radius = 0f;
			bulletAttack.maxDistance = maxDistance;
			bulletAttack.Fire();
		}
	}

	public override void OnSerialize(NetworkWriter writer)
	{
		base.OnSerialize(writer);
		writer.Write(HurtBoxReference.FromHurtBox(lockedOnHurtBox));
		writer.Write(stopwatch);
	}

	public override void OnDeserialize(NetworkReader reader)
	{
		base.OnDeserialize(reader);
		HurtBoxReference hurtBoxReference = reader.ReadHurtBoxReference();
		stopwatch = reader.ReadSingle();
		GameObject obj = hurtBoxReference.ResolveGameObject();
		lockedOnHurtBox = ((obj != null) ? obj.GetComponent<HurtBox>() : null);
	}
}
