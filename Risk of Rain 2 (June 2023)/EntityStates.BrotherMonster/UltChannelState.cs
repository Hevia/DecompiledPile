using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using UnityEngine;

namespace EntityStates.BrotherMonster;

public class UltChannelState : BaseState
{
	public static GameObject waveProjectileLeftPrefab;

	public static GameObject waveProjectileRightPrefab;

	public static int waveProjectileCount;

	public static float waveProjectileDamageCoefficient;

	public static float waveProjectileForce;

	public static int totalWaves;

	public static float maxDuration;

	public static GameObject channelBeginMuzzleflashEffectPrefab;

	public static GameObject channelEffectPrefab;

	public static string enterSoundString;

	public static string exitSoundString;

	private GameObject channelEffectInstance;

	public static SkillDef replacementSkillDef;

	private int wavesFired;

	public override void OnEnter()
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		Util.PlaySound(enterSoundString, base.gameObject);
		Transform val = FindModelChild("MuzzleUlt");
		if (Object.op_Implicit((Object)(object)val) && Object.op_Implicit((Object)(object)channelEffectPrefab))
		{
			channelEffectInstance = Object.Instantiate<GameObject>(channelEffectPrefab, val.position, Quaternion.identity, val);
		}
		if (Object.op_Implicit((Object)(object)channelBeginMuzzleflashEffectPrefab))
		{
			EffectManager.SimpleMuzzleFlash(channelBeginMuzzleflashEffectPrefab, base.gameObject, "MuzzleUlt", transmit: false);
		}
	}

	private void FireWave()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		wavesFired++;
		float num = 360f / (float)waveProjectileCount;
		Vector3 val = Vector3.ProjectOnPlane(Random.onUnitSphere, Vector3.up);
		Vector3 normalized = ((Vector3)(ref val)).normalized;
		Vector3 footPosition = base.characterBody.footPosition;
		GameObject prefab = waveProjectileLeftPrefab;
		if (Random.value <= 0.5f)
		{
			prefab = waveProjectileRightPrefab;
		}
		for (int i = 0; i < waveProjectileCount; i++)
		{
			Vector3 forward = Quaternion.AngleAxis(num * (float)i, Vector3.up) * normalized;
			ProjectileManager.instance.FireProjectile(prefab, footPosition, Util.QuaternionSafeLookRotation(forward), base.gameObject, base.characterBody.damage * waveProjectileDamageCoefficient, waveProjectileForce, Util.CheckRoll(base.characterBody.crit, base.characterBody.master));
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.isAuthority)
		{
			if (Mathf.CeilToInt(base.fixedAge / maxDuration * (float)totalWaves) > wavesFired)
			{
				FireWave();
			}
			if (base.fixedAge > maxDuration)
			{
				outer.SetNextState(new UltExitState());
			}
		}
	}

	public override void OnExit()
	{
		Util.PlaySound(exitSoundString, base.gameObject);
		if (Object.op_Implicit((Object)(object)channelEffectInstance))
		{
			EntityState.Destroy((Object)(object)channelEffectInstance);
		}
		base.OnExit();
	}
}
