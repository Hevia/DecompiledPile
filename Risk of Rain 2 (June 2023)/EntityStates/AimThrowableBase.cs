using System;
using RoR2;
using RoR2.Projectile;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace EntityStates;

public abstract class AimThrowableBase : BaseSkillState
{
	private struct CalculateArcPointsJob : IJobParallelFor, IDisposable
	{
		[ReadOnly]
		private Vector3 origin;

		[ReadOnly]
		private Vector3 velocity;

		[ReadOnly]
		private float indexMultiplier;

		[ReadOnly]
		private float gravity;

		[WriteOnly]
		public NativeArray<Vector3> outputPositions;

		public void SetParameters(Vector3 origin, Vector3 velocity, float totalTravelTime, int positionCount, float gravity)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			this.origin = origin;
			this.velocity = velocity;
			if (outputPositions.Length != positionCount)
			{
				if (outputPositions.IsCreated)
				{
					outputPositions.Dispose();
				}
				outputPositions = new NativeArray<Vector3>(positionCount, (Allocator)4, (NativeArrayOptions)0);
			}
			indexMultiplier = totalTravelTime / (float)(positionCount - 1);
			this.gravity = gravity;
		}

		public void Dispose()
		{
			if (outputPositions.IsCreated)
			{
				outputPositions.Dispose();
			}
		}

