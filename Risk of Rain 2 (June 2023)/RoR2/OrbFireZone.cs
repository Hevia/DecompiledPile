using System.Collections.Generic;
using RoR2.Orbs;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public class OrbFireZone : MonoBehaviour
{
	public float baseDamage;

	public float procCoefficient;

	public float orbRemoveFromBottomOfListFrequency;

	public float orbResetListFrequency;

	private List<Collider> previousColliderList = new List<Collider>();

	private float resetStopwatch;

	private float removeFromBottomOfListStopwatch;

	private void Awake()
	{
	}

	private void FixedUpdate()
	{
		if (previousColliderList.Count > 0)
		{
			resetStopwatch += Time.fixedDeltaTime;
			removeFromBottomOfListStopwatch += Time.fixedDeltaTime;
			if (removeFromBottomOfListStopwatch > 1f / orbRemoveFromBottomOfListFrequency)
			{
				removeFromBottomOfListStopwatch -= 1f / orbRemoveFromBottomOfListFrequency;
				previousColliderList.RemoveAt(previousColliderList.Count - 1);
			}
			if (resetStopwatch > 1f / orbResetListFrequency)
			{
				resetStopwatch -= 1f / orbResetListFrequency;
				previousColliderList.Clear();
			}
		}
	}

	private void OnTriggerStay(Collider other)
	{
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active || previousColliderList.Contains(other))
		{
			return;
		}
		previousColliderList.Add(other);
		CharacterBody component = ((Component)other).GetComponent<CharacterBody>();
		if (Object.op_Implicit((Object)(object)component) && Object.op_Implicit((Object)(object)component.mainHurtBox))
		{
			DamageOrb damageOrb = new DamageOrb();
			damageOrb.attacker = null;
			damageOrb.damageOrbType = DamageOrb.DamageOrbType.ClayGooOrb;
			damageOrb.procCoefficient = procCoefficient;
			damageOrb.damageValue = baseDamage * Run.instance.teamlessDamageCoefficient;
			damageOrb.target = component.mainHurtBox;
			damageOrb.teamIndex = TeamIndex.None;
			RaycastHit val = default(RaycastHit);
			if (Physics.Raycast(((Component)damageOrb.target).transform.position + Random.insideUnitSphere * 3f, Vector3.down, ref val, 1000f, LayerMask.op_Implicit(LayerIndex.world.mask)))
			{
				damageOrb.origin = ((RaycastHit)(ref val)).point;
				OrbManager.instance.AddOrb(damageOrb);
			}
		}
	}
}
