using System;
using System.Collections.ObjectModel;
using RoR2.WwiseUtils;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace RoR2;

public class MusicController : MonoBehaviour
{
	public delegate void PickTrackDelegate(MusicController musicController, ref MusicTrackDef newTrack);

	private struct EnemyInfo
	{
		public Ray aimRay;

		public float lookScore;

		public float proximityScore;

		public float threatScore;
	}

	private struct CalculateIntensityJob : IJobParallelFor
	{
		[ReadOnly]
		public Vector3 targetPosition;

		[ReadOnly]
		public int elementCount;

		public NativeArray<EnemyInfo> enemyInfoBuffer;

		[ReadOnly]
		public float nearDistance;

		[ReadOnly]
		public float farDistance;

		public void Execute(int i)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			EnemyInfo enemyInfo = enemyInfoBuffer[i];
			Vector3 val = targetPosition - ((Ray)(ref enemyInfo.aimRay)).origin;
			float magnitude = ((Vector3)(ref val)).magnitude;
			float num = Mathf.Clamp01(Vector3.Dot(val / magnitude, ((Ray)(ref enemyInfo.aimRay)).direction));
			float num2 = Mathf.Clamp01(Mathf.InverseLerp(farDistance, nearDistance, magnitude));
			enemyInfo.lookScore = num * enemyInfo.threatScore;
			enemyInfo.proximityScore = num2 * enemyInfo.threatScore;
			enemyInfoBuffer[i] = enemyInfo;
		}

