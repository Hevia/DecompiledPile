using System.Collections.Generic;
using UnityEngine;

namespace RoR2.UI;

public class RuleBookViewerStrip : MonoBehaviour
{
	public GameObject choicePrefab;

	public RectTransform choiceContainer;

	public Axis movementAxis = (Axis)1;

	public float movementDuration = 0.1f;

	public readonly List<RuleChoiceController> choiceControllers = new List<RuleChoiceController>();

	public int currentDisplayChoiceIndex;

	private float velocity;

	private float currentPosition;

	private RuleChoiceController CreateChoice()
	{
		GameObject obj = Object.Instantiate<GameObject>(choicePrefab, (Transform)(object)choiceContainer);
		obj.SetActive(true);
		RuleChoiceController component = obj.GetComponent<RuleChoiceController>();
		component.strip = this;
		return component;
	}

	private void DestroyChoice(RuleChoiceController choiceController)
	{
		Object.Destroy((Object)(object)((Component)choiceController).gameObject);
	}

	public void SetData(List<RuleChoiceDef> newChoices, int choiceIndex)
	{
		AllocateChoices(newChoices.Count);
		int num = currentDisplayChoiceIndex;
		int count = newChoices.Count;
		bool canVote = count > 1;
		for (int i = 0; i < count; i++)
		{
			choiceControllers[i].canVote = canVote;
			choiceControllers[i].SetChoice(newChoices[i]);
			if (newChoices[i].localIndex == choiceIndex)
			{
				num = i;
			}
		}
		currentDisplayChoiceIndex = num;
	}

	private void AllocateChoices(int desiredCount)
	{
		while (choiceControllers.Count > desiredCount)
		{
			int index = choiceControllers.Count - 1;
			DestroyChoice(choiceControllers[index]);
			choiceControllers.RemoveAt(index);
		}
		while (choiceControllers.Count < desiredCount)
		{
			choiceControllers.Add(CreateChoice());
		}
	}

	public void Update()
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Invalid comparison between Unknown and I4
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		if (choiceControllers.Count == 0)
		{
			return;
		}
		if (currentDisplayChoiceIndex >= choiceControllers.Count)
		{
			currentDisplayChoiceIndex = choiceControllers.Count - 1;
		}
		Vector3 localPosition = ((Component)choiceControllers[currentDisplayChoiceIndex]).transform.localPosition;
		float num = 0f;
		Axis val = movementAxis;
		if ((int)val != 0)
		{
			if ((int)val == 1)
			{
				num = 0f - localPosition.y;
			}
		}
		else
		{
			num = 0f - localPosition.x;
		}
		currentPosition = Mathf.SmoothDamp(currentPosition, num, ref velocity, movementDuration);
		UpdatePosition();
	}

	private void OnEnable()
	{
		UpdatePosition();
	}

	private void UpdatePosition()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Invalid comparison between Unknown and I4
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		Vector3 localPosition = ((Transform)choiceContainer).localPosition;
		Axis val = movementAxis;
		if ((int)val != 0)
		{
			if ((int)val == 1)
			{
				localPosition.y = currentPosition;
			}
		}
		else
		{
			localPosition.x = currentPosition;
		}
		((Transform)choiceContainer).localPosition = localPosition;
	}
}
