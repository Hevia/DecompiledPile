using RoR2;
using UnityEngine;

namespace EntityStates.Croco;

public class Leap : BaseLeap
{
	protected override DamageType GetBlastDamageType()
	{
		return (Object.op_Implicit((Object)(object)crocoDamageTypeController) ? crocoDamageTypeController.GetDamageType() : DamageType.Generic) | DamageType.Stun1s;
	}

	protected override void DoImpactAuthority()
	{
		base.DoImpactAuthority();
		DetonateAuthority();
		DropAcidPoolAuthority();
	}
}
