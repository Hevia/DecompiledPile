using System;
using System.Collections.Generic;
using System.Net;
using Facepunch.Steamworks;
using HGsignup.Service;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RoR2.UI.MainMenu;

public class SignupMainMenuScreen : BaseMainMenuScreen
{
	private const string environment = "prod";

	private const string title = "Canary";

	private static string viewableName = "Signup";

	[SerializeField]
	private TMP_InputField emailInput;

	[SerializeField]
	private HGButton submitButton;

	[SerializeField]
	private HGButton backButton;

	[SerializeField]
	private HGTextMeshProUGUI successText;

	[SerializeField]
	private HGTextMeshProUGUI failureText;

	[SerializeField]
	private HGTextMeshProUGUI waitingText;

	[SerializeField]
	private HGGamepadInputEvent backGamepadInputEvent;

	private SignupClient signupClient;

	private string email;

	private bool awaitingResponse;

	private HashSet<string> successfulEmailSubmissions;

	[SystemInitializer(new Type[] { })]
	private static void Init()
	{
		ViewablesCatalog.AddNodeToRoot(new ViewablesCatalog.Node(viewableName, isFolder: false)
		{
			shouldShowUnviewed = CheckViewable
		});
	}

	private static bool CheckViewable(UserProfile userProfile)
	{
		return !userProfile.HasViewedViewable("/" + viewableName);
	}

