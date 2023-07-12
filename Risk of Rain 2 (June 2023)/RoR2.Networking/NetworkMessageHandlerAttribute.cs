using System;
using System.Collections.Generic;
using System.Reflection;
using HG.Reflection;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2.Networking;

[MeansImplicitUse]
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class NetworkMessageHandlerAttribute : SearchableAttribute
{
	public short msgType;

	public bool server;

	public bool client;

	private NetworkMessageDelegate messageHandler;

	private static List<NetworkMessageHandlerAttribute> clientMessageHandlers = new List<NetworkMessageHandlerAttribute>();

	private static List<NetworkMessageHandlerAttribute> serverMessageHandlers = new List<NetworkMessageHandlerAttribute>();

	[RuntimeInitializeOnLoadMethod(/*Could not decode attribute arguments.*/)]
	private static void CollectHandlers()
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Expected O, but got Unknown
		clientMessageHandlers.Clear();
		serverMessageHandlers.Clear();
		HashSet<short> hashSet = new HashSet<short>();
		foreach (NetworkMessageHandlerAttribute instance in SearchableAttribute.GetInstances<NetworkMessageHandlerAttribute>())
		{
			MethodInfo methodInfo = ((SearchableAttribute)instance).target as MethodInfo;
			if (methodInfo == null || !methodInfo.IsStatic)
			{
				continue;
			}
			instance.messageHandler = (NetworkMessageDelegate)Delegate.CreateDelegate(typeof(NetworkMessageDelegate), methodInfo);
			if (instance.messageHandler != null)
			{
				if (instance.client)
				{
					clientMessageHandlers.Add(instance);
					hashSet.Add(instance.msgType);
				}
				if (instance.server)
				{
					serverMessageHandlers.Add(instance);
					hashSet.Add(instance.msgType);
				}
			}
			if (instance.messageHandler == null)
			{
				Debug.LogWarningFormat("Could not register message handler for {0}. The function signature is likely incorrect.", new object[1] { methodInfo.Name });
			}
			if (!instance.client && !instance.server)
			{
				Debug.LogWarningFormat("Could not register message handler for {0}. It is marked as neither server nor client.", new object[1] { methodInfo.Name });
			}
		}
		for (short num = 48; num < 78; num = (short)(num + 1))
		{
			if (!hashSet.Contains(num))
			{
				Debug.LogWarningFormat("Network message MsgType.Highest + {0} is unregistered.", new object[1] { num - 47 });
			}
		}
	}

	public static void RegisterServerMessages()
	{
		foreach (NetworkMessageHandlerAttribute serverMessageHandler in serverMessageHandlers)
		{
			NetworkServer.RegisterHandler(serverMessageHandler.msgType, serverMessageHandler.messageHandler);
		}
	}

	public static void RegisterClientMessages(NetworkClient client)
	{
		foreach (NetworkMessageHandlerAttribute clientMessageHandler in clientMessageHandlers)
		{
			client.RegisterHandler(clientMessageHandler.msgType, clientMessageHandler.messageHandler);
		}
	}
}
