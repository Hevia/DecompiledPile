using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public class DamageDealtMessage : MessageBase
{
	public GameObject victim;

	public float damage;

	public GameObject attacker;

	public Vector3 position;

	public bool crit;

	public DamageType damageType;

	public DamageColorIndex damageColorIndex;

	public bool hitLowHealth;

	public bool isSilent => (damageType & DamageType.Silent) != 0;

	public override void Serialize(NetworkWriter writer)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		((MessageBase)this).Serialize(writer);
		writer.Write(victim);
		writer.Write(damage);
		writer.Write(attacker);
		writer.Write(position);
		writer.Write(crit);
		writer.Write(damageType);
		writer.Write(damageColorIndex);
		writer.Write(hitLowHealth);
	}

	public override void Deserialize(NetworkReader reader)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		((MessageBase)this).Deserialize(reader);
		victim = reader.ReadGameObject();
		damage = reader.ReadSingle();
		attacker = reader.ReadGameObject();
		position = reader.ReadVector3();
		crit = reader.ReadBoolean();
		damageType = reader.ReadDamageType();
		damageColorIndex = reader.ReadDamageColorIndex();
		hitLowHealth = reader.ReadBoolean();
	}
}
