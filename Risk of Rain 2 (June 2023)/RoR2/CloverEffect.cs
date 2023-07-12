using UnityEngine;

namespace RoR2;

public class CloverEffect : MonoBehaviour
{
	public GameObject triggerEffect;

	private CharacterBody characterBody;

	private GameObject triggerEffectInstance;

	private bool trigger;

	private void Start()
	{
		CharacterBody body = ((Component)this).GetComponentInParent<CharacterModel>().body;
		characterBody = ((Component)body).GetComponent<CharacterBody>();
	}

	private void FixedUpdate()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)characterBody) && characterBody.wasLucky)
		{
			characterBody.wasLucky = false;
			EffectData effectData = new EffectData();
			effectData.origin = ((Component)this).transform.position;
			effectData.rotation = ((Component)this).transform.rotation;
			EffectManager.SpawnEffect(triggerEffect, effectData, transmit: true);
		}
	}
}
