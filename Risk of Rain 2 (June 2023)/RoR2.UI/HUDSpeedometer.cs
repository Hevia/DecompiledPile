using TMPro;
using UnityEngine;

namespace RoR2.UI;

public class HUDSpeedometer : MonoBehaviour
{
	public TextMeshProUGUI label;

	public TextMeshProUGUI fixedUpdateLabel;

	private Transform _targetTransform;

	private Vector3 lastTargetPosition;

	private Vector3 lastTargetPositionFixedUpdate;

	public Transform targetTransform
	{
		get
		{
			return _targetTransform;
		}
		set
		{
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			if (!((Object)(object)_targetTransform == (Object)(object)value))
			{
				_targetTransform = value;
				if (Object.op_Implicit((Object)(object)_targetTransform))
				{
					lastTargetPosition = _targetTransform.position;
					lastTargetPositionFixedUpdate = _targetTransform.position;
				}
			}
		}
	}

	private float EstimateSpeed(ref Vector3 oldPosition, Vector3 newPosition, float deltaTime)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		float result = 0f;
		if (deltaTime != 0f)
		{
			Vector3 val = newPosition - oldPosition;
			result = ((Vector3)(ref val)).magnitude / deltaTime;
		}
		oldPosition = newPosition;
		return result;
	}

	private void Update()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)_targetTransform))
		{
			float num = EstimateSpeed(ref lastTargetPosition, _targetTransform.position, Time.deltaTime);
			((TMP_Text)label).text = $"{num:0.00} m/s";
		}
	}

	private void FixedUpdate()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)_targetTransform))
		{
			float num = EstimateSpeed(ref lastTargetPositionFixedUpdate, _targetTransform.position, Time.deltaTime);
			((TMP_Text)fixedUpdateLabel).text = $"{num:0.00} m/s";
		}
	}
}
