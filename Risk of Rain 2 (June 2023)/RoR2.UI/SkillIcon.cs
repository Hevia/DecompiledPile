using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RoR2.UI;

public class SkillIcon : MonoBehaviour
{
	public SkillSlot targetSkillSlot;

	public PlayerCharacterMasterController playerCharacterMasterController;

	public GenericSkill targetSkill;

	public Image iconImage;

	public RawImage cooldownRemapPanel;

	public TextMeshProUGUI cooldownText;

	public TextMeshProUGUI stockText;

	public GameObject flashPanelObject;

	public GameObject isReadyPanelObject;

	public Animator animator;

	public string animatorStackString;

	public bool wasReady;

	public int previousStock;

	public TooltipProvider tooltipProvider;

	private static readonly StringBuilder sharedStringBuilder = new StringBuilder();

	private void Update()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_021b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0291: Unknown result type (might be due to invalid IL or missing references)
		//IL_032f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0334: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)targetSkill))
		{
			if (Object.op_Implicit((Object)(object)tooltipProvider))
			{
				Color bodyColor = targetSkill.characterBody.bodyColor;
				SurvivorCatalog.GetSurvivorIndexFromBodyIndex(targetSkill.characterBody.bodyIndex);
				float num = default(float);
				float num2 = default(float);
				float num3 = default(float);
				Color.RGBToHSV(bodyColor, ref num, ref num2, ref num3);
				num3 = ((num3 > 0.7f) ? 0.7f : num3);
				bodyColor = Color.HSVToRGB(num, num2, num3);
				tooltipProvider.titleColor = bodyColor;
				tooltipProvider.titleToken = targetSkill.skillNameToken;
				tooltipProvider.bodyToken = targetSkill.skillDescriptionToken;
			}
			float cooldownRemaining = targetSkill.cooldownRemaining;
			float num4 = targetSkill.CalculateFinalRechargeInterval();
			int stock = targetSkill.stock;
			bool flag = stock > 0 || cooldownRemaining == 0f;
			bool flag2 = targetSkill.IsReady();
			if (previousStock < stock)
			{
				Util.PlaySound("Play_UI_cooldownRefresh", ((Component)RoR2Application.instance).gameObject);
			}
			if (Object.op_Implicit((Object)(object)animator))
			{
				if (targetSkill.maxStock > 1)
				{
					animator.SetBool(animatorStackString, true);
				}
				else
				{
					animator.SetBool(animatorStackString, false);
				}
			}
			if (Object.op_Implicit((Object)(object)isReadyPanelObject))
			{
				isReadyPanelObject.SetActive(flag2);
			}
			if (!wasReady && flag && Object.op_Implicit((Object)(object)flashPanelObject))
			{
				flashPanelObject.SetActive(true);
			}
			if (Object.op_Implicit((Object)(object)cooldownText))
			{
				if (flag)
				{
					((Component)cooldownText).gameObject.SetActive(false);
				}
				else
				{
					sharedStringBuilder.Clear();
					sharedStringBuilder.AppendInt(Mathf.CeilToInt(cooldownRemaining));
					((TMP_Text)cooldownText).SetText(sharedStringBuilder);
					((Component)cooldownText).gameObject.SetActive(true);
				}
			}
			if (Object.op_Implicit((Object)(object)iconImage))
			{
				((Behaviour)iconImage).enabled = true;
				((Graphic)iconImage).color = (flag2 ? Color.white : Color.gray);
				iconImage.sprite = targetSkill.icon;
			}
			if (Object.op_Implicit((Object)(object)cooldownRemapPanel))
			{
				float num5 = 1f;
				if (num4 >= Mathf.Epsilon)
				{
					num5 = 1f - cooldownRemaining / num4;
				}
				float num6 = num5;
				((Behaviour)cooldownRemapPanel).enabled = num6 < 1f;
				((Graphic)cooldownRemapPanel).color = new Color(1f, 1f, 1f, num5);
			}
			if (Object.op_Implicit((Object)(object)stockText))
			{
				if (targetSkill.maxStock > 1)
				{
					((Component)stockText).gameObject.SetActive(true);
					sharedStringBuilder.Clear();
					sharedStringBuilder.AppendInt(targetSkill.stock);
					((TMP_Text)stockText).SetText(sharedStringBuilder);
				}
				else
				{
					((Component)stockText).gameObject.SetActive(false);
				}
			}
			wasReady = flag;
			previousStock = stock;
		}
		else
		{
			if (Object.op_Implicit((Object)(object)tooltipProvider))
			{
				tooltipProvider.bodyColor = Color.gray;
				tooltipProvider.titleToken = "";
				tooltipProvider.bodyToken = "";
			}
			if (Object.op_Implicit((Object)(object)cooldownText))
			{
				((Component)cooldownText).gameObject.SetActive(false);
			}
			if (Object.op_Implicit((Object)(object)stockText))
			{
				((Component)stockText).gameObject.SetActive(false);
			}
			if (Object.op_Implicit((Object)(object)iconImage))
			{
				((Behaviour)iconImage).enabled = false;
				iconImage.sprite = null;
			}
		}
	}
}
