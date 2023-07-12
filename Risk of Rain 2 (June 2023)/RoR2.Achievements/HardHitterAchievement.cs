using UnityEngine;

namespace RoR2.Achievements;

[RegisterAchievement("HardHitter", "Items.ShockNearby", null, null)]
public class HardHitterAchievement : BaseAchievement
{
	private const float requirement = 5000f;

	public override void OnInstall()
	{
		base.OnInstall();
		GlobalEventManager.onClientDamageNotified += CheckDamage;
	}

	public override void OnUninstall()
	{
		GlobalEventManager.onClientDamageNotified -= CheckDamage;
		base.OnUninstall();
	}

	public void CheckDamage(DamageDealtMessage damageDealtMessage)
	{
		if (damageDealtMessage.damage >= 5000f && Object.op_Implicit((Object)(object)damageDealtMessage.attacker) && (Object)(object)damageDealtMessage.attacker == (Object)(object)base.localUser.cachedBodyObject)
		{
			Grant();
		}
	}
}
