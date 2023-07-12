using System;
using System.IO;
using System.Text;
using Facepunch.Steamworks;
using HG;
using RoR2.Networking;
using SteamAPIValidator;
using UnityEngine;
using Zio.FileSystems;

namespace RoR2;

public sealed class SteamworksClientManager : IDisposable
{
	private bool disposed;

	public static SteamworksClientManager instance { get; private set; }

	public Client steamworksClient { get; private set; }

	private static event Action _onLoaded;

	public static event Action onLoaded
	{
		add
		{
			if (instance == null)
			{
				_onLoaded += value;
			}
			else if (!instance.disposed)
			{
				value();
			}
		}
		remove
		{
			_onLoaded -= value;
		}
	}

	private SteamworksClientManager()
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Expected O, but got Unknown
		if (!Application.isEditor && File.Exists("steam_appid.txt"))
		{
			try
			{
				File.Delete("steam_appid.txt");
			}
			catch (Exception ex)
			{
				Debug.Log((object)ex.Message);
			}
			if (File.Exists("steam_appid.txt"))
			{
				Debug.Log((object)"Cannot delete steam_appid.txt. Quitting...");
				Dispose();
				return;
			}
		}
		RuntimePlatform platform = Application.platform;
		Config.ForUnity(((object)(RuntimePlatform)(ref platform)).ToString());
		steamworksClient = new Client(632360u);
		if (!((BaseSteamworks)steamworksClient).IsValid)
		{
			Dispose();
			return;
		}
		if (!Application.isEditor)
		{
			if (Client.RestartIfNecessary(632360u) || !((BaseSteamworks)steamworksClient).IsValid || !SteamApiValidator.IsValidSteamApiDll())
			{
				Debug.Log((object)"Unable to initialize Facepunch.Steamworks.");
				Dispose();
				return;
			}
			if (!steamworksClient.App.IsSubscribed(632360u))
			{
				Debug.Log((object)"Steam user not subscribed to app. Quitting...");
				Dispose();
				return;
			}
		}
		RoR2Application.steamBuildId = TextSerialization.ToStringInvariant(steamworksClient.BuildId);
		RoR2Application.onUpdate += Update;
		RoR2Application.cloudStorage = (FileSystem)(object)new SteamworksRemoteStorageFileSystem();
		(PlatformSystems.lobbyManager as SteamworksLobbyManager)?.Init();
	}

	private void Update()
	{
		((BaseSteamworks)steamworksClient).Update();
	}

	public void Dispose()
	{
		if (disposed)
		{
			return;
		}
		disposed = true;
		RoR2Application.onUpdate -= Update;
		if (steamworksClient != null)
		{
			if (Object.op_Implicit((Object)(object)NetworkManagerSystem.singleton))
			{
				NetworkManagerSystem.singleton.ForceCloseAllConnections();
			}
			Debug.Log((object)"Shutting down Steamworks...");
			steamworksClient.Lobby.Leave();
			if (Server.Instance != null)
			{
				((BaseSteamworks)Server.Instance).Dispose();
			}
			((BaseSteamworks)steamworksClient).Update();
			((BaseSteamworks)steamworksClient).Dispose();
			steamworksClient = null;
			Debug.Log((object)"Shut down Steamworks.");
		}
	}

	public static void Init()
	{
		RoR2Application.loadSteamworksClient = delegate
		{
			instance = new SteamworksClientManager();
			if (!instance.disposed)
			{
				SteamworksClientManager._onLoaded?.Invoke();
				SteamworksClientManager._onLoaded = null;
				return true;
			}
			return false;
		};
		RoR2Application.unloadSteamworksClient = delegate
		{
			instance?.Dispose();
			instance = null;
		};
	}

	[ConCommand(commandName = "steamworks_client_print_p2p_connection_status", flags = ConVarFlags.None, helpText = "Prints debug information for any established P2P connection to the specified Steam ID.")]
	private static void CCSteamworksClientPrintP2PConnectionStatus(ConCommandArgs args)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		P2PSessionState val = default(P2PSessionState);
		if (((BaseSteamworks)Client.Instance).Networking.GetP2PSessionState(args.GetArgSteamID(0).steamValue, ref val))
		{
			StringBuilder stringBuilder = StringBuilderPool.RentStringBuilder();
			stringBuilder.Append("BytesQueuedForSend").Append("=").Append(val.BytesQueuedForSend)
				.AppendLine();
			stringBuilder.Append("Connecting").Append("=").Append(val.Connecting)
				.AppendLine();
			stringBuilder.Append("ConnectionActive").Append("=").Append(val.ConnectionActive)
				.AppendLine();
			stringBuilder.Append("PacketsQueuedForSend").Append("=").Append(val.PacketsQueuedForSend)
				.AppendLine();
			stringBuilder.Append("P2PSessionError").Append("=").Append(val.P2PSessionError)
				.AppendLine();
			stringBuilder.Append("UsingRelay").Append("=").Append(val.UsingRelay)
				.AppendLine();
			Debug.Log((object)stringBuilder.ToString());
			StringBuilderPool.ReturnStringBuilder(stringBuilder);
		}
		else
		{
			Debug.Log((object)"Failed to retrieve P2P info for the specified Steam ID.");
		}
	}
}
