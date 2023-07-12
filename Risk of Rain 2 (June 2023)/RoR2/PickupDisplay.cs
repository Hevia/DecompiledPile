using UnityEngine;

namespace RoR2;

public class PickupDisplay : MonoBehaviour
{
	[Tooltip("The vertical motion of the display model.")]
	public Wave verticalWave;

	public bool dontInstantiatePickupModel;

	[Tooltip("The speed in degrees/second at which the display model rotates about the y axis.")]
	public float spinSpeed = 75f;

	public GameObject tier1ParticleEffect;

	public GameObject tier2ParticleEffect;

	public GameObject tier3ParticleEffect;

	public GameObject equipmentParticleEffect;

	public GameObject lunarParticleEffect;

	public GameObject bossParticleEffect;

	public GameObject voidParticleEffect;

	[Tooltip("The particle system to tint.")]
	public ParticleSystem[] coloredParticleSystems;

	private PickupIndex pickupIndex = PickupIndex.none;

	private bool hidden;

	public Highlight highlight;

	private static readonly Vector3 idealModelBox = Vector3.one;

	private static readonly float idealVolume = idealModelBox.x * idealModelBox.y * idealModelBox.z;

	private GameObject modelObject;

	private GameObject modelPrefab;

	private float modelScale;

	private float localTime;

	public Renderer modelRenderer { get; private set; }

	private Vector3 localModelPivotPosition => Vector3.up * verticalWave.Evaluate(localTime);

	public void SetPickupIndex(PickupIndex newPickupIndex, bool newHidden = false)
	{
		if (!(pickupIndex == newPickupIndex) || hidden != newHidden)
		{
			pickupIndex = newPickupIndex;
			hidden = newHidden;
			RebuildModel();
		}
	}

	private void DestroyModel()
	{
		if (Object.op_Implicit((Object)(object)modelObject))
		{
			Object.Destroy((Object)(object)modelObject);
			modelObject = null;
			modelRenderer = null;
		}
	}

