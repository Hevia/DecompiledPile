using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public class FireAuraController : NetworkBehaviour
{
	private const float fireAttackRadiusMin = 0.5f;

	private const float fireAttackRadiusMax = 6f;

	private const float fireDamageCoefficient = 1f;

	private const float fireProcCoefficient = 0.1f;

	private const float maxTimer = 8f;

	private float timer;

	private float attackStopwatch;

	[SyncVar]
	public GameObject owner;

	private NetworkInstanceId ___ownerNetId;

	public GameObject Networkowner
	{
		get
		{
			return owner;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).SetSyncVarGameObject(value, ref owner, 1u, ref ___ownerNetId);
		}
	}

	private void FixedUpdate()
	{
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		timer += Time.fixedDeltaTime;
		CharacterBody characterBody = null;
		float num = 0f;
		if (Object.op_Implicit((Object)(object)owner))
		{
			characterBody = owner.GetComponent<CharacterBody>();
			num = (Object.op_Implicit((Object)(object)characterBody) ? Mathf.Lerp(characterBody.radius * 0.5f, characterBody.radius * 6f, 1f - Mathf.Abs(-1f + 2f * timer / 8f)) : 0f);
			((Component)this).transform.position = owner.transform.position;
			((Component)this).transform.localScale = new Vector3(num, num, num);
		}
		if (!NetworkServer.active)
		{
			return;
		}
		if (!Object.op_Implicit((Object)(object)owner))
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
			return;
		}
		attackStopwatch += Time.fixedDeltaTime;
		if (Object.op_Implicit((Object)(object)characterBody) && attackStopwatch >= 0.25f)
		{
			attackStopwatch = 0f;
			BlastAttack obj = new BlastAttack
			{
				attacker = owner,
				inflictor = ((Component)this).gameObject
			};
			obj.teamIndex = TeamComponent.GetObjectTeam(obj.attacker);
			obj.position = ((Component)this).transform.position;
			obj.procCoefficient = 0.1f;
			obj.radius = num;
			obj.baseForce = 0f;
			obj.baseDamage = 1f * characterBody.damage;
			obj.bonusForce = Vector3.zero;
			obj.crit = false;
			obj.damageType = DamageType.Generic;
			obj.attackerFiltering = AttackerFiltering.NeverHitSelf;
			obj.Fire();
		}
		if (timer >= 8f)
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.Write(owner);
			return true;
		}
		bool flag = false;
		if ((((NetworkBehaviour)this).syncVarDirtyBits & (true ? 1u : 0u)) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
				flag = true;
			}
			writer.Write(owner);
		}
		if (!flag)
		{
			writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
		}
		return flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		if (initialState)
		{
			___ownerNetId = reader.ReadNetworkId();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if (((uint)num & (true ? 1u : 0u)) != 0)
		{
			owner = reader.ReadGameObject();
		}
	}

	public override void PreStartClient()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		if (!((NetworkInstanceId)(ref ___ownerNetId)).IsEmpty())
		{
			Networkowner = ClientScene.FindLocalObject(___ownerNetId);
		}
	}
}
