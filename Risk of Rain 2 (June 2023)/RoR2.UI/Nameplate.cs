using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RoR2.UI;

public class Nameplate : MonoBehaviour
{
	public TextMeshPro label;

	private CharacterBody body;

	public GameObject aliveObject;

	public GameObject deadObject;

	public SpriteRenderer criticallyHurtSpriteRenderer;

	public SpriteRenderer[] coloredSprites;

	public Color baseColor;

	public Color combatColor;

	public void SetBody(CharacterBody body)
	{
		this.body = body;
	}

	private void LateUpdate()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		string text = "";
		Color val = baseColor;
		bool flag = true;
		bool flag2 = false;
		bool flag3 = false;
		if (Object.op_Implicit((Object)(object)body))
		{
			text = body.GetDisplayName();
			flag = body.healthComponent.alive;
			flag2 = !body.outOfCombat || !body.outOfDanger;
			flag3 = body.healthComponent.isHealthLow;
			CharacterMaster master = body.master;
			if (Object.op_Implicit((Object)(object)master))
			{
				PlayerCharacterMasterController component = ((Component)master).GetComponent<PlayerCharacterMasterController>();
				if (Object.op_Implicit((Object)(object)component))
				{
					GameObject networkUserObject = component.networkUserObject;
					if (Object.op_Implicit((Object)(object)networkUserObject))
					{
						NetworkUser component2 = networkUserObject.GetComponent<NetworkUser>();
						if (Object.op_Implicit((Object)(object)component2))
						{
							text = component2.userName;
						}
					}
				}
				else
				{
					text = Language.GetString(body.baseNameToken);
				}
			}
		}
		val = (flag2 ? combatColor : baseColor);
		aliveObject.SetActive(flag);
		deadObject.SetActive(!flag);
		if (Object.op_Implicit((Object)(object)criticallyHurtSpriteRenderer))
		{
			((Renderer)criticallyHurtSpriteRenderer).enabled = flag3 && flag;
			criticallyHurtSpriteRenderer.color = HealthBar.GetCriticallyHurtColor();
		}
		if (Object.op_Implicit((Object)(object)label))
		{
			((TMP_Text)label).text = text;
			((Graphic)label).color = val;
		}
		SpriteRenderer[] array = coloredSprites;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].color = val;
		}
	}
}
