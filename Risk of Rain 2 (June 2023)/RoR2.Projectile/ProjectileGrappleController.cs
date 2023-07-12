using System;
using EntityStates;
using EntityStates.Loader;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2.Projectile;

[RequireComponent(typeof(ProjectileController))]
[RequireComponent(typeof(ProjectileSimple))]
[RequireComponent(typeof(EntityStateMachine))]
[RequireComponent(typeof(ProjectileStickOnImpact))]
public class ProjectileGrappleController : MonoBehaviour
{
	private struct OwnerInfo
	{
		public readonly GameObject gameObject;

		public readonly CharacterBody characterBody;

		public readonly CharacterMotor characterMotor;

		public readonly Rigidbody rigidbody;

		public readonly EntityStateMachine stateMachine;

		public readonly bool hasEffectiveAuthority;

		public OwnerInfo(GameObject ownerGameObject)
		{
			this = default(OwnerInfo);
			gameObject = ownerGameObject;
			if (!Object.op_Implicit((Object)(object)gameObject))
			{
				return;
			}
			characterBody = gameObject.GetComponent<CharacterBody>();
			characterMotor = gameObject.GetComponent<CharacterMotor>();
			rigidbody = gameObject.GetComponent<Rigidbody>();
			hasEffectiveAuthority = Util.HasEffectiveAuthority(gameObject);
			EntityStateMachine[] components = gameObject.GetComponents<EntityStateMachine>();
			for (int i = 0; i < components.Length; i++)
			{
				if (components[i].customName == "Hook")
				{
					stateMachine = components[i];
					break;
				}
			}
		}
	}

	private class BaseState : EntityStates.BaseState
	{
		protected ProjectileGrappleController grappleController;

		protected Vector3 aimOrigin;

		protected Vector3 position;

		protected bool ownerValid { get; private set; }

		protected ref OwnerInfo owner => ref grappleController.owner;

		private void UpdatePositions()
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			aimOrigin = grappleController.owner.characterBody.aimOrigin;
			position = base.transform.position + base.transform.up * grappleController.normalOffset;
		}

		public override void OnEnter()
		{
			base.OnEnter();
			grappleController = GetComponent<ProjectileGrappleController>();
			ownerValid = Object.op_Implicit((Object)(object)grappleController) && Object.op_Implicit((Object)(object)grappleController.owner.gameObject);
			if (ownerValid)
			{
				UpdatePositions();
			}
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (ownerValid)
			{
				ownerValid &= Object.op_Implicit((Object)(object)grappleController.owner.gameObject);
				if (ownerValid)
				{
					UpdatePositions();
					FixedUpdateBehavior();
				}
			}
			if (NetworkServer.active && !ownerValid)
			{
				ownerValid = false;
				EntityState.Destroy((Object)(object)base.gameObject);
			}
		}

		protected virtual void FixedUpdateBehavior()
		{
			if (base.isAuthority && !grappleController.OwnerIsInFiringState())
			{
				outer.SetNextState(new ReturnState());
			}
		}

