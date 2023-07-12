using System;
using System.Collections.Generic;
using UnityEngine;

namespace RoR2;

[RequireComponent(typeof(Animator))]
public class AimAnimator : MonoBehaviour, ILifeBehavior
{
	public enum AimType
	{
		Direct,
		Smart
	}

	public class DirectionOverrideRequest : IDisposable
	{
		public readonly Func<Vector3> directionGetter;

		private Action<DirectionOverrideRequest> disposeCallback;

		public DirectionOverrideRequest(Func<Vector3> getter, Action<DirectionOverrideRequest> onDispose)
		{
			disposeCallback = onDispose;
			directionGetter = getter;
		}

		public void Dispose()
		{
			disposeCallback?.Invoke(this);
			disposeCallback = null;
		}
	}

	private struct AimAngles
	{
		public float pitch;

		public float yaw;
	}

	[Tooltip("The input bank component of the character.")]
	public InputBankTest inputBank;

	[Tooltip("The direction component of the character.")]
	public CharacterDirection directionComponent;

	[Tooltip("The minimum pitch supplied by the aiming animation.")]
	public float pitchRangeMin;

	[Tooltip("The maximum pitch supplied by the aiming animation.")]
	public float pitchRangeMax;

	[Tooltip("The minimum yaw supplied by the aiming animation.")]
	public float yawRangeMin;

	[Tooltip("The maximum yaw supplied by the aiming animation.")]
	public float yawRangeMax;

	[Tooltip("If the pitch is this many degrees beyond the range the aiming animations support, the character will return to neutral pose after waiting the giveup duration.")]
	public float pitchGiveupRange;

	[Tooltip("If the yaw is this many degrees beyond the range the aiming animations support, the character will return to neutral pose after waiting the giveup duration.")]
	public float yawGiveupRange;

	[Tooltip("If the pitch or yaw exceed the range supported by the aiming animations, the character will return to neutral pose after waiting this long.")]
	public float giveupDuration;

	[Tooltip("The speed in degrees/second to approach the desired pitch/yaw by while the weapon should be raised.")]
	public float raisedApproachSpeed = 720f;

	[Tooltip("The speed in degrees/second to approach the desired pitch/yaw by while the weapon should be lowered.")]
	public float loweredApproachSpeed = 360f;

	[Tooltip("The smoothing time for the motion.")]
	public float smoothTime = 0.1f;

	[Tooltip("Whether or not the character can do full 360 yaw turns.")]
	public bool fullYaw;

	[Tooltip("Switches between Direct (point straight at target) or Smart (only turn when outside angle range).")]
	public AimType aimType;

	[Tooltip("Assigns the weight of the aim from the center as an animator value 'aimWeight' between 0-1.")]
	public bool enableAimWeight;

	public bool UseTransformedAimVector;

	private Animator animatorComponent;

	private float pitchClipCycleEnd;

	private float yawClipCycleEnd;

	private float giveupTimer;

	private AimAngles localAnglesToAimVector;

	private AimAngles overshootAngles;

	private AimAngles clampedLocalAnglesToAimVector;

	private AimAngles currentLocalAngles;

	private AimAngles smoothingVelocity;

	private List<DirectionOverrideRequest> directionOverrideRequests = new List<DirectionOverrideRequest>();

	private static readonly int aimPitchCycleHash = Animator.StringToHash("aimPitchCycle");

	private static readonly int aimYawCycleHash = Animator.StringToHash("aimYawCycle");

	private static readonly int aimWeightHash = Animator.StringToHash("aimWeight");

	public bool isOutsideOfRange { get; private set; }

	private bool shouldGiveup => giveupTimer <= 0f;

	public DirectionOverrideRequest RequestDirectionOverride(Func<Vector3> getter)
	{
		DirectionOverrideRequest directionOverrideRequest = new DirectionOverrideRequest(getter, RemoveRequest);
		directionOverrideRequests.Add(directionOverrideRequest);
		return directionOverrideRequest;
	}

