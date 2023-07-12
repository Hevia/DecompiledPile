using Rewired;
using UnityEngine;
using UnityEngine.Events;

namespace RoR2;

public class InputResponse : MonoBehaviour
{
	public string[] inputActionNames;

	public UnityEvent onPress;

	private void Update()
	{
		Player player = MPEventSystemManager.combinedEventSystem.player;
		for (int i = 0; i < inputActionNames.Length; i++)
		{
			if (player.GetButtonDown(inputActionNames[i]))
			{
				UnityEvent obj = onPress;
				if (obj != null)
				{
					obj.Invoke();
				}
				break;
			}
		}
	}
}
