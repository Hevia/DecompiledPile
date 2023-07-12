using UnityEngine;

namespace RoR2;

public class TemporaryOverlay : MonoBehaviour
{
	public Material originalMaterial;

	[HideInInspector]
	public Material materialInstance;

	private bool isAssigned;

	private CharacterModel assignedCharacterModel;

	public CharacterModel inspectorCharacterModel;

	public bool animateShaderAlpha;

	public AnimationCurve alphaCurve;

	public float duration;

	public bool destroyComponentOnEnd;

	public bool destroyObjectOnEnd;

	public GameObject destroyEffectPrefab;

	public string destroyEffectChildString;

	private float stopwatch;

	private void Start()
	{
		SetupMaterial();
		if (Object.op_Implicit((Object)(object)inspectorCharacterModel))
		{
			AddToCharacerModel(inspectorCharacterModel);
		}
	}

	private void SetupMaterial()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Expected O, but got Unknown
		if (!Object.op_Implicit((Object)(object)materialInstance))
		{
			materialInstance = new Material(originalMaterial);
		}
	}

	public void AddToCharacerModel(CharacterModel characterModel)
	{
		SetupMaterial();
		if (Object.op_Implicit((Object)(object)characterModel))
		{
			characterModel.temporaryOverlays.Add(this);
			isAssigned = true;
			assignedCharacterModel = characterModel;
		}
	}

	public void RemoveFromCharacterModel()
	{
		if (Object.op_Implicit((Object)(object)assignedCharacterModel))
		{
			assignedCharacterModel.temporaryOverlays.Remove(this);
			isAssigned = false;
			assignedCharacterModel = null;
		}
	}

	private void OnDestroy()
	{
		RemoveFromCharacterModel();
		if (Object.op_Implicit((Object)(object)materialInstance))
		{
			Object.Destroy((Object)(object)materialInstance);
		}
	}

	private void Update()
	{
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		if (!animateShaderAlpha)
		{
			return;
		}
		stopwatch += Time.deltaTime;
		float num = alphaCurve.Evaluate(stopwatch / duration);
		materialInstance.SetFloat("_ExternalAlpha", num);
		if (!(stopwatch >= duration) || (!destroyComponentOnEnd && !destroyObjectOnEnd))
		{
			return;
		}
		if (Object.op_Implicit((Object)(object)destroyEffectPrefab))
		{
			ChildLocator component = ((Component)this).GetComponent<ChildLocator>();
			if (Object.op_Implicit((Object)(object)component))
			{
				Transform val = component.FindChild(destroyEffectChildString);
				if (Object.op_Implicit((Object)(object)val))
				{
					EffectManager.SpawnEffect(destroyEffectPrefab, new EffectData
					{
						origin = val.position,
						rotation = val.rotation
					}, transmit: true);
				}
			}
		}
		if (destroyObjectOnEnd)
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
		else
		{
			Object.Destroy((Object)(object)this);
		}
	}
}
