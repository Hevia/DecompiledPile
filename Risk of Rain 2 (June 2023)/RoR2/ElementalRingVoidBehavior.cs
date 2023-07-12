using UnityEngine;

namespace RoR2;

public class ElementalRingVoidBehavior : CharacterBody.ItemBehavior
{
	private void Awake()
	{
		((Behaviour)this).enabled = false;
	}

	private void OnEnable()
	{
	}

	private void OnDisable()
	{
		if (Object.op_Implicit((Object)(object)body))
		{
			if (body.HasBuff(DLC1Content.Buffs.ElementalRingVoidReady))
			{
				body.RemoveBuff(DLC1Content.Buffs.ElementalRingVoidReady);
			}
			if (body.HasBuff(DLC1Content.Buffs.ElementalRingVoidCooldown))
			{
				body.RemoveBuff(DLC1Content.Buffs.ElementalRingVoidCooldown);
			}
		}
	}

	private void FixedUpdate()
	{
		if (Object.op_Implicit((Object)(object)body) && !body.HasBuff(DLC1Content.Buffs.ElementalRingVoidReady) && !body.HasBuff(DLC1Content.Buffs.ElementalRingVoidCooldown))
		{
			body.AddBuff(DLC1Content.Buffs.ElementalRingVoidReady);
		}
	}
}
