using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using RoR2.CameraModes;
using RoR2.ConVar;
using RoR2.Networking;
using RoR2.UI;
using UnityEngine;

namespace RoR2;

public class CameraRigController : MonoBehaviour
{
	[Obsolete("CameraMode objects are now used instead of enums.", true)]
	public enum CameraMode
	{
		None,
		PlayerBasic,
		Fly,
		SpectateOrbit,
		SpectateUser
	}

	public struct AimAssistInfo
	{
		public HurtBox aimAssistHurtbox;

		public Vector3 localPositionOnHurtbox;

		public Vector3 worldPosition;
	}

	[Header("Component References")]
	[Tooltip("The main camera for rendering the scene.")]
	public Camera sceneCam;

	[Tooltip("The UI camera.")]
	public Camera uiCam;

	[Tooltip("The skybox camera.")]
	public Camera skyboxCam;

	[Tooltip("The particle system to play while sprinting.")]
	public ParticleSystem sprintingParticleSystem;

	[Tooltip("The default FOV of this camera.")]
	[Header("Camera Parameters")]
	public float baseFov = 60f;

	[Tooltip("The viewport to use.")]
	public Rect viewport = new Rect(0f, 0f, 1f, 1f);

	[Tooltip("The maximum distance of the raycast used to determine the aim vector.")]
	public float maxAimRaycastDistance = 1000f;

	[Tooltip("If true, treat this camera as though it's in a cutscene")]
	public bool isCutscene;

	[Header("Near-Camera Character Dithering")]
	public bool enableFading = true;

	public float fadeStartDistance = 1f;

	public float fadeEndDistance = 4f;

	[Header("Behavior")]
	[Tooltip("Whether or not to create a HUD.")]
	public bool createHud = true;

	[Tooltip("Whether or not this camera being enabled forces player-owned cameras to be disabled.")]
	public bool suppressPlayerCameras;

	[Header("Target (for debug only)")]
	public GameObject nextTarget;

	private CameraModeBase _cameraMode;

	private GameObject _target;

	private CameraState desiredCameraState;

	private CameraState currentCameraState;

	private CameraState lerpCameraState;

	private float lerpCameraTime = 1f;

	private float lerpCameraTimeScale = 1f;

	private ICameraStateProvider overrideCam;

	private CameraModeBase.CameraModeContext cameraModeContext;

	private NetworkUser _viewer;

	private LocalUser _localUserViewer;

	private CameraTargetParams targetParams;

	public AimAssistInfo lastAimAssist;

	public AimAssistInfo aimAssist;

	private static List<CameraRigController> instancesList = new List<CameraRigController>();

	public static readonly ReadOnlyCollection<CameraRigController> readOnlyInstancesList = instancesList.AsReadOnly();

	public static FloatConVar aimStickExponent = new FloatConVar("aim_stick_exponent", ConVarFlags.None, "1", "The exponent for stick input used for aiming.");

	public static FloatConVar aimStickDualZoneThreshold = new FloatConVar("aim_stick_dual_zone_threshold", ConVarFlags.None, "0.90", "The threshold for stick dual zone behavior.");

	public static FloatConVar aimStickDualZoneSlope = new FloatConVar("aim_stick_dual_zone_slope", ConVarFlags.None, "0.40", "The slope value for stick dual zone behavior.");

	public static FloatConVar aimStickDualZoneSmoothing = new FloatConVar("aim_stick_smoothing", ConVarFlags.None, "0.05", "The smoothing value for stick aiming.");

	public static FloatConVar aimStickGlobalScale = new FloatConVar("aim_stick_global_scale", ConVarFlags.Archive, "1.00", "The global sensitivity scale for stick aiming.");

	public static FloatConVar aimStickAssistMinSlowdownScale = new FloatConVar("aim_stick_assist_min_slowdown_scale", ConVarFlags.None, "1", "The MAX amount the sensitivity scales down when passing over an enemy.");

	public static FloatConVar aimStickAssistMaxSlowdownScale = new FloatConVar("aim_stick_assist_max_slowdown_scale", ConVarFlags.None, "0.4", "The MAX amount the sensitivity scales down when passing over an enemy.");

	public static FloatConVar aimStickAssistMinDelta = new FloatConVar("aim_stick_assist_min_delta", ConVarFlags.None, "0", "The MIN amount in radians the aim assist will turn towards");

