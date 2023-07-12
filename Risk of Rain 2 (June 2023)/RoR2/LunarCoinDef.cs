using UnityEngine;

namespace RoR2;

[CreateAssetMenu(menuName = "RoR2/MiscPickupDefs/LunarCoinDef")]
public class LunarCoinDef : MiscPickupDef
{
	[SerializeField]
	public string internalNameOverride;

	public override string GetInternalName()
	{
		if (!string.IsNullOrEmpty(internalNameOverride))
		{
			return internalNameOverride;
		}
		return "MiscPickupIndex." + ((Object)this).name;
	}

	public override void GrantPickup(ref PickupDef.GrantContext context)
	{
		NetworkUser networkUser = Util.LookUpBodyNetworkUser(context.body);
		if (Object.op_Implicit((Object)(object)networkUser))
		{
			networkUser.AwardLunarCoins(coinValue);
			context.shouldDestroy = true;
			context.shouldNotify = true;
		}
		else
		{
			context.shouldNotify = false;
			context.shouldDestroy = false;
		}
	}
}
