using RoR2.Projectile;
using UnityEngine;

namespace RoR2;

public class WinchControl : MonoBehaviour
{
	public Transform tailTransform;

	public string attachmentString;

	private ProjectileGhostController projectileGhostController;

	private Transform attachmentTransform;

	private void Start()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		attachmentTransform = FindAttachmentTransform();
		if (Object.op_Implicit((Object)(object)attachmentTransform))
		{
			tailTransform.position = attachmentTransform.position;
		}
	}

	private void Update()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)attachmentTransform))
		{
			attachmentTransform = FindAttachmentTransform();
		}
		if (Object.op_Implicit((Object)(object)attachmentTransform))
		{
			tailTransform.position = attachmentTransform.position;
		}
	}

	private Transform FindAttachmentTransform()
	{
		projectileGhostController = ((Component)this).GetComponent<ProjectileGhostController>();
		if (Object.op_Implicit((Object)(object)projectileGhostController))
		{
			Transform authorityTransform = projectileGhostController.authorityTransform;
			if (Object.op_Implicit((Object)(object)authorityTransform))
			{
				ProjectileController component = ((Component)authorityTransform).GetComponent<ProjectileController>();
				if (Object.op_Implicit((Object)(object)component))
				{
					GameObject owner = component.owner;
					if (Object.op_Implicit((Object)(object)owner))
					{
						ModelLocator component2 = owner.GetComponent<ModelLocator>();
						if (Object.op_Implicit((Object)(object)component2))
						{
							Transform modelTransform = component2.modelTransform;
							if (Object.op_Implicit((Object)(object)modelTransform))
							{
								ChildLocator component3 = ((Component)modelTransform).GetComponent<ChildLocator>();
								if (Object.op_Implicit((Object)(object)component3))
								{
									return component3.FindChild(attachmentString);
								}
							}
						}
					}
				}
			}
		}
		return null;
	}
}
