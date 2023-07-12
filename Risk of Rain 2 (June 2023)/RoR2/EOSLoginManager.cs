using System;
using Epic.OnlineServices;
using Epic.OnlineServices.Auth;
using Epic.OnlineServices.Connect;
using Facepunch.Steamworks;
using RoR2.ConVar;
using UnityEngine;

namespace RoR2;

public class EOSLoginManager
{
	public enum EOSLoginState
	{
		None,
		AttemptingLogin,
		AttemptingLink,
		FailedLogin,
		FailedLink,
		Success
	}

	public LoginCredentialType _loginCredentialType = (LoginCredentialType)1;

	public string _loginCredentialId;

	public string _loginCredentialToken;

	public static EOSLoginManager instance = null;

	public static readonly BoolConVar cvLinkEOSAccount = new BoolConVar("eos_link_account", ConVarFlags.None, "0", "trigger the account linking process");

	public static EpicAccountId loggedInAuthId = null;

	public static ProductUserId loggedInProductId = null;

	public static UserID loggedInUserID;

	private static string ticket;

	private ConnectInterface connectInterface;

	public static EOSLoginState loginState { get; private set; } = EOSLoginState.None;


	public static event Action<EpicAccountId> OnAuthLoggedIn;

	public static event Action<ProductUserId> OnConnectLoggedIn;

	public static bool IsWaitingOnLogin()
	{
		return loginState != EOSLoginState.Success;
	}

	public void TryLogin()
	{
		instance = this;
		RoR2Application.onUpdate += OnUpdate;
		ExternalAuthLogin_Steam();
	}

	private void OnUpdate()
	{
		if (cvLinkEOSAccount.value && !IsWaitingOnLogin())
		{
			StartSteamLoginWithDefaultOptions(attemptAccountLink: true);
		}
	}

	private void OnSteamworksInitialized()
	{
		byte[] data = Client.Instance.Auth.GetAuthSessionTicket().Data;
		if (data != null)
		{
			Debug.Log((object)"Successfully got authSessionTicketData from Steam!");
			ticket = HelperExtensions.ToHexString(data);
			StartSteamLoginWithDefaultOptions();
		}
		else
		{
			Debug.Log((object)"Failure getting authSessionTicketData from Steam!  Can't log into EGS!");
		}
	}

	public void ExternalAuthLogin_Steam()
	{
		if (Client.Instance != null)
		{
			OnSteamworksInitialized();
		}
		else
		{
			SteamworksClientManager.onLoaded += OnSteamworksInitialized;
		}
	}

