using System;
using System.Runtime.CompilerServices;
using Epic.OnlineServices;
using Epic.OnlineServices.Logging;
using Epic.OnlineServices.Platform;
using UnityEngine;

namespace RoR2;

public class EOSPlatformManager : PlatformManager
{
	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static LogMessageFunc _003C_003E9__13_0;

		internal void _003CSetupLogging_003Eb__13_0(LogMessage logMessage)
		{
			System.Console.WriteLine(logMessage.Message);
		}
	}

	private static PlatformInterface _platformInterface;

	private string _productName = Application.productName;

	private string _productVersion = Application.version;

	private string _productId = "3bf8fc77540f41b5bb253d9563eec679";

	private string _sandboxId = "0dfcaeede0214b80b597f08fd7d64b1b";

	private string _deploymentId = "f2ca672e660a4792a17b124291408566";

	private string _clientId;

	private string _clientSecret;

	private EOSLibraryManager libManager;

	public EOSPlatformManager()
	{
		libManager = new EOSLibraryManager();
		InitializePlatformInterface();
		SetupLogging();
		CreatePlatformInterface();
	}

	public override void InitializePlatformManager()
	{
		RoR2Application.onShutDown = (Action)Delegate.Combine(RoR2Application.onShutDown, new Action(Shutdown));
		RoR2Application.onUpdate += UpdatePlatformManager;
	}

	public static PlatformInterface GetPlatformInterface()
	{
		if ((Handle)(object)_platformInterface == (Handle)null)
		{
			throw new Exception("_platformInterface has not been set. Initialize EOSPlatformManager before attempting to access _platformInterface.");
		}
		return _platformInterface;
	}

	private void InitializePlatformInterface()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Invalid comparison between Unknown and I4
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		Result val = PlatformInterface.Initialize(new InitializeOptions
		{
			ProductName = _productName,
			ProductVersion = _productVersion
		});
		if ((int)val != 0 && (int)val != 15)
		{
			throw new Exception("Failed to initialize platform: " + val);
		}
	}

	private void SetupLogging()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Expected O, but got Unknown
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		LoggingInterface.SetLogLevel((LogCategory)int.MaxValue, (LogLevel)600);
		object obj = _003C_003Ec._003C_003E9__13_0;
		if (obj == null)
		{
			LogMessageFunc val = delegate(LogMessage logMessage)
			{
				System.Console.WriteLine(logMessage.Message);
			};
			obj = (object)val;
			_003C_003Ec._003C_003E9__13_0 = val;
		}
		LoggingInterface.SetCallback((LogMessageFunc)obj);
	}

	private void CreatePlatformInterface()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Expected O, but got Unknown
		//IL_004f: Expected O, but got Unknown
		_platformInterface = PlatformInterface.Create(new Options
		{
			ProductId = _productId,
			SandboxId = _sandboxId,
			DeploymentId = _deploymentId,
			ClientCredentials = new ClientCredentials
			{
				ClientId = "xyza7891fuF5aodDdJ1ITnkYeRbyJbnT",
				ClientSecret = "QMwgx1LJIkRt25iA9YjXldcMD/8aAeQTke7a5FVU3no"
			}
		});
		if ((Handle)(object)_platformInterface == (Handle)null)
		{
			throw new Exception("Failed to create platform");
		}
	}

	private void Shutdown()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		if ((Handle)(object)_platformInterface != (Handle)null)
		{
			_platformInterface.Release();
			_platformInterface = null;
			PlatformInterface.Shutdown();
		}
		libManager.Shutdown();
	}

	protected override void UpdatePlatformManager()
	{
		if ((Handle)(object)_platformInterface != (Handle)null)
		{
			_platformInterface.Tick();
		}
	}
}
