using Unity;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

[RequireComponent(typeof(VectorPID))]
[RequireComponent(typeof(CharacterBody))]
[RequireComponent(typeof(InputBankTest))]
public class RigidbodyMotor : NetworkBehaviour, IPhysMotor, IDisplacementReceiver
{
	[HideInInspector]
	public Vector3 moveVector;

	public Rigidbody rigid;

	public VectorPID forcePID;

	public Vector3 centerOfMassOffset;

	public string animatorForward;

	public string animatorRight;

	public string animatorUp;

	public bool enableOverrideMoveVectorInLocalSpace;

	public bool canTakeImpactDamage = true;

	public Vector3 overrideMoveVectorInLocalSpace;

	private NetworkIdentity networkIdentity;

	private CharacterBody characterBody;

	private InputBankTest inputBank;

	private ModelLocator modelLocator;

	private Animator animator;

	private BodyAnimatorSmoothingParameters bodyAnimatorSmoothingParameters;

	private HealthComponent healthComponent;

	private Vector3 rootMotion;

	private const float impactDamageStrength = 0.07f;

	private static int kRpcRpcApplyForceImpulse;

	public bool hasEffectiveAuthority => Util.HasEffectiveAuthority(networkIdentity);

	public float mass => rigid.mass;

	float IPhysMotor.mass => rigid.mass;

	Vector3 IPhysMotor.velocity => rigid.velocity;