		protected Ray GetOwnerAimRay()
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			if (!Object.op_Implicit((Object)(object)owner.characterBody))
			{
				return default(Ray);
			}
			return owner.characterBody.inputBank.GetAimRay();
		}
	}

	private class FlyState : BaseState
	{
		private float duration;

		public override void OnEnter()
		{
			base.OnEnter();
			duration = grappleController.maxTravelDistance / ((Component)grappleController).GetComponent<ProjectileSimple>().velocity;
		}

		protected override void FixedUpdateBehavior()
		{
			base.FixedUpdateBehavior();
			if (!base.isAuthority)
			{
				return;
			}
			if (grappleController.projectileStickOnImpactController.stuck)
			{
				EntityState entityState = null;
				if (Object.op_Implicit((Object)(object)grappleController.projectileStickOnImpactController.stuckBody))
				{
					Rigidbody component = ((Component)grappleController.projectileStickOnImpactController.stuckBody).GetComponent<Rigidbody>();
					if (Object.op_Implicit((Object)(object)component) && component.mass < grappleController.yankMassLimit)
					{
						CharacterBody component2 = ((Component)component).GetComponent<CharacterBody>();
						if (!Object.op_Implicit((Object)(object)component2) || !component2.isPlayerControlled || component2.teamComponent.teamIndex != base.projectileController.teamFilter.teamIndex || FriendlyFireManager.ShouldDirectHitProceed(component2.healthComponent, base.projectileController.teamFilter.teamIndex))
						{
							entityState = new YankState();
						}
					}
				}
				if (entityState == null)
				{
					entityState = new GripState();
				}
				DeductOwnerStock();
				outer.SetNextState(entityState);
			}
			else if (duration <= base.fixedAge)
			{
				outer.SetNextState(new ReturnState());
			}
		}

		private void DeductOwnerStock()
		{
			if (!base.ownerValid || !base.owner.hasEffectiveAuthority)
			{
				return;
			}
			SkillLocator component = base.owner.gameObject.GetComponent<SkillLocator>();
			if (Object.op_Implicit((Object)(object)component))
			{
				GenericSkill secondary = component.secondary;
				if (Object.op_Implicit((Object)(object)secondary))
				{
					secondary.DeductStock(1);
				}
			}
		}
	}

	private class BaseGripState : BaseState
	{
		protected float currentDistance;

		public override void OnEnter()
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			base.OnEnter();
			currentDistance = Vector3.Distance(aimOrigin, position);
		}

		protected override void FixedUpdateBehavior()
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			base.FixedUpdateBehavior();
			currentDistance = Vector3.Distance(aimOrigin, position);
			if (base.isAuthority)
			{
				bool flag = !grappleController.projectileStickOnImpactController.stuck;
				bool flag2 = currentDistance < grappleController.nearBreakDistance;
				bool flag3 = !grappleController.OwnerIsInFiringState();
				if (!Object.op_Implicit((Object)(object)base.owner.stateMachine) || !((base.owner.stateMachine.state as BaseSkillState)?.IsKeyDownAuthority() ?? false) || flag3 || flag2 || flag)
				{
					outer.SetNextState(new ReturnState());
				}
			}
		}
	}

	private class GripState : BaseGripState
	{
		private float lastDistance;

		private void DeductStockIfStruckNonPylon()
		{
			GameObject victim = grappleController.projectileStickOnImpactController.victim;
			if (Object.op_Implicit((Object)(object)victim))
			{
				GameObject val = victim;
				EntityLocator component = val.GetComponent<EntityLocator>();
				if (Object.op_Implicit((Object)(object)component))
				{
					val = component.entity;
				}
				Object.op_Implicit((Object)(object)val.GetComponent<ProjectileController>());
			}
		}

		public override void OnEnter()
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			//IL_0086: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			//IL_0098: Unknown result type (might be due to invalid IL or missing references)
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			//IL_009e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
			base.OnEnter();
			lastDistance = Vector3.Distance(aimOrigin, position);
			if (base.ownerValid)
			{
				grappleController.didStick = true;
				if (Object.op_Implicit((Object)(object)base.owner.characterMotor))
				{
					Ray ownerAimRay = GetOwnerAimRay();
					Vector3 direction = ((Ray)(ref ownerAimRay)).direction;
					Vector3 velocity = base.owner.characterMotor.velocity;
					velocity = ((Vector3.Dot(velocity, direction) < 0f) ? Vector3.zero : Vector3.Project(velocity, direction));
					velocity += direction * grappleController.initialLookImpulse;
					velocity += base.owner.characterMotor.moveDirection * grappleController.initiallMoveImpulse;
					base.owner.characterMotor.velocity = velocity;
				}
			}
		}

		protected override void FixedUpdateBehavior()
		{
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			//IL_0091: Unknown result type (might be due to invalid IL or missing references)
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			//IL_009b: Unknown result type (might be due to invalid IL or missing references)
			//IL_009f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_0102: Unknown result type (might be due to invalid IL or missing references)
			//IL_0105: Unknown result type (might be due to invalid IL or missing references)
			//IL_011b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0120: Unknown result type (might be due to invalid IL or missing references)
			//IL_0125: Unknown result type (might be due to invalid IL or missing references)
			//IL_0126: Unknown result type (might be due to invalid IL or missing references)
			//IL_0132: Unknown result type (might be due to invalid IL or missing references)
			//IL_0142: Unknown result type (might be due to invalid IL or missing references)
			//IL_0147: Unknown result type (might be due to invalid IL or missing references)
			//IL_014c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0158: Unknown result type (might be due to invalid IL or missing references)
			//IL_016f: Unknown result type (might be due to invalid IL or missing references)
			base.FixedUpdateBehavior();
			float num = grappleController.acceleration;
			if (currentDistance > lastDistance)
			{
				num *= grappleController.escapeForceMultiplier;
			}
			lastDistance = currentDistance;
			if (base.owner.hasEffectiveAuthority && Object.op_Implicit((Object)(object)base.owner.characterMotor) && Object.op_Implicit((Object)(object)base.owner.characterBody))
			{
				Ray ownerAimRay = GetOwnerAimRay();
				Vector3 val = base.transform.position - base.owner.characterBody.aimOrigin;
				Vector3 normalized = ((Vector3)(ref val)).normalized;
				Vector3 val2 = normalized * num;
				float num2 = Mathf.Clamp01(base.fixedAge / grappleController.lookAccelerationRampUpDuration);
				float num3 = grappleController.lookAccelerationRampUpCurve.Evaluate(num2);
				float num4 = Util.Remap(Vector3.Dot(((Ray)(ref ownerAimRay)).direction, normalized), -1f, 1f, 1f, 0f);
				val2 += ((Ray)(ref ownerAimRay)).direction * (grappleController.lookAcceleration * num3 * num4);
				val2 += base.owner.characterMotor.moveDirection * grappleController.moveAcceleration;
				base.owner.characterMotor.ApplyForce(val2 * (base.owner.characterMotor.mass * Time.fixedDeltaTime), alwaysApply: true, disableAirControlUntilCollision: true);
			}
		}
	}

	private class YankState : BaseGripState
	{
		public static float yankSpeed;

		public static float delayBeforeYanking;

		public static float hoverTimeLimit = 0.5f;

		private CharacterBody stuckBody;

		public override void OnEnter()
		{
			base.OnEnter();
			stuckBody = grappleController.projectileStickOnImpactController.stuckBody;
		}

		protected override void FixedUpdateBehavior()
		{
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Expected O, but got Unknown
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
			base.FixedUpdateBehavior();
			if (!Object.op_Implicit((Object)(object)stuckBody))
			{
				return;
			}
			if (Util.HasEffectiveAuthority(((Component)stuckBody).gameObject))
			{
				Vector3 val = aimOrigin - position;
				IDisplacementReceiver component = ((Component)stuckBody).GetComponent<IDisplacementReceiver>();
				if (Object.op_Implicit((Object)(Component)component) && base.fixedAge >= delayBeforeYanking)
				{
					component.AddDisplacement(val * (yankSpeed * Time.fixedDeltaTime));
				}
			}
			if (base.owner.hasEffectiveAuthority && Object.op_Implicit((Object)(object)base.owner.characterMotor) && base.fixedAge < hoverTimeLimit)
			{
				Vector3 velocity = base.owner.characterMotor.velocity;
				if (velocity.y < 0f)
				{
					velocity.y = 0f;
					base.owner.characterMotor.velocity = velocity;
				}
			}
		}
	}

	private class ReturnState : BaseState
	{
		private float returnSpeedAcceleration = 240f;

		private float returnSpeed;

		public override void OnEnter()
		{
			base.OnEnter();
			if (base.ownerValid)
			{
				returnSpeed = grappleController.projectileSimple.velocity;
				returnSpeedAcceleration = returnSpeed * 2f;
			}
			if (NetworkServer.active && Object.op_Implicit((Object)(object)grappleController))
			{
				grappleController.projectileStickOnImpactController.Detach();
				grappleController.projectileStickOnImpactController.ignoreCharacters = true;
				grappleController.projectileStickOnImpactController.ignoreWorld = true;
			}
			Collider component = GetComponent<Collider>();
			if (Object.op_Implicit((Object)(object)component))
			{
				component.enabled = false;
			}
		}

		protected override void FixedUpdateBehavior()
		{
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_008e: Unknown result type (might be due to invalid IL or missing references)
			base.FixedUpdateBehavior();
			if (!Object.op_Implicit((Object)(object)base.rigidbody))
			{
				return;
			}
			returnSpeed += returnSpeedAcceleration * Time.fixedDeltaTime;
			Rigidbody obj = base.rigidbody;
			Vector3 val = aimOrigin - position;
			obj.velocity = ((Vector3)(ref val)).normalized * returnSpeed;
			if (NetworkServer.active)
			{
				Vector3 endPosition = position + base.rigidbody.velocity * Time.fixedDeltaTime;
				if (HGMath.Overshoots(position, endPosition, aimOrigin))
				{
					EntityState.Destroy((Object)(object)base.gameObject);
				}
			}
		}
	}

	private ProjectileController projectileController;

	private ProjectileStickOnImpact projectileStickOnImpactController;

	private ProjectileSimple projectileSimple;

	public SerializableEntityStateType ownerHookStateType;

	public float acceleration;

	public float lookAcceleration = 4f;

	public float lookAccelerationRampUpDuration = 0.25f;

	public float initialLookImpulse = 5f;

	public float initiallMoveImpulse = 5f;

	public float moveAcceleration = 4f;

	public string enterSoundString;

	public string exitSoundString;

	public string hookDistanceRTPCstring;

	public float minHookDistancePitchModifier;

	public float maxHookDistancePitchModifier;

	public AnimationCurve lookAccelerationRampUpCurve;

	public Transform ropeEndTransform;

	public string muzzleStringOnBody = "MuzzleLeft";

	[Tooltip("The minimum distance the hook can be from the target before it detaches.")]
	public float nearBreakDistance;

	[Tooltip("The maximum distance this hook can travel.")]
	public float maxTravelDistance;

	public float escapeForceMultiplier = 2f;

	public float normalOffset = 1f;

	public float yankMassLimit;

	private Type resolvedOwnerHookStateType;

	private OwnerInfo owner;

	private bool didStick;

	private uint soundID;

	private void Awake()
	{
		projectileStickOnImpactController = ((Component)this).GetComponent<ProjectileStickOnImpact>();
		projectileController = ((Component)this).GetComponent<ProjectileController>();
		projectileSimple = ((Component)this).GetComponent<ProjectileSimple>();
		resolvedOwnerHookStateType = ownerHookStateType.stateType;
		if (Object.op_Implicit((Object)(object)ropeEndTransform))
		{
			soundID = Util.PlaySound(enterSoundString, ((Component)ropeEndTransform).gameObject);
		}
	}

	private void FixedUpdate()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)ropeEndTransform))
		{
			Vector3 val = ((Component)ropeEndTransform).transform.position - ((Component)this).transform.position;
			float num = Util.Remap(((Vector3)(ref val)).magnitude, minHookDistancePitchModifier, maxHookDistancePitchModifier, 0f, 100f);
			AkSoundEngine.SetRTPCValueByPlayingID(hookDistanceRTPCstring, num, soundID);
		}
	}

	private void AssignHookReferenceToBodyStateMachine()
	{
		if (Object.op_Implicit((Object)(object)owner.stateMachine) && owner.stateMachine.state is FireHook fireHook)
		{
			fireHook.SetHookReference(((Component)this).gameObject);
		}
		Transform modelTransform = owner.gameObject.GetComponent<ModelLocator>().modelTransform;
		if (!Object.op_Implicit((Object)(object)modelTransform))
		{
			return;
		}
		ChildLocator component = ((Component)modelTransform).GetComponent<ChildLocator>();
		if (Object.op_Implicit((Object)(object)component))
		{
			Transform val = component.FindChild(muzzleStringOnBody);
			if (Object.op_Implicit((Object)(object)val))
			{
				ropeEndTransform.SetParent(val, false);
			}
		}
	}

	private void Start()
	{
		owner = new OwnerInfo(projectileController.owner);
		AssignHookReferenceToBodyStateMachine();
	}

	private void OnDestroy()
	{
		if (Object.op_Implicit((Object)(object)ropeEndTransform))
		{
			Util.PlaySound(exitSoundString, ((Component)ropeEndTransform).gameObject);
			Object.Destroy((Object)(object)((Component)ropeEndTransform).gameObject);
		}
		else
		{
			AkSoundEngine.StopPlayingID(soundID);
		}
	}

	private bool OwnerIsInFiringState()
	{
		if (Object.op_Implicit((Object)(object)owner.stateMachine))
		{
			return owner.stateMachine.state.GetType() == resolvedOwnerHookStateType;
		}
		return false;
	}
}
