using System;
using RoR2.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace RoR2;

public static class QuitConfirmationHelper
{
	private enum NetworkStatus
	{
		None,
		SinglePlayer,
		Client,
		Host
	}

	private static bool IsQuitConfirmationRequired()
	{
		if (Object.op_Implicit((Object)(object)Run.instance) && !Object.op_Implicit((Object)(object)GameOverController.instance))
		{
			return true;
		}
		return false;
	}

	public static void IssueQuitCommand(NetworkUser sender, string consoleCmd)
	{
		IssueQuitCommand(RunCmd);
		void RunCmd()
		{
			Console.instance.SubmitCmd(sender, consoleCmd);
		}
	}

	public static void IssueQuitCommand(Action action)
	{
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Expected O, but got Unknown
		if (!IsQuitConfirmationRequired())
		{
			action();
			return;
		}
		NetworkStatus networkStatus = ((NetworkUser.readOnlyInstancesList.Count <= NetworkUser.readOnlyLocalPlayersList.Count) ? NetworkStatus.SinglePlayer : ((!NetworkServer.active) ? NetworkStatus.Client : NetworkStatus.Host));
		string text = "";
		text = networkStatus switch
		{
			NetworkStatus.None => "", 
			NetworkStatus.SinglePlayer => "QUIT_RUN_CONFIRM_DIALOG_BODY_SINGLEPLAYER", 
			NetworkStatus.Client => "QUIT_RUN_CONFIRM_DIALOG_BODY_CLIENT", 
			NetworkStatus.Host => "QUIT_RUN_CONFIRM_DIALOG_BODY_HOST", 
			_ => throw new ArgumentOutOfRangeException(), 
		};
		SimpleDialogBox simpleDialogBox = SimpleDialogBox.Create();
		simpleDialogBox.headerToken = new SimpleDialogBox.TokenParamsPair("QUIT_RUN_CONFIRM_DIALOG_TITLE");
		simpleDialogBox.descriptionToken = new SimpleDialogBox.TokenParamsPair(text);
		simpleDialogBox.AddActionButton(new UnityAction(action.Invoke), "DIALOG_OPTION_YES", true);
		simpleDialogBox.AddCancelButton("CANCEL");
	}

	[ConCommand(commandName = "quit_confirmed_command", flags = ConVarFlags.None, helpText = "Runs the command provided in the argument only if the user confirms they want to quit the current game via dialog UI.")]
	private static void CCQuitConfirmedCommand(ConCommandArgs args)
	{
		NetworkUser sender = args.sender;
		string consoleCmd = args[0];
		IssueQuitCommand(RunCmd);
		void RunCmd()
		{
			Console.instance.SubmitCmd(sender, consoleCmd);
		}
	}
}
