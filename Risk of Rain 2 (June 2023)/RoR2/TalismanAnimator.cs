using UnityEngine;

namespace RoR2;

[RequireComponent(typeof(ItemFollower))]
public class TalismanAnimator : MonoBehaviour
{
	private float lastCooldownTimer;

	private EquipmentSlot equipmentSlot;

	private ItemFollower itemFollower;

	private ParticleSystem[] killEffects;

	private void Start()
	{
		itemFollower = ((Component)this).GetComponent<ItemFollower>();
		CharacterModel componentInParent = ((Component)this).GetComponentInParent<CharacterModel>();
		if (Object.op_Implicit((Object)(object)componentInParent))
		{
			CharacterBody body = componentInParent.body;
			if (Object.op_Implicit((Object)(object)body))
			{
				equipmentSlot = body.equipmentSlot;
			}
		}
	}

	private void FixedUpdate()
	{
		if (!Object.op_Implicit((Object)(object)equipmentSlot))
		{
			return;
		}
		float cooldownTimer = equipmentSlot.cooldownTimer;
		if (lastCooldownTimer - cooldownTimer >= 0.5f && Object.op_Implicit((Object)(object)itemFollower.followerInstance))
		{
			if (killEffects == null || killEffects.Length == 0 || (Object)(object)killEffects[0] == (Object)null)
			{
				killEffects = itemFollower.followerInstance.GetComponentsInChildren<ParticleSystem>();
			}
			ParticleSystem[] array = killEffects;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Play();
			}
		}
		lastCooldownTimer = cooldownTimer;
	}
}
