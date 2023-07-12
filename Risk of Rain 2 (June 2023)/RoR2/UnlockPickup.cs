using System;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

[RequireComponent(typeof(EntityLocator))]
public class UnlockPickup : MonoBehaviour
{
	public static string itemPickupSoundString = "Play_UI_item_pickup";

	private bool consumed;

	private float stopWatch;

	public float waitDuration = 0.5f;

	public string displayNameToken;

	[Obsolete("'unlockableName' will be discontinued. Use 'unlockableDef' instead.", false)]
	[Tooltip("'unlockableName' will be discontinued. Use 'unlockableDef' instead.")]
	public string unlockableName;

	public UnlockableDef unlockableDef;

	private void FixedUpdate()
	{
		stopWatch += Time.fixedDeltaTime;
	}

	private void GrantPickup(GameObject activator)
	{
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)Run.instance))
		{
			Util.PlaySound(itemPickupSoundString, activator);
			string text = unlockableName;
			if (!Object.op_Implicit((Object)(object)unlockableDef) && !string.IsNullOrEmpty(text))
			{
				unlockableDef = UnlockableCatalog.GetUnlockableDef(text);
			}
			Run.instance.GrantUnlockToAllParticipatingPlayers(unlockableDef);
			string pickupToken = "???";
			if (Object.op_Implicit((Object)(object)unlockableDef))
			{
				pickupToken = unlockableDef.nameToken;
			}
			Chat.SendBroadcastChat(new Chat.PlayerPickupChatMessage
			{
				subjectAsCharacterBody = activator.GetComponent<CharacterBody>(),
				baseToken = "PLAYER_PICKUP",
				pickupToken = pickupToken,
				pickupColor = ColorCatalog.GetColor(ColorCatalog.ColorIndex.Unlockable),
				pickupQuantity = 1u
			});
			consumed = true;
			GameObject val = ((Component)this).gameObject;
			EntityLocator component = ((Component)this).GetComponent<EntityLocator>();
			if (Object.op_Implicit((Object)(object)component.entity))
			{
				val = component.entity;
			}
			Object.Destroy((Object)(object)val);
		}
	}

	private static bool BodyHasPickupPermission(CharacterBody body)
	{
		if (Object.op_Implicit((Object)(object)(Object.op_Implicit((Object)(object)body.masterObject) ? body.masterObject.GetComponent<PlayerCharacterMasterController>() : null)))
		{
			return Object.op_Implicit((Object)(object)body.inventory);
		}
		return false;
	}

	private void OnTriggerStay(Collider other)
	{
		if (!NetworkServer.active || !(stopWatch >= waitDuration) || consumed)
		{
			return;
		}
		CharacterBody component = ((Component)other).GetComponent<CharacterBody>();
		if (Object.op_Implicit((Object)(object)component))
		{
			TeamComponent component2 = ((Component)component).GetComponent<TeamComponent>();
			if (Object.op_Implicit((Object)(object)component2) && component2.teamIndex == TeamIndex.Player && Object.op_Implicit((Object)(object)component.inventory))
			{
				GrantPickup(((Component)component).gameObject);
			}
		}
	}
}
