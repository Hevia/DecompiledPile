using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public class CharacterDirection : NetworkBehaviour, ILifeBehavior
{
	private struct TurnAnimatorParamsSet
	{
		public float angleMin;

		public float angleMax;

		public bool turnRight45;

		public bool turnRight90;

		public bool turnRight135;

		public bool turnLeft45;

		public bool turnLeft90;

		public bool turnLeft135;

		private static readonly int turnRight45ParamHash = Animator.StringToHash("turnRight45");

		private static readonly int turnRight90ParamHash = Animator.StringToHash("turnRight90");

		private static readonly int turnRight135ParamHash = Animator.StringToHash("turnRight135");

		private static readonly int turnLeft45ParamHash = Animator.StringToHash("turnLeft45");

		private static readonly int turnLeft90ParamHash = Animator.StringToHash("turnLeft90");

		private static readonly int turnLeft135ParamHash = Animator.StringToHash("turnLeft135");

		public void Apply(Animator animator)
		{
			animator.SetBool(turnRight45ParamHash, turnRight45);
			animator.SetBool(turnRight90ParamHash, turnRight90);
			animator.SetBool(turnRight135ParamHash, turnRight135);
			animator.SetBool(turnLeft45ParamHash, turnLeft45);
			animator.SetBool(turnLeft90ParamHash, turnLeft90);
			animator.SetBool(turnLeft135ParamHash, turnLeft135);
		}
	}

	[HideInInspector]
	public Vector3 moveVector;

	[Tooltip("The transform to rotate.")]
	public Transform targetTransform;

	[Tooltip("The transform to take the rotation from for animator purposes. Commonly the root node.")]
	public Transform overrideAnimatorForwardTransform;

	public RootMotionAccumulator rootMotionAccumulator;

	public Animator modelAnimator;

	[Tooltip("The character direction is set by root rotation, rather than moveVector.")]
	public bool driveFromRootRotation;

	[Tooltip("The maximum turn rate in degrees/second.")]
	public float turnSpeed = 360f;

	[SerializeField]
	private string turnSoundName;

	private int previousParamsIndex = -1;

	private float yRotationVelocity;

	private float _yaw;

	private Vector3 targetVector = Vector3.zero;

	private const float offset = 22.5f;

	private static readonly TurnAnimatorParamsSet[] turnAnimatorParamsSets = new TurnAnimatorParamsSet[7]
	{
		new TurnAnimatorParamsSet
		{
			angleMin = -180f,
			angleMax = -112.5f,
			turnRight45 = false,
			turnRight90 = false,
			turnRight135 = false,
			turnLeft45 = false,
			turnLeft90 = false,
			turnLeft135 = true
		},
		new TurnAnimatorParamsSet
		{
			angleMin = -112.5f,
			angleMax = -67.5f,
			turnRight45 = false,
			turnRight90 = false,
			turnRight135 = false,
			turnLeft45 = false,
			turnLeft90 = true,
			turnLeft135 = false
		},
		new TurnAnimatorParamsSet
		{
			angleMin = -67.5f,
			angleMax = -22.5f,
			turnRight45 = false,
			turnRight90 = false,
			turnRight135 = false,
			turnLeft45 = true,
			turnLeft90 = false,
			turnLeft135 = false
		},
		new TurnAnimatorParamsSet
		{
			turnRight45 = false,
			turnRight90 = false,
			turnRight135 = false,
			turnLeft45 = false,
			turnLeft90 = false,
			turnLeft135 = false
		},
		new TurnAnimatorParamsSet
		{
			angleMin = 22.5f,
			angleMax = 67.5f,
			turnRight45 = true,
			turnRight90 = false,
			turnRight135 = false,
			turnLeft45 = false,
			turnLeft90 = false,
			turnLeft135 = false
		},
		new TurnAnimatorParamsSet
		{
			angleMin = 67.5f,
			angleMax = 112.5f,
			turnRight45 = false,
			turnRight90 = true,
			turnRight135 = false,
			turnLeft45 = false,
			turnLeft90 = false,
			turnLeft135 = false
		},
		new TurnAnimatorParamsSet
		{
			angleMin = 112.5f,
			angleMax = 180f,
			turnRight45 = false,
			turnRight90 = false,
			turnRight135 = true,
			turnLeft45 = false,
			turnLeft90 = false,
			turnLeft135 = false
		}
	};

	private static readonly int paramsMidIndex = turnAnimatorParamsSets.Length >> 1;

	public float yaw
	{
		get
		{
			return _yaw;
		}
		set
		{
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			_yaw = value;
			if (Object.op_Implicit((Object)(object)targetTransform))
			{
				targetTransform.rotation = Quaternion.Euler(0f, _yaw, 0f);
			}
		}
	}

	public Vector3 animatorForward
	{
		get
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			if (!Object.op_Implicit((Object)(object)overrideAnimatorForwardTransform))
			{
				return forward;
			}
			float y = overrideAnimatorForwardTransform.eulerAngles.y;
			return Quaternion.Euler(0f, y, 0f) * Vector3.forward;
		}
	}

	public Vector3 forward
	{
		get
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			return Quaternion.Euler(0f, yaw, 0f) * Vector3.forward;
		}
		set
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			value.y = 0f;
			Quaternion val = Util.QuaternionSafeLookRotation(value, Vector3.up);
			yaw = ((Quaternion)(ref val)).eulerAngles.y;
		}
	}

	public bool hasEffectiveAuthority { get; private set; }

	private void UpdateAuthority()
	{
		hasEffectiveAuthority = Util.HasEffectiveAuthority(((Component)this).gameObject);
	}

	public override void OnStartAuthority()
	{
		UpdateAuthority();
	}

	public override void OnStopAuthority()
	{
		UpdateAuthority();
	}

	private void Start()
	{
		UpdateAuthority();
		ModelLocator component = ((Component)this).GetComponent<ModelLocator>();
		if (Object.op_Implicit((Object)(object)component))
		{
			modelAnimator = ((Component)component.modelTransform).GetComponent<Animator>();
		}
	}

	private void Update()
	{
		Simulate(Time.deltaTime);
	}

	public void OnDeathStart()
	{
		((Behaviour)this).enabled = false;
	}

	private static int PickIndex(float angle)
	{
		float num = Mathf.Sign(angle);
		int num2 = Mathf.CeilToInt((angle * num - 22.5f) * (1f / 45f));
		return Mathf.Clamp(paramsMidIndex + num2 * (int)num, 0, turnAnimatorParamsSets.Length - 1);
	}

	private void Simulate(float deltaTime)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		Quaternion val = Quaternion.Euler(0f, yaw, 0f);
		if (!hasEffectiveAuthority)
		{
			return;
		}
		if (driveFromRootRotation)
		{
			Quaternion val2 = rootMotionAccumulator.ExtractRootRotation();
			if (Object.op_Implicit((Object)(object)targetTransform))
			{
				targetTransform.rotation = val * val2;
				Quaternion rotation = targetTransform.rotation;
				float y = ((Quaternion)(ref rotation)).eulerAngles.y;
				yaw = y;
				float angle = 0f;
				if (((Vector3)(ref moveVector)).sqrMagnitude > 0f)
				{
					angle = Util.AngleSigned(Vector3.ProjectOnPlane(moveVector, Vector3.up), targetTransform.forward, -Vector3.up);
				}
				int num = PickIndex(angle);
				if (turnSoundName != null && num != previousParamsIndex)
				{
					Util.PlaySound(turnSoundName, ((Component)this).gameObject);
				}
				previousParamsIndex = num;
				turnAnimatorParamsSets[num].Apply(modelAnimator);
			}
		}
		targetVector = moveVector;
		targetVector.y = 0f;
		if (targetVector != Vector3.zero && deltaTime != 0f)
		{
			((Vector3)(ref targetVector)).Normalize();
			Quaternion val3 = Util.QuaternionSafeLookRotation(targetVector, Vector3.up);
			float num2 = Mathf.SmoothDampAngle(yaw, ((Quaternion)(ref val3)).eulerAngles.y, ref yRotationVelocity, 360f / turnSpeed * 0.25f, float.PositiveInfinity, deltaTime);
			val = Quaternion.Euler(0f, num2, 0f);
			yaw = num2;
		}
		if (Object.op_Implicit((Object)(object)targetTransform))
		{
			targetTransform.rotation = val;
		}
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool result = default(bool);
		return result;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
	}

	public override void PreStartClient()
	{
	}
}
