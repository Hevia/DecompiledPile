using System.Text;
using EntityStates.TimedChest;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace RoR2;

public sealed class TimedChestController : NetworkBehaviour, IInteractable
{
	public float lockTime = 600f;

	public TextMeshPro displayTimer;

	public ObjectScaleCurve displayScaleCurve;

	public string contextString;

	public Color displayIsAvailableColor;

	public Color displayIsLockedColor;

	public bool purchased;

	private const int minTime = -599;

	private static readonly StringBuilder sharedStringBuilder = new StringBuilder();

	private int remainingTime
	{
		get
		{
			float num = 0f;
			if (Object.op_Implicit((Object)(object)Run.instance))
			{
				num = Run.instance.GetRunStopwatch();
			}
			return (int)(lockTime - num);
		}
	}

	private bool locked => remainingTime <= 0;

	public string GetContextString(Interactor activator)
	{
		return Language.GetString(contextString);
	}

	public Interactability GetInteractability(Interactor activator)
	{
		if (!purchased)
		{
			if (!locked)
			{
				return Interactability.Available;
			}
			return Interactability.ConditionsNotMet;
		}
		return Interactability.Disabled;
	}

	public void OnInteractionBegin(Interactor activator)
	{
		((Component)this).GetComponent<EntityStateMachine>().SetNextState(new Opening());
	}

	public bool ShouldIgnoreSpherecastForInteractibility(Interactor activator)
	{
		return false;
	}

	public void FixedUpdate()
	{
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkClient.active)
		{
			return;
		}
		if (!purchased)
		{
			int num = remainingTime;
			bool flag = num >= 0;
			bool flag2 = true;
			if (num < -599)
			{
				flag2 = (num & 1) != 0;
				num = -599;
			}
			int num2 = (flag ? num : (-num));
			uint num3 = (uint)num2 / 60u;
			uint value = (uint)num2 - num3 * 60;
			sharedStringBuilder.Clear();
			sharedStringBuilder.Append("<mspace=0.75em>");
			if (flag2)
			{
				uint num4 = 2u;
				if (!flag)
				{
					sharedStringBuilder.Append("-");
					num4 = 1u;
				}
				sharedStringBuilder.AppendUint(num3, num4, num4);
				sharedStringBuilder.Append(":");
				sharedStringBuilder.AppendUint(value, 2u, 2u);
			}
			else
			{
				sharedStringBuilder.Append("--:--");
			}
			sharedStringBuilder.Append("</mspace>");
			((TMP_Text)displayTimer).SetText(sharedStringBuilder);
			((Graphic)displayTimer).color = (locked ? displayIsLockedColor : displayIsAvailableColor);
			((TMP_Text)displayTimer).SetText(sharedStringBuilder);
			((Behaviour)displayScaleCurve).enabled = false;
		}
		else
		{
			((Behaviour)displayScaleCurve).enabled = true;
		}
	}

	private void OnEnable()
	{
		InstanceTracker.Add<TimedChestController>(this);
	}

	private void OnDisable()
	{
		InstanceTracker.Remove<TimedChestController>(this);
	}

	public bool ShouldShowOnScanner()
	{
		return !purchased;
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool result = default(bool);
		return result;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
	}

	public override void PreStartClient()
	{
	}
}
