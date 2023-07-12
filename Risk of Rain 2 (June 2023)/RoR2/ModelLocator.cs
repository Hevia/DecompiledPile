using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace RoR2;

[DisallowMultipleComponent]
public class ModelLocator : MonoBehaviour, ILifeBehavior
{
	private class DestructionNotifier : MonoBehaviour
	{
		public ModelLocator subscriber { private get; set; }

		private void OnDestroy()
		{
			if (subscriber != null)
			{
				subscriber.modelTransform = null;
			}
		}
	}

	[SerializeField]
	[Tooltip("The transform of the child gameobject which acts as the model for this entity.")]
	[FormerlySerializedAs("modelTransform")]
	[Header("Cached Model Values")]
	private Transform _modelTransform;

	private DestructionNotifier modelDestructionNotifier;

	[Tooltip("The transform of the child gameobject which acts as the base for this entity's model. If provided, this will be detached from the hierarchy and positioned to match this object's position.")]
	public Transform modelBaseTransform;

	[Tooltip("Whether or not to update the model transforms automatically.")]
	[Header("Update Properties")]
	public bool autoUpdateModelTransform = true;

	[Tooltip("Forces the model to remain in hierarchy, rather that detaching on start. You usually don't want this for anything that moves.")]
	public bool dontDetatchFromParent;

	private Transform modelParentTransform;

	[Header("Corpse Properties")]
	[Tooltip("Only matters if preserveModel=true. Prevents the addition of a Corpse component to the model when this object is destroyed.")]
	public bool noCorpse;

	[Tooltip("If true, ownership of the model will not be relinquished by death.")]
	public bool dontReleaseModelOnDeath;

	[Tooltip("Prevents the model from being destroyed when this object is destroyed. This is rarely used, as character death states are usually responsible for snatching the model away and leaving this ModelLocator with nothing to destroy.")]
	public bool preserveModel;

	[Header("Normal Properties")]
	[Tooltip("Allows the model to align to the floor.")]
	public bool normalizeToFloor;

	public float normalSmoothdampTime = 0.1f;

	[Range(0f, 90f)]
	public float normalMaxAngleDelta = 90f;

	private Vector3 normalSmoothdampVelocity;

	private Vector3 targetNormal = Vector3.up;

	private Vector3 currentNormal = Vector3.up;

	private CharacterMotor characterMotor;

	public Transform modelTransform
	{
		get
		{
			return _modelTransform;
		}
		set
		{
			if (_modelTransform != value)
			{
				if (modelDestructionNotifier != null)
				{
					modelDestructionNotifier.subscriber = null;
					Object.Destroy((Object)(object)modelDestructionNotifier);
					modelDestructionNotifier = null;
				}
				_modelTransform = value;
				if (Object.op_Implicit((Object)(object)_modelTransform))
				{
					modelDestructionNotifier = ((Component)_modelTransform).gameObject.AddComponent<DestructionNotifier>();
					modelDestructionNotifier.subscriber = this;
				}
				this.onModelChanged?.Invoke(_modelTransform);
			}
		}
	}

	public event Action<Transform> onModelChanged;

	private void Awake()
	{
		characterMotor = ((Component)this).GetComponent<CharacterMotor>();
	}

	public void Start()
	{
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			modelParentTransform = modelTransform.parent;
			if (!dontDetatchFromParent)
			{
				modelTransform.parent = null;
			}
		}
	}

	private void UpdateModelTransform(float deltaTime)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)modelTransform) && Object.op_Implicit((Object)(object)modelParentTransform))
		{
			Vector3 position = modelParentTransform.position;
			Quaternion rotation = modelParentTransform.rotation;
			UpdateTargetNormal();
			SmoothNormals(deltaTime);
			rotation = Quaternion.FromToRotation(Vector3.up, currentNormal) * rotation;
			modelTransform.SetPositionAndRotation(position, rotation);
		}
	}

	private void SmoothNormals(float deltaTime)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		currentNormal = Vector3.SmoothDamp(currentNormal, targetNormal, ref normalSmoothdampVelocity, normalSmoothdampTime, float.PositiveInfinity, deltaTime);
	}

	private void UpdateTargetNormal()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		if (normalizeToFloor && Object.op_Implicit((Object)(object)characterMotor))
		{
			targetNormal = (characterMotor.isGrounded ? characterMotor.estimatedGroundNormal : Vector3.up);
			targetNormal = Vector3.RotateTowards(Vector3.up, targetNormal, normalMaxAngleDelta * (MathF.PI / 180f), float.PositiveInfinity);
		}
		else
		{
			targetNormal = Vector3.up;
		}
	}

	public void LateUpdate()
	{
		if (autoUpdateModelTransform)
		{
			UpdateModelTransform(Time.deltaTime);
		}
	}

	private void OnDestroy()
	{
		if (!Object.op_Implicit((Object)(object)modelTransform))
		{
			return;
		}
		if (preserveModel)
		{
			if (!noCorpse)
			{
				((Component)modelTransform).gameObject.AddComponent<Corpse>();
			}
			modelTransform = null;
		}
		else
		{
			Object.Destroy((Object)(object)((Component)modelTransform).gameObject);
		}
	}

	public void OnDeathStart()
	{
		if (!dontReleaseModelOnDeath)
		{
			preserveModel = true;
		}
	}
}
