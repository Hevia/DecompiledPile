using RoR2;
using UnityEngine;

namespace EntityStates.Engi.EngiWeapon;

public class PlaceTurret : BaseState
{
	private struct PlacementInfo
	{
		public bool ok;

		public Vector3 position;

		public Quaternion rotation;
	}

	[SerializeField]
	public GameObject wristDisplayPrefab;

	[SerializeField]
	public string placeSoundString;

	[SerializeField]
	public GameObject blueprintPrefab;

	[SerializeField]
	public GameObject turretMasterPrefab;

	private const float placementMaxUp = 1f;

	private const float placementMaxDown = 3f;

	private const float placementForwardDistance = 2f;

	private const float entryDelay = 0.1f;

	private const float exitDelay = 0.25f;

	private const float turretRadius = 0.5f;

	private const float turretHeight = 1.82f;

	private const float turretCenter = 0f;

	private const float turretModelYOffset = -0.75f;

	private GameObject wristDisplayObject;

	private BlueprintController blueprints;

	private float exitCountdown;

	private bool exitPending;

	private float entryCountdown;

	private PlacementInfo currentPlacementInfo;

	public override void OnEnter()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		if (base.isAuthority)
		{
			currentPlacementInfo = GetPlacementInfo();
			blueprints = Object.Instantiate<GameObject>(blueprintPrefab, currentPlacementInfo.position, currentPlacementInfo.rotation).GetComponent<BlueprintController>();
		}
		PlayAnimation("Gesture", "PrepTurret");
		entryCountdown = 0.1f;
		exitCountdown = 0.25f;
		exitPending = false;
		if (!Object.op_Implicit((Object)(object)base.modelLocator))
		{
			return;
		}
		ChildLocator component = ((Component)base.modelLocator.modelTransform).GetComponent<ChildLocator>();
		if (Object.op_Implicit((Object)(object)component))
		{
			Transform val = component.FindChild("WristDisplay");
			if (Object.op_Implicit((Object)(object)val))
			{
				wristDisplayObject = Object.Instantiate<GameObject>(wristDisplayPrefab, val);
			}
		}
	}

	private PlacementInfo GetPlacementInfo()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		Ray aimRay = GetAimRay();
		Vector3 direction = ((Ray)(ref aimRay)).direction;
		direction.y = 0f;
		((Vector3)(ref direction)).Normalize();
		((Ray)(ref aimRay)).direction = direction;
		PlacementInfo result = default(PlacementInfo);
		result.ok = false;
		result.rotation = Util.QuaternionSafeLookRotation(-direction);
		Ray val = default(Ray);
		((Ray)(ref val))._002Ector(((Ray)(ref aimRay)).GetPoint(2f) + Vector3.up * 1f, Vector3.down);
		float num = 4f;
		float num2 = num;
		RaycastHit val2 = default(RaycastHit);
		if (Physics.SphereCast(val, 0.5f, ref val2, num, LayerMask.op_Implicit(LayerIndex.world.mask)) && ((RaycastHit)(ref val2)).normal.y > 0.5f)
		{
			num2 = ((RaycastHit)(ref val2)).distance;
			result.ok = true;
		}
		Vector3 point = ((Ray)(ref val)).GetPoint(num2 + 0.5f);
		result.position = point;
		if (result.ok)
		{
			float num3 = Mathf.Max(1.82f, 0f);
			if (Physics.CheckCapsule(result.position + Vector3.up * (num3 - 0.5f), result.position + Vector3.up * 0.5f, 0.45f, LayerMask.op_Implicit(LayerIndex.world.mask) | LayerMask.op_Implicit(LayerIndex.defaultLayer.mask)))
			{
				result.ok = false;
			}
		}
		return result;
	}

	private void DestroyBlueprints()
	{
		if (Object.op_Implicit((Object)(object)blueprints))
		{
			EntityState.Destroy((Object)(object)((Component)blueprints).gameObject);
			blueprints = null;
		}
	}

	public override void OnExit()
	{
		base.OnExit();
		PlayAnimation("Gesture", "PlaceTurret");
		if (Object.op_Implicit((Object)(object)wristDisplayObject))
		{
			EntityState.Destroy((Object)(object)wristDisplayObject);
		}
		DestroyBlueprints();
	}

	public override void Update()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		base.Update();
		currentPlacementInfo = GetPlacementInfo();
		if (Object.op_Implicit((Object)(object)blueprints))
		{
			blueprints.PushState(currentPlacementInfo.position, currentPlacementInfo.rotation, currentPlacementInfo.ok);
		}
	}

	public override void FixedUpdate()
	{
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		if (!base.isAuthority)
		{
			return;
		}
		entryCountdown -= Time.fixedDeltaTime;
		if (exitPending)
		{
			exitCountdown -= Time.fixedDeltaTime;
			if (exitCountdown <= 0f)
			{
				outer.SetNextStateToMain();
			}
		}
		else
		{
			if (!Object.op_Implicit((Object)(object)base.inputBank) || !(entryCountdown <= 0f))
			{
				return;
			}
			if ((base.inputBank.skill1.down || base.inputBank.skill4.justPressed) && currentPlacementInfo.ok)
			{
				if (Object.op_Implicit((Object)(object)base.characterBody))
				{
					base.characterBody.SendConstructTurret(base.characterBody, currentPlacementInfo.position, currentPlacementInfo.rotation, MasterCatalog.FindMasterIndex(turretMasterPrefab));
					if (Object.op_Implicit((Object)(object)base.skillLocator))
					{
						GenericSkill skill = base.skillLocator.GetSkill(SkillSlot.Special);
						if (Object.op_Implicit((Object)(object)skill))
						{
							skill.DeductStock(1);
						}
					}
				}
				Util.PlaySound(placeSoundString, base.gameObject);
				DestroyBlueprints();
				exitPending = true;
			}
			if (base.inputBank.skill2.justPressed)
			{
				DestroyBlueprints();
				exitPending = true;
			}
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.PrioritySkill;
	}
}
