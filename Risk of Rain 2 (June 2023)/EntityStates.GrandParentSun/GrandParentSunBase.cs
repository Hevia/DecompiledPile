using RoR2;
using UnityEngine;

namespace EntityStates.GrandParentSun;

public abstract class GrandParentSunBase : BaseState
{
	[SerializeField]
	public GameObject enterEffectPrefab;

	protected GrandParentSunController sunController { get; private set; }

	protected Transform vfxRoot { get; private set; }

	protected virtual bool shouldEnableSunController => false;

	protected abstract float desiredVfxScale { get; }

	public override void OnEnter()
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		sunController = GetComponent<GrandParentSunController>();
		((Behaviour)sunController).enabled = shouldEnableSunController;
		vfxRoot = base.transform.Find("VfxRoot");
		if (Object.op_Implicit((Object)(object)enterEffectPrefab))
		{
			EffectManager.SimpleImpactEffect(enterEffectPrefab, vfxRoot.position, Vector3.up, transmit: false);
		}
		SetVfxScale(desiredVfxScale);
	}

	public override void Update()
	{
		base.Update();
		SetVfxScale(desiredVfxScale);
	}

	private void SetVfxScale(float newScale)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		newScale = Mathf.Max(newScale, 0.01f);
		if (Object.op_Implicit((Object)(object)vfxRoot) && ((Component)vfxRoot).transform.localScale.x != newScale)
		{
			((Component)vfxRoot).transform.localScale = new Vector3(newScale, newScale, newScale);
		}
	}
}
