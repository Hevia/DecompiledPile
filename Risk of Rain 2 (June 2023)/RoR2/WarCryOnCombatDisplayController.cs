using System;
using UnityEngine;

namespace RoR2;

[Obsolete]
public class WarCryOnCombatDisplayController : MonoBehaviour
{
	private CharacterBody body;

	[Tooltip("The child gameobject to enable when the warcry is ready.")]
	public GameObject readyIndicator;

	public void Start()
	{
		CharacterModel component = ((Component)((Component)this).transform.root).gameObject.GetComponent<CharacterModel>();
		if (Object.op_Implicit((Object)(object)component))
		{
			body = component.body;
		}
		UpdateReadyIndicator();
	}

	public void FixedUpdate()
	{
		UpdateReadyIndicator();
	}

	private void UpdateReadyIndicator()
	{
		bool active = Object.op_Implicit((Object)(object)body) && IsBodyWarCryReady(body);
		readyIndicator.SetActive(active);
	}

	private static bool IsBodyWarCryReady(CharacterBody body)
	{
		return false;
	}
}
