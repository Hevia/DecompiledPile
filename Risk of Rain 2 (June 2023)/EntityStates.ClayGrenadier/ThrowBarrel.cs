using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace EntityStates.ClayGrenadier;

public class ThrowBarrel : GenericProjectileBaseState
{
	[SerializeField]
	public float aimCalculationRaycastDistance;

	[SerializeField]
	public string animationLayerName = "Body";

	[SerializeField]
	public string animationStateName = "FaceSlam";

	[SerializeField]
	public string playbackRateParam = "FaceSlam.playbackRate";

	[SerializeField]
	public int projectileCount;

	[SerializeField]
	public float projectilePitchBonusPerProjectile;

	[SerializeField]
	public float projectileYawBonusPerProjectile;

	[SerializeField]
	public GameObject chargeEffectPrefab;

	[SerializeField]
	public string chargeEffectMuzzleString;

	[SerializeField]
	public string enterSoundString;

	private GameObject chargeInstance;

	private int currentProjectileCount;

	protected override void PlayAnimation(float duration)
	{
		base.PlayAnimation(duration);
		PlayCrossfade(animationLayerName, animationStateName, playbackRateParam, duration, 0.2f);
	}

	public override void OnEnter()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		Util.PlayAttackSpeedSound(enterSoundString, base.gameObject, attackSpeedStat);
		if (Object.op_Implicit((Object)(object)base.characterDirection))
		{
			CharacterDirection obj = base.characterDirection;
			Ray aimRay = GetAimRay();
			obj.moveVector = aimRay.direction;
		}
		base.characterBody.SetAimTimer(0f);
		Transform val = FindModelChild(chargeEffectMuzzleString);
		if (Object.op_Implicit((Object)(object)val) && Object.op_Implicit((Object)(object)chargeEffectPrefab))
		{
			chargeInstance = Object.Instantiate<GameObject>(chargeEffectPrefab, val.position, val.rotation);
			chargeInstance.transform.parent = val;
			ScaleParticleSystemDuration component = chargeInstance.GetComponent<ScaleParticleSystemDuration>();
			if (Object.op_Implicit((Object)(object)component))
			{
				component.newDuration = delayBeforeFiringProjectile;
			}
		}
	}

	protected override Ray ModifyProjectileAimRay(Ray aimRay)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		RaycastHit hitInfo = default(RaycastHit);
		Ray ray = aimRay;
		float desiredForwardSpeed = projectilePrefab.GetComponent<ProjectileSimple>().desiredForwardSpeed;
		((Ray)(ref ray)).origin = aimRay.origin;
		if (Util.CharacterRaycast(base.gameObject, ray, out hitInfo, float.PositiveInfinity, LayerMask.op_Implicit(LayerMask.op_Implicit(LayerIndex.world.mask) | LayerMask.op_Implicit(LayerIndex.entityPrecise.mask)), (QueryTriggerInteraction)1))
		{
			float num = desiredForwardSpeed;
			Vector3 val = ((RaycastHit)(ref hitInfo)).point - aimRay.origin;
			Vector2 val2 = default(Vector2);
			((Vector2)(ref val2))._002Ector(val.x, val.z);
			float magnitude = ((Vector2)(ref val2)).magnitude;
			float num2 = Trajectory.CalculateInitialYSpeed(magnitude / num, val.y);
			Vector3 val3 = default(Vector3);
			((Vector3)(ref val3))._002Ector(val2.x / magnitude * num, num2, val2.y / magnitude * num);
			desiredForwardSpeed = ((Vector3)(ref val3)).magnitude;
			aimRay.direction = val3 / desiredForwardSpeed;
		}
		aimRay.direction = Util.ApplySpread(aimRay.direction, 0f, 0f, 1f, 1f, projectileYawBonusPerProjectile * (float)currentProjectileCount, projectilePitchBonusPerProjectile * (float)currentProjectileCount);
		return aimRay;
	}

	protected override void FireProjectile()
	{
		for (int i = 0; i < projectileCount; i++)
		{
			base.FireProjectile();
			currentProjectileCount++;
		}
		if (Object.op_Implicit((Object)(object)chargeInstance))
		{
			EntityState.Destroy((Object)(object)chargeInstance);
		}
	}

	public override void OnExit()
	{
		if (Object.op_Implicit((Object)(object)chargeInstance))
		{
			EntityState.Destroy((Object)(object)chargeInstance);
		}
		base.OnExit();
	}
}
