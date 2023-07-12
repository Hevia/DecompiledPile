using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace RoR2.UI;

[ExecuteAlways]
public class TypewriteTextController : MonoBehaviour
{
	[Serializable]
	private struct TimedTextChunk
	{
		public TextMeshProUGUI label;

		public int startCharIndex;

		public int endCharIndex;

		public float startTime;

		public float duration;

		public float interval;
	}

	[Min(0f)]
	[Tooltip("How long to delay before beginning to reveal characters. 0 is no delay.")]
	public float initialDelay;

	[Min(0f)]
	[Tooltip("How long to delay between individual characters. 0 is no delay.")]
	public float delayBetweenKeys = 0.1f;

	[Min(0f)]
	[Tooltip("How long to delay between sentences. 0 is no delay.")]
	public float delayBetweenSentences;

	[Min(0f)]
	[Tooltip("How long to delay between line breaks.")]
	public float delayBetweenNewLines;

	[Tooltip("How long to delay between labels. 0 is no delay.")]
	[Min(0f)]
	public float delayBetweenTexts = 1f;

	[Tooltip("The labels to control.")]
	[FormerlySerializedAs("textMeshProUGui")]
	public TextMeshProUGUI[] labels;

	[Tooltip("The sound to play for each character typed.")]
	public string soundString;

	[Tooltip("Whether or not to fade out the label once typing is finished.")]
	public bool fadeOutAfterCompletion;

	[Min(0f)]
	[Tooltip("How long to wait after typing is finished to begin fading out.")]
	public float fadeOutDelay;

	[Tooltip("How long it takes to fade out once fading begins.")]
	[Min(0f)]
	public float fadeOutDuration;

	[Tooltip("The current simulation time of this object. You'll probably want to make sure this is set to zero when saving if you preview the animation in-editor.")]
	[Min(0f)]
	public float stopwatch;

	[Tooltip("Whether or not to disable the attached GameObject when fading is complete.")]
	public bool disableObjectOnFadeEnd = true;

	[Tooltip("Whether or not to start playing the animation immediately.")]
	public bool playOnStart = true;

	[Tooltip("How long (in seconds) it should take to complete this animation if nonzero. Mainly useful for achieving consistent timings with different languages.")]
	public float timeToFit;

	[Tooltip("Whether or not timeToFit includes fade values.")]
	public bool includeFadeoutInTimeToFit;

	[Tooltip("Event called when typing has finished.")]
	public UnityEvent onFinishTyping;

	[Tooltip("Event called when fadeout has finished.")]
	public UnityEvent onFinishFade;

	private TimedTextChunk[] textChunks = Array.Empty<TimedTextChunk>();

	private float totalTypingDuration;

	private float totalFadingDuration;

	private float typingTimeScale;

	private float fadingTimeScale;

	private int totalCharacterCount;

	private int totalRevealedCharacterCount;

	private bool isPlayingAnimation;

	private bool isDoneTyping;

	private bool isDoneFading;

	private bool isEnginePlaying;

	private static readonly char[] sentenceTerminators = new char[4] { '.', '?', '!', '。' };

	private static readonly List<TimedTextChunk> sharedChunkBuilder = new List<TimedTextChunk>();

	private void Start()
	{
		isEnginePlaying = Application.IsPlaying((Object)(object)this);
		if (isEnginePlaying && playOnStart)
		{
			StartTyping();
		}
	}

	private void Update()
	{
		float num = Time.deltaTime;
		if (!isEnginePlaying)
		{
			num = Time.unscaledDeltaTime;
		}
		if (isPlayingAnimation)
		{
			stopwatch += num;
		}
		SetTime(stopwatch);
	}

	private static bool IsEndOfSentence(TMP_TextInfo textInfo, in TMP_CharacterInfo characterInfo)
	{
		char character = characterInfo.character;
		if (Array.IndexOf(sentenceTerminators, character) != -1)
		{
			if (character == '.' && characterInfo.index + 1 < textInfo.characterCount)
			{
				char character2 = textInfo.characterInfo[characterInfo.index + 1].character;
				if (!char.IsLetter(character2))
				{
					char.IsWhiteSpace(character2);
				}
				return true;
			}
			return true;
		}
		return false;
	}

