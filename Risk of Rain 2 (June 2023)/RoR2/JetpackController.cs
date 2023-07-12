using System.Collections.Generic;
using KinematicCharacterController;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

[RequireComponent(typeof(NetworkedBodyAttachment))]
public class JetpackController : NetworkBehaviour
{
	private static readonly List<JetpackController> instancesList;

	private NetworkedBodyAttachment networkedBodyAttachment;

	public float duration;

	public float acceleration;

	public float boostSpeedMultiplier = 3f;

	public float boostCooldown = 0.5f;

	private float stopwatch;

	private Transform wingTransform;

	private Animator wingAnimator;

	private GameObject[] wingMotions;

	private GameObject wingMeshObject;

	private float boostCooldownTimer;

	private bool hasBegunSoundLoop;

	private ICharacterGravityParameterProvider targetCharacterGravityParameterProvider;

	private ICharacterFlightParameterProvider targetCharacterFlightParameterProvider;

	private bool _providingAntiGravity;

	private bool _providingFlight;

	private static int kRpcRpcResetTimer;

	private CharacterBody targetBody => networkedBodyAttachment.attachedBody;

	private bool providingAntiGravity
	{
		get
		{
			return _providingAntiGravity;
		}
		set
		{
			if (_providingAntiGravity != value)
			{
				_providingAntiGravity = value;
				if (targetCharacterGravityParameterProvider != null)
				{
					CharacterGravityParameters gravityParameters = targetCharacterGravityParameterProvider.gravityParameters;
					gravityParameters.channeledAntiGravityGranterCount += (_providingAntiGravity ? 1 : (-1));
					targetCharacterGravityParameterProvider.gravityParameters = gravityParameters;
				}
			}
		}
	}

	private bool providingFlight
	{
		get
		{
			return _providingFlight;
		}
		set
		{
			if (_providingFlight != value)
			{
				_providingFlight = value;
				if (targetCharacterFlightParameterProvider != null)
				{
					CharacterFlightParameters flightParameters = targetCharacterFlightParameterProvider.flightParameters;
					flightParameters.channeledFlightGranterCount += (_providingFlight ? 1 : (-1));
					targetCharacterFlightParameterProvider.flightParameters = flightParameters;
				}
			}
		}
	}

	public static JetpackController FindJetpackController(GameObject targetObject)
	{
		if (!Object.op_Implicit((Object)(object)targetObject))
		{
			return null;
		}
		for (int i = 0; i < instancesList.Count; i++)
		{
			if ((Object)(object)instancesList[i].networkedBodyAttachment.attachedBodyObject == (Object)(object)targetObject)
			{
				return instancesList[i];
			}
		}
		return null;
	}

	private void Awake()
	{
		networkedBodyAttachment = ((Component)this).GetComponent<NetworkedBodyAttachment>();
	}

	private void Start()
	{
		SetupWings();
		if (Object.op_Implicit((Object)(object)targetBody))
		{
			targetCharacterGravityParameterProvider = ((Component)targetBody).GetComponent<ICharacterGravityParameterProvider>();
			targetCharacterFlightParameterProvider = ((Component)targetBody).GetComponent<ICharacterFlightParameterProvider>();
			StartFlight();
		}
	}

