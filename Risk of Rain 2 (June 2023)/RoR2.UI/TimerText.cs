using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace RoR2.UI;

[RequireComponent(typeof(RectTransform))]
public class TimerText : MonoBehaviour
{
	[FormerlySerializedAs("targetText")]
	public TextMeshProUGUI targetLabel;

	[SerializeField]
	private TimerStringFormatter _format;

	[SerializeField]
	private double _seconds;

	private static readonly StringBuilder sharedStringBuilder = new StringBuilder();

	public TimerStringFormatter format
	{
		get
		{
			return _format;
		}
		set
		{
			if (!((Object)(object)_format == (Object)(object)value))
			{
				_format = value;
				Rebuild();
			}
		}
	}

	public double seconds
	{
		get
		{
			return _seconds;
		}
		set
		{
			if (_seconds != value)
			{
				_seconds = value;
				Rebuild();
			}
		}
	}

	private void Awake()
	{
		Rebuild();
	}

	private void Rebuild()
	{
		if (Object.op_Implicit((Object)(object)targetLabel))
		{
			sharedStringBuilder.Clear();
			if (Object.op_Implicit((Object)(object)format))
			{
				format.AppendToStringBuilder(seconds, sharedStringBuilder);
			}
			((TMP_Text)targetLabel).SetText(sharedStringBuilder);
		}
	}

	private void OnValidate()
	{
		Rebuild();
		if (!Object.op_Implicit((Object)(object)targetLabel))
		{
			Debug.LogErrorFormat((Object)(object)this, "TimerText does not specify a target label.", Array.Empty<object>());
		}
	}
}
