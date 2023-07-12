using UnityEngine;

namespace RoR2;

public class TemporaryVisualEffect : MonoBehaviour
{
	public enum VisualState
	{
		Enter,
		Exit
	}

	public float radius = 1f;

	public Transform parentTransform;

	public Transform visualTransform;

	public MonoBehaviour[] enterComponents;

	public MonoBehaviour[] exitComponents;

	public VisualState visualState;

	private VisualState previousVisualState;

	[HideInInspector]
	public HealthComponent healthComponent;

	private void Start()
	{
		RebuildVisualComponents();
	}

	private void FixedUpdate()
	{
		if (previousVisualState != visualState)
		{
			RebuildVisualComponents();
		}
		previousVisualState = visualState;
	}

	private void RebuildVisualComponents()
	{
		switch (visualState)
		{
		case VisualState.Enter:
		{
			MonoBehaviour[] array = enterComponents;
			for (int i = 0; i < array.Length; i++)
			{
				((Behaviour)array[i]).enabled = true;
			}
			array = exitComponents;
			for (int i = 0; i < array.Length; i++)
			{
				((Behaviour)array[i]).enabled = false;
			}
			break;
		}
		case VisualState.Exit:
		{
			MonoBehaviour[] array = enterComponents;
			for (int i = 0; i < array.Length; i++)
			{
				((Behaviour)array[i]).enabled = false;
			}
			array = exitComponents;
			for (int i = 0; i < array.Length; i++)
			{
				((Behaviour)array[i]).enabled = true;
			}
			break;
		}
		}
	}

	private void LateUpdate()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		bool flag = Object.op_Implicit((Object)(object)healthComponent);
		if (Object.op_Implicit((Object)(object)parentTransform))
		{
			((Component)this).transform.position = parentTransform.position;
		}
		else
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
		if (!flag || (flag && !healthComponent.alive))
		{
			visualState = VisualState.Exit;
		}
		if (Object.op_Implicit((Object)(object)visualTransform))
		{
			visualTransform.localScale = new Vector3(radius, radius, radius);
		}
	}
}
