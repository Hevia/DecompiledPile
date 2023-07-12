using RoR2;
using UnityEngine;

namespace EntityStates.Mage.Weapon;

public class ChargeMeteor : BaseState
{
	public static float baseChargeDuration;

	public static float baseDuration;

	public static GameObject areaIndicatorPrefab;

	public static float minMeteorRadius = 0f;

	public static float maxMeteorRadius = 10f;

	public static GameObject meteorEffect;

	public static float minDamageCoefficient;

	public static float maxDamageCoefficient;

	public static float procCoefficient;

	public static float force;

	public static GameObject muzzleflashEffect;

	private float stopwatch;

	private GameObject areaIndicatorInstance;

	private bool fireMeteor;

	private float radius;

	private float chargeDuration;

	private float duration;

	public override void OnEnter()
	{
		base.OnEnter();
		chargeDuration = baseChargeDuration / attackSpeedStat;
		duration = baseDuration / attackSpeedStat;
		UpdateAreaIndicator();
	}

	private void UpdateAreaIndicator()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)areaIndicatorInstance))
		{
			float num = 1000f;
			RaycastHit val = default(RaycastHit);
			if (Physics.Raycast(GetAimRay(), ref val, num, LayerMask.op_Implicit(LayerIndex.world.mask)))
			{
				areaIndicatorInstance.transform.position = ((RaycastHit)(ref val)).point;
				areaIndicatorInstance.transform.up = ((RaycastHit)(ref val)).normal;
			}
		}
		else
		{
			areaIndicatorInstance = Object.Instantiate<GameObject>(areaIndicatorPrefab);
		}
		radius = Util.Remap(Mathf.Clamp01(stopwatch / chargeDuration), 0f, 1f, minMeteorRadius, maxMeteorRadius);
		areaIndicatorInstance.transform.localScale = new Vector3(radius, radius, radius);
	}

	public override void Update()
	{
		base.Update();
		UpdateAreaIndicator();
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		stopwatch += Time.fixedDeltaTime;
		if ((stopwatch >= duration || base.inputBank.skill2.justReleased) && base.isAuthority)
		{
			fireMeteor = true;
			outer.SetNextStateToMain();
		}
	}

	public override void OnExit()
	{
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		EffectManager.SimpleMuzzleFlash(muzzleflashEffect, base.gameObject, "Muzzle", transmit: false);
		if (Object.op_Implicit((Object)(object)areaIndicatorInstance))
		{
			if (fireMeteor)
			{
				float num = Util.Remap(Mathf.Clamp01(stopwatch / chargeDuration), 0f, 1f, minDamageCoefficient, maxDamageCoefficient);
				EffectManager.SpawnEffect(meteorEffect, new EffectData
				{
					origin = areaIndicatorInstance.transform.position,
					scale = radius
				}, transmit: true);
				BlastAttack obj = new BlastAttack
				{
					radius = radius,
					procCoefficient = procCoefficient,
					position = areaIndicatorInstance.transform.position,
					attacker = base.gameObject,
					crit = Util.CheckRoll(base.characterBody.crit, base.characterBody.master),
					baseDamage = base.characterBody.damage * num,
					falloffModel = BlastAttack.FalloffModel.SweetSpot,
					baseForce = force
				};
				obj.teamIndex = TeamComponent.GetObjectTeam(obj.attacker);
				obj.Fire();
			}
			EntityState.Destroy((Object)(object)areaIndicatorInstance.gameObject);
		}
		base.OnExit();
	}
}
