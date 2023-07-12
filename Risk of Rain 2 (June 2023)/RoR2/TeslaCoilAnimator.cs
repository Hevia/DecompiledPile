using UnityEngine;

namespace RoR2;

public class TeslaCoilAnimator : MonoBehaviour
{
	public GameObject activeEffectParent;

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
			activeEffectParent.SetActive(characterBody.HasBuff(RoR2Content.Buffs.TeslaField));
		}
	}
}
