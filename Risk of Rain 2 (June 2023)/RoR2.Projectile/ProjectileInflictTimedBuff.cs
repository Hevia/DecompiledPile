using UnityEngine;

namespace RoR2.Projectile;

public class ProjectileInflictTimedBuff : MonoBehaviour, IOnDamageInflictedServerReceiver
{
	public BuffDef buffDef;

	public float duration;

	public void OnDamageInflictedServer(DamageReport damageReport)
	{
		CharacterBody victimBody = damageReport.victimBody;
		if (Object.op_Implicit((Object)(object)victimBody))
		{
			victimBody.AddTimedBuff(buffDef.buffIndex, duration);
		}
	}

	private void OnValidate()
	{
		if (!Object.op_Implicit((Object)(object)buffDef))
		{
			Debug.LogWarningFormat((Object)(object)this, "ProjectileInflictTimedBuff {0} has no buff specified.", new object[1] { this });
		}
	}
}
