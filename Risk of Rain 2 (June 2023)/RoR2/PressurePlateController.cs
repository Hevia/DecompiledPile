using UnityEngine;
using UnityEngine.Events;

namespace RoR2;

public class PressurePlateController : MonoBehaviour
{
	public bool enableOverlapSphere = true;

	public float overlapSphereRadius;

	public float overlapSphereFrequency;

	public string switchDownSoundString;

	public string switchUpSoundString;

	public UnityEvent OnSwitchDown;

	public UnityEvent OnSwitchUp;

	public Collider pingCollider;

	public AnimationCurve switchVisualPositionFromUpToDown;

	public AnimationCurve switchVisualPositionFromDownToUp;

	public Transform switchVisualTransform;

	private float overlapSphereStopwatch;

	private float animationStopwatch;

	private bool switchDown;

	private void Start()
	{
	}

	private void FixedUpdate()
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		if (enableOverlapSphere)
		{
			overlapSphereStopwatch += Time.fixedDeltaTime;
			if (overlapSphereStopwatch >= 1f / overlapSphereFrequency)
			{
				overlapSphereStopwatch -= 1f / overlapSphereFrequency;
				Collider[] array = Physics.OverlapSphere(((Component)this).transform.position, overlapSphereRadius, LayerMask.op_Implicit(LayerIndex.defaultLayer.mask) | LayerMask.op_Implicit(LayerIndex.fakeActor.mask), (QueryTriggerInteraction)0);
				bool @switch = array.Length > 1 || (array.Length == 1 && array[0] != pingCollider);
				SetSwitch(@switch);
			}
		}
	}

	public void EnableOverlapSphere(bool input)
	{
		enableOverlapSphere = input;
	}

	public void SetSwitch(bool switchIsDown)
	{
		if (switchIsDown == switchDown)
		{
			return;
		}
		if (switchIsDown)
		{
			animationStopwatch = 0f;
			Util.PlaySound(switchDownSoundString, ((Component)this).gameObject);
			UnityEvent onSwitchDown = OnSwitchDown;
			if (onSwitchDown != null)
			{
				onSwitchDown.Invoke();
			}
		}
		else
		{
			animationStopwatch = 0f;
			Util.PlaySound(switchUpSoundString, ((Component)this).gameObject);
			UnityEvent onSwitchUp = OnSwitchUp;
			if (onSwitchUp != null)
			{
				onSwitchUp.Invoke();
			}
		}
		switchDown = switchIsDown;
	}

	private void Update()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		animationStopwatch += Time.deltaTime;
		if (Object.op_Implicit((Object)(object)switchVisualTransform))
		{
			Vector3 localPosition = ((Component)switchVisualTransform).transform.localPosition;
			switch (switchDown)
			{
			case true:
				localPosition.z = switchVisualPositionFromUpToDown.Evaluate(animationStopwatch);
				break;
			case false:
				localPosition.z = switchVisualPositionFromDownToUp.Evaluate(animationStopwatch);
				break;
			}
			switchVisualTransform.localPosition = localPosition;
		}
	}

	private void OnDrawGizmos()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		Gizmos.DrawWireSphere(((Component)this).transform.position, overlapSphereRadius);
	}
}