	Vector3 IPhysMotor.velocityAuthority
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return rigid.velocity;
		}
		set
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			rigid.velocity = value;
		}
	}

	private void Awake()
	{
		networkIdentity = ((Component)this).GetComponent<NetworkIdentity>();
		characterBody = ((Component)this).GetComponent<CharacterBody>();
		inputBank = ((Component)this).GetComponent<InputBankTest>();
		modelLocator = ((Component)this).GetComponent<ModelLocator>();
		healthComponent = ((Component)this).GetComponent<HealthComponent>();
		bodyAnimatorSmoothingParameters = ((Component)this).GetComponent<BodyAnimatorSmoothingParameters>();
	}

	private void Start()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		UpdateAuthority();
		Vector3 centerOfMass = rigid.centerOfMass;
		centerOfMass += centerOfMassOffset;
		rigid.centerOfMass = centerOfMass;
		if (Object.op_Implicit((Object)(object)modelLocator))
		{
			Transform modelTransform = modelLocator.modelTransform;
			if (Object.op_Implicit((Object)(object)modelTransform))
			{
				animator = ((Component)modelTransform).GetComponent<Animator>();
			}
		}
	}

	private void OnDrawGizmosSelected()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		Gizmos.color = Color.red;
		Gizmos.DrawSphere(((Component)this).transform.position + rigid.centerOfMass, 0.5f);
	}

	public static float GetPitch(Vector3 v)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Sqrt(v.x * v.x + v.z * v.z);
		return 0f - Mathf.Atan2(v.y, num);
	}

	private void Update()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)animator))
		{
			Vector3 val = ((Component)this).transform.InverseTransformVector(moveVector) / Mathf.Max(1f, ((Vector3)(ref moveVector)).magnitude);
			BodyAnimatorSmoothingParameters.SmoothingParameters smoothingParameters = (Object.op_Implicit((Object)(object)bodyAnimatorSmoothingParameters) ? bodyAnimatorSmoothingParameters.smoothingParameters : BodyAnimatorSmoothingParameters.defaultParameters);
			if (animatorForward.Length > 0)
			{
				animator.SetFloat(animatorForward, val.z, smoothingParameters.forwardSpeedSmoothDamp, Time.deltaTime);
			}
			if (animatorRight.Length > 0)
			{
				animator.SetFloat(animatorRight, val.x, smoothingParameters.rightSpeedSmoothDamp, Time.deltaTime);
			}
			if (animatorUp.Length > 0)
			{
				animator.SetFloat(animatorUp, val.y, smoothingParameters.forwardSpeedSmoothDamp, Time.deltaTime);
			}
		}
	}

	private void FixedUpdate()
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)inputBank) || !Object.op_Implicit((Object)(object)rigid))
		{
			return;
		}
		if (Object.op_Implicit((Object)(object)forcePID))
		{
			if (enableOverrideMoveVectorInLocalSpace)
			{
				moveVector = ((Component)this).transform.TransformDirection(overrideMoveVectorInLocalSpace) * characterBody.moveSpeed;
			}
			_ = inputBank.aimDirection;
			Vector3 targetVector = moveVector;
			forcePID.inputVector = rigid.velocity;
			forcePID.targetVector = targetVector;
			Debug.DrawLine(((Component)this).transform.position, ((Component)this).transform.position + forcePID.targetVector, Color.red, 0.1f);
			Vector3 val = forcePID.UpdatePID();
			rigid.AddForceAtPosition(Vector3.ClampMagnitude(val * (characterBody.acceleration / 3f), characterBody.acceleration), ((Component)this).transform.position, (ForceMode)5);
		}
		if (rootMotion != Vector3.zero)
		{
			rigid.MovePosition(rigid.position + rootMotion);
			rootMotion = Vector3.zero;
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		if (canTakeImpactDamage && collision.gameObject.layer == LayerIndex.world.intVal)
		{
			float num = Mathf.Max(characterBody.moveSpeed, characterBody.baseMoveSpeed) * 4f;
			Vector3 relativeVelocity = collision.relativeVelocity;
			float magnitude = ((Vector3)(ref relativeVelocity)).magnitude;
			if (magnitude >= num)
			{
				float num2 = magnitude / characterBody.moveSpeed * 0.07f;
				DamageInfo damageInfo = new DamageInfo();
				damageInfo.damage = Mathf.Min(healthComponent.fullCombinedHealth, healthComponent.fullCombinedHealth * num2);
				damageInfo.procCoefficient = 0f;
				damageInfo.position = ((ContactPoint)(ref collision.contacts[0])).point;
				damageInfo.attacker = healthComponent.lastHitAttacker;
				healthComponent.TakeDamage(damageInfo);
			}
		}
	}

	public override void OnStartAuthority()
	{
		UpdateAuthority();
	}

	public override void OnStopAuthority()
	{
		UpdateAuthority();
	}

	public void AddDisplacement(Vector3 displacement)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		rootMotion += displacement;
	}

	public void ApplyForceImpulse(in PhysForceInfo forceInfo)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		if (NetworkServer.active && !hasEffectiveAuthority)
		{
			CallRpcApplyForceImpulse(forceInfo);
		}
		else
		{
			rigid.AddForce(forceInfo.force, (ForceMode)((!forceInfo.massIsOne) ? 1 : 2));
		}
	}

	[ClientRpc]
	private void RpcApplyForceImpulse(PhysForceInfo physForceInfo)
	{
		if (!NetworkServer.active)
		{
			ApplyForceImpulse(in physForceInfo);
		}
	}

	private void UpdateAuthority()
	{
		((Behaviour)this).enabled = hasEffectiveAuthority;
	}

	void IPhysMotor.ApplyForceImpulse(in PhysForceInfo physForceInfo)
	{
		ApplyForceImpulse(in physForceInfo);
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeRpcRpcApplyForceImpulse(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError((object)"RPC RpcApplyForceImpulse called on server.");
		}
		else
		{
			((RigidbodyMotor)(object)obj).RpcApplyForceImpulse(GeneratedNetworkCode._ReadPhysForceInfo_None(reader));
		}
	}

	public void CallRpcApplyForceImpulse(PhysForceInfo physForceInfo)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogError((object)"RPC Function RpcApplyForceImpulse called on client.");
			return;
		}
		NetworkWriter val = new NetworkWriter();
		val.Write((short)0);
		val.Write((short)2);
		val.WritePackedUInt32((uint)kRpcRpcApplyForceImpulse);
		val.Write(((Component)this).GetComponent<NetworkIdentity>().netId);
		GeneratedNetworkCode._WritePhysForceInfo_None(val, physForceInfo);
		((NetworkBehaviour)this).SendRPCInternal(val, 0, "RpcApplyForceImpulse");
	}

	static RigidbodyMotor()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		kRpcRpcApplyForceImpulse = 1386350170;
		NetworkBehaviour.RegisterRpcDelegate(typeof(RigidbodyMotor), kRpcRpcApplyForceImpulse, new CmdDelegate(InvokeRpcRpcApplyForceImpulse));
		NetworkCRC.RegisterBehaviour("RigidbodyMotor", 0);
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
