using RoR2;
using RoR2.UI;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Captain.Weapon;

public class SetupSupplyDrop : BaseState
{
	public struct PlacementInfo
	{
		public bool ok;

		public Vector3 position;

		public Quaternion rotation;

		public void Serialize(NetworkWriter writer)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			writer.Write(ok);
			writer.Write(position);
			writer.Write(rotation);
		}

		public void Deserialize(NetworkReader reader)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			ok = reader.ReadBoolean();
			position = reader.ReadVector3();
			rotation = reader.ReadQuaternion();
		}
	}

	public static GameObject crosshairOverridePrefab;

	public static string enterSoundString;

	public static string exitSoundString;

	public static GameObject effectMuzzlePrefab;

	public static string effectMuzzleString;

	public static float baseExitDuration;

	public static float maxPlacementDistance;

	public static GameObject blueprintPrefab;

	public static float normalYThreshold;

	private PlacementInfo currentPlacementInfo;

	private CrosshairUtils.OverrideRequest crosshairOverrideRequest;

	private GenericSkill primarySkillSlot;

	private AimAnimator modelAimAnimator;

	private GameObject effectMuzzleInstance;

	private Animator modelAnimator;

	private float timerSinceComplete;

	private bool beginExit;

	private GenericSkill originalPrimarySkill;

	private GenericSkill originalSecondarySkill;

	private BlueprintController blueprints;

	private CameraTargetParams.AimRequest aimRequest;

	private float exitDuration => baseExitDuration / attackSpeedStat;

	public override void OnEnter()
	{
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		modelAnimator = GetModelAnimator();
		PlayAnimation("Gesture, Override", "PrepSupplyDrop");
		PlayAnimation("Gesture, Additive", "PrepSupplyDrop");
		if (Object.op_Implicit((Object)(object)modelAnimator))
		{
			modelAnimator.SetBool("PrepSupplyDrop", true);
		}
		Transform val = FindModelChild(effectMuzzleString);
		if (Object.op_Implicit((Object)(object)val))
		{
			effectMuzzleInstance = Object.Instantiate<GameObject>(effectMuzzlePrefab, val);
		}
		if (Object.op_Implicit((Object)(object)crosshairOverridePrefab))
		{
			crosshairOverrideRequest = CrosshairUtils.RequestOverrideForBody(base.characterBody, crosshairOverridePrefab, CrosshairUtils.OverridePriority.Skill);
		}
		Util.PlaySound(enterSoundString, base.gameObject);
		blueprints = Object.Instantiate<GameObject>(blueprintPrefab, currentPlacementInfo.position, currentPlacementInfo.rotation).GetComponent<BlueprintController>();
		if (Object.op_Implicit((Object)(object)base.cameraTargetParams))
		{
			aimRequest = base.cameraTargetParams.RequestAimType(CameraTargetParams.AimType.Aura);
		}
		originalPrimarySkill = base.skillLocator.primary;
		originalSecondarySkill = base.skillLocator.secondary;
		base.skillLocator.primary = base.skillLocator.FindSkill("SupplyDrop1");
		base.skillLocator.secondary = base.skillLocator.FindSkill("SupplyDrop2");
	}

	public override void Update()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		base.Update();
		currentPlacementInfo = GetPlacementInfo(GetAimRay(), base.gameObject);
		if (Object.op_Implicit((Object)(object)blueprints))
		{
			blueprints.PushState(currentPlacementInfo.position, currentPlacementInfo.rotation, currentPlacementInfo.ok);
		}
	}

	public override void FixedUpdate()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		base.FixedUpdate();
		if (Object.op_Implicit((Object)(object)base.characterDirection))
		{
			CharacterDirection obj = base.characterDirection;
			Ray aimRay = GetAimRay();
			obj.moveVector = ((Ray)(ref aimRay)).direction;
		}
		if (base.isAuthority && beginExit)
		{
			timerSinceComplete += Time.fixedDeltaTime;
			if (timerSinceComplete > exitDuration)
			{
				outer.SetNextStateToMain();
			}
		}
	}

	public override void OnExit()
	{
		if (!outer.destroying)
		{
			Util.PlaySound(exitSoundString, base.gameObject);
		}
		if (Object.op_Implicit((Object)(object)effectMuzzleInstance))
		{
			EntityState.Destroy((Object)(object)effectMuzzleInstance);
		}
		crosshairOverrideRequest?.Dispose();
		base.skillLocator.primary = originalPrimarySkill;
		base.skillLocator.secondary = originalSecondarySkill;
		if (Object.op_Implicit((Object)(object)modelAnimator))
		{
			modelAnimator.SetBool("PrepSupplyDrop", false);
		}
		if (Object.op_Implicit((Object)(object)blueprints))
		{
			EntityState.Destroy((Object)(object)((Component)blueprints).gameObject);
			blueprints = null;
		}
		aimRequest?.Dispose();
		base.OnExit();
	}

	public static PlacementInfo GetPlacementInfo(Ray aimRay, GameObject gameObject)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		float extraRaycastDistance = 0f;
		CameraRigController.ModifyAimRayIfApplicable(aimRay, gameObject, out extraRaycastDistance);
		Vector3 val = -((Ray)(ref aimRay)).direction;
		Vector3 val2 = Vector3.up;
		Vector3 val3 = Vector3.Cross(val2, val);
		PlacementInfo result = default(PlacementInfo);
		result.ok = false;
		RaycastHit val4 = default(RaycastHit);
		if (Physics.Raycast(aimRay, ref val4, maxPlacementDistance, LayerMask.op_Implicit(LayerIndex.world.mask)) && ((RaycastHit)(ref val4)).normal.y > normalYThreshold)
		{
			val2 = ((RaycastHit)(ref val4)).normal;
			val = Vector3.Cross(val3, val2);
			result.ok = true;
		}
		result.rotation = Util.QuaternionSafeLookRotation(val, val2);
		Vector3 point = ((RaycastHit)(ref val4)).point;
		result.position = point;
		return result;
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.PrioritySkill;
	}
}
