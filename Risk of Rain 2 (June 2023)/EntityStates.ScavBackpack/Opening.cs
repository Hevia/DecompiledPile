using EntityStates.Barrel;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.ScavBackpack;

public class Opening : EntityState
{
	public static float duration = 1f;

	public static int maxItemDropCount;

	private ChestBehavior chestBehavior;

	private float itemDropCount;

	private float timeBetweenDrops;

	private float itemDropAge;

	public override void OnEnter()
	{
		base.OnEnter();
		PlayAnimation("Body", "Opening");
		timeBetweenDrops = duration / (float)maxItemDropCount;
		chestBehavior = GetComponent<ChestBehavior>();
		if (Object.op_Implicit((Object)(object)base.sfxLocator))
		{
			Util.PlaySound(base.sfxLocator.openSound, base.gameObject);
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (NetworkServer.active)
		{
			itemDropAge += Time.fixedDeltaTime;
			if (itemDropCount < (float)maxItemDropCount && itemDropAge > timeBetweenDrops)
			{
				itemDropCount += 1f;
				itemDropAge -= timeBetweenDrops;
				chestBehavior.RollItem();
				chestBehavior.ItemDrop();
			}
			if (base.fixedAge >= duration)
			{
				outer.SetNextState(new Opened());
			}
		}
	}
}