	private void GenerateTimingInfo()
	{
		UpdateAllLabelTextInfo();
		TimedTextChunk currentTimedTextChunk = default(TimedTextChunk);
		bool flag = delayBetweenSentences > 0f;
		bool flag2 = delayBetweenTexts > 0f;
		bool flag3 = delayBetweenNewLines > 0f;
		totalCharacterCount = 0;
		for (int i = 0; i < labels.Length; i++)
		{
			TextMeshProUGUI val = labels[i];
			if (!Object.op_Implicit((Object)(object)val))
			{
				continue;
			}
			TMP_TextInfo textInfo = ((TMP_Text)val).textInfo;
			StartNewChunk(val, delayBetweenKeys);
			totalCharacterCount += textInfo.characterCount;
			if (flag2 && i > 0)
			{
				currentTimedTextChunk.duration = delayBetweenTexts;
				StartNewChunk(val, delayBetweenKeys);
			}
			else if (i == 0)
			{
				currentTimedTextChunk.duration = initialDelay;
				StartNewChunk(val, delayBetweenKeys);
			}
			int num = Math.Min(textInfo.characterCount, textInfo.characterInfo.Length);
			while (currentTimedTextChunk.endCharIndex < num)
			{
				ref TMP_CharacterInfo reference = ref textInfo.characterInfo[currentTimedTextChunk.endCharIndex];
				if (flag && IsEndOfSentence(textInfo, in reference))
				{
					StartNewChunk(val, delayBetweenSentences);
					currentTimedTextChunk.endCharIndex++;
					StartNewChunk(val, delayBetweenKeys);
				}
				else if (flag3 && reference.character == '\n')
				{
					StartNewChunk(val, delayBetweenNewLines);
					currentTimedTextChunk.endCharIndex++;
					StartNewChunk(val, delayBetweenKeys);
				}
				currentTimedTextChunk.endCharIndex++;
			}
		}
		StartNewChunk(null, 0f);
		textChunks = sharedChunkBuilder.ToArray();
		sharedChunkBuilder.Clear();
		totalTypingDuration = 0f;
		if (textChunks.Length != 0)
		{
			ref TimedTextChunk reference2 = ref textChunks[textChunks.Length - 1];
			totalTypingDuration = reference2.startTime + reference2.duration;
		}
		totalFadingDuration = fadeOutDelay + fadeOutDuration;
		typingTimeScale = 1f;
		fadingTimeScale = 1f;
		if (timeToFit > 0f)
		{
			if (includeFadeoutInTimeToFit)
			{
				fadingTimeScale = (typingTimeScale = (totalTypingDuration + fadeOutDelay + fadeOutDuration) / timeToFit);
			}
			else
			{
				typingTimeScale = totalTypingDuration / timeToFit;
			}
		}
		void StartNewChunk(TextMeshProUGUI label, float interval)
		{
			if (currentTimedTextChunk.endCharIndex != currentTimedTextChunk.startCharIndex)
			{
				currentTimedTextChunk.duration = (float)(currentTimedTextChunk.endCharIndex - currentTimedTextChunk.startCharIndex) * currentTimedTextChunk.interval;
			}
			if (currentTimedTextChunk.duration > 0f)
			{
				sharedChunkBuilder.Add(currentTimedTextChunk);
			}
			float startTime = currentTimedTextChunk.startTime + currentTimedTextChunk.duration;
			int num2 = currentTimedTextChunk.endCharIndex;
			if (label != currentTimedTextChunk.label)
			{
				num2 = 0;
			}
			currentTimedTextChunk = new TimedTextChunk
			{
				label = label,
				startTime = startTime,
				duration = 0f,
				startCharIndex = num2,
				endCharIndex = num2,
				interval = interval
			};
		}
	}

	[ContextMenu("Start preview")]
	public void StartTyping()
	{
		GenerateTimingInfo();
		isPlayingAnimation = true;
		stopwatch = 0f;
		SetTime(stopwatch);
	}

	private void UpdateAllLabelTextInfo()
	{
		for (int i = 0; i < labels.Length; i++)
		{
			TextMeshProUGUI val = labels[i];
			if (Object.op_Implicit((Object)(object)val))
			{
				((TMP_Text)val).ForceMeshUpdate(true, false);
			}
		}
	}

	public void SetTime(float t)
	{
		float num = totalTypingDuration / typingTimeScale;
		float typingTime = Mathf.Clamp(t * typingTimeScale, 0f, totalTypingDuration);
		float fadingTime = Mathf.Clamp((t - num) * fadingTimeScale, 0f, totalFadingDuration);
		SetTypingTime(typingTime);
		SetFadingTime(fadingTime);
		if (isDoneTyping && isDoneFading)
		{
			isPlayingAnimation = false;
		}
	}

