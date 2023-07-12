using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

[DefaultExecutionOrder(-1)]
public class NetworkRuleBook : NetworkBehaviour
{
	private const uint ruleBookDirtyBit = 1u;

	public RuleBook ruleBook { get; private set; }

	public event Action<NetworkRuleBook> onRuleBookUpdated;

	private void Awake()
	{
		ruleBook = new RuleBook();
	}

	[Server]
	public void SetRuleBook([NotNull] RuleBook newRuleBook)
	{
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'System.Void RoR2.NetworkRuleBook::SetRuleBook(RoR2.RuleBook)' called on client");
		}
		else if (!ruleBook.Equals(newRuleBook))
		{
			((NetworkBehaviour)this).SetDirtyBit(1u);
			ruleBook.Copy(newRuleBook);
			this.onRuleBookUpdated?.Invoke(this);
		}
	}

	public override bool OnSerialize(NetworkWriter writer, bool initialState)
	{
		uint num = ((NetworkBehaviour)this).syncVarDirtyBits;
		if (initialState)
		{
			num = 1u;
		}
		bool num2 = (num & 1) != 0;
		writer.Write((byte)num);
		if (num2)
		{
			writer.Write(ruleBook);
		}
		if (!initialState)
		{
			return num != 0;
		}
		return false;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		if (((uint)reader.ReadByte() & (true ? 1u : 0u)) != 0)
		{
			reader.ReadRuleBook(ruleBook);
			try
			{
				this.onRuleBookUpdated?.Invoke(this);
			}
			catch (Exception ex)
			{
				Debug.LogError((object)ex);
			}
		}
	}

	private void UNetVersion()
	{
	}

	public override void PreStartClient()
	{
	}
}
