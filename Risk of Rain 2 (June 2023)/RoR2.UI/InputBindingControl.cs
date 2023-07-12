using System.Collections.Generic;
using System.Linq;
using Rewired;
using UnityEngine;
using UnityEngine.UI;

namespace RoR2.UI;

[RequireComponent(typeof(MPEventSystemLocator))]
public class InputBindingControl : MonoBehaviour
{
	public string actionName;

	public AxisRange axisRange;

	public LanguageTextMeshController nameLabel;

	public InputBindingDisplayController bindingDisplay;

	public MPEventSystem.InputSource inputSource;

	public MPButton button;

	private MPEventSystemLocator eventSystemLocator;

	private InputAction action;

	private InputMapperHelper inputMapperHelper;

	private Player currentPlayer;

	private float buttonReactivationTime;

	private bool isListening => inputMapperHelper.isListening;

	public void Awake()
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		eventSystemLocator = ((Component)this).GetComponent<MPEventSystemLocator>();
		bindingDisplay.actionName = actionName;
		bindingDisplay.useExplicitInputSource = true;
		bindingDisplay.explicitInputSource = inputSource;
		bindingDisplay.axisRange = axisRange;
		nameLabel.token = InputCatalog.GetActionNameToken(actionName, axisRange);
		action = ReInput.mapping.GetAction(actionName);
	}

	public void ToggleListening()
	{
		if (!isListening)
		{
			StartListening();
		}
		else
		{
			StopListening();
		}
	}

	public void StartListening()
	{
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		if (!((Selectable)button).IsInteractable())
		{
			return;
		}
		inputMapperHelper.Stop();
		currentPlayer = eventSystemLocator.eventSystem?.localUser?.inputPlayer;
		if (currentPlayer != null)
		{
			IList<Controller> controllers = null;
			switch (inputSource)
			{
			case MPEventSystem.InputSource.MouseAndKeyboard:
				controllers = (IList<Controller>)(object)new Controller[2]
				{
					(Controller)currentPlayer.controllers.Keyboard,
					(Controller)currentPlayer.controllers.Mouse
				};
				break;
			case MPEventSystem.InputSource.Gamepad:
				controllers = (IList<Controller>)(object)currentPlayer.controllers.Joysticks.ToArray();
				break;
			}
			inputMapperHelper.Start(currentPlayer, controllers, action, axisRange);
			if (Object.op_Implicit((Object)(object)button))
			{
				((Selectable)button).interactable = false;
			}
		}
	}

	private void StopListening()
	{
		if (currentPlayer != null)
		{
			currentPlayer = null;
			inputMapperHelper.Stop();
		}
	}

	private void OnEnable()
	{
		if (!Object.op_Implicit((Object)(object)eventSystemLocator.eventSystem))
		{
			((Behaviour)this).enabled = false;
		}
		else
		{
			inputMapperHelper = eventSystemLocator.eventSystem.inputMapperHelper;
		}
	}

	private void OnDisable()
	{
		StopListening();
	}

	private void Update()
	{
		if (!Object.op_Implicit((Object)(object)button))
		{
			return;
		}
		if (!Object.op_Implicit((Object)(object)eventSystemLocator?.eventSystem))
		{
			Debug.LogError((object)"MPEventSystem is invalid.");
			return;
		}
		bool flag = !eventSystemLocator.eventSystem.inputMapperHelper.isListening;
		if (!flag)
		{
			buttonReactivationTime = Time.unscaledTime + 0.25f;
		}
		((Selectable)button).interactable = flag && buttonReactivationTime <= Time.unscaledTime;
	}
}
