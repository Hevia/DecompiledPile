using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace EntityStates.Huntress;

public class ArrowRain : BaseArrowBarrage
{
	public static float arrowRainRadius;

	public static float damageCoefficient;

	public static GameObject projectilePrefab;

	public static GameObject areaIndicatorPrefab;

	public static GameObject muzzleFlashEffect;

	private GameObject areaIndicatorInstance;

	private bool shouldFireArrowRain;

	public override void OnEnter()
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		PlayAnimation("FullBody, Override", "LoopArrowRain");
		if (Object.op_Implicit((Object)(object)areaIndicatorPrefab))
		{
			areaIndicatorInstance = Object.Instantiate<GameObject>(areaIndicatorPrefab);
			areaIndicatorInstance.transform.localScale = new Vector3(arrowRainRadius, arrowRainRadius, arrowRainRadius);
		}
	}

	private void UpdateAreaIndicator()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
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
	}

	public override void Update()
	{
		base.Update();
		UpdateAreaIndicator();
	}

	protected override void HandlePrimaryAttack()
	{
		base.HandlePrimaryAttack();
		shouldFireArrowRain = true;
		outer.SetNextStateToMain();
	}

	protected void DoFireArrowRain()
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		EffectManager.SimpleMuzzleFlash(muzzleFlashEffect, base.gameObject, "Muzzle", transmit: false);
		if (Object.op_Implicit((Object)(object)areaIndicatorInstance) && shouldFireArrowRain)
		{
			ProjectileManager.instance.FireProjectile(projectilePrefab, areaIndicatorInstance.transform.position, areaIndicatorInstance.transform.rotation, base.gameObject, damageStat * damageCoefficient, 0f, Util.CheckRoll(critStat, base.characterBody.master));
		}
	}

	public override void OnExit()
	{
		if (shouldFireArrowRain && !outer.destroying)
		{
			DoFireArrowRain();
		}
		if (Object.op_Implicit((Object)(object)areaIndicatorInstance))
		{
			EntityState.Destroy((Object)(object)areaIndicatorInstance.gameObject);
		}
		base.OnExit();
	}
}