	private new void Awake()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Expected O, but got Unknown
		signupClient = new SignupClient("prod");
		successfulEmailSubmissions = new HashSet<string>();
	}

	private void OnEnable()
	{
		HGTextMeshProUGUI hGTextMeshProUGUI = waitingText;
		if (hGTextMeshProUGUI != null)
		{
			GameObject gameObject = ((Component)hGTextMeshProUGUI).gameObject;
			if (gameObject != null)
			{
				gameObject.SetActive(false);
			}
		}
		HGTextMeshProUGUI hGTextMeshProUGUI2 = successText;
		if (hGTextMeshProUGUI2 != null)
		{
			GameObject gameObject2 = ((Component)hGTextMeshProUGUI2).gameObject;
			if (gameObject2 != null)
			{
				gameObject2.SetActive(false);
			}
		}
		HGTextMeshProUGUI hGTextMeshProUGUI3 = failureText;
		if (hGTextMeshProUGUI3 != null)
		{
			GameObject gameObject3 = ((Component)hGTextMeshProUGUI3).gameObject;
			if (gameObject3 != null)
			{
				gameObject3.SetActive(false);
			}
		}
		if (Object.op_Implicit((Object)(object)backGamepadInputEvent))
		{
			((Behaviour)backGamepadInputEvent).enabled = true;
		}
	}

	private new void Update()
	{
		base.Update();
		if (Object.op_Implicit((Object)(object)submitButton))
		{
			if (Object.op_Implicit((Object)(object)emailInput) && SignupClient.IsEmailValid(emailInput.text) && !awaitingResponse && !successfulEmailSubmissions.Contains(emailInput.text))
			{
				((Selectable)submitButton).interactable = true;
			}
			else
			{
				((Selectable)submitButton).interactable = false;
			}
		}
		if (Object.op_Implicit((Object)(object)backButton))
		{
			((Selectable)backButton).interactable = !awaitingResponse;
		}
		if (Object.op_Implicit((Object)(object)backGamepadInputEvent))
		{
			((Behaviour)backGamepadInputEvent).enabled = !awaitingResponse;
		}
	}

	public void TrySubmit()
	{
		if (awaitingResponse || !Object.op_Implicit((Object)(object)emailInput) || successfulEmailSubmissions.Contains(emailInput.text) || !SignupClient.IsEmailValid(emailInput.text))
		{
			return;
		}
		email = emailInput.text;
		User user = Client.Instance.User;
		user.OnEncryptedAppTicketRequestComplete = (Action<bool, byte[]>)Delegate.Combine(user.OnEncryptedAppTicketRequestComplete, new Action<bool, byte[]>(ProcessAppTicketRefresh));
		Client.Instance.User.RequestEncryptedAppTicketAsync(new byte[0]);
		awaitingResponse = true;
		HGTextMeshProUGUI hGTextMeshProUGUI = waitingText;
		if (hGTextMeshProUGUI != null)
		{
			GameObject gameObject = ((Component)hGTextMeshProUGUI).gameObject;
			if (gameObject != null)
			{
				gameObject.SetActive(true);
			}
		}
		HGTextMeshProUGUI hGTextMeshProUGUI2 = successText;
		if (hGTextMeshProUGUI2 != null)
		{
			GameObject gameObject2 = ((Component)hGTextMeshProUGUI2).gameObject;
			if (gameObject2 != null)
			{
				gameObject2.SetActive(false);
			}
		}
		HGTextMeshProUGUI hGTextMeshProUGUI3 = failureText;
		if (hGTextMeshProUGUI3 != null)
		{
			GameObject gameObject3 = ((Component)hGTextMeshProUGUI3).gameObject;
			if (gameObject3 != null)
			{
				gameObject3.SetActive(false);
			}
		}
	}

	private void ProcessAppTicketRefresh(bool success, byte[] ticket)
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Expected O, but got Unknown
		bool flag = success;
		User user = Client.Instance.User;
		user.OnEncryptedAppTicketRequestComplete = (Action<bool, byte[]>)Delegate.Remove(user.OnEncryptedAppTicketRequestComplete, new Action<bool, byte[]>(ProcessAppTicketRefresh));
		if (success)
		{
			Debug.Log((object)"Successfully got Encrypted App Ticket from Steam for mailing list!");
			if (SignupClient.IsTicketValid(ticket))
			{
				if (signupClient != null)
				{
					signupClient.SignupCompleted += new SignupCompletedEventHandler(SignupCompleted);
					signupClient.SignupSteamAsync(email, "Canary", ticket);
				}
			}
			else
			{
				Debug.LogError((object)"Can't signup with an invalid ticket!");
				flag = false;
			}
		}
		else
		{
			Debug.LogError((object)"Failure refreshing Encrypted App Ticket from Steam for mailing list!");
			flag = false;
		}
		if (flag)
		{
			return;
		}
		awaitingResponse = false;
		HGTextMeshProUGUI hGTextMeshProUGUI = waitingText;
		if (hGTextMeshProUGUI != null)
		{
			GameObject gameObject = ((Component)hGTextMeshProUGUI).gameObject;
			if (gameObject != null)
			{
				gameObject.SetActive(false);
			}
		}
		HGTextMeshProUGUI hGTextMeshProUGUI2 = successText;
		if (hGTextMeshProUGUI2 != null)
		{
			GameObject gameObject2 = ((Component)hGTextMeshProUGUI2).gameObject;
			if (gameObject2 != null)
			{
				gameObject2.SetActive(false);
			}
		}
		HGTextMeshProUGUI hGTextMeshProUGUI3 = failureText;
		if (hGTextMeshProUGUI3 != null)
		{
			GameObject gameObject3 = ((Component)hGTextMeshProUGUI3).gameObject;
			if (gameObject3 != null)
			{
				gameObject3.SetActive(true);
			}
		}
	}

	private void SignupCompleted(string email, HttpStatusCode statusCode)
	{
		awaitingResponse = false;
		HGTextMeshProUGUI hGTextMeshProUGUI = waitingText;
		if (hGTextMeshProUGUI != null)
		{
			GameObject gameObject = ((Component)hGTextMeshProUGUI).gameObject;
			if (gameObject != null)
			{
				gameObject.SetActive(false);
			}
		}
		if (statusCode == HttpStatusCode.OK)
		{
			successfulEmailSubmissions.Add(email);
			HGTextMeshProUGUI hGTextMeshProUGUI2 = successText;
			if (hGTextMeshProUGUI2 != null)
			{
				GameObject gameObject2 = ((Component)hGTextMeshProUGUI2).gameObject;
				if (gameObject2 != null)
				{
					gameObject2.SetActive(true);
				}
			}
			HGTextMeshProUGUI hGTextMeshProUGUI3 = failureText;
			if (hGTextMeshProUGUI3 != null)
			{
				GameObject gameObject3 = ((Component)hGTextMeshProUGUI3).gameObject;
				if (gameObject3 != null)
				{
					gameObject3.SetActive(false);
				}
			}
			return;
		}
		HGTextMeshProUGUI hGTextMeshProUGUI4 = successText;
		if (hGTextMeshProUGUI4 != null)
		{
			GameObject gameObject4 = ((Component)hGTextMeshProUGUI4).gameObject;
			if (gameObject4 != null)
			{
				gameObject4.SetActive(false);
			}
		}
		HGTextMeshProUGUI hGTextMeshProUGUI5 = failureText;
		if (hGTextMeshProUGUI5 != null)
		{
			GameObject gameObject5 = ((Component)hGTextMeshProUGUI5).gameObject;
			if (gameObject5 != null)
			{
				gameObject5.SetActive(true);
			}
		}
	}
}
