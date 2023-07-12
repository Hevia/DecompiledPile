using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RoR2;

public static class FadeToBlackManager
{
	private static Image image;

	public static int fadeCount;

	private static float alpha;

	private const float fadeDuration = 0.25f;

	private const float inversefadeDuration = 4f;

	public static bool fullyFaded => alpha == 2f;

	[RuntimeInitializeOnLoadMethod(/*Could not decode attribute arguments.*/)]
	private static void Init()
	{
		GameObject obj = Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/UI/ScreenTintCanvas"), ((Component)RoR2Application.instance.mainCanvas).transform);
		alpha = 0f;
		image = ((Component)obj.transform.GetChild(0)).GetComponent<Image>();
		UpdateImageAlpha(alpha);
		RoR2Application.onUpdate += Update;
		SceneManager.sceneUnloaded += OnSceneUnloaded;
	}

	public static void OnSceneUnloaded(Scene scene)
	{
		ForceFullBlack();
	}

	public static void ForceFullBlack()
	{
		alpha = 2f;
	}

	private static void Update()
	{
		float num = 2f;
		float num2 = 4f;
		if (fadeCount <= 0)
		{
			num = 0f;
			num2 *= 0.25f;
		}
		alpha = Mathf.MoveTowards(alpha, num, Time.unscaledDeltaTime * num2);
		float num3 = 0f;
		List<FadeToBlackOffset> instancesList = InstanceTracker.GetInstancesList<FadeToBlackOffset>();
		for (int i = 0; i < instancesList.Count; i++)
		{
			FadeToBlackOffset fadeToBlackOffset = instancesList[i];
			num3 += fadeToBlackOffset.value;
		}
		UpdateImageAlpha(alpha + num3);
	}

	private static void UpdateImageAlpha(float finalAlpha)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		Color color = ((Graphic)image).color;
		Color val = color;
		val.a = finalAlpha;
		((Graphic)image).color = val;
		((Graphic)image).raycastTarget = val.a > color.a;
	}

	public static void ForceClear()
	{
		fadeCount = 0;
		alpha = 0f;
		TransitionCommand.ForceClearFadeToBlack();
	}
}
