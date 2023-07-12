using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Facepunch.Steamworks;

namespace RoR2;

[Serializable]
public class ConCommandException : Exception
{
	public ConCommandException()
	{
	}

	public ConCommandException(string message)
		: base(message)
	{
	}

	public ConCommandException(string message, Exception inner)
		: base(message, inner)
	{
	}

	protected ConCommandException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	public static void CheckSteamworks()
	{
		if (Client.Instance == null)
		{
			throw new ConCommandException("Steamworks client not available.");
		}
	}

	public static void CheckArgumentCount(List<string> args, int requiredArgCount)
	{
		if (args.Count < requiredArgCount)
		{
			throw new ConCommandException($"{requiredArgCount} argument(s) required, {args.Count} argument(s) provided.");
		}
	}
}
