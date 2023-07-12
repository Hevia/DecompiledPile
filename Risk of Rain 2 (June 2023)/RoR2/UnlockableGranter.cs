using System;
using UnityEngine;

namespace RoR2;

public class UnlockableGranter : MonoBehaviour
{
	[Obsolete("'unlockableString' will be discontinued. Use 'unlockableDef' instead.", false)]
	[Tooltip("'unlockableString' will be discontinued. Use 'unlockableDef' instead.")]
	public string unlockableString;

	public UnlockableDef unlockableDef;

	public void GrantUnlockable(Interactor interactor)
	{
		string text = unlockableString;
		if (!Object.op_Implicit((Object)(object)unlockableDef) && !string.IsNullOrEmpty(text))
		{
			unlockableDef = UnlockableCatalog.GetUnlockableDef(text);
		}
		CharacterBody component = ((Component)interactor).GetComponent<CharacterBody>();
		if (Object.op_Implicit((Object)(object)component))
		{
			Run.instance.GrantUnlockToSinglePlayer(unlockableDef, component);
		}
	}
}
