using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public class GameObjectUnlockableFilter : NetworkBehaviour
{
	[Tooltip("'requiredUnlockable' will be discontinued. Use 'requiredUnlockableDef' instead.")]
	[Obsolete("'requiredUnlockable' will be discontinued. Use 'requiredUnlockableDef' instead.", false)]
	public string requiredUnlockable;

	[Tooltip("'forbiddenUnlockable' will be discontinued. Use 'forbiddenUnlockableDef' instead.")]
	[Obsolete("'forbiddenUnlockable' will be discontinued. Use 'forbiddenUnlockableDef' instead.", false)]
	public string forbiddenUnlockable;

	public UnlockableDef requiredUnlockableDef;

	public UnlockableDef forbiddenUnlockableDef;

	[SyncVar]
	private bool active;

	public bool Networkactive
	{
		get
		{
			return active;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).SetSyncVar<bool>(value, ref active, 1u);
		}
	}

	private void Start()
	{
		if (NetworkServer.active)
		{
			Networkactive = GameObjectIsValid();
		}
	}

	private void FixedUpdate()
	{
		((Component)this).gameObject.SetActive(active);
	}

	private bool GameObjectIsValid()
	{
		if (Object.op_Implicit((Object)(object)Run.instance))
		{
			ref string reference = ref requiredUnlockable;
			ref string reference2 = ref forbiddenUnlockable;
			if (!Object.op_Implicit((Object)(object)requiredUnlockableDef) && !string.IsNullOrEmpty(reference))
			{
				requiredUnlockableDef = UnlockableCatalog.GetUnlockableDef(reference);
				reference = null;
			}
			if (!Object.op_Implicit((Object)(object)forbiddenUnlockableDef) && !string.IsNullOrEmpty(reference2))
			{
				forbiddenUnlockableDef = UnlockableCatalog.GetUnlockableDef(reference2);
				reference2 = null;
			}
			bool num = !Object.op_Implicit((Object)(object)requiredUnlockableDef) || Run.instance.IsUnlockableUnlocked(requiredUnlockableDef);
			bool flag = !Object.op_Implicit((Object)(object)forbiddenUnlockableDef) || Run.instance.DoesEveryoneHaveThisUnlockableUnlocked(forbiddenUnlockableDef);
			if (num)
			{
				return !flag;
			}
			return false;
		}
		return true;
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.Write(active);
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
			writer.Write(active);
		}
		if (!flag)
		{
			writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
		}
		return flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		if (initialState)
		{
			active = reader.ReadBoolean();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if (((uint)num & (true ? 1u : 0u)) != 0)
		{
			active = reader.ReadBoolean();
		}
	}

	public override void PreStartClient()
	{
	}
}
