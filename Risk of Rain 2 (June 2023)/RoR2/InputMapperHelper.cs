using System;
using System.Collections.Generic;
using System.Linq;
using Rewired;
using RoR2.UI;
using UnityEngine;

namespace RoR2;

public class InputMapperHelper : IDisposable
{
	private readonly MPEventSystem eventSystem;

	private readonly List<InputMapper> inputMappers = new List<InputMapper>();

	private ControllerMap[] maps = Array.Empty<ControllerMap>();

	private SimpleDialogBox dialogBox;

	public float timeout = 5f;

	private float timer;

	private Player currentPlayer;

	private InputAction currentAction;

	private AxisRange currentAxisRange;

	private Action<ConflictResponse> conflictResponseCallback;

	private static readonly HashSet<string> forbiddenElements = new HashSet<string>
	{
		"Left Stick X",
		"Left Stick Y",
		"Right Stick X",
		"Right Stick Y",
		"Mouse Horizontal",
		"Mouse Vertical",
		Keyboard.GetKeyName((KeyCode)27),
		Keyboard.GetKeyName((KeyCode)271),
		Keyboard.GetKeyName((KeyCode)13)
	};

	public bool isListening { get; private set; }

	public InputMapperHelper(MPEventSystem eventSystem)
	{
		this.eventSystem = eventSystem;
	}

