using UnityEngine;
using UnityEngine.Events;

namespace RoR2.UI;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(HudElement))]
public class LoaderHookCrosshairController : MonoBehaviour
{
	public float range;

	public UnityEvent onAvailable;

	public UnityEvent onUnavailable;

	public UnityEvent onEnterRange;

	public UnityEvent onExitRange;

	private HudElement hudElement;

	private bool isAvailable;

	private bool inRange;

	private void Awake()
	{
		hudElement = ((Component)this).GetComponent<HudElement>();
	}

	private void SetAvailable(bool newIsAvailable)
	{
		if (isAvailable != newIsAvailable)
		{
			isAvailable = newIsAvailable;
			(isAvailable ? onAvailable : onUnavailable).Invoke();
		}
	}

	private void SetInRange(bool newInRange)
	{
		if (inRange != newInRange)
		{
			inRange = newInRange;
			UnityEvent obj = (inRange ? onEnterRange : onExitRange);
			if (obj != null)
			{
				obj.Invoke();
			}
		}
	}

	private void Update()
	{
		if (!Object.op_Implicit((Object)(object)hudElement.targetCharacterBody))
		{
			SetAvailable(newIsAvailable: false);
			SetInRange(newInRange: false);
			return;
		}
		SetAvailable(hudElement.targetCharacterBody.skillLocator.secondary.CanExecute());
		bool flag = false;
		if (isAvailable)
		{
			flag = hudElement.targetCharacterBody.inputBank.GetAimRaycast(range, out var _);
		}
		SetInRange(flag);
	}
}
