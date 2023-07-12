using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

[RequireComponent(typeof(VehicleSeat))]
[RequireComponent(typeof(EntityStateMachine))]
public sealed class SurvivorPodController : NetworkBehaviour, ICameraStateProvider
{
	private EntityStateMachine stateMachine;

	[Tooltip("The bone which controls the camera during the entry animation.")]
	public Transform cameraBone;

	public bool exitAllowed;

	public EntityStateMachine characterStateMachine { get; private set; }

	public VehicleSeat vehicleSeat { get; private set; }

	private void Awake()
	{
		stateMachine = ((Component)this).GetComponent<EntityStateMachine>();
		vehicleSeat = ((Component)this).GetComponent<VehicleSeat>();
		vehicleSeat.onPassengerEnter += OnPassengerEnter;
		vehicleSeat.onPassengerExit += OnPassengerExit;
		vehicleSeat.enterVehicleAllowedCheck.AddCallback(CheckEnterAllowed);
		vehicleSeat.exitVehicleAllowedCheck.AddCallback(CheckExitAllowed);
	}

	private void OnPassengerEnter(GameObject passenger)
	{
		UpdateCameras(passenger);
	}

	private void OnPassengerExit(GameObject passenger)
	{
		UpdateCameras(null);
		((Behaviour)vehicleSeat).enabled = false;
	}

	private void CheckEnterAllowed(CharacterBody characterBody, ref Interactability? resultOverride)
	{
		resultOverride = Interactability.Disabled;
	}

	private void CheckExitAllowed(CharacterBody characterBody, ref Interactability? resultOverride)
	{
		resultOverride = (exitAllowed ? Interactability.Available : Interactability.Disabled);
	}

	private void Update()
	{
		UpdateCameras(Object.op_Implicit((Object)(object)vehicleSeat.currentPassengerBody) ? ((Component)vehicleSeat.currentPassengerBody).gameObject : null);
	}

	private void UpdateCameras(GameObject characterBodyObject)
	{
		foreach (CameraRigController readOnlyInstances in CameraRigController.readOnlyInstancesList)
		{
			if (Object.op_Implicit((Object)(object)characterBodyObject) && (Object)(object)readOnlyInstances.target == (Object)(object)characterBodyObject)
			{
				readOnlyInstances.SetOverrideCam(this, 0f);
			}
			else if (readOnlyInstances.IsOverrideCam(this))
			{
				readOnlyInstances.SetOverrideCam(null, 0.05f);
			}
		}
	}

	void ICameraStateProvider.GetCameraState(CameraRigController cameraRigController, ref CameraState cameraState)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = ((Component)this).transform.position;
		Vector3 position2 = cameraBone.position;
		Vector3 val = position2 - position;
		Ray val2 = default(Ray);
		((Ray)(ref val2))._002Ector(position, val);
		Vector3 position3 = position2;
		RaycastHit val3 = default(RaycastHit);
		if (Physics.Raycast(val2, ref val3, ((Vector3)(ref val)).magnitude, LayerMask.op_Implicit(LayerIndex.world.mask), (QueryTriggerInteraction)1))
		{
			position3 = ((Ray)(ref val2)).GetPoint(Mathf.Max(((RaycastHit)(ref val3)).distance - 0.25f, 0.25f));
		}
		cameraState = new CameraState
		{
			position = position3,
			rotation = cameraBone.rotation,
			fov = 60f
		};
	}

	bool ICameraStateProvider.IsUserLookAllowed(CameraRigController cameraRigController)
	{
		return false;
	}

	bool ICameraStateProvider.IsUserControlAllowed(CameraRigController cameraRigController)
	{
		return true;
	}

	bool ICameraStateProvider.IsHudAllowed(CameraRigController cameraRigController)
	{
		return true;
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
