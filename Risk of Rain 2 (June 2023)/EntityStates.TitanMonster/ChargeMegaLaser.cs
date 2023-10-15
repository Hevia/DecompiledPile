using System.Linq;
using RoR2;
using UnityEngine;

namespace EntityStates.TitanMonster;

public class ChargeMegaLaser : BaseState
{
	public static float baseDuration = 3f;

	public static float laserMaxWidth = 0.2f;

	[SerializeField]
	public GameObject effectPrefab;

	[SerializeField]
	public GameObject laserPrefab;

	public static string chargeAttackSoundString;

	public static float lockOnAngle;

	private HurtBox lockedOnHurtBox;

	public float duration;

	private GameObject chargeEffect;

	private GameObject laserEffect;

	private LineRenderer laserLineComponent;

	private Vector3 visualEndPosition;

	private float flashTimer;

	private bool laserOn;

	private BullseyeSearch enemyFinder;

	private const float originalSoundDuration = 2.1f;

	public override void OnEnter()
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		Transform modelTransform = GetModelTransform();
		Util.PlayAttackSpeedSound(chargeAttackSoundString, base.gameObject, 2.1f / duration);
		Ray aimRay = GetAimRay();
		enemyFinder = new BullseyeSearch();
		enemyFinder.maxDistanceFilter = 2000f;
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
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		base.Update();
		if (!Object.op_Implicit((Object)(object)laserEffect) || !Object.op_Implicit((Object)(object)laserLineComponent))
		{
			return;
		}
		float num = 1000f;
		Ray aimRay = GetAimRay();
		enemyFinder.RefreshCandidates();
		lockedOnHurtBox = enemyFinder.GetResults().FirstOrDefault();
		if (Object.op_Implicit((Object)(object)lockedOnHurtBox))
		{
			aimRay.direction = ((Component)lockedOnHurtBox).transform.position - aimRay.origin;
		}
		Vector3 position = laserEffect.transform.parent.position;
		Vector3 point = aimRay.GetPoint(num);
		RaycastHit val = default(RaycastHit);
		if (Physics.Raycast(aimRay, ref val, num, LayerMask.op_Implicit(LayerIndex.world.mask) | LayerMask.op_Implicit(LayerIndex.defaultLayer.mask)))
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
		base.FixedUpdate();
		if (base.fixedAge >= duration && base.isAuthority)
		{
			FireMegaLaser nextState = new FireMegaLaser();
			outer.SetNextState(nextState);
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Skill;
	}
}