	private InputMapper AddInputMapper()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Expected O, but got Unknown
		InputMapper val = new InputMapper();
		val.ConflictFoundEvent += InputMapperOnConflictFoundEvent;
		val.CanceledEvent += InputMapperOnCanceledEvent;
		val.ErrorEvent += InputMapperOnErrorEvent;
		val.InputMappedEvent += InputMapperOnInputMappedEvent;
		val.StartedEvent += InputMapperOnStartedEvent;
		val.StoppedEvent += InputMapperOnStoppedEvent;
		val.TimedOutEvent += InputMapperOnTimedOutEvent;
		val.options = new Options
		{
			allowAxes = true,
			allowButtons = true,
			allowKeyboardKeysWithModifiers = false,
			allowKeyboardModifierKeyAsPrimary = true,
			checkForConflicts = true,
			checkForConflictsWithAllPlayers = false,
			checkForConflictsWithPlayerIds = Array.Empty<int>(),
			checkForConflictsWithSelf = true,
			checkForConflictsWithSystemPlayer = false,
			defaultActionWhenConflictFound = (ConflictResponse)2,
			holdDurationToMapKeyboardModifierKeyAsPrimary = 0f,
			ignoreMouseXAxis = true,
			ignoreMouseYAxis = true,
			timeout = float.PositiveInfinity
		};
		inputMappers.Add(val);
		return val;
	}

	private void RemoveInputMapper(InputMapper inputMapper)
	{
		inputMapper.ConflictFoundEvent -= InputMapperOnConflictFoundEvent;
		inputMapper.CanceledEvent -= InputMapperOnCanceledEvent;
		inputMapper.ErrorEvent -= InputMapperOnErrorEvent;
		inputMapper.InputMappedEvent -= InputMapperOnInputMappedEvent;
		inputMapper.StartedEvent -= InputMapperOnStartedEvent;
		inputMapper.StoppedEvent -= InputMapperOnStoppedEvent;
		inputMapper.TimedOutEvent -= InputMapperOnTimedOutEvent;
		inputMappers.Remove(inputMapper);
	}

	public void Start(Player player, IList<Controller> controllers, InputAction action, AxisRange axisRange)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Expected O, but got Unknown
		Stop();
		isListening = true;
		currentPlayer = player;
		currentAction = action;
		currentAxisRange = axisRange;
		maps = (from controller in controllers
			select player.controllers.maps.GetFirstMapInCategory(controller, 0) into map
			where map != null
			select map).Distinct().ToArray();
		Debug.Log((object)maps.Length);
		ControllerMap[] array = maps;
		foreach (ControllerMap controllerMap in array)
		{
			Context val = new Context
			{
				actionId = action.id,
				controllerMap = controllerMap,
				actionRange = currentAxisRange
			};
			AddInputMapper().Start(val);
		}
		dialogBox = SimpleDialogBox.Create(eventSystem);
		timer = timeout;
		UpdateDialogBoxString();
		RoR2Application.onUpdate += Update;
	}

	public void Stop()
	{
		if (isListening)
		{
			maps = Array.Empty<ControllerMap>();
			currentPlayer = null;
			currentAction = null;
			for (int num = inputMappers.Count - 1; num >= 0; num--)
			{
				InputMapper val = inputMappers[num];
				val.Stop();
				RemoveInputMapper(val);
			}
			if (Object.op_Implicit((Object)(object)dialogBox))
			{
				Object.Destroy((Object)(object)dialogBox.rootObject);
				dialogBox = null;
			}
			isListening = false;
			RoR2Application.onUpdate -= Update;
		}
	}

	private void Update()
	{
		float unscaledDeltaTime = Time.unscaledDeltaTime;
		if (isListening)
		{
			timer -= unscaledDeltaTime;
			if (timer < 0f)
			{
				Stop();
			}
			else if (currentPlayer.GetButtonDown(25))
			{
				Stop();
				SimpleDialogBox simpleDialogBox = SimpleDialogBox.Create(eventSystem);
				simpleDialogBox.headerToken = new SimpleDialogBox.TokenParamsPair("OPTION_REBIND_DIALOG_TITLE");
				simpleDialogBox.descriptionToken = new SimpleDialogBox.TokenParamsPair("OPTION_REBIND_CANCELLED_DIALOG_DESCRIPTION");
				simpleDialogBox.AddCancelButton(CommonLanguageTokens.ok);
			}
			else
			{
				UpdateDialogBoxString();
			}
		}
	}

	private void UpdateDialogBoxString()
	{
		if (Object.op_Implicit((Object)(object)dialogBox) && timer >= 0f)
		{
			string @string = Language.GetString(InputCatalog.GetActionNameToken(currentAction.name, (AxisRange)0));
			dialogBox.headerToken = new SimpleDialogBox.TokenParamsPair
			{
				token = CommonLanguageTokens.optionRebindDialogTitle,
				formatParams = Array.Empty<object>()
			};
			dialogBox.descriptionToken = new SimpleDialogBox.TokenParamsPair
			{
				token = CommonLanguageTokens.optionRebindDialogDescription,
				formatParams = new object[2] { @string, timer }
			};
		}
	}

	private void InputMapperOnTimedOutEvent(TimedOutEventData timedOutEventData)
	{
		Debug.Log((object)"InputMapperOnTimedOutEvent");
	}

	private void InputMapperOnStoppedEvent(StoppedEventData stoppedEventData)
	{
		Debug.Log((object)"InputMapperOnStoppedEvent");
	}

	private void InputMapperOnStartedEvent(StartedEventData startedEventData)
	{
		Debug.Log((object)"InputMapperOnStartedEvent");
	}

	private void InputMapperOnInputMappedEvent(InputMappedEventData inputMappedEventData)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		Debug.Log((object)"InputMapperOnInputMappedEvent");
		ActionElementMap incomingActionElementMap = inputMappedEventData.actionElementMap;
		int incomingActionId = inputMappedEventData.actionElementMap.actionId;
		int incomingElementIndex = inputMappedEventData.actionElementMap.elementIndex;
		ControllerElementType incomingElementType = inputMappedEventData.actionElementMap.elementType;
		ControllerMap map = inputMappedEventData.actionElementMap.controllerMap;
		ControllerMap[] array = maps;
		foreach (ControllerMap val in array)
		{
			if (val != map)
			{
				val.DeleteElementMapsWithAction(incomingActionId);
			}
		}
		while (DeleteFirstConflictingElementMap())
		{
		}
		eventSystem?.localUser?.userProfile.RequestEventualSave();
		Debug.Log((object)"Mapping accepted.");
		for (int j = 0; j < InputBindingDisplayController.instances.Count; j++)
		{
			InputBindingDisplayController.instances[j].Refresh(forceRefresh: true);
		}
		Stop();
		bool ActionElementMapConflicts(ActionElementMap actionElementMap)
		{
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			if (actionElementMap == incomingActionElementMap)
			{
				return false;
			}
			bool num = actionElementMap.elementIndex == incomingElementIndex && actionElementMap.elementType == incomingElementType;
			bool flag = actionElementMap.actionId == incomingActionId && actionElementMap.axisContribution == incomingActionElementMap.axisContribution;
			return num || flag;
		}
		bool DeleteFirstConflictingElementMap()
		{
			foreach (ActionElementMap allMap in map.AllMaps)
			{
				if (ActionElementMapConflicts(allMap))
				{
					Debug.LogFormat("Deleting conflicting mapping {0}", new object[1] { allMap });
					map.DeleteElementMap(allMap.id);
					return true;
				}
			}
			return false;
		}
	}

	private void InputMapperOnErrorEvent(ErrorEventData errorEventData)
	{
		Debug.Log((object)"InputMapperOnErrorEvent");
	}

	private void InputMapperOnCanceledEvent(CanceledEventData canceledEventData)
	{
		Debug.Log((object)"InputMapperOnCanceledEvent");
	}

	private void InputMapperOnConflictFoundEvent(ConflictFoundEventData conflictFoundEventData)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		Debug.Log((object)"InputMapperOnConflictFoundEvent");
		ConflictResponse obj = ((!conflictFoundEventData.conflicts.Any((ElementAssignmentConflictInfo elementAssignmentConflictInfo) => forbiddenElements.Contains(((ElementAssignmentConflictInfo)(ref elementAssignmentConflictInfo)).elementIdentifier.name))) ? ((ConflictResponse)2) : ((ConflictResponse)3));
		conflictFoundEventData.responseCallback(obj);
	}

	public void Dispose()
	{
		Stop();
	}
}