		public void CalculateSum(out float proximityScore, out float lookScore)
		{
			proximityScore = 0f;
			lookScore = 0f;
			for (int i = 0; i < elementCount; i++)
			{
				proximityScore += enemyInfoBuffer[i].proximityScore;
				lookScore += enemyInfoBuffer[i].lookScore;
			}
		}
	}

	public GameObject target;

	public bool enableMusicSystem = true;

	private CameraRigController targetCamera;

	private MusicTrackDef _currentTrack;

	private RtpcSetter rtpcPlayerHealthValue;

	private RtpcSetter rtpcEnemyValue;

	private RtpcSetter rtpcTeleporterProximityValue;

	private RtpcSetter rtpcTeleporterDirectionValue;

	private RtpcSetter rtpcTeleporterPlayerStatus;

	private StateSetter stBossStatus;

	private bool wasPaused;

	private NativeArray<EnemyInfo> enemyInfoBuffer;

	private CalculateIntensityJob calculateIntensityJob;

	private JobHandle calculateIntensityJobHandle;

	private MusicTrackDef currentTrack
	{
		get
		{
			return _currentTrack;
		}
		set
		{
			if (_currentTrack != value)
			{
				if (_currentTrack != null)
				{
					_currentTrack.Stop();
				}
				_currentTrack = value;
				if (_currentTrack != null)
				{
					_currentTrack.Play();
				}
			}
		}
	}

	public static event PickTrackDelegate pickTrackHook;

	private void InitializeEngineDependentValues()
	{
		rtpcPlayerHealthValue = new RtpcSetter("playerHealth");
		rtpcEnemyValue = new RtpcSetter("enemyValue");
		rtpcTeleporterProximityValue = new RtpcSetter("teleporterProximity");
		rtpcTeleporterDirectionValue = new RtpcSetter("teleporterDirection");
		rtpcTeleporterPlayerStatus = new RtpcSetter("teleporterPlayerStatus");
		stBossStatus = new StateSetter("bossStatus");
	}

	private void Start()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		enemyInfoBuffer = new NativeArray<EnemyInfo>(64, (Allocator)4, (NativeArrayOptions)1);
		InitializeEngineDependentValues();
		if (enableMusicSystem)
		{
			AkSoundEngine.PostEvent("Play_Music_System", ((Component)this).gameObject);
		}
	}

	private void Update()
	{
		UpdateState();
		targetCamera = ((CameraRigController.readOnlyInstancesList.Count > 0) ? CameraRigController.readOnlyInstancesList[0] : null);
		target = (Object.op_Implicit((Object)(object)targetCamera) ? targetCamera.target : null);
		ScheduleIntensityCalculation(target);
	}

	private void RecalculateHealth(GameObject playerObject)
	{
		rtpcPlayerHealthValue.value = 100f;
		if (!Object.op_Implicit((Object)(object)target))
		{
			return;
		}
		CharacterBody component = target.GetComponent<CharacterBody>();
		if (!Object.op_Implicit((Object)(object)component))
		{
			return;
		}
		if (component.HasBuff(JunkContent.Buffs.Deafened))
		{
			rtpcPlayerHealthValue.value = -100f;
			return;
		}
		HealthComponent healthComponent = component.healthComponent;
		if (Object.op_Implicit((Object)(object)healthComponent))
		{
			rtpcPlayerHealthValue.value = healthComponent.combinedHealthFraction * 100f;
		}
	}

	private void UpdateTeleporterParameters(TeleporterInteraction teleporter, Transform cameraTransform, CharacterBody targetBody)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		float num = float.PositiveInfinity;
		float value = 0f;
		bool flag = true;
		stBossStatus.valueId = CommonWwiseIds.alive;
		if (Object.op_Implicit((Object)(object)teleporter))
		{
			if (Object.op_Implicit((Object)(object)cameraTransform))
			{
				Vector3 position = cameraTransform.position;
				Vector3 forward = cameraTransform.forward;
				Vector3 val = ((Component)teleporter).transform.position - position;
				float num2 = Vector2.SignedAngle(new Vector2(val.x, val.z), new Vector2(forward.x, forward.z));
				if (num2 < 0f)
				{
					num2 += 360f;
				}
				num = ((Vector3)(ref val)).magnitude;
				value = num2;
			}
			if (TeleporterInteraction.instance.isIdleToCharging || TeleporterInteraction.instance.isCharging)
			{
				stBossStatus.valueId = CommonWwiseIds.alive;
			}
			else if (TeleporterInteraction.instance.isCharged)
			{
				stBossStatus.valueId = CommonWwiseIds.dead;
			}
			if (teleporter.isCharging && Object.op_Implicit((Object)(object)targetBody))
			{
				flag = teleporter.holdoutZoneController.IsBodyInChargingRadius(targetBody);
			}
		}
		num = Mathf.Clamp(num, 20f, 250f);
		rtpcTeleporterProximityValue.value = Util.Remap(num, 20f, 250f, 0f, 10000f);
		rtpcTeleporterDirectionValue.value = value;
		rtpcTeleporterPlayerStatus.value = (flag ? 1f : 0f);
	}

	private void LateUpdate()
	{
		bool flag = Time.timeScale == 0f;
		if (wasPaused != flag)
		{
			AkSoundEngine.PostEvent(flag ? "Pause_Music" : "Unpause_Music", ((Component)this).gameObject);
			wasPaused = flag;
		}
		RecalculateHealth(target);
		UpdateTeleporterParameters(TeleporterInteraction.instance, Object.op_Implicit((Object)(object)targetCamera) ? ((Component)targetCamera).transform : null, Object.op_Implicit((Object)(object)target) ? target.GetComponent<CharacterBody>() : null);
		((JobHandle)(ref calculateIntensityJobHandle)).Complete();
		calculateIntensityJob.CalculateSum(out var proximityScore, out var lookScore);
		float num = 0.025f;
		Mathf.Clamp(1f - rtpcPlayerHealthValue.value * 0.01f, 0.25f, 0.75f);
		float value = (proximityScore * 0.75f + lookScore * 0.25f) * num;
		rtpcEnemyValue.value = value;
		FlushValuesToEngine();
	}

	private void FlushValuesToEngine()
	{
		stBossStatus.FlushIfChanged();
		rtpcPlayerHealthValue.FlushIfChanged();
		rtpcTeleporterProximityValue.FlushIfChanged();
		rtpcTeleporterDirectionValue.FlushIfChanged();
		rtpcTeleporterPlayerStatus.FlushIfChanged();
		rtpcEnemyValue.FlushIfChanged();
	}

	private void PickCurrentTrack(ref MusicTrackDef newTrack)
	{
		SceneDef mostRecentSceneDef = SceneCatalog.mostRecentSceneDef;
		bool flag = false;
		if (Object.op_Implicit((Object)(object)TeleporterInteraction.instance) && !TeleporterInteraction.instance.isIdle)
		{
			flag = true;
		}
		if (Object.op_Implicit((Object)(object)mostRecentSceneDef))
		{
			if (flag && Object.op_Implicit((Object)(object)mostRecentSceneDef.bossTrack))
			{
				newTrack = mostRecentSceneDef.bossTrack;
			}
			else if (Object.op_Implicit((Object)(object)mostRecentSceneDef.mainTrack))
			{
				newTrack = mostRecentSceneDef.mainTrack;
			}
		}
		MusicController.pickTrackHook?.Invoke(this, ref newTrack);
	}

	private void UpdateState()
	{
		MusicTrackDef newTrack = currentTrack;
		PickCurrentTrack(ref newTrack);
		currentTrack = newTrack;
	}

	private void EnsureEnemyBufferSize(int requiredSize)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		if (enemyInfoBuffer.Length < requiredSize)
		{
			if (enemyInfoBuffer.Length != 0)
			{
				enemyInfoBuffer.Dispose();
			}
			enemyInfoBuffer = new NativeArray<EnemyInfo>(requiredSize, (Allocator)4, (NativeArrayOptions)1);
		}
	}

	private void OnDestroy()
	{
		enemyInfoBuffer.Dispose();
	}

	private void ScheduleIntensityCalculation(GameObject targetBodyObject)
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)targetBodyObject))
		{
			return;
		}
		ReadOnlyCollection<TeamComponent> teamMembers = TeamComponent.GetTeamMembers(TeamIndex.Monster);
		int count = teamMembers.Count;
		EnsureEnemyBufferSize(count);
		int num = 0;
		int i = 0;
		for (int num2 = count; i < num2; i++)
		{
			TeamComponent teamComponent = teamMembers[i];
			InputBankTest component = ((Component)teamComponent).GetComponent<InputBankTest>();
			CharacterBody component2 = ((Component)teamComponent).GetComponent<CharacterBody>();
			if (Object.op_Implicit((Object)(object)component))
			{
				enemyInfoBuffer[num++] = new EnemyInfo
				{
					aimRay = new Ray(component.aimOrigin, component.aimDirection),
					threatScore = (Object.op_Implicit((Object)(object)component2.master) ? component2.GetNormalizedThreatValue() : 0f)
				};
			}
		}
		calculateIntensityJob = new CalculateIntensityJob
		{
			enemyInfoBuffer = enemyInfoBuffer,
			elementCount = num,
			targetPosition = targetBodyObject.transform.position,
			nearDistance = 20f,
			farDistance = 75f
		};
		calculateIntensityJobHandle = IJobParallelForExtensions.Schedule<CalculateIntensityJob>(calculateIntensityJob, num, 32, default(JobHandle));
	}

	[RuntimeInitializeOnLoadMethod(/*Could not decode attribute arguments.*/)]
	private static void Init()
	{
		RoR2Application.onLoad = (Action)Delegate.Combine(RoR2Application.onLoad, (Action)delegate
		{
			Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/MusicController"), ((Component)RoR2Application.instance).transform);
		});
	}
}
