using System.Globalization;
using TMPro;
using UnityEngine;

namespace RoR2.UI;

[RequireComponent(typeof(MPEventSystemLocator))]
public class ContextManager : MonoBehaviour
{
	public TextMeshProUGUI glyphTMP;

	public TextMeshProUGUI descriptionTMP;

	public GameObject contextDisplay;

	public HUD hud;

	private MPEventSystemLocator eventSystemLocator;

	private void Awake()
	{
		eventSystemLocator = ((Component)this).GetComponent<MPEventSystemLocator>();
	}

	private void Start()
	{
		Update();
	}

	private void Update()
	{
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		string text = "";
		string text2 = "";
		bool active = false;
		if (Object.op_Implicit((Object)(object)hud) && Object.op_Implicit((Object)(object)hud.targetBodyObject))
		{
			InteractionDriver component = hud.targetBodyObject.GetComponent<InteractionDriver>();
			if (Object.op_Implicit((Object)(object)component))
			{
				GameObject val = component.FindBestInteractableObject();
				if (Object.op_Implicit((Object)(object)val))
				{
					PlayerCharacterMasterController component2 = ((Component)hud.targetMaster).GetComponent<PlayerCharacterMasterController>();
					if (Object.op_Implicit((Object)(object)component2) && Object.op_Implicit((Object)(object)component2.networkUser) && component2.networkUser.localUser != null)
					{
						IInteractable component3 = val.GetComponent<IInteractable>();
						if (component3 != null && ((Behaviour)(MonoBehaviour)component3).isActiveAndEnabled)
						{
							string text3 = ((component3.GetInteractability(component.interactor) == Interactability.Available) ? component3.GetContextString(component.interactor) : null);
							if (text3 != null)
							{
								text2 = text3;
								text = string.Format(CultureInfo.InvariantCulture, "<style=cKeyBinding>{0}</style>", Glyphs.GetGlyphString(eventSystemLocator, "Interact"));
								active = true;
							}
						}
					}
				}
			}
		}
		((TMP_Text)glyphTMP).text = text;
		((TMP_Text)descriptionTMP).text = text2;
		contextDisplay.SetActive(active);
	}
}
