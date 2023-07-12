using UnityEngine;

namespace EntityStates.GrandParent;

public abstract class ChannelSunBase : BaseSkillState
{
	[SerializeField]
	public GameObject handVfxPrefab;

	public static string leftHandVfxTargetNameInChildLocator;

	public static string rightHandVfxTargetNameInChildLocator;

	private GameObject leftHandVfxInstance;

	private GameObject rightHandVfxInstance;

	public override void OnEnter()
	{
		base.OnEnter();
		if (Object.op_Implicit((Object)(object)handVfxPrefab))
		{
			ChildLocator modelChildLocator = GetModelChildLocator();
			if (Object.op_Implicit((Object)(object)modelChildLocator))
			{
				CreateVfxInstanceForHand(modelChildLocator, leftHandVfxTargetNameInChildLocator, ref leftHandVfxInstance);
				CreateVfxInstanceForHand(modelChildLocator, rightHandVfxTargetNameInChildLocator, ref rightHandVfxInstance);
			}
		}
	}

	public override void OnExit()
	{
		DestroyVfxInstance(ref leftHandVfxInstance);
		DestroyVfxInstance(ref rightHandVfxInstance);
		base.OnExit();
	}

	protected void CreateVfxInstanceForHand(ChildLocator childLocator, string nameInChildLocator, ref GameObject dest)
	{
		Transform val = childLocator.FindChild(nameInChildLocator);
		if (Object.op_Implicit((Object)(object)val))
		{
			dest = Object.Instantiate<GameObject>(handVfxPrefab, val);
		}
		else
		{
			dest = null;
		}
	}

	protected void DestroyVfxInstance(ref GameObject vfxInstance)
	{
		EntityState.Destroy((Object)(object)vfxInstance);
		vfxInstance = null;
	}
}
