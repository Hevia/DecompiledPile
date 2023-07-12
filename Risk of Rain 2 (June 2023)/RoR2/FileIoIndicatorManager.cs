using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace RoR2;

public static class FileIoIndicatorManager
{
	private static int activeWriteCount;

	private static volatile float saveIconAlpha;

	public static void IncrementActiveWriteCount()
	{
		Interlocked.Increment(ref activeWriteCount);
		saveIconAlpha = 2f;
	}

	public static void DecrementActiveWriteCount()
	{
		Interlocked.Decrement(ref activeWriteCount);
	}

	[SystemInitializer(new Type[] { })]
	private static void Init()
	{
		Image saveImage = ((Component)((Component)RoR2Application.instance.mainCanvas).transform.Find("SafeArea/SaveIcon")).GetComponent<Image>();
		RoR2Application.onUpdate += delegate
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			Color color = ((Graphic)saveImage).color;
			if (activeWriteCount <= 0)
			{
				color.a = (saveIconAlpha = Mathf.Max(saveIconAlpha - 4f * Time.unscaledDeltaTime, 0f));
			}
			((Graphic)saveImage).color = color;
		};
	}
}
