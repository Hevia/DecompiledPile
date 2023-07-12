using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using UnityEngine;

namespace EntityStates.VoidRaidCrab.Weapon;

public class FireMissiles : BaseState
{
	[SerializeField]
	public float baseInitialDelay;

	[SerializeField]
	public float baseDelayBetweenWaves;

	[SerializeField]
	public float baseEndDelay;

	[SerializeField]
	public int numWaves;

	[SerializeField]
	public int numMissilesPerWave;

	[SerializeField]
	public string muzzleName;

	[SerializeField]
	public GameObject muzzleFlashPrefab;

	[SerializeField]
	public GameObject projectilePrefab;

	[SerializeField]
	public float damageCoefficient;

	[SerializeField]
	public float force;

	[SerializeField]
	public float minSpreadDegrees;

	[SerializeField]
	public float rangeSpreadDegrees;

	[SerializeField]
	public string fireWaveSoundString;

	[SerializeField]
	public bool isSoundScaledByAttackSpeed;

	[SerializeField]
	public string animationLayerName;

	[SerializeField]
	public string animationStateName;

	[SerializeField]
	public string animationPlaybackRateParam;

	[SerializeField]
	public SkillDef skillDefToReplaceAtStocksEmpty;

	[SerializeField]
	public SkillDef nextSkillDef;

	private float delayBetweenWaves;

	private float duration;

	private int numWavesFired;

	private float timeUntilNextWave;

	private Transform muzzleTransform;

	public override void OnEnter()
	{
		base.OnEnter();
		if (Object.op_Implicit((Object)(object)nextSkillDef))
		{
			GenericSkill genericSkill = base.skillLocator.FindSkillByDef(skillDefToReplaceAtStocksEmpty);
			if (Object.op_Implicit((Object)(object)genericSkill) && genericSkill.stock == 0)
			{
				genericSkill.SetBaseSkill(nextSkillDef);
			}
		}
		float num = baseInitialDelay + Mathf.Max(0f, baseDelayBetweenWaves * (float)(numWaves - 1)) + baseEndDelay;
		duration = num / attackSpeedStat;
		timeUntilNextWave = baseInitialDelay / attackSpeedStat;
		delayBetweenWaves = baseDelayBetweenWaves / attackSpeedStat;
		muzzleTransform = FindModelChild(muzzleName);
	}

	public override void FixedUpdate()
	{
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		timeUntilNextWave -= Time.fixedDeltaTime;
		while (timeUntilNextWave < 0f && numWavesFired < numWaves)
		{
			PlayAnimation(animationLayerName, animationStateName, animationPlaybackRateParam, duration);
			timeUntilNextWave += delayBetweenWaves;
			numWavesFired++;
			EffectManager.SimpleMuzzleFlash(muzzleFlashPrefab, base.gameObject, muzzleName, transmit: false);
			if (base.isAuthority)
			{
				Ray aimRay = GetAimRay();
				Quaternion val = Util.QuaternionSafeLookRotation(((Ray)(ref aimRay)).direction);
				FireProjectileInfo fireProjectileInfo = default(FireProjectileInfo);
				fireProjectileInfo.projectilePrefab = projectilePrefab;
				fireProjectileInfo.position = muzzleTransform.position;
				fireProjectileInfo.owner = base.gameObject;
				fireProjectileInfo.damage = damageStat * damageCoefficient;
				fireProjectileInfo.force = force;
				FireProjectileInfo fireProjectileInfo2 = fireProjectileInfo;
				for (int i = 0; i < numMissilesPerWave; i++)
				{
					fireProjectileInfo2.rotation = val * GetRandomRollPitch();
					fireProjectileInfo2.crit = Util.CheckRoll(critStat, base.characterBody.master);
					ProjectileManager.instance.FireProjectile(fireProjectileInfo2);
				}
			}
		}
		if (base.isAuthority && base.fixedAge >= duration)
		{
			outer.SetNextStateToMain();
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.PrioritySkill;
	}

	protected Quaternion GetRandomRollPitch()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		Quaternion val = Quaternion.AngleAxis((float)Random.Range(0, 360), Vector3.forward);
		Quaternion val2 = Quaternion.AngleAxis(minSpreadDegrees + Random.Range(0f, rangeSpreadDegrees), Vector3.left);
		return val * val2;
	}
}
