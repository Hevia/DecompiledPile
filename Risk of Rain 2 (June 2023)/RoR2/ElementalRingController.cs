using UnityEngine;

namespace RoR2;

public class ElementalRingController : MonoBehaviour
{
	public GameObject elementalRingAvailableObject;

	private CharacterBody characterBody;

	private void Start()
	{
		CharacterModel componentInParent = ((Component)this).GetComponentInParent<CharacterModel>();
		if (Object.op_Implicit((Object)(object)componentInParent))
		{
			characterBody = componentInParent.body;
		}
	}

	private void FixedUpdate()
	{
		if (Object.op_Implicit((Object)(object)characterBody))
		{
			bool flag = characterBody.HasBuff(RoR2Content.Buffs.ElementalRingsReady);
			if (flag != elementalRingAvailableObject.activeSelf)
			{
				elementalRingAvailableObject.SetActive(flag);
			}
		}
	}
}
