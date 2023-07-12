using UnityEngine;
using UnityEngine.Networking;

namespace RoR2.CharacterSpeech;

[RequireComponent(typeof(CharacterSpeechController))]
public class BaseCharacterSpeechDriver : MonoBehaviour
{
	protected CharacterSpeechController characterSpeechController { get; private set; }

	protected CharacterBody currentCharacterBody { get; private set; }

	protected void Awake()
	{
		if (!NetworkServer.active)
		{
			((Behaviour)this).enabled = false;
		}
		else
		{
			characterSpeechController = ((Component)this).GetComponent<CharacterSpeechController>();
		}
	}

	protected void OnEnable()
	{
		if (NetworkServer.active)
		{
			characterSpeechController.onCharacterBodyDiscovered += OnCharacterBodyDiscovered;
			characterSpeechController.onCharacterBodyLost += OnCharacterBodyLost;
			if (characterSpeechController.currentCharacterBody != null)
			{
				OnCharacterBodyDiscovered(characterSpeechController.currentCharacterBody);
			}
		}
	}

	protected void OnDisable()
	{
		if (NetworkServer.active)
		{
			if (currentCharacterBody != null)
			{
				OnCharacterBodyLost(currentCharacterBody);
			}
			characterSpeechController.onCharacterBodyLost -= OnCharacterBodyLost;
			characterSpeechController.onCharacterBodyDiscovered -= OnCharacterBodyDiscovered;
		}
	}

	protected void OnDestroy()
	{
		_ = NetworkServer.active;
	}

	protected virtual void OnCharacterBodyDiscovered(CharacterBody characterBody)
	{
		currentCharacterBody = characterBody;
	}

	protected virtual void OnCharacterBodyLost(CharacterBody characterBody)
	{
		currentCharacterBody = null;
	}
}
