using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using Rewired;
using Rewired.Integration.UnityUI;
using RoR2.UI;
using UnityEngine;

namespace RoR2;

public class MPEventSystemManager : MonoBehaviour
{
	private static readonly Dictionary<int, MPEventSystem> eventSystems;

	public static ResourceAvailability availability;

	public static MPEventSystem combinedEventSystem { get; private set; }

	public static MPEventSystem kbmEventSystem { get; private set; }

	public static MPEventSystem primaryEventSystem { get; private set; }

	public static MPEventSystem FindEventSystem(Player inputPlayer)
	{
		eventSystems.TryGetValue(inputPlayer.id, out var value);
		return value;
	}

	private static void Initialize()
	{
		GameObject val = LegacyResourcesAPI.Load<GameObject>("Prefabs/UI/MPEventSystem");
		IList<Player> players = ReInput.players.Players;
		for (int i = 0; i < players.Count; i++)
		{
			GameObject val2 = Object.Instantiate<GameObject>(val, ((Component)RoR2Application.instance).transform);
			((Object)val2).name = string.Format(CultureInfo.InvariantCulture, "MPEventSystem Player{0}", i);
			MPEventSystem component = val2.GetComponent<MPEventSystem>();
			RewiredStandaloneInputModule component2 = val2.GetComponent<RewiredStandaloneInputModule>();
			Player val3 = players[i];
			component2.RewiredPlayerIds = new int[1] { val3.id };
			val2.GetComponent<MPInput>().player = val3;
			if (i == 1)
			{
				kbmEventSystem = component;
				component.allowCursorPush = false;
			}
			component.player = players[i];
			eventSystems[players[i].id] = component;
		}
		combinedEventSystem = eventSystems[0];
		combinedEventSystem.isCombinedEventSystem = true;
		RefreshEventSystems();
	}

	private static void RefreshEventSystems()
	{
		int count = LocalUserManager.readOnlyLocalUsersList.Count;
		ReadOnlyCollection<MPEventSystem> readOnlyInstancesList = MPEventSystem.readOnlyInstancesList;
		((Behaviour)readOnlyInstancesList[0]).enabled = count <= 1;
		for (int i = 1; i < readOnlyInstancesList.Count; i++)
		{
			((Behaviour)readOnlyInstancesList[i]).enabled = readOnlyInstancesList[i].localUser != null;
		}
		int num = 0;
		for (int j = 0; j < readOnlyInstancesList.Count; j++)
		{
			readOnlyInstancesList[j].playerSlot = (((Behaviour)readOnlyInstancesList[j]).enabled ? num++ : (-1));
		}
		primaryEventSystem = ((count > 0) ? LocalUserManager.readOnlyLocalUsersList[0].eventSystem : combinedEventSystem);
		availability.MakeAvailable();
	}

	private void Awake()
	{
		Initialize();
	}

	private void Update()
	{
		if (!Application.isBatchMode)
		{
			Cursor.lockState = (CursorLockMode)((!kbmEventSystem.isCursorVisible && !combinedEventSystem.isCursorVisible) ? 1 : 2);
			Cursor.visible = false;
		}
	}

	static MPEventSystemManager()
	{
		eventSystems = new Dictionary<int, MPEventSystem>();
		LocalUserManager.onLocalUsersUpdated += RefreshEventSystems;
	}
}
