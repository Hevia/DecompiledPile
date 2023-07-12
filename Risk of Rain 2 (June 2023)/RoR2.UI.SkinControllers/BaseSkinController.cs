using UnityEngine;

namespace RoR2.UI.SkinControllers;

[ExecuteAlways]
public abstract class BaseSkinController : MonoBehaviour
{
	public UISkinData skinData;

	protected abstract void OnSkinUI();

	protected void Awake()
	{
		if (Object.op_Implicit((Object)(object)skinData))
		{
			DoSkinUI();
		}
	}

	private void DoSkinUI()
	{
		if (Object.op_Implicit((Object)(object)skinData))
		{
			OnSkinUI();
		}
	}
}
