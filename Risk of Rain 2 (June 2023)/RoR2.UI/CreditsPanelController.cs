using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EntityStates;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RoR2.UI;

[RequireComponent(typeof(RectTransform))]
[ExecuteAlways]
public class CreditsPanelController : MonoBehaviour
{
	public abstract class BaseCreditsPanelState : BaseState
	{
		protected CreditsPanelController creditsPanelController { get; private set; }

		protected abstract bool enableSkipButton { get; }

		public override void OnEnter()
		{
			base.OnEnter();
			creditsPanelController = GetComponent<CreditsPanelController>();
			if (Object.op_Implicit((Object)(object)creditsPanelController) && Object.op_Implicit((Object)(object)creditsPanelController.skipButton))
			{
				((Component)creditsPanelController.skipButton).gameObject.SetActive(enableSkipButton);
			}
		}

		protected void SetScroll(float scroll)
		{
			if (Object.op_Implicit((Object)(object)creditsPanelController))
			{
				creditsPanelController.SetScroll(scroll);
			}
		}

		protected void SetFade(float fade)
		{
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			fade = Mathf.Clamp01(fade);
			if (Object.op_Implicit((Object)(object)creditsPanelController) && Object.op_Implicit((Object)(object)creditsPanelController.fadePanel))
			{
				Color color = ((Graphic)creditsPanelController.fadePanel).color;
				color.a = fade;
				((Graphic)creditsPanelController.fadePanel).color = color;
			}
		}
	}

	public class IntroState : BaseCreditsPanelState
	{
		protected override bool enableSkipButton => false;

		public override void OnEnter()
		{
			base.OnEnter();
			SetScroll(0f);
			SetFade(base.creditsPanelController.introFadeCurve.Evaluate(0f));
		}

		public override void Update()
		{
			base.Update();
			float num = Mathf.Clamp01(base.age / base.creditsPanelController.introDuration);
			SetFade(1f - num);
			if (num >= 1f)
			{
				outer.SetNextState(new MainScrollState());
			}
		}
	}

	public class MainScrollState : BaseCreditsPanelState
	{
		public static AnimationCurve scrollCurve;

		protected override bool enableSkipButton => true;

		public override void OnEnter()
		{
			base.OnEnter();
			SetFade(0f);
			((Component)base.creditsPanelController.skipButton).gameObject.SetActive(true);
		}

		public override void Update()
		{
			base.Update();
			float num = Mathf.Clamp01(base.age / base.creditsPanelController.scrollDuration);
			SetScroll(scrollCurve.Evaluate(num));
			if (num >= 1f)
			{
				outer.SetNextState(new OutroState());
			}
		}
	}

	public class OutroState : BaseCreditsPanelState
	{
		protected override bool enableSkipButton => true;

		public override void OnEnter()
		{
			base.OnEnter();
			SetScroll(1f);
			SetFade(base.creditsPanelController.outroFadeCurve.Evaluate(0f));
		}

		public override void Update()
		{
			base.Update();
			float num = Mathf.Clamp01(base.age / base.creditsPanelController.outroDuration);
			SetFade(num);
			if (num >= 1f)
			{
				EntityState.Destroy((Object)(object)base.gameObject);
			}
		}
	}

	public RectTransform content;

	public ScrollRect scrollRect;

	public float introDuration;

	public AnimationCurve introFadeCurve;

	public float outroDuration;

	public AnimationCurve outroFadeCurve;

	public float scrollDuration;

	public RawImage fadePanel;

	public MPButton skipButton;

	public VoteInfoPanelController voteInfoPanel;

	[Range(0f, 1f)]
	public float editorScroll;

	private void OnEnable()
	{
		InstanceTracker.Add(this);
	}

	private void OnDisable()
	{
		InstanceTracker.Remove(this);
	}

	private void SetScroll(float scroll)
	{
		if (Object.op_Implicit((Object)(object)scrollRect))
		{
			scrollRect.verticalNormalizedPosition = 1f - scroll;
		}
	}

	private void Update()
	{
		if (!Application.IsPlaying((Object)(object)this))
		{
			SetScroll(editorScroll);
		}
	}

	[ContextMenu("Generate Credits Roles JSON")]
	private void GenerateCreditsRoles()
	{
		CreditsStripGroupBuilder[] componentsInChildren = ((Component)this).GetComponentsInChildren<CreditsStripGroupBuilder>();
		Dictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		CreditsStripGroupBuilder[] array = componentsInChildren;
		for (int i = 0; i < array.Length; i++)
		{
			foreach (var namesAndEnglishRole in array[i].GetNamesAndEnglishRoles())
			{
				string text = CreditsStripGroupBuilder.EnglishRoleToToken(namesAndEnglishRole.englishRoleName);
				if (dictionary.TryGetValue(text, out var value))
				{
					if (!string.Equals(namesAndEnglishRole.englishRoleName, value, StringComparison.Ordinal))
					{
						Debug.LogError((object)("Conflict in role \"" + text + "\": a=\"" + value + "\" b=\"" + namesAndEnglishRole.englishRoleName + "\""));
					}
				}
				else
				{
					dictionary.Add(text, namesAndEnglishRole.englishRoleName);
				}
			}
		}
		List<KeyValuePair<string, string>> list = dictionary.OrderBy((KeyValuePair<string, string> kv) => kv.Key).ToList();
		StringBuilder stringBuilder = new StringBuilder();
		foreach (KeyValuePair<string, string> item in list)
		{
			stringBuilder.Append("\"").Append(item.Key).Append("\": \"")
				.Append(item.Value)
				.Append("\",")
				.AppendLine();
		}
		Debug.Log((object)stringBuilder);
		GUIUtility.systemCopyBuffer = stringBuilder.ToString();
	}

	[ConCommand(commandName = "credits_start", flags = ConVarFlags.None, helpText = "Begins the credits sequence.")]
	private static void CCCreditsStart(ConCommandArgs args)
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Expected O, but got Unknown
		if (InstanceTracker.GetInstancesList<CreditsPanelController>().Count != 0)
		{
			throw new ConCommandException("Already in credits sequence.");
		}
		CreditsPanelController creditsPanelController = Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/UI/Credits/CreditsPanel"), ((Component)RoR2Application.instance.mainCanvas).transform).GetComponent<CreditsPanelController>();
		((UnityEvent)((Button)creditsPanelController.skipButton).onClick).AddListener((UnityAction)delegate
		{
			Object.Destroy((Object)(object)((Component)creditsPanelController).gameObject);
		});
	}
}
