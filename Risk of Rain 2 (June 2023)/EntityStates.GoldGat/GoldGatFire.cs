using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.GoldGat;

public class GoldGatFire : BaseGoldGatState
{
	public static float minFireFrequency;

	public static float maxFireFrequency;

	public static float minSpread;

	public static float maxSpread;

	public static float windUpDuration;

	public static float force;

	public static float damageCoefficient;

	public static float procCoefficient;

	public static string attackSoundString;

	public static GameObject tracerEffectPrefab;

	public static GameObject impactEffectPrefab;

	public static GameObject muzzleFlashEffectPrefab;

	public static int baseMoneyCostPerBullet;

	public static string windUpSoundString;

	public static string windUpRTPC;

	public float totalStopwatch;

	private float stopwatch;

	private float fireFrequency;

	private uint loopSoundID;

	public override void OnEnter()
	{
		base.OnEnter();
		loopSoundID = Util.PlaySound(windUpSoundString, base.gameObject);
		FireBullet();
	}

	private void FireBullet()
	{
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		body.SetAimTimer(2f);
		float num = Mathf.Clamp01(totalStopwatch / windUpDuration);
		fireFrequency = Mathf.Lerp(minFireFrequency, maxFireFrequency, num);
		float num2 = Mathf.Lerp(minSpread, maxSpread, num);
		Util.PlaySound(attackSoundString, base.gameObject);
		int num3 = (int)((float)baseMoneyCostPerBullet * (1f + ((float)TeamManager.instance.GetTeamLevel(bodyMaster.teamIndex) - 1f) * 0.25f));
		if (base.isAuthority)
		{
			Transform aimOriginTransform = body.aimOriginTransform;
			if (Object.op_Implicit((Object)(object)gunChildLocator))
			{
				gunChildLocator.FindChild("Muzzle");
			}
			if (Object.op_Implicit((Object)(object)aimOriginTransform))
			{
				BulletAttack bulletAttack = new BulletAttack();
				bulletAttack.owner = networkedBodyAttachment.attachedBodyObject;
				bulletAttack.aimVector = bodyInputBank.aimDirection;
				bulletAttack.origin = bodyInputBank.aimOrigin;
				bulletAttack.falloffModel = BulletAttack.FalloffModel.DefaultBullet;
				bulletAttack.force = force;
				bulletAttack.damage = body.damage * damageCoefficient;
				bulletAttack.damageColorIndex = DamageColorIndex.Item;
				bulletAttack.bulletCount = 1u;
				bulletAttack.minSpread = 0f;
				bulletAttack.maxSpread = num2;
				bulletAttack.tracerEffectPrefab = tracerEffectPrefab;
				bulletAttack.isCrit = Util.CheckRoll(body.crit, bodyMaster);
				bulletAttack.procCoefficient = procCoefficient;
				bulletAttack.muzzleName = "Muzzle";
				bulletAttack.weapon = base.gameObject;
				bulletAttack.Fire();
				Animator obj = gunAnimator;
				if (obj != null)
				{
					obj.Play("Fire");
				}
			}
		}
		if (NetworkServer.active)
		{
			bodyMaster.money = (uint)Mathf.Max(0f, (float)(bodyMaster.money - num3));
		}
		Animator obj2 = gunAnimator;
		if (obj2 != null)
		{
			obj2.SetFloat("Crank.playbackRate", fireFrequency);
		}
		EffectManager.SimpleMuzzleFlash(muzzleFlashEffectPrefab, base.gameObject, "Muzzle", transmit: false);
	}

	public override void FixedUpdate()
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		totalStopwatch += Time.fixedDeltaTime;
		stopwatch += Time.fixedDeltaTime;
		AkSoundEngine.SetRTPCValueByPlayingID(windUpRTPC, Mathf.InverseLerp(minFireFrequency, maxFireFrequency, fireFrequency) * 100f, loopSoundID);
		if (!CheckReturnToIdle() && stopwatch > 1f / fireFrequency)
		{
			stopwatch = 0f;
			FireBullet();
		}
	}

	public override void OnExit()
	{
		AkSoundEngine.StopPlayingID(loopSoundID);
		base.OnExit();
	}
}
