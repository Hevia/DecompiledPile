using UnityEngine;

namespace RoR2;

public class MushroomVoidBehavior : CharacterBody.ItemBehavior
{
	private const float healPercentagePerStack = 0.01f;

	private const float healPeriodSeconds = 0.5f;

	private bool wasSprinting;

	private HealthComponent healthComponent;

	private float healTimer;

	private void Awake()
	{
		((Behaviour)this).enabled = false;
	}

	private void OnEnable()
	{
		if (Object.op_Implicit((Object)(object)body))
		{
			wasSprinting = body.GetBuffCount(DLC1Content.Buffs.MushroomVoidActive) > 0;
			healthComponent = ((Component)body).GetComponent<HealthComponent>();
		}
		healTimer = 0f;
	}

	private void OnDisable()
	{
		if (Object.op_Implicit((Object)(object)body) && wasSprinting)
		{
			body.RemoveBuff(DLC1Content.Buffs.MushroomVoidActive);
		}
		healthComponent = null;
	}

	private void FixedUpdate()
	{
		if (Object.op_Implicit((Object)(object)body))
		{
			if (body.isSprinting)
			{
				healTimer += Time.fixedDeltaTime;
				if (!wasSprinting)
				{
					wasSprinting = true;
					body.AddBuff(DLC1Content.Buffs.MushroomVoidActive);
				}
			}
			else if (wasSprinting)
			{
				body.RemoveBuff(DLC1Content.Buffs.MushroomVoidActive);
				wasSprinting = false;
			}
		}
		while (healTimer > 0.5f)
		{
			healthComponent.HealFraction(0.01f * (float)stack, default(ProcChainMask));
			healTimer -= 0.5f;
		}
	}
}
