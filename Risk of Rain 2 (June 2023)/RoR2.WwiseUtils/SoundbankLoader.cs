using UnityEngine;

namespace RoR2.WwiseUtils;

public class SoundbankLoader : MonoBehaviour
{
	public string[] soundbankStrings;

	public bool decodeBank;

	public bool saveDecodedBank;

	private void Start()
	{
		for (int i = 0; i < soundbankStrings.Length; i++)
		{
			AkBankManager.LoadBank(soundbankStrings[i], decodeBank, saveDecodedBank);
		}
	}
}