	private void RebuildModel()
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03de: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03eb: Unknown result type (might be due to invalid IL or missing references)
		PickupDef pickupDef = PickupCatalog.GetPickupDef(pickupIndex);
		GameObject val = null;
		if (pickupDef != null)
		{
			val = (hidden ? PickupCatalog.GetHiddenPickupDisplayPrefab() : pickupDef.displayPrefab);
		}
		if ((Object)(object)modelPrefab != (Object)(object)val)
		{
			DestroyModel();
			modelPrefab = val;
			modelScale = ((Component)this).transform.lossyScale.x;
			if (!dontInstantiatePickupModel && (Object)(object)modelPrefab != (Object)null)
			{
				modelObject = Object.Instantiate<GameObject>(modelPrefab);
				modelRenderer = modelObject.GetComponentInChildren<Renderer>();
				if (Object.op_Implicit((Object)(object)modelRenderer))
				{
					modelObject.transform.rotation = Quaternion.identity;
					Bounds bounds = modelRenderer.bounds;
					Vector3 size = ((Bounds)(ref bounds)).size;
					float num = size.x * size.y * size.z;
					if (num <= float.Epsilon)
					{
						Debug.LogError((object)"PickupDisplay bounds are zero! This is not allowed!");
						num = 1f;
					}
					modelScale *= Mathf.Pow(idealVolume, 1f / 3f) / Mathf.Pow(num, 1f / 3f);
					if (Object.op_Implicit((Object)(object)highlight))
					{
						highlight.targetRenderer = modelRenderer;
					}
				}
				modelObject.transform.parent = ((Component)this).transform;
				modelObject.transform.localPosition = localModelPivotPosition;
				modelObject.transform.localRotation = Quaternion.identity;
				modelObject.transform.localScale = new Vector3(modelScale, modelScale, modelScale);
			}
		}
		if (Object.op_Implicit((Object)(object)tier1ParticleEffect))
		{
			tier1ParticleEffect.SetActive(false);
		}
		if (Object.op_Implicit((Object)(object)tier2ParticleEffect))
		{
			tier2ParticleEffect.SetActive(false);
		}
		if (Object.op_Implicit((Object)(object)tier3ParticleEffect))
		{
			tier3ParticleEffect.SetActive(false);
		}
		if (Object.op_Implicit((Object)(object)equipmentParticleEffect))
		{
			equipmentParticleEffect.SetActive(false);
		}
		if (Object.op_Implicit((Object)(object)lunarParticleEffect))
		{
			lunarParticleEffect.SetActive(false);
		}
		if (Object.op_Implicit((Object)(object)voidParticleEffect))
		{
			voidParticleEffect.SetActive(false);
		}
		ItemIndex itemIndex = pickupDef?.itemIndex ?? ItemIndex.None;
		EquipmentIndex equipmentIndex = pickupDef?.equipmentIndex ?? EquipmentIndex.None;
		if (itemIndex != ItemIndex.None)
		{
			switch (ItemCatalog.GetItemDef(itemIndex).tier)
			{
			case ItemTier.Tier1:
				if (Object.op_Implicit((Object)(object)tier1ParticleEffect))
				{
					tier1ParticleEffect.SetActive(true);
				}
				break;
			case ItemTier.Tier2:
				if (Object.op_Implicit((Object)(object)tier2ParticleEffect))
				{
					tier2ParticleEffect.SetActive(true);
				}
				break;
			case ItemTier.Tier3:
				if (Object.op_Implicit((Object)(object)tier3ParticleEffect))
				{
					tier3ParticleEffect.SetActive(true);
				}
				break;
			case ItemTier.VoidTier1:
			case ItemTier.VoidTier2:
			case ItemTier.VoidTier3:
			case ItemTier.VoidBoss:
				if (Object.op_Implicit((Object)(object)voidParticleEffect))
				{
					voidParticleEffect.SetActive(true);
				}
				break;
			}
		}
		else if (equipmentIndex != EquipmentIndex.None && Object.op_Implicit((Object)(object)equipmentParticleEffect))
		{
			equipmentParticleEffect.SetActive(true);
		}
		if (Object.op_Implicit((Object)(object)bossParticleEffect))
		{
			bossParticleEffect.SetActive(pickupDef?.isBoss ?? false);
		}
		if (Object.op_Implicit((Object)(object)lunarParticleEffect))
		{
			lunarParticleEffect.SetActive(pickupDef?.isLunar ?? false);
		}
		if (Object.op_Implicit((Object)(object)highlight))
		{
			highlight.isOn = true;
			highlight.pickupIndex = pickupIndex;
		}
		ParticleSystem[] array = coloredParticleSystems;
		foreach (ParticleSystem obj in array)
		{
			((Component)obj).gameObject.SetActive((Object)(object)modelPrefab != (Object)null);
			MainModule main = obj.main;
			((MainModule)(ref main)).startColor = MinMaxGradient.op_Implicit(pickupDef?.baseColor ?? PickupCatalog.invalidPickupColor);
		}
	}

	private void Start()
	{
		localTime = 0f;
	}

	private void Update()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		localTime += Time.deltaTime;
		if (Object.op_Implicit((Object)(object)modelObject))
		{
			Transform transform = modelObject.transform;
			Vector3 localEulerAngles = transform.localEulerAngles;
			localEulerAngles.y = spinSpeed * localTime;
			transform.localEulerAngles = localEulerAngles;
			transform.localPosition = localModelPivotPosition;
		}
	}

	private void OnDrawGizmos()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		Gizmos.color = Color.yellow;
		Matrix4x4 matrix = Gizmos.matrix;
		Gizmos.matrix = Matrix4x4.TRS(((Component)this).transform.position, ((Component)this).transform.rotation, ((Component)this).transform.lossyScale);
		Gizmos.DrawWireCube(Vector3.zero, idealModelBox);
		Gizmos.matrix = matrix;
	}
}
