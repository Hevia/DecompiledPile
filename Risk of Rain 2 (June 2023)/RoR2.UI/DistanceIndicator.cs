using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace RoR2.UI;

[RequireComponent(typeof(PositionIndicator))]
public class DistanceIndicator : MonoBehaviour
{
	public PositionIndicator positionIndicator;

	public TextMeshPro tmp;

	private static readonly List<DistanceIndicator> instancesList;

	private void OnEnable()
	{
		instancesList.Add(this);
	}

	private void OnDisable()
	{
		instancesList.Remove(this);
	}

	static DistanceIndicator()
	{
		instancesList = new List<DistanceIndicator>();
		UICamera.onUICameraPreCull += UpdateText;
	}

	private static void UpdateText(UICamera uiCamera)
	{
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		CameraRigController cameraRigController = uiCamera.cameraRigController;
		Transform val = null;
		if (Object.op_Implicit((Object)(object)cameraRigController) && Object.op_Implicit((Object)(object)cameraRigController.target))
		{
			CharacterBody component = cameraRigController.target.GetComponent<CharacterBody>();
			val = ((!Object.op_Implicit((Object)(object)component)) ? cameraRigController.target.transform : component.coreTransform);
		}
		if (Object.op_Implicit((Object)(object)val))
		{
			for (int i = 0; i < instancesList.Count; i++)
			{
				DistanceIndicator distanceIndicator = instancesList[i];
				Vector3 val2 = distanceIndicator.positionIndicator.targetTransform.position - val.position;
				string text = ((Vector3)(ref val2)).magnitude.ToString("0.0") + "m";
				((TMP_Text)distanceIndicator.tmp).text = text;
			}
		}
	}
}
