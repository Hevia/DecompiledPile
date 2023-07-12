using System.Collections.Generic;
using System.Text;
using Rewired;
using RoR2.UI;
using TMPro;
using UnityEngine;

namespace RoR2;

[RequireComponent(typeof(MPEventSystemLocator))]
public class InputBindingDisplayController : MonoBehaviour
{
	public string actionName;

	public AxisRange axisRange;

	public bool useExplicitInputSource;

	public MPEventSystem.InputSource explicitInputSource;

	private MPEventSystemLocator eventSystemLocator;

	private TextMeshProUGUI guiLabel;

	private TextMeshPro label;

	private MPEventSystem lastEventSystem;

	private MPEventSystem.InputSource lastInputSource;

	public static readonly List<InputBindingDisplayController> instances = new List<InputBindingDisplayController>();

	private static readonly StringBuilder sharedStringBuilder = new StringBuilder();

	private void Awake()
	{
		eventSystemLocator = ((Component)this).GetComponent<MPEventSystemLocator>();
		guiLabel = ((Component)this).GetComponent<TextMeshProUGUI>();
		label = ((Component)this).GetComponent<TextMeshPro>();
	}

	private void OnEnable()
	{
		instances.Add(this);
	}

	private void OnDisable()
	{
		instances.Remove(this);
	}

	public void Refresh(bool forceRefresh = false)
	{
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		MPEventSystem mPEventSystem = eventSystemLocator?.eventSystem;
		if (!Object.op_Implicit((Object)(object)mPEventSystem))
		{
			Debug.LogError((object)"MPEventSystem is invalid.");
		}
		else if (forceRefresh || !((Object)(object)mPEventSystem == (Object)(object)lastEventSystem) || mPEventSystem.currentInputSource != lastInputSource)
		{
			if (useExplicitInputSource)
			{
				sharedStringBuilder.Clear();
				sharedStringBuilder.Append(Glyphs.GetGlyphString(eventSystemLocator.eventSystem, actionName, axisRange, explicitInputSource));
			}
			else
			{
				sharedStringBuilder.Clear();
				sharedStringBuilder.Append(Glyphs.GetGlyphString(eventSystemLocator.eventSystem, actionName, (AxisRange)0));
			}
			if (Object.op_Implicit((Object)(object)guiLabel))
			{
				((TMP_Text)guiLabel).SetText(sharedStringBuilder);
			}
			else if (Object.op_Implicit((Object)(object)label))
			{
				((TMP_Text)label).SetText(sharedStringBuilder);
			}
			lastEventSystem = mPEventSystem;
			lastInputSource = mPEventSystem.currentInputSource;
		}
	}

	private void Update()
	{
		Refresh();
	}
}