		public void Execute(int index)
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			float t = (float)index * indexMultiplier;
			outputPositions[index] = Trajectory.CalculatePositionAtTime(origin, velocity, t, gravity);
		}
	}

	protected struct TrajectoryInfo
	{
		public Ray finalRay;

		public Vector3 hitPoint;

		public Vector3 hitNormal;

		public float travelTime;

		public float speedOverride;
	}

	[SerializeField]
	public float maxDistance;

	[SerializeField]
	public float rayRadius;

	[SerializeField]
	public GameObject arcVisualizerPrefab;

	[SerializeField]
	public GameObject projectilePrefab;

	[SerializeField]
	public GameObject endpointVisualizerPrefab;

	[SerializeField]
	public float endpointVisualizerRadiusScale;

	[SerializeField]
	public bool setFuse;

	[SerializeField]
	public float damageCoefficient;

	[SerializeField]
	public float baseMinimumDuration;

	protected LineRenderer arcVisualizerLineRenderer;

	protected Transform endpointVisualizerTransform;

	protected float projectileBaseSpeed;

	protected float detonationRadius;

	protected float minimumDuration;

	protected bool useGravity;

	private CalculateArcPointsJob calculateArcPointsJob;

	private JobHandle calculateArcPointsJobHandle;

	private Vector3[] pointsBuffer = Array.Empty<Vector3>();

	private Action completeArcPointsVisualizerJobMethod;

	protected TrajectoryInfo currentTrajectoryInfo;

	public override void OnEnter()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		if (Object.op_Implicit((Object)(object)arcVisualizerPrefab))
		{
			arcVisualizerLineRenderer = Object.Instantiate<GameObject>(arcVisualizerPrefab, base.transform.position, Quaternion.identity).GetComponent<LineRenderer>();
			calculateArcPointsJob = default(CalculateArcPointsJob);
			completeArcPointsVisualizerJobMethod = CompleteArcVisualizerJob;
			RoR2Application.onLateUpdate += completeArcPointsVisualizerJobMethod;
		}
		if (Object.op_Implicit((Object)(object)endpointVisualizerPrefab))
		{
			endpointVisualizerTransform = Object.Instantiate<GameObject>(endpointVisualizerPrefab, base.transform.position, Quaternion.identity).transform;
		}
		if (Object.op_Implicit((Object)(object)base.characterBody))
		{
			base.characterBody.hideCrosshair = true;
		}
		ProjectileSimple component = projectilePrefab.GetComponent<ProjectileSimple>();
		if (Object.op_Implicit((Object)(object)component))
		{
			projectileBaseSpeed = component.velocity;
		}
		Rigidbody component2 = projectilePrefab.GetComponent<Rigidbody>();
		if (Object.op_Implicit((Object)(object)component2))
		{
			useGravity = component2.useGravity;
		}
		minimumDuration = baseMinimumDuration / attackSpeedStat;
		ProjectileImpactExplosion component3 = projectilePrefab.GetComponent<ProjectileImpactExplosion>();
		if (Object.op_Implicit((Object)(object)component3))
		{
			detonationRadius = component3.blastRadius;
			if (Object.op_Implicit((Object)(object)endpointVisualizerTransform))
			{
				endpointVisualizerTransform.localScale = new Vector3(detonationRadius, detonationRadius, detonationRadius);
			}
		}
		UpdateVisualizers(currentTrajectoryInfo);
		SceneCamera.onSceneCameraPreRender += OnPreRenderSceneCam;
	}

	public override void OnExit()
	{
		SceneCamera.onSceneCameraPreRender -= OnPreRenderSceneCam;
		if (!outer.destroying)
		{
			if (base.isAuthority)
			{
				FireProjectile();
			}
			OnProjectileFiredLocal();
		}
		if (Object.op_Implicit((Object)(object)base.characterBody))
		{
			base.characterBody.hideCrosshair = false;
		}
		((JobHandle)(ref calculateArcPointsJobHandle)).Complete();
		if (Object.op_Implicit((Object)(object)arcVisualizerLineRenderer))
		{
			EntityState.Destroy((Object)(object)((Component)arcVisualizerLineRenderer).gameObject);
			arcVisualizerLineRenderer = null;
		}
		if (completeArcPointsVisualizerJobMethod != null)
		{
			RoR2Application.onLateUpdate -= completeArcPointsVisualizerJobMethod;
			completeArcPointsVisualizerJobMethod = null;
		}
		calculateArcPointsJob.Dispose();
		pointsBuffer = Array.Empty<Vector3>();
		if (Object.op_Implicit((Object)(object)endpointVisualizerTransform))
		{
			EntityState.Destroy((Object)(object)((Component)endpointVisualizerTransform).gameObject);
			endpointVisualizerTransform = null;
		}
		base.OnExit();
	}

	protected virtual bool KeyIsDown()
	{
		return IsKeyDownAuthority();
	}

	protected virtual void OnProjectileFiredLocal()
	{
	}

	protected virtual void FireProjectile()
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		FireProjectileInfo fireProjectileInfo = default(FireProjectileInfo);
		fireProjectileInfo.crit = RollCrit();
		fireProjectileInfo.owner = base.gameObject;
		fireProjectileInfo.position = ((Ray)(ref currentTrajectoryInfo.finalRay)).origin;
		fireProjectileInfo.projectilePrefab = projectilePrefab;
		fireProjectileInfo.rotation = Util.QuaternionSafeLookRotation(((Ray)(ref currentTrajectoryInfo.finalRay)).direction, Vector3.up);
		fireProjectileInfo.speedOverride = currentTrajectoryInfo.speedOverride;
		fireProjectileInfo.damage = damageCoefficient * damageStat;
		FireProjectileInfo fireProjectileInfo2 = fireProjectileInfo;
		if (setFuse)
		{
			fireProjectileInfo2.fuseOverride = currentTrajectoryInfo.travelTime;
		}
		ModifyProjectile(ref fireProjectileInfo2);
		ProjectileManager.instance.FireProjectile(fireProjectileInfo2);
	}

	protected virtual void ModifyProjectile(ref FireProjectileInfo fireProjectileInfo)
	{
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.isAuthority && !KeyIsDown() && base.fixedAge >= minimumDuration)
		{
			UpdateTrajectoryInfo(out currentTrajectoryInfo);
			EntityState entityState = PickNextState();
			if (entityState != null)
			{
				outer.SetNextState(entityState);
			}
			else
			{
				outer.SetNextStateToMain();
			}
		}
	}

	protected virtual EntityState PickNextState()
	{
		return null;
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.Skill;
	}

	public override void Update()
	{
		base.Update();
		if (CameraRigController.IsObjectSpectatedByAnyCamera(base.gameObject))
		{
			UpdateTrajectoryInfo(out currentTrajectoryInfo);
			UpdateVisualizers(currentTrajectoryInfo);
		}
	}

	protected virtual void UpdateTrajectoryInfo(out TrajectoryInfo dest)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		dest = default(TrajectoryInfo);
		Ray aimRay = GetAimRay();
		RaycastHit hitInfo = default(RaycastHit);
		bool flag = false;
		if (rayRadius > 0f && Util.CharacterSpherecast(base.gameObject, aimRay, rayRadius, out hitInfo, maxDistance, LayerIndex.CommonMasks.bullet, (QueryTriggerInteraction)0) && Object.op_Implicit((Object)(object)((Component)((RaycastHit)(ref hitInfo)).collider).GetComponent<HurtBox>()))
		{
			flag = true;
		}
		if (!flag)
		{
			flag = Util.CharacterRaycast(base.gameObject, aimRay, out hitInfo, maxDistance, LayerIndex.CommonMasks.bullet, (QueryTriggerInteraction)0);
		}
		if (flag)
		{
			dest.hitPoint = ((RaycastHit)(ref hitInfo)).point;
			dest.hitNormal = ((RaycastHit)(ref hitInfo)).normal;
		}
		else
		{
			dest.hitPoint = aimRay.GetPoint(maxDistance);
			dest.hitNormal = -aimRay.direction;
		}
		Vector3 val = dest.hitPoint - aimRay.origin;
		if (useGravity)
		{
			float num = projectileBaseSpeed;
			Vector2 val2 = default(Vector2);
			((Vector2)(ref val2))._002Ector(val.x, val.z);
			float magnitude = ((Vector2)(ref val2)).magnitude;
			float num2 = Trajectory.CalculateInitialYSpeed(magnitude / num, val.y);
			Vector3 val3 = default(Vector3);
			((Vector3)(ref val3))._002Ector(val2.x / magnitude * num, num2, val2.y / magnitude * num);
			dest.speedOverride = ((Vector3)(ref val3)).magnitude;
			dest.finalRay = new Ray(aimRay.origin, val3 / dest.speedOverride);
			dest.travelTime = Trajectory.CalculateGroundTravelTime(num, magnitude);
		}
		else
		{
			dest.speedOverride = projectileBaseSpeed;
			dest.finalRay = aimRay;
			dest.travelTime = projectileBaseSpeed / ((Vector3)(ref val)).magnitude;
		}
	}

	private void CompleteArcVisualizerJob()
	{
		((JobHandle)(ref calculateArcPointsJobHandle)).Complete();
		if (Object.op_Implicit((Object)(object)arcVisualizerLineRenderer))
		{
			Array.Resize(ref pointsBuffer, calculateArcPointsJob.outputPositions.Length);
			calculateArcPointsJob.outputPositions.CopyTo(pointsBuffer);
			arcVisualizerLineRenderer.SetPositions(pointsBuffer);
		}
	}

	private void UpdateVisualizers(TrajectoryInfo trajectoryInfo)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)arcVisualizerLineRenderer) && ((JobHandle)(ref calculateArcPointsJobHandle)).IsCompleted)
		{
			calculateArcPointsJob.SetParameters(((Ray)(ref trajectoryInfo.finalRay)).origin, ((Ray)(ref trajectoryInfo.finalRay)).direction * trajectoryInfo.speedOverride, trajectoryInfo.travelTime, arcVisualizerLineRenderer.positionCount, useGravity ? Physics.gravity.y : 0f);
			calculateArcPointsJobHandle = IJobParallelForExtensions.Schedule<CalculateArcPointsJob>(calculateArcPointsJob, calculateArcPointsJob.outputPositions.Length, 32, default(JobHandle));
		}
		if (Object.op_Implicit((Object)(object)endpointVisualizerTransform))
		{
			endpointVisualizerTransform.SetPositionAndRotation(trajectoryInfo.hitPoint, Util.QuaternionSafeLookRotation(trajectoryInfo.hitNormal));
			if (!endpointVisualizerRadiusScale.Equals(0f))
			{
				endpointVisualizerTransform.localScale = new Vector3(endpointVisualizerRadiusScale, endpointVisualizerRadiusScale, endpointVisualizerRadiusScale);
			}
		}
	}

	private void OnPreRenderSceneCam(SceneCamera sceneCam)
	{
		if (Object.op_Implicit((Object)(object)arcVisualizerLineRenderer))
		{
			((Renderer)arcVisualizerLineRenderer).renderingLayerMask = (((Object)(object)sceneCam.cameraRigController.target == (Object)(object)base.gameObject) ? 1u : 0u);
		}
		if (Object.op_Implicit((Object)(object)endpointVisualizerTransform))
		{
			((Component)endpointVisualizerTransform).gameObject.layer = (((Object)(object)sceneCam.cameraRigController.target == (Object)(object)base.gameObject) ? LayerIndex.defaultLayer.intVal : LayerIndex.noDraw.intVal);
		}
	}
}
