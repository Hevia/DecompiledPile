using RoR2.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SessionButtonController : MonoBehaviour
{
	public HGButton Button;

	public HGTextMeshProUGUI Text;

	public void AddListener(UnityAction call)
	{
		((UnityEvent)((Button)Button).onClick).AddListener(call);
	}

	public void SetText(int currentParticipationNumber, int maxParticipationNumber, string hostName)
	{
		((TMP_Text)Text).text = currentParticipationNumber + "/" + maxParticipationNumber + " " + hostName;
	}
}