	private void StartFlight()
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		Debug.Log((object)"Starting flight");
		providingAntiGravity = true;
		providingFlight = true;
		if (targetBody.hasEffectiveAuthority && Object.op_Implicit((Object)(object)targetBody.characterMotor) && targetBody.characterMotor.isGrounded)
		{
			Vector3 velocity = targetBody.characterMotor.velocity;
			velocity.y = 15f;
			targetBody.characterMotor.velocity = velocity;
			((BaseCharacterController)targetBody.characterMotor).Motor.ForceUnground();
		}
	}

	private void OnDestroy()
	{
		ShowMotionLines(showWings: false);
		if (Object.op_Implicit((Object)(object)targetBody))
		{
			providingFlight = false;
			targetCharacterFlightParameterProvider = null;
			providingAntiGravity = false;
			targetCharacterGravityParameterProvider = null;
		}
	}

	private void OnEnable()
	{
		instancesList.Add(this);
	}

	private void OnDisable()
	{
		instancesList.Remove(this);
	}

	private void FixedUpdate()
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		stopwatch += Time.fixedDeltaTime;
		boostCooldownTimer -= Time.fixedDeltaTime;
		if (Object.op_Implicit((Object)(object)targetBody))
		{
			((Component)this).transform.position = ((Component)targetBody).transform.position;
			if (targetBody.hasEffectiveAuthority && Object.op_Implicit((Object)(object)targetBody.characterMotor))
			{
				if (stopwatch < duration)
				{
					if (boostCooldownTimer <= 0f && targetBody.inputBank.jump.justPressed && targetBody.inputBank.moveVector != Vector3.zero)
					{
						boostCooldownTimer = boostCooldown;
						targetBody.characterMotor.velocity = targetBody.inputBank.moveVector * (targetBody.moveSpeed * boostSpeedMultiplier);
						targetBody.characterMotor.disableAirControlUntilCollision = false;
					}
				}
				else
				{
					Vector3 velocity = targetBody.characterMotor.velocity;
					velocity.y = Mathf.Max(velocity.y, -5f);
					targetBody.characterMotor.velocity = velocity;
					providingAntiGravity = false;
					providingFlight = false;
				}
			}
		}
		if (stopwatch >= duration)
		{
			bool flag = !Object.op_Implicit((Object)(object)targetBody.characterMotor) || !targetBody.characterMotor.isGrounded;
			if (Object.op_Implicit((Object)(object)wingAnimator) && !flag)
			{
				wingAnimator.SetBool("wingsReady", false);
			}
			if (NetworkServer.active && !flag)
			{
				Object.Destroy((Object)(object)((Component)this).gameObject);
			}
			return;
		}
		float num = 4f;
		if (Object.op_Implicit((Object)(object)targetBody.characterMotor))
		{
			float magnitude = ((Vector3)(ref targetBody.characterMotor.velocity)).magnitude;
			float moveSpeed = targetBody.moveSpeed;
			if (magnitude != 0f && moveSpeed != 0f)
			{
				num += magnitude / moveSpeed * 6f;
			}
		}
		if (Object.op_Implicit((Object)(object)wingAnimator))
		{
			wingAnimator.SetBool("wingsReady", true);
			wingAnimator.SetFloat("fly.playbackRate", num, 0.1f, Time.fixedDeltaTime);
			ShowMotionLines(showWings: true);
		}
	}

	public void ResetTimer()
	{
		stopwatch = 0f;
		StartFlight();
		if (NetworkServer.active)
		{
			CallRpcResetTimer();
		}
	}

	[ClientRpc]
	private void RpcResetTimer()
	{
		if (!NetworkServer.active)
		{
			ResetTimer();
		}
	}

	private Transform FindWings()
	{
		ModelLocator modelLocator = targetBody.modelLocator;
		if (Object.op_Implicit((Object)(object)modelLocator))
		{
			Transform modelTransform = modelLocator.modelTransform;
			if (Object.op_Implicit((Object)(object)modelTransform))
			{
				CharacterModel component = ((Component)modelTransform).GetComponent<CharacterModel>();
				if (Object.op_Implicit((Object)(object)component))
				{
					List<GameObject> equipmentDisplayObjects = component.GetEquipmentDisplayObjects(RoR2Content.Equipment.Jetpack.equipmentIndex);
					if (equipmentDisplayObjects.Count > 0)
					{
						return equipmentDisplayObjects[0].transform;
					}
				}
			}
		}
		return null;
	}

	private void ShowMotionLines(bool showWings)
	{
		if (wingMotions == null)
		{
			return;
		}
		for (int i = 0; i < wingMotions.Length; i++)
		{
			if (Object.op_Implicit((Object)(object)wingMotions[i]))
			{
				wingMotions[i].SetActive(showWings);
			}
		}
		if (Object.op_Implicit((Object)(object)wingMeshObject))
		{
			wingMeshObject.SetActive(!showWings);
		}
		if (hasBegunSoundLoop != showWings)
		{
			switch (showWings)
			{
			case true:
				Util.PlaySound("Play_item_use_bugWingFlapLoop", ((Component)this).gameObject);
				break;
			case false:
				Util.PlaySound("Stop_item_use_bugWingFlapLoop", ((Component)this).gameObject);
				break;
			}
			hasBegunSoundLoop = showWings;
		}
	}

	public void SetupWings()
	{
		wingTransform = FindWings();
		if (Object.op_Implicit((Object)(object)wingTransform))
		{
			wingAnimator = ((Component)wingTransform).GetComponentInChildren<Animator>();
			ChildLocator component = ((Component)wingTransform).GetComponent<ChildLocator>();
			if (Object.op_Implicit((Object)(object)wingAnimator))
			{
				wingAnimator.SetBool("wingsReady", true);
			}
			if (Object.op_Implicit((Object)(object)component))
			{
				wingMotions = (GameObject[])(object)new GameObject[4];
				wingMotions[0] = ((Component)component.FindChild("WingMotionLargeL")).gameObject;
				wingMotions[1] = ((Component)component.FindChild("WingMotionLargeR")).gameObject;
				wingMotions[2] = ((Component)component.FindChild("WingMotionSmallL")).gameObject;
				wingMotions[3] = ((Component)component.FindChild("WingMotionSmallR")).gameObject;
				wingMeshObject = ((Component)component.FindChild("WingMesh")).gameObject;
			}
		}
	}

	static JetpackController()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Expected O, but got Unknown
		instancesList = new List<JetpackController>();
		kRpcRpcResetTimer = 1278379706;
		NetworkBehaviour.RegisterRpcDelegate(typeof(JetpackController), kRpcRpcResetTimer, new CmdDelegate(InvokeRpcRpcResetTimer));
		NetworkCRC.RegisterBehaviour("JetpackController", 0);
	}

	private void UNetVersion()
	{
	}

	protected static void InvokeRpcRpcResetTimer(NetworkBehaviour obj, NetworkReader reader)
	{
		if (!NetworkClient.active)
		{
			Debug.LogError((object)"RPC RpcResetTimer called on server.");
		}
		else
		{
			((JetpackController)(object)obj).RpcResetTimer();
		}
	}

	public void CallRpcResetTimer()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogError((object)"RPC Function RpcResetTimer called on client.");
			return;
		}
		NetworkWriter val = new NetworkWriter();
		val.Write((short)0);
		val.Write((short)2);
		val.WritePackedUInt32((uint)kRpcRpcResetTimer);
		val.Write(((Component)this).GetComponent<NetworkIdentity>().netId);
		((NetworkBehaviour)this).SendRPCInternal(val, 0, "RpcResetTimer");
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