	public void StartSteamLoginWithDefaultOptions(bool attemptAccountLink = false)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Expected O, but got Unknown
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Expected O, but got Unknown
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		LoginOptions val = new LoginOptions
		{
			Credentials = new Credentials
			{
				Type = (LoginCredentialType)7,
				Token = ticket,
				ExternalType = (ExternalCredentialType)18
			},
			ScopeFlags = (AuthScopeFlags)7
		};
		Debug.Log((object)string.Format("Attempting Auth login with {0}.{1}", "ExternalCredentialType", val.Credentials.ExternalType));
		StartEGSLogin(val, attemptAccountLink);
	}

	private void StartEGSLoginWithDefaultOptions()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Expected O, but got Unknown
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected O, but got Unknown
		LoginOptions loginOptions = new LoginOptions
		{
			Credentials = new Credentials
			{
				Type = _loginCredentialType,
				Id = _loginCredentialId,
				Token = _loginCredentialToken
			},
			ScopeFlags = (AuthScopeFlags)7
		};
		StartEGSLogin(loginOptions);
	}

	private void StartEGSLogin(LoginOptions loginOptions, bool attemptLink = false)
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Expected O, but got Unknown
		loginState = EOSLoginState.AttemptingLogin;
		AuthInterface authInterface = EOSPlatformManager.GetPlatformInterface().GetAuthInterface();
		if ((Handle)(object)authInterface != (Handle)null)
		{
			OnLinkAccountCallback val = default(OnLinkAccountCallback);
			authInterface.Login(loginOptions, (object)null, (OnLoginCallback)delegate(LoginCallbackInfo loginCallbackInfo)
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_002c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0064: Unknown result type (might be due to invalid IL or missing references)
				//IL_0069: Unknown result type (might be due to invalid IL or missing references)
				//IL_0075: Unknown result type (might be due to invalid IL or missing references)
				//IL_007d: Expected O, but got Unknown
				//IL_0097: Unknown result type (might be due to invalid IL or missing references)
				//IL_009c: Unknown result type (might be due to invalid IL or missing references)
				//IL_009e: Expected O, but got Unknown
				//IL_00a3: Expected O, but got Unknown
				//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
				//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
				if ((int)loginCallbackInfo.ResultCode == 0)
				{
					loginState = EOSLoginState.Success;
					cvLinkEOSAccount.SetBool(newValue: false);
					ProcessSuccessfulAuthLogin(loginCallbackInfo.LocalUserId);
				}
				else if (Common.IsOperationComplete(loginCallbackInfo.ResultCode))
				{
					if ((Handle)(object)loginCallbackInfo.ContinuanceToken != (Handle)null)
					{
						if (attemptLink)
						{
							loginState = EOSLoginState.AttemptingLink;
							Debug.Log((object)"EOS Auth Login failed but we have a Continuance Token, so we're gonna use that");
							LinkAccountOptions val2 = new LinkAccountOptions
							{
								ContinuanceToken = loginCallbackInfo.ContinuanceToken,
								LinkAccountFlags = (LinkAccountFlags)0
							};
							AuthInterface obj = authInterface;
							OnLinkAccountCallback obj2 = val;
							if (obj2 == null)
							{
								OnLinkAccountCallback val3 = delegate(LinkAccountCallbackInfo linkAccountCallbackInfo)
								{
									//IL_0001: Unknown result type (might be due to invalid IL or missing references)
									//IL_002c: Unknown result type (might be due to invalid IL or missing references)
									if ((int)linkAccountCallbackInfo.ResultCode == 0)
									{
										loginState = EOSLoginState.Success;
										ProcessSuccessfulAuthLogin(linkAccountCallbackInfo.LocalUserId);
									}
									else
									{
										loginState = EOSLoginState.FailedLink;
										Debug.Log((object)("EOS Account Linking failed: " + linkAccountCallbackInfo.ResultCode));
									}
								};
								OnLinkAccountCallback val4 = val3;
								val = val3;
								obj2 = val4;
							}
							obj.LinkAccount(val2, (object)null, obj2);
						}
						else
						{
							Debug.Log((object)"EOS Auth Login failed so we're going to try with connect login with the app ticket");
							ProcessSteamAppTicketConnectLogin(loginOptions.Credentials.Token);
						}
					}
					else
					{
						loginState = EOSLoginState.FailedLogin;
						Debug.Log((object)("EOS Auth Login failed: " + loginCallbackInfo.ResultCode));
					}
				}
				else
				{
					loginState = EOSLoginState.FailedLogin;
					Debug.Log((object)("EOS Auth Login failed: " + loginCallbackInfo.ResultCode));
				}
			});
			return;
		}
		throw new Exception("Failed to get auth interface");
	}

	private void ProcessSuccessfulAuthLogin(EpicAccountId loggedInId)
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Expected O, but got Unknown
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Expected O, but got Unknown
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Expected O, but got Unknown
		if ((Handle)(object)loggedInId == (Handle)null || !loggedInId.IsValid())
		{
			Debug.LogError((object)"loggedInId is not valid.");
			loggedInAuthId = null;
			return;
		}
		loggedInAuthId = loggedInId;
		Debug.Log((object)("Auth Login succeeded, Id = " + loggedInAuthId));
		EOSLoginManager.OnAuthLoggedIn?.Invoke(loggedInAuthId);
		Debug.Log((object)"Attempting Connect login with Auth Credentials");
		Token val = null;
		if ((int)EOSPlatformManager.GetPlatformInterface().GetAuthInterface().CopyUserAuthToken(new CopyUserAuthTokenOptions(), loggedInAuthId, ref val) == 0)
		{
			LoginOptions options = new LoginOptions
			{
				Credentials = new Credentials
				{
					Token = val.AccessToken,
					Type = (ExternalCredentialType)0
				},
				UserLoginInfo = null
			};
			AttemptConnectLogin(options);
		}
	}

	private void ProcessSteamAppTicketConnectLogin(string appTicket)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Expected O, but got Unknown
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Expected O, but got Unknown
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		LoginOptions val = new LoginOptions
		{
			Credentials = new Credentials
			{
				Token = appTicket,
				Type = (ExternalCredentialType)18
			},
			UserLoginInfo = null
		};
		Debug.Log((object)string.Format("Attempting Connect login with {0}.{1}", "ExternalCredentialType", val.Credentials.Type));
		AttemptConnectLogin(val);
	}

	private bool IsConnectInterfaceValid()
	{
		if ((Handle)(object)connectInterface == (Handle)null)
		{
			connectInterface = EOSPlatformManager.GetPlatformInterface().GetConnectInterface();
		}
		return (Handle)(object)connectInterface != (Handle)null;
	}

	private void AttemptConnectLogin(LoginOptions options)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		if (IsConnectInterfaceValid())
		{
			connectInterface.Login(options, (object)null, (OnLoginCallback)delegate(LoginCallbackInfo loginCallbackInfo)
			{
				//IL_001a: Unknown result type (might be due to invalid IL or missing references)
				//IL_0039: Unknown result type (might be due to invalid IL or missing references)
				//IL_003f: Invalid comparison between Unknown and I4
				//IL_004b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0050: Unknown result type (might be due to invalid IL or missing references)
				//IL_0062: Expected O, but got Unknown
				//IL_0071: Unknown result type (might be due to invalid IL or missing references)
				//IL_007b: Expected O, but got Unknown
				//IL_0082: Unknown result type (might be due to invalid IL or missing references)
				//IL_0099: Unknown result type (might be due to invalid IL or missing references)
				if ((int)loginCallbackInfo.ResultCode == 0)
				{
					CompleteConnectLogin(loginCallbackInfo.LocalUserId);
				}
				else if ((int)loginCallbackInfo.ResultCode == 3)
				{
					Debug.Log((object)"EOS Connect Login returned InvalidUser, attempting to create new user");
					CreateUserOptions val = new CreateUserOptions
					{
						ContinuanceToken = loginCallbackInfo.ContinuanceToken
					};
					connectInterface.CreateUser(val, (object)null, (OnCreateUserCallback)delegate(CreateUserCallbackInfo createUserCallbackInfo)
					{
						//IL_0001: Unknown result type (might be due to invalid IL or missing references)
						//IL_0025: Unknown result type (might be due to invalid IL or missing references)
						if ((int)createUserCallbackInfo.ResultCode == 0)
						{
							CompleteConnectLogin(createUserCallbackInfo.LocalUserId);
						}
						else
						{
							Debug.Log((object)("EOS Connect Create User failed: " + loginCallbackInfo.ResultCode));
						}
					});
				}
				else if (Common.IsOperationComplete(loginCallbackInfo.ResultCode))
				{
					Debug.Log((object)("EOS Connect Login failed: " + loginCallbackInfo.ResultCode));
				}
			});
			return;
		}
		throw new Exception("Failed to get connect interface");
	}

	private void CompleteConnectLogin(ProductUserId localUserId)
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Expected O, but got Unknown
		//IL_006e: Expected O, but got Unknown
		if ((Handle)(object)localUserId == (Handle)null || !localUserId.IsValid())
		{
			Debug.LogError((object)"localUserId is not valid.");
			loggedInProductId = null;
			return;
		}
		loggedInProductId = localUserId;
		loggedInUserID = new UserID(new CSteamID(loggedInProductId));
		EOSLoginManager.OnConnectLoggedIn?.Invoke(loggedInProductId);
		connectInterface.AddNotifyAuthExpiration(new AddNotifyAuthExpirationOptions(), (object)null, new OnAuthExpirationCallback(OnAuthExpirationCallback));
		loginState = EOSLoginState.Success;
		Debug.Log((object)("Connect Login Successful!  User Id = " + ((object)localUserId).ToString()));
	}

	private void OnAuthExpirationCallback(AuthExpirationCallbackInfo data)
	{
		object clientData = data.ClientData;
		Token val = (Token)((clientData is Token) ? clientData : null);
		Debug.Log((object)string.Format("Auth expired! Attempting to refresh now.\n{0} : {1}\n{2} : {3}", "LocalUserId", data.LocalUserId, "ExpiresIn", (val != null) ? new double?(val.ExpiresIn) : null));
		if ((Handle)(object)loggedInAuthId != (Handle)null)
		{
			ProcessSuccessfulAuthLogin(loggedInAuthId);
		}
		else
		{
			ProcessAppTicketRefresh();
		}
	}

	private void ProcessAppTicketRefresh()
	{
		byte[] data = Client.Instance.Auth.GetAuthSessionTicket().Data;
		if (data != null)
		{
			Debug.Log((object)"Successfully got authSessionTicketData from Steam!");
			ticket = HelperExtensions.ToHexString(data);
			ProcessSteamAppTicketConnectLogin(ticket);
		}
		else
		{
			Debug.Log((object)"Failure getting authSessionTicketData from Steam!  Can't log into EGS!");
		}
	}
}