	public static FloatConVar aimStickAssistMaxDelta = new FloatConVar("aim_stick_assist_max_delta", ConVarFlags.None, "1.57", "The MAX amount in radians the aim assist will turn towards");

	public static FloatConVar aimStickAssistMaxInputHelp = new FloatConVar("aim_stick_assist_max_input_help", ConVarFlags.None, "0.2", "The amount, from 0-1, that the aim assist will actually ADD magnitude towards. Helps you keep target while strafing. CURRENTLY UNUSED.");

	public static FloatConVar aimStickAssistMaxSize = new FloatConVar("aim_stick_assist_max_size", ConVarFlags.None, "3", "The size, as a coefficient, of the aim assist 'white' zone.");

	public static FloatConVar aimStickAssistMinSize = new FloatConVar("aim_stick_assist_min_size", ConVarFlags.None, "1", "The minimum size, as a percentage of the GUI, of the aim assist 'red' zone.");

	public static BoolConVar enableSprintSensitivitySlowdown = new BoolConVar("enable_sprint_sensitivity_slowdown", ConVarFlags.Archive, "1", "Enables sensitivity reduction while sprinting.");

	private float hitmarkerAlpha;

	private float hitmarkerTimer;

	public bool disableSpectating { get; set; }

	public CameraModeBase cameraMode
	{
		get
		{
			return _cameraMode;
		}
		set
		{
			if (_cameraMode != value)
			{
				_cameraMode?.OnUninstall(this);
				_cameraMode = value;
				_cameraMode?.OnInstall(this);
			}
		}
	}

	public HUD hud { get; private set; }

	public GameObject firstPersonTarget { get; private set; }

	public TeamIndex targetTeamIndex { get; private set; } = TeamIndex.None;


	public CharacterBody targetBody { get; private set; }

	public Vector3 rawScreenShakeDisplacement { get; private set; }

	public Vector3 crosshairWorldPosition { get; private set; }

	public bool hasOverride => overrideCam != null;

	public bool isControlAllowed
	{
		get
		{
			if (hasOverride)
			{
				return overrideCam.IsUserControlAllowed(this);
			}
			return true;
		}
	}

	public bool isHudAllowed
	{
		get
		{
			if (Object.op_Implicit((Object)(object)target))
			{
				if (hasOverride)
				{
					return overrideCam.IsHudAllowed(this);
				}
				return true;
			}
			return false;
		}
	}

	public GameObject target
	{
		get
		{
			return _target;
		}
		private set
		{
			if (_target != value)
			{
				GameObject oldTarget = _target;
				_target = value;
				bool flag = Object.op_Implicit((Object)(object)_target);
				targetParams = (flag ? target.GetComponent<CameraTargetParams>() : null);
				targetBody = (flag ? target.GetComponent<CharacterBody>() : null);
				cameraMode?.OnTargetChanged(this, new CameraModeBase.OnTargetChangedArgs
				{
					oldTarget = oldTarget,
					newTarget = _target
				});
				CameraRigController.onCameraTargetChanged?.Invoke(this, target);
			}
		}
	}

	public NetworkUser viewer
	{
		get
		{
			return _viewer;
		}
		set
		{
			_viewer = value;
			localUserViewer = (Object.op_Implicit((Object)(object)_viewer) ? _viewer.localUser : null);
		}
	}

	public LocalUser localUserViewer
	{
		get
		{
			return _localUserViewer;
		}
		private set
		{
			if (_localUserViewer != value)
			{
				if (_localUserViewer != null)
				{
					_localUserViewer.cameraRigController = null;
				}
				_localUserViewer = value;
				if (_localUserViewer != null)
				{
					_localUserViewer.cameraRigController = this;
				}
				if (Object.op_Implicit((Object)(object)hud))
				{
					hud.localUserViewer = _localUserViewer;
				}
			}
		}
	}

	public HurtBox lastCrosshairHurtBox { get; private set; }

	public static event Action<CameraRigController, GameObject> onCameraTargetChanged;

	public static event Action<CameraRigController> onCameraEnableGlobal;

	public static event Action<CameraRigController> onCameraDisableGlobal;

