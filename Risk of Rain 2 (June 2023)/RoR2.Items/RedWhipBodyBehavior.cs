using UnityEngine;

namespace RoR2.Items;

internal class RedWhipBodyBehavior : BaseItemBodyBehavior
{
	private bool providingBuff;

	[ItemDefAssociation(useOnServer = true, useOnClient = false)]
	private static ItemDef GetItemDef()
	{
		return RoR2Content.Items.SprintOutOfCombat;
	}

	private void SetProvidingBuff(bool shouldProvideBuff)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		if (shouldProvideBuff == providingBuff)
		{
			return;
		}
		providingBuff = shouldProvideBuff;
		if (providingBuff)
		{
			base.body.AddBuff(RoR2Content.Buffs.WhipBoost);
			EffectData effectData = new EffectData();
			effectData.origin = base.body.corePosition;
			CharacterDirection characterDirection = base.body.characterDirection;
			bool flag = false;
			if (Object.op_Implicit((Object)(object)characterDirection) && characterDirection.moveVector != Vector3.zero)
			{
				effectData.rotation = Util.QuaternionSafeLookRotation(characterDirection.moveVector);
				flag = true;
			}
			if (!flag)
			{
				effectData.rotation = ((Component)base.body).transform.rotation;
			}
			EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/SprintActivate"), effectData, transmit: true);
		}
		else
		{
			base.body.RemoveBuff(RoR2Content.Buffs.WhipBoost);
		}
	}

	private void OnDisable()
	{
		SetProvidingBuff(shouldProvideBuff: false);
	}

	private void FixedUpdate()
	{
		SetProvidingBuff(base.body.outOfCombat);
	}
}
