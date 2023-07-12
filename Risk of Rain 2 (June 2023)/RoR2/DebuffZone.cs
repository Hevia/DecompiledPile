using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public class DebuffZone : MonoBehaviour
{
	[Tooltip("The buff type to grant")]
	public BuffDef buffType;

	[Tooltip("The buff duration")]
	public float buffDuration;

	public string buffApplicationSoundString;

	public GameObject buffApplicationEffectPrefab;

	private void Awake()
	{
	}

	private void OnTriggerEnter(Collider other)
	{
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active || !Object.op_Implicit((Object)(object)buffType))
		{
			return;
		}
		CharacterBody component = ((Component)other).GetComponent<CharacterBody>();
		if (Object.op_Implicit((Object)(object)component))
		{
			component.AddTimedBuff(buffType.buffIndex, buffDuration);
			Util.PlaySound(buffApplicationSoundString, ((Component)component).gameObject);
			if (Object.op_Implicit((Object)(object)buffApplicationEffectPrefab))
			{
				EffectManager.SpawnEffect(buffApplicationEffectPrefab, new EffectData
				{
					origin = ((Component)component.mainHurtBox).transform.position,
					scale = component.radius
				}, transmit: true);
			}
		}
	}
}