	private void StartStateLerp(float lerpDuration)
	{
		lerpCameraState = currentCameraState;
		if (lerpDuration > 0f)
		{
			lerpCameraTime = 0f;
			lerpCameraTimeScale = 1f / lerpDuration;
		}
		else
		{
			lerpCameraTime = 1f;
			lerpCameraTimeScale = 0f;
		}
	}

	public void SetOverrideCam(ICameraStateProvider newOverrideCam, float lerpDuration = 1f)
	{
		if (newOverrideCam != overrideCam)
		{
			if (overrideCam != null && newOverrideCam == null)
			{
				cameraMode?.MatchState(in cameraModeContext, in currentCameraState);
			}
			overrideCam = newOverrideCam;
			StartStateLerp(lerpDuration);
		}
	}

	public bool IsOverrideCam(ICameraStateProvider testOverrideCam)
	{
		return overrideCam == testOverrideCam;
	}

	private void Start()
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		if (createHud)
		{
			GameObject val = Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/HUDSimple"));
			hud = val.GetComponent<HUD>();
			hud.cameraRigController = this;
			((Component)hud).GetComponent<Canvas>().worldCamera = uiCam;
			((Component)hud).GetComponent<CrosshairManager>().cameraRigController = this;
			hud.localUserViewer = localUserViewer;
		}
		if (Object.op_Implicit((Object)(object)uiCam))
		{
			((Component)uiCam).transform.parent = null;
			((Component)uiCam).transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
		}
		if (!Object.op_Implicit((Object)(object)DamageNumberManager.instance))
		{
			Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/DamageNumberManager"));
		}
		currentCameraState = new CameraState
		{
			position = ((Component)this).transform.position,
			rotation = ((Component)this).transform.rotation,
			fov = baseFov
		};
		cameraMode = CameraModePlayerBasic.playerBasic;
	}

	private void OnEnable()
	{
		instancesList.Add(this);
		if (Object.op_Implicit((Object)(object)uiCam))
		{
			((Component)uiCam).gameObject.SetActive(true);
		}
		if (Object.op_Implicit((Object)(object)hud))
		{
			((Component)hud).gameObject.SetActive(true);
		}
		CameraRigController.onCameraEnableGlobal?.Invoke(this);
	}

	private void OnDisable()
	{
		CameraRigController.onCameraDisableGlobal?.Invoke(this);
		if (Object.op_Implicit((Object)(object)uiCam))
		{
			((Component)uiCam).gameObject.SetActive(false);
		}
		if (Object.op_Implicit((Object)(object)hud))
		{
			((Component)hud).gameObject.SetActive(false);
		}
		instancesList.Remove(this);
	}

	private void OnDestroy()
	{
		cameraMode = null;
		if (Object.op_Implicit((Object)(object)uiCam))
		{
			Object.Destroy((Object)(object)((Component)uiCam).gameObject);
		}
		if (Object.op_Implicit((Object)(object)hud))
		{
			Object.Destroy((Object)(object)((Component)hud).gameObject);
		}
	}

	private void Update()
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		target = nextTarget;
		if (Object.op_Implicit((Object)(object)targetBody))
		{
			targetTeamIndex = targetBody.teamComponent.teamIndex;
		}
		if (Time.deltaTime == 0f || isCutscene)
		{
			return;
		}
		lerpCameraTime += Time.deltaTime * lerpCameraTimeScale;
		firstPersonTarget = null;
		sceneCam.rect = viewport;
		GenerateCameraModeContext(out cameraModeContext);
		CameraState cameraState = currentCameraState;
		if (cameraMode != null)
		{
			cameraMode.CollectLookInput(in cameraModeContext, out var result);
			CameraModeBase cameraModeBase = cameraMode;
			ref CameraModeBase.CameraModeContext context = ref cameraModeContext;
			CameraModeBase.ApplyLookInputArgs args = new CameraModeBase.ApplyLookInputArgs
			{
				lookInput = result.lookInput
			};
			cameraModeBase.ApplyLookInput(in context, in args);
			cameraMode.Update(in cameraModeContext, out var result2);
			cameraState = result2.cameraState;
			firstPersonTarget = result2.firstPersonTarget;
			crosshairWorldPosition = result2.crosshairWorldPosition;
			SetSprintParticlesActive(result2.showSprintParticles);
		}
		if (Object.op_Implicit((Object)(object)hud))
		{
			CharacterMaster targetMaster = (Object.op_Implicit((Object)(object)targetBody) ? targetBody.master : null);
			hud.targetMaster = targetMaster;
		}
		CameraState cameraState2 = cameraState;
		if (overrideCam != null)
		{
			ICameraStateProvider cameraStateProvider = overrideCam;
			Object val;
			if ((val = (Object)((cameraStateProvider is Object) ? cameraStateProvider : null)) == null || Object.op_Implicit(val))
			{
				overrideCam.GetCameraState(this, ref cameraState2);
			}
			else
			{
				overrideCam = null;
			}
		}
		if (lerpCameraTime >= 1f)
		{
			currentCameraState = cameraState2;
		}
		else
		{
			currentCameraState = CameraState.Lerp(ref lerpCameraState, ref cameraState2, RemapLerpTime(lerpCameraTime));
		}
		SetCameraState(currentCameraState);
	}

	private void GenerateCameraModeContext(out CameraModeBase.CameraModeContext result)
	{
		result.cameraInfo = default(CameraModeBase.CameraInfo);
		result.targetInfo = default(CameraModeBase.TargetInfo);
		result.viewerInfo = default(CameraModeBase.ViewerInfo);
		ref CameraModeBase.CameraInfo cameraInfo = ref result.cameraInfo;
		ref CameraModeBase.TargetInfo targetInfo = ref result.targetInfo;
		ref CameraModeBase.ViewerInfo viewerInfo = ref result.viewerInfo;
		cameraInfo.cameraRigController = this;
		cameraInfo.sceneCam = sceneCam;
		cameraInfo.overrideCam = overrideCam;
		cameraInfo.previousCameraState = currentCameraState;
		cameraInfo.baseFov = baseFov;
		targetInfo.target = (Object.op_Implicit((Object)(object)target) ? target : null);
		targetInfo.body = targetBody;
		GameObject obj = targetInfo.target;
		targetInfo.inputBank = ((obj != null) ? obj.GetComponent<InputBankTest>() : null);
		targetInfo.targetParams = targetParams;
		GameObject obj2 = targetInfo.target;
		targetInfo.teamIndex = ((obj2 == null) ? null : obj2.GetComponent<TeamComponent>()?.teamIndex) ?? TeamIndex.None;
		targetInfo.isSprinting = Object.op_Implicit((Object)(object)targetInfo.body) && targetInfo.body.isSprinting;
		targetInfo.master = targetInfo.body?.master;
		targetInfo.networkUser = targetInfo.master?.playerCharacterMasterController?.networkUser;
		NetworkUser networkUser = targetInfo.networkUser;
		targetInfo.networkedViewAngles = ((networkUser != null) ? ((Component)networkUser).GetComponent<NetworkedViewAngles>() : null);
		targetInfo.isViewerControlled = Object.op_Implicit((Object)(object)targetInfo.networkUser) && targetInfo.networkUser == localUserViewer?.currentNetworkUser;
		viewerInfo.localUser = localUserViewer;
		viewerInfo.userProfile = localUserViewer?.userProfile;
		viewerInfo.inputPlayer = localUserViewer?.inputPlayer;
		viewerInfo.eventSystem = localUserViewer?.eventSystem;
		viewerInfo.hasCursor = Object.op_Implicit((Object)(object)viewerInfo.eventSystem) && viewerInfo.eventSystem.isCursorVisible;
		viewerInfo.isUIFocused = localUserViewer?.isUIFocused ?? false;
	}

	public float Raycast(Ray ray, float maxDistance, float wallCushion)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		RaycastHit[] array = Physics.SphereCastAll(ray, wallCushion, maxDistance, LayerMask.op_Implicit(LayerIndex.world.mask), (QueryTriggerInteraction)1);
		float num = maxDistance;
		for (int i = 0; i < array.Length; i++)
		{
			float distance = ((RaycastHit)(ref array[i])).distance;
			if (distance < num)
			{
				Collider collider = ((RaycastHit)(ref array[i])).collider;
				if (Object.op_Implicit((Object)(object)collider) && !Object.op_Implicit((Object)(object)((Component)collider).GetComponent<NonSolidToCamera>()))
				{
					num = distance;
				}
			}
		}
		return num;
	}

	private static float RemapLerpTime(float t)
	{
		float num = 1f;
		float num2 = 0f;
		float num3 = 1f;
		if ((t /= num / 2f) < 1f)
		{
			return num3 / 2f * t * t + num2;
		}
		return (0f - num3) / 2f * ((t -= 1f) * (t - 2f) - 1f) + num2;
	}

	private void SetCameraState(CameraState cameraState)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		currentCameraState = cameraState;
		float num = ((localUserViewer == null) ? 1f : localUserViewer.userProfile.screenShakeScale);
		Vector3 position = cameraState.position;
		rawScreenShakeDisplacement = ShakeEmitter.ComputeTotalShakeAtPoint(cameraState.position);
		Vector3 val = rawScreenShakeDisplacement * num;
		Vector3 val2 = position + val;
		if (val != Vector3.zero)
		{
			Vector3 val3 = val;
			RaycastHit val4 = default(RaycastHit);
			if (Physics.SphereCast(position, sceneCam.nearClipPlane, val3, ref val4, ((Vector3)(ref val)).magnitude, LayerMask.op_Implicit(LayerIndex.world.mask), (QueryTriggerInteraction)1))
			{
				val2 = position + ((Vector3)(ref val)).normalized * ((RaycastHit)(ref val4)).distance;
			}
		}
		((Component)this).transform.SetPositionAndRotation(val2, cameraState.rotation);
		if (Object.op_Implicit((Object)(object)sceneCam))
		{
			sceneCam.fieldOfView = cameraState.fov;
		}
	}

	public static Ray ModifyAimRayIfApplicable(Ray originalAimRay, GameObject target, out float extraRaycastDistance)
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		CameraRigController cameraRigController = null;
		for (int i = 0; i < readOnlyInstancesList.Count; i++)
		{
			CameraRigController cameraRigController2 = readOnlyInstancesList[i];
			if ((Object)(object)cameraRigController2.target == (Object)(object)target && (Object)(object)cameraRigController2._localUserViewer.cachedBodyObject == (Object)(object)target && !cameraRigController2.hasOverride)
			{
				cameraRigController = cameraRigController2;
				break;
			}
		}
		if (Object.op_Implicit((Object)(object)cameraRigController))
		{
			Camera val = cameraRigController.sceneCam;
			Vector3 val2 = ((Ray)(ref originalAimRay)).origin - ((Component)val).transform.position;
			extraRaycastDistance = ((Vector3)(ref val2)).magnitude;
			return val.ViewportPointToRay(Vector2.op_Implicit(new Vector2(0.5f, 0.5f)));
		}
		extraRaycastDistance = 0f;
		return originalAimRay;
	}

	private void SetSprintParticlesActive(bool newSprintParticlesActive)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)sprintingParticleSystem))
		{
			return;
		}
		MainModule main = sprintingParticleSystem.main;
		if (newSprintParticlesActive)
		{
			((MainModule)(ref main)).loop = true;
			if (!sprintingParticleSystem.isPlaying)
			{
				sprintingParticleSystem.Play();
			}
		}
		else
		{
			((MainModule)(ref main)).loop = false;
		}
	}

	[RuntimeInitializeOnLoadMethod]
	private static void Init()
	{
		SceneCamera.onSceneCameraPreCull += delegate(SceneCamera sceneCam)
		{
			((Component)sceneCam.cameraRigController.sprintingParticleSystem).gameObject.layer = LayerIndex.defaultLayer.intVal;
		};
		SceneCamera.onSceneCameraPostRender += delegate(SceneCamera sceneCam)
		{
			((Component)sceneCam.cameraRigController.sprintingParticleSystem).gameObject.layer = LayerIndex.noDraw.intVal;
		};
	}

	public static bool IsObjectSpectatedByAnyCamera(GameObject gameObject)
	{
		for (int i = 0; i < instancesList.Count; i++)
		{
			if ((Object)(object)instancesList[i].target == (Object)(object)gameObject)
			{
				return true;
			}
		}
		return false;
	}

	[ContextMenu("Print Debug Info")]
	private void PrintDebugInfo()
	{
		Debug.LogFormat("hasOverride={0} overrideCam={1} isControlAllowed={2}", new object[3] { hasOverride, overrideCam, isControlAllowed });
	}
}