	private void SetTypingTime(float typingTime)
	{
		bool flag = isDoneTyping;
		isDoneTyping = typingTime >= totalTypingDuration;
		int newTotalRevealedCharacterCount = 0;
		TextMeshProUGUI currentLabel = null;
		int currentLabelRevealedCharacterCount = 0;
		bool isPlaying = isEnginePlaying;
		int i;
		for (i = 0; i < textChunks.Length; i++)
		{
			ref TimedTextChunk reference = ref textChunks[i];
			if (!(reference.startTime <= typingTime))
			{
				break;
			}
			float num = reference.startTime + reference.duration;
			if (typingTime < num)
			{
				float num2 = Mathf.InverseLerp(reference.startTime, num, typingTime);
				int newRevealedCharacterCount2 = Mathf.CeilToInt(Mathf.Lerp((float)reference.startCharIndex, (float)reference.endCharIndex, num2));
				UpdateCurrentLabel(reference.label, newRevealedCharacterCount2);
				break;
			}
			UpdateCurrentLabel(reference.label, reference.endCharIndex);
		}
		for (i++; i < textChunks.Length; i++)
		{
			ref TimedTextChunk reference2 = ref textChunks[i];
			if (currentLabel != reference2.label)
			{
				UpdateCurrentLabel(reference2.label, 0);
			}
		}
		UpdateCurrentLabel(null, 0);
		if (newTotalRevealedCharacterCount > totalRevealedCharacterCount && soundString.Length > 0 && isEnginePlaying)
		{
			Util.PlaySound(soundString, ((Component)RoR2Application.instance).gameObject);
		}
		totalRevealedCharacterCount = newTotalRevealedCharacterCount;
		isDoneTyping = totalRevealedCharacterCount >= totalCharacterCount;
		if (!flag && isDoneTyping && isEnginePlaying)
		{
			UnityEvent obj = onFinishTyping;
			if (obj != null)
			{
				obj.Invoke();
			}
		}
		void UpdateCurrentLabel(TextMeshProUGUI newCurrentLabel, int newRevealedCharacterCount)
		{
			if (currentLabel != newCurrentLabel && currentLabel != null)
			{
				((TMP_Text)currentLabel).maxVisibleCharacters = currentLabelRevealedCharacterCount;
				if (currentLabelRevealedCharacterCount == 0)
				{
					if (((Behaviour)currentLabel).enabled && isPlaying)
					{
						((Behaviour)currentLabel).enabled = false;
					}
				}
				else
				{
					if (!((Behaviour)currentLabel).enabled)
					{
						((Behaviour)currentLabel).enabled = true;
					}
					newTotalRevealedCharacterCount += currentLabelRevealedCharacterCount;
				}
			}
			currentLabel = newCurrentLabel;
			currentLabelRevealedCharacterCount = newRevealedCharacterCount;
		}
	}

	private void SetFadingTime(float fadingTime)
	{
		bool flag = isDoneFading;
		isDoneFading = fadingTime >= totalFadingDuration;
		if (!fadeOutAfterCompletion)
		{
			return;
		}
		float num = 1f;
		float num2 = Mathf.Clamp01((fadingTime - fadeOutDelay) / fadeOutDuration);
		num = 1f - num2;
		SetLabelAlpha(num);
		if (isDoneFading && !flag && isEnginePlaying)
		{
			UnityEvent obj = onFinishFade;
			if (obj != null)
			{
				obj.Invoke();
			}
			if (disableObjectOnFadeEnd)
			{
				((Component)this).gameObject.SetActive(false);
			}
		}
	}

	private void SetLabelAlpha(float alpha)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < labels.Length; i++)
		{
			TextMeshProUGUI val = labels[i];
			if (Object.op_Implicit((Object)(object)val))
			{
				Color color = ((Graphic)val).color;
				if (!color.a.Equals(alpha))
				{
					color.a = alpha;
					((Graphic)val).color = color;
				}
			}
		}
	}

	private bool IsBeingEdited()
	{
		return false;
	}

	private void OnValidate()
	{
		if (IsBeingEdited() && !isPlayingAnimation)
		{
			GenerateTimingInfo();
		}
	}
}
