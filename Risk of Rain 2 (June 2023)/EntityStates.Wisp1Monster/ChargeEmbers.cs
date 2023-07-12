using RoR2;
using UnityEngine;

namespace EntityStates.Wisp1Monster;

public class ChargeEmbers : BaseState
{
	public static float baseDuration = 3f;

	public static GameObject chargeEffectPrefab;

	public static GameObject laserEffectPrefab;

	public static string attackString;

	private float duration;

	private float stopwatch;

	private uint soundID;

	private GameObject chargeEffectInstance;

	private GameObject laserEffectInstance;

	private LineRenderer laserEffectInstanceLineRenderer;

	public override void OnEnter()
	{
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		stopwatch = 0f;
		duration = baseDuration / attackSpeedStat;
		soundID = Util.PlayAttackSpeedSound(attackString, base.gameObject, attackSpeedStat);
		Transform modelTransform = GetModelTransform();
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			ChildLocator component = ((Component)modelTransform).GetComponent<ChildLocator>();
			if (Object.op_Implicit((Object)(object)component))
			{
				Transform val = component.FindChild("Muzzle");
				if (Object.op_Implicit((Object)(object)val))
				{
					if (Object.op_Implicit((Object)(object)chargeEffectPrefab))
					{
						chargeEffectInstance = Object.Instantiate<GameObject>(chargeEffectPrefab, val.position, val.rotation);
						chargeEffectInstance.transform.parent = val;
						ScaleParticleSystemDuration component2 = chargeEffectInstance.GetComponent<ScaleParticleSystemDuration>();
						if (Object.op_Implicit((Object)(object)component2))
						{
							component2.newDuration = duration;
						}
					}
					if (Object.op_Implicit((Object)(object)laserEffectPrefab))
					{
						laserEffectInstance = Object.Instantiate<GameObject>(laserEffectPrefab, val.position, val.rotation);
						laserEffectInstance.transform.parent = val;
						laserEffectInstanceLineRenderer = laserEffectInstance.GetComponent<LineRenderer>();
					}
				}
			}
		}
		PlayAnimation("Body", "ChargeAttack1", "ChargeAttack1.playbackRate", duration);
		if (Object.op_Implicit((Object)(object)base.characterBody))
		{
			base.characterBody.SetAimTimer(duration);
		}
	}

	public override void OnExit()
	{
		base.OnExit();
		AkSoundEngine.StopPlayingID(soundID);
		if (Object.op_Implicit((Object)(object)chargeEffectInstance))
		{
			EntityState.Destroy((Object)(object)chargeEffectInstance);
		}
		if (Object.op_Implicit((Object)(object)laserEffectInstance))
		{
			EntityState.Destroy((Object)(object)laserEffectInstance);
		}
	}

	public override void Update()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		base.Update();
		Ray aimRay = GetAimRay();
		float num = 50f;
		Vector3 origin = ((Ray)(ref aimRay)).origin;
		Vector3 point = ((Ray)(ref aimRay)).GetPoint(num);
		laserEffectInstanceLineRenderer.SetPosition(0, origin);
		laserEffectInstanceLineRenderer.SetPosition(1, point);
		Color startColor = default(Color);
		((Color)(ref startColor))._002Ector(1f, 1f, 1f, stopwatch / duration);
		Color clear = Color.clear;
		laserEffectInstanceLineRenderer.startColor = startColor;
		laserEffectInstanceLineRenderer.endColor = clear;
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		stopwatch += Time.fixedDeltaTime;
		if (stopwatch >= duration && base.isAuthority)
		{
			outer.SetNextState(new FireEmbers());
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Skill;
	}
}