	private void RemoveRequest(DirectionOverrideRequest request)
	{
		directionOverrideRequests.Remove(request);
	}

	private void Awake()
	{
		animatorComponent = ((Component)this).GetComponent<Animator>();
	}

	private void Start()
	{
		int layerIndex = animatorComponent.GetLayerIndex("AimPitch");
		int layerIndex2 = animatorComponent.GetLayerIndex("AimYaw");
		animatorComponent.Play("PitchControl", layerIndex);
		animatorComponent.Play("YawControl", layerIndex2);
		animatorComponent.Update(0f);
		AnimatorClipInfo[] currentAnimatorClipInfo = animatorComponent.GetCurrentAnimatorClipInfo(layerIndex);
		AnimatorClipInfo[] currentAnimatorClipInfo2 = animatorComponent.GetCurrentAnimatorClipInfo(layerIndex2);
		if (currentAnimatorClipInfo.Length != 0)
		{
			AnimationClip clip = ((AnimatorClipInfo)(ref currentAnimatorClipInfo[0])).clip;
			double num = clip.length * clip.frameRate;
			pitchClipCycleEnd = (float)((num - 1.0) / num);
		}
		if (currentAnimatorClipInfo2.Length != 0)
		{
			AnimationClip clip2 = ((AnimatorClipInfo)(ref currentAnimatorClipInfo2[0])).clip;
			_ = clip2.length;
			_ = clip2.frameRate;
			yawClipCycleEnd = 0.999f;
		}
	}

	private void Update()
	{
		if (!(Time.deltaTime <= 0f))
		{
			UpdateLocalAnglesToAimVector();
			UpdateGiveup();
			ApproachDesiredAngles();
			UpdateAnimatorParameters(animatorComponent, pitchRangeMin, pitchRangeMax, yawRangeMin, yawRangeMax);
		}
	}

	public void OnDeathStart()
	{
		((Behaviour)this).enabled = false;
		currentLocalAngles = new AimAngles
		{
			pitch = 0f,
			yaw = 0f
		};
		UpdateAnimatorParameters(animatorComponent, pitchRangeMin, pitchRangeMax, yawRangeMin, yawRangeMax);
	}

	private static float Remap(float value, float inMin, float inMax, float outMin, float outMax)
	{
		return outMin + (value - inMin) / (inMax - inMin) * (outMax - outMin);
	}

	private static float NormalizeAngle(float angle)
	{
		return Mathf.Repeat(angle + 180f, 360f) - 180f;
	}

