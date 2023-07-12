using RoR2;
using UnityEngine;

namespace EntityStates.GolemMonster;

public class ChargeLaser : BaseState
{
	public static float baseDuration = 3f;

	public static float laserMaxWidth = 0.2f;

	public static GameObject effectPrefab;

	public static GameObject laserPrefab;

	public static string attackSoundString;

	private float duration;

	private uint chargePlayID;

	private GameObject chargeEffect;

	private GameObject laserEffect;

	private LineRenderer laserLineComponent;

	private Vector3 laserDirection;

	private Vector3 visualEndPosition;

	private float flashTimer;

	private bool laserOn;

	public override void OnEnter()
	{
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		Transform modelTransform = GetModelTransform();
		chargePlayID = Util.PlayAttackSpeedSound(attackSoundString, base.gameObject, attackSpeedStat);
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			ChildLocator component = ((Component)modelTransform).GetComponent<ChildLocator>();
			if (Object.op_Implicit((Object)(object)component))
			{
				Transform val = component.FindChild("MuzzleLaser");
				if (Object.op_Implicit((Object)(object)val))
				{
					if (Object.op_Implicit((Object)(object)effectPrefab))
					{
						chargeEffect = Object.Instantiate<GameObject>(effectPrefab, val.position, val.rotation);
						chargeEffect.transform.parent = val;
						ScaleParticleSystemDuration component2 = chargeEffect.GetComponent<ScaleParticleSystemDuration>();
						if (Object.op_Implicit((Object)(object)component2))
						{
							component2.newDuration = duration;
						}
					}
					if (Object.op_Implicit((Object)(object)laserPrefab))
					{
						laserEffect = Object.Instantiate<GameObject>(laserPrefab, val.position, val.rotation);
						laserEffect.transform.parent = val;
						laserLineComponent = laserEffect.GetComponent<LineRenderer>();
					}
				}
			}
		}
		if (Object.op_Implicit((Object)(object)base.characterBody))
		{
			base.characterBody.SetAimTimer(duration);
		}
		flashTimer = 0f;
		laserOn = true;
	}

	public override void OnExit()
	{
		AkSoundEngine.StopPlayingID(chargePlayID);
		base.OnExit();
		if (Object.op_Implicit((Object)(object)chargeEffect))
		{
			EntityState.Destroy((Object)(object)chargeEffect);
		}
		if (Object.op_Implicit((Object)(object)laserEffect))
		{
			EntityState.Destroy((Object)(object)laserEffect);
		}
	}

	public override void Update()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		base.Update();
		if (!Object.op_Implicit((Object)(object)laserEffect) || !Object.op_Implicit((Object)(object)laserLineComponent))
		{
			return;
		}
		float num = 1000f;
		Ray aimRay = GetAimRay();
		Vector3 position = laserEffect.transform.parent.position;
		Vector3 point = ((Ray)(ref aimRay)).GetPoint(num);
		laserDirection = point - position;
		RaycastHit val = default(RaycastHit);
		if (Physics.Raycast(aimRay, ref val, num, LayerMask.op_Implicit(LayerIndex.world.mask) | LayerMask.op_Implicit(LayerIndex.entityPrecise.mask)))
		{
			point = ((RaycastHit)(ref val)).point;
		}
		laserLineComponent.SetPosition(0, position);
		laserLineComponent.SetPosition(1, point);
		float num2;
		if (duration - base.age > 0.5f)
		{
			num2 = base.age / duration;
		}
		else
		{
			flashTimer -= Time.deltaTime;
			if (flashTimer <= 0f)
			{
				laserOn = !laserOn;
				flashTimer = 1f / 30f;
			}
			num2 = (laserOn ? 1f : 0f);
		}
		num2 *= laserMaxWidth;
		laserLineComponent.startWidth = num2;
		laserLineComponent.endWidth = num2;
	}

	public override void FixedUpdate()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		if (base.fixedAge >= duration && base.isAuthority)
		{
			FireLaser fireLaser = new FireLaser();
			fireLaser.laserDirection = laserDirection;
			outer.SetNextState(fireLaser);
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Skill;
	}
}
