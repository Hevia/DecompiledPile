using System.Linq;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.VoidJailer.Weapon;

public class Capture2 : BaseState
{
	[SerializeField]
	public string animationLayerName;

	[SerializeField]
	public string animationStateName;

	[SerializeField]
	public string animationPlaybackRateName;

	[SerializeField]
	public float baseDuration;

	[SerializeField]
	public string enterSoundString;

	[SerializeField]
	public float pullFieldOfView;

	[SerializeField]
	public float pullMinDistance;

	[SerializeField]
	public float pullMaxDistance;

	[SerializeField]
	public AnimationCurve pullSuitabilityCurve;

	[SerializeField]
	public GameObject pullTracerPrefab;

	[SerializeField]
	public float pullLiftVelocity;

	[SerializeField]
	public BuffDef debuffDef;

	[SerializeField]
	public float debuffDuration;

	[SerializeField]
	public float damageCoefficient;

	[SerializeField]
	public float procCoefficient;

	[SerializeField]
	public GameObject muzzleflashEffectPrefab;

	[SerializeField]
	public string muzzleString;

	private float duration;

	public override void OnEnter()
	{
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0212: Unknown result type (might be due to invalid IL or missing references)
		//IL_0217: Unknown result type (might be due to invalid IL or missing references)
		//IL_0230: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Unknown result type (might be due to invalid IL or missing references)
		//IL_027a: Unknown result type (might be due to invalid IL or missing references)
		//IL_027f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0287: Unknown result type (might be due to invalid IL or missing references)
		//IL_028c: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02be: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		PlayAnimation(animationLayerName, animationStateName, animationPlaybackRateName, duration);
		Util.PlaySound(enterSoundString, base.gameObject);
		if (Object.op_Implicit((Object)(object)muzzleflashEffectPrefab))
		{
			EffectManager.SimpleMuzzleFlash(muzzleflashEffectPrefab, base.gameObject, muzzleString, transmit: false);
		}
		Ray aimRay = GetAimRay();
		if (!NetworkServer.active)
		{
			return;
		}
		BullseyeSearch bullseyeSearch = new BullseyeSearch();
		bullseyeSearch.teamMaskFilter = TeamMask.all;
		bullseyeSearch.maxAngleFilter = pullFieldOfView * 0.5f;
		bullseyeSearch.maxDistanceFilter = pullMaxDistance;
		bullseyeSearch.searchOrigin = ((Ray)(ref aimRay)).origin;
		bullseyeSearch.searchDirection = ((Ray)(ref aimRay)).direction;
		bullseyeSearch.sortMode = BullseyeSearch.SortMode.Angle;
		bullseyeSearch.filterByLoS = true;
		bullseyeSearch.RefreshCandidates();
		bullseyeSearch.FilterOutGameObject(base.gameObject);
		HurtBox hurtBox = bullseyeSearch.GetResults().FirstOrDefault();
		GetTeam();
		if (!Object.op_Implicit((Object)(object)hurtBox))
		{
			return;
		}
		Vector3 val = ((Component)hurtBox).transform.position - ((Ray)(ref aimRay)).origin;
		float magnitude = ((Vector3)(ref val)).magnitude;
		Vector3 val2 = val / magnitude;
		float num = 1f;
		CharacterBody body = hurtBox.healthComponent.body;
		if (Object.op_Implicit((Object)(object)body.characterMotor))
		{
			num = body.characterMotor.mass;
		}
		else if (Object.op_Implicit((Object)(object)((Component)hurtBox.healthComponent).GetComponent<Rigidbody>()))
		{
			num = base.rigidbody.mass;
		}
		if (Object.op_Implicit((Object)(object)debuffDef))
		{
			body.AddTimedBuff(debuffDef, debuffDuration);
		}
		float num2 = pullSuitabilityCurve.Evaluate(num);
		Vector3 val3 = val2;
		float num3 = Trajectory.CalculateInitialYSpeedForHeight(Mathf.Abs(pullMinDistance - magnitude)) * Mathf.Sign(pullMinDistance - magnitude);
		val3 *= num3;
		val3.y = pullLiftVelocity;
		DamageInfo damageInfo = new DamageInfo
		{
			attacker = base.gameObject,
			damage = damageStat * damageCoefficient,
			position = ((Component)hurtBox).transform.position,
			procCoefficient = procCoefficient
		};
		hurtBox.healthComponent.TakeDamageForce(val3 * (num * num2), alwaysApply: true, disableAirControlUntilCollision: true);
		hurtBox.healthComponent.TakeDamage(damageInfo);
		GlobalEventManager.instance.OnHitEnemy(damageInfo, ((Component)hurtBox.healthComponent).gameObject);
		if (Object.op_Implicit((Object)(object)pullTracerPrefab))
		{
			Vector3 position = ((Component)hurtBox).transform.position;
			Vector3 start = base.characterBody.corePosition;
			Transform val4 = FindModelChild(muzzleString);
			if (Object.op_Implicit((Object)(object)val4))
			{
				start = val4.position;
			}
			EffectData effectData = new EffectData
			{
				origin = position,
				start = start
			};
			EffectManager.SpawnEffect(pullTracerPrefab, effectData, transmit: true);
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.fixedAge > duration)
		{
			outer.SetNextState(new ExitCapture());
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.PrioritySkill;
	}
}