	private void UpdateLocalAnglesToAimVector()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = ((directionOverrideRequests.Count <= 0) ? (Object.op_Implicit((Object)(object)inputBank) ? inputBank.aimDirection : ((Component)this).transform.forward) : directionOverrideRequests[directionOverrideRequests.Count - 1].directionGetter());
		Quaternion val2;
		if (UseTransformedAimVector)
		{
			val2 = Util.QuaternionSafeLookRotation(((Component)this).transform.InverseTransformDirection(val), ((Component)this).transform.up);
			Vector3 eulerAngles = ((Quaternion)(ref val2)).eulerAngles;
			localAnglesToAimVector = new AimAngles
			{
				pitch = NormalizeAngle(eulerAngles.x),
				yaw = NormalizeAngle(eulerAngles.y)
			};
		}
		else
		{
			float num = (Object.op_Implicit((Object)(object)directionComponent) ? directionComponent.yaw : ((Component)this).transform.eulerAngles.y);
			float num2 = (Object.op_Implicit((Object)(object)directionComponent) ? ((Component)directionComponent).transform.eulerAngles.x : ((Component)this).transform.eulerAngles.x);
			float num3 = (Object.op_Implicit((Object)(object)directionComponent) ? ((Component)directionComponent).transform.eulerAngles.z : ((Component)this).transform.eulerAngles.z);
			val2 = Util.QuaternionSafeLookRotation(val, ((Component)this).transform.up);
			Vector3 eulerAngles2 = ((Quaternion)(ref val2)).eulerAngles;
			Vector3 val3 = val;
			Vector3 val4 = default(Vector3);
			((Vector3)(ref val4))._002Ector(num2, num, num3);
			val3.y = 0f;
			localAnglesToAimVector = new AimAngles
			{
				pitch = (0f - Mathf.Atan2(val.y, ((Vector3)(ref val3)).magnitude)) * 57.29578f,
				yaw = NormalizeAngle(eulerAngles2.y - val4.y)
			};
		}
		overshootAngles = new AimAngles
		{
			pitch = Mathf.Max(pitchRangeMin - localAnglesToAimVector.pitch, localAnglesToAimVector.pitch - pitchRangeMax),
			yaw = Mathf.Max(Mathf.DeltaAngle(localAnglesToAimVector.yaw, yawRangeMin), Mathf.DeltaAngle(yawRangeMax, localAnglesToAimVector.yaw))
		};
		clampedLocalAnglesToAimVector = new AimAngles
		{
			pitch = Mathf.Clamp(localAnglesToAimVector.pitch, pitchRangeMin, pitchRangeMax),
			yaw = Mathf.Clamp(localAnglesToAimVector.yaw, yawRangeMin, yawRangeMax)
		};
	}

	private void ApproachDesiredAngles()
	{
		AimAngles aimAngles2;
		float num;
		if (shouldGiveup)
		{
			AimAngles aimAngles = default(AimAngles);
			aimAngles.pitch = 0f;
			aimAngles.yaw = 0f;
			aimAngles2 = aimAngles;
			num = loweredApproachSpeed;
		}
		else
		{
			aimAngles2 = clampedLocalAnglesToAimVector;
			num = raisedApproachSpeed;
		}
		float yaw = ((!fullYaw) ? Mathf.SmoothDamp(currentLocalAngles.yaw, aimAngles2.yaw, ref smoothingVelocity.yaw, smoothTime, num, Time.deltaTime) : NormalizeAngle(Mathf.SmoothDampAngle(currentLocalAngles.yaw, aimAngles2.yaw, ref smoothingVelocity.yaw, smoothTime, num, Time.deltaTime)));
		currentLocalAngles = new AimAngles
		{
			pitch = Mathf.SmoothDampAngle(currentLocalAngles.pitch, aimAngles2.pitch, ref smoothingVelocity.pitch, smoothTime, num, Time.deltaTime),
			yaw = yaw
		};
	}

	private void ResetGiveup()
	{
		giveupTimer = giveupDuration;
	}

	private void UpdateGiveup()
	{
		if (overshootAngles.pitch > pitchGiveupRange || (!fullYaw && overshootAngles.yaw > yawGiveupRange))
		{
			giveupTimer -= Time.deltaTime;
			isOutsideOfRange = true;
		}
		else
		{
			isOutsideOfRange = false;
			ResetGiveup();
		}
	}

	public void AimImmediate()
	{
		UpdateLocalAnglesToAimVector();
		ResetGiveup();
		currentLocalAngles = clampedLocalAnglesToAimVector;
		smoothingVelocity = new AimAngles
		{
			pitch = 0f,
			yaw = 0f
		};
		UpdateAnimatorParameters(animatorComponent, pitchRangeMin, pitchRangeMax, yawRangeMin, yawRangeMax);
	}

	public void UpdateAnimatorParameters(Animator animator, float pitchRangeMin, float pitchRangeMax, float yawRangeMin, float yawRangeMax)
	{
		float num = 1f;
		if (enableAimWeight)
		{
			num = animatorComponent.GetFloat(aimWeightHash);
		}
		animator.SetFloat(aimPitchCycleHash, Remap(currentLocalAngles.pitch * num, pitchRangeMin, pitchRangeMax, pitchClipCycleEnd, 0f));
		animator.SetFloat(aimYawCycleHash, Remap(currentLocalAngles.yaw * num, yawRangeMin, yawRangeMax, 0f, yawClipCycleEnd));
	}
}
