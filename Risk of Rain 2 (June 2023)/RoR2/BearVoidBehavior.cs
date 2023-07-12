using UnityEngine;

namespace RoR2;

public class BearVoidBehavior : CharacterBody.ItemBehavior
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
			if (body.HasBuff(DLC1Content.Buffs.BearVoidReady))
			{
				body.RemoveBuff(DLC1Content.Buffs.BearVoidReady);
			}
			if (body.HasBuff(DLC1Content.Buffs.BearVoidCooldown))
			{
				body.RemoveBuff(DLC1Content.Buffs.BearVoidCooldown);
			}
		}
	}

	private void FixedUpdate()
	{
		if (Object.op_Implicit((Object)(object)body) && !body.HasBuff(DLC1Content.Buffs.BearVoidReady) && !body.HasBuff(DLC1Content.Buffs.BearVoidCooldown))
		{
			body.AddBuff(DLC1Content.Buffs.BearVoidReady);
		}
	}
}
