using System.Text;
using On.RoR2.UI;
using RoR2;
using RoR2.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Rorschach;

internal class RorschachRageBarBehaviour : MonoBehaviour
{
	private GameObject rageBar;

	private TextMeshProUGUI currentRage;

	private TextMeshProUGUI fullRage;

	private Image barImage;

	public float rageValue = 0f;

	private bool barSetupDone;

	private bool decrease;

	private static readonly StringBuilder sharedStringBuilder = new StringBuilder();

	private static Color baseRageColor = new Color(0.66667f, 0.08235f, 0.10196f);

	private static Color inRageColor = new Color(0.44447f, 0.06235f, 0.08196f);

	public SkillLocator skillLocator;

	public bool canExecute = true;

	private GameObject rageEffect;

	private bool playingSound;

	private uint ID;

	private float stopwatch;

	private bool inRageMode;

	private float rageStopwatch;

	private CharacterBody body;

	internal bool maxRage
	{
		get
		{
			if (Mathf.RoundToInt(rageValue * 100f) >= 100)
			{
				return true;
			}
			return false;
		}
	}

	internal void ResetRage()
	{
		if (Object.op_Implicit((Object)(object)rageEffect))
		{
			rageEffect.SetActive(false);
		}
		rageValue = 0f;
	}

	internal void DecreaseRage()
	{
		decrease = true;
	}

	internal void AddRage(float amount)
	{
		decrease = false;
		rageValue += amount;
	}

	private void OnEnable()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected O, but got Unknown
		HUD.Update += new hook_Update(HUD_Update);
		rageEffect = ((Component)((Component)((Component)this).GetComponent<ModelLocator>().modelTransform).GetComponent<ChildLocator>().FindChild("rageEffect")).gameObject;
		body = ((Component)this).GetComponent<CharacterBody>();
	}

	private void HUD_Update(orig_Update orig, HUD self)
	{
		//IL_049e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0489: Unknown result type (might be due to invalid IL or missing references)
		//IL_07d8: Unknown result type (might be due to invalid IL or missing references)
		orig.Invoke(self);
		if (Object.op_Implicit((Object)(object)body) && Object.op_Implicit((Object)(object)body.skillLocator))
		{
			if (maxRage)
			{
				rageStopwatch += Time.fixedDeltaTime;
				inRageMode = true;
				GenericSkill val = (Object.op_Implicit((Object)(object)body.skillLocator) ? body.skillLocator.primary : null);
				if (Object.op_Implicit((Object)(object)val) && !val.HasSkillOverrideOfPriority((SkillOverridePriority)4))
				{
					val.SetSkillOverride((object)this, MainPlugin.PrimaryRageDef, (SkillOverridePriority)4);
				}
				GenericSkill val2 = (Object.op_Implicit((Object)(object)body.skillLocator) ? body.skillLocator.secondary : null);
				if (Object.op_Implicit((Object)(object)val2) && !val2.HasSkillOverrideOfPriority((SkillOverridePriority)4))
				{
					val2.SetSkillOverride((object)this, MainPlugin.SecondaryRageDef, (SkillOverridePriority)4);
				}
				GenericSkill val3 = (Object.op_Implicit((Object)(object)body.skillLocator) ? body.skillLocator.utility : null);
				if (Object.op_Implicit((Object)(object)val3) && !val3.HasSkillOverrideOfPriority((SkillOverridePriority)4))
				{
					val3.SetSkillOverride((object)this, MainPlugin.UtilityRageDef, (SkillOverridePriority)4);
				}
				GenericSkill val4 = (Object.op_Implicit((Object)(object)body.skillLocator) ? body.skillLocator.special : null);
				if (Object.op_Implicit((Object)(object)val4) && !val4.HasSkillOverrideOfPriority((SkillOverridePriority)4))
				{
					val4.SetSkillOverride((object)this, MainPlugin.SpecialRageDef, (SkillOverridePriority)4);
				}
			}
			if (rageStopwatch >= 15f)
			{
				inRageMode = false;
				ResetRage();
				rageStopwatch = 0f;
				GenericSkill val5 = (Object.op_Implicit((Object)(object)body.skillLocator) ? body.skillLocator.primary : null);
				if (Object.op_Implicit((Object)(object)val5) && val5.HasSkillOverrideOfPriority((SkillOverridePriority)4))
				{
					val5.UnsetSkillOverride((object)this, MainPlugin.PrimaryRageDef, (SkillOverridePriority)4);
				}
				GenericSkill val6 = (Object.op_Implicit((Object)(object)body.skillLocator) ? body.skillLocator.secondary : null);
				if (Object.op_Implicit((Object)(object)val6) && val6.HasSkillOverrideOfPriority((SkillOverridePriority)4))
				{
					val6.UnsetSkillOverride((object)this, MainPlugin.SecondaryRageDef, (SkillOverridePriority)4);
				}
				GenericSkill val7 = (Object.op_Implicit((Object)(object)body.skillLocator) ? body.skillLocator.utility : null);
				if (Object.op_Implicit((Object)(object)val7) && val7.HasSkillOverrideOfPriority((SkillOverridePriority)4))
				{
					val7.UnsetSkillOverride((object)this, MainPlugin.UtilityRageDef, (SkillOverridePriority)4);
				}
				GenericSkill val8 = (Object.op_Implicit((Object)(object)body.skillLocator) ? body.skillLocator.special : null);
				if (Object.op_Implicit((Object)(object)val8) && val8.HasSkillOverrideOfPriority((SkillOverridePriority)4))
				{
					val8.UnsetSkillOverride((object)this, MainPlugin.SpecialRageDef, (SkillOverridePriority)4);
				}
			}
			if (stopwatch >= 30f && !inRageMode)
			{
				DecreaseRage();
			}
		}
		if (body.outOfCombat)
		{
			stopwatch += Time.fixedDeltaTime;
		}
		else
		{
			stopwatch = 0f;
		}
		if (maxRage)
		{
			if (!playingSound)
			{
				playingSound = true;
				AkSoundEngine.PostEvent(Sounds.Play_Rorschach_Rage, ((Component)this).gameObject);
				ID = AkSoundEngine.PostEvent(Sounds.Play_Rorschach_Rage_Loop, ((Component)this).gameObject);
			}
			if (Object.op_Implicit((Object)(object)rageEffect))
			{
				rageEffect.SetActive(true);
			}
		}
		else if (playingSound)
		{
			playingSound = false;
			AkSoundEngine.StopPlayingID(ID);
		}
		rageValue = Mathf.Clamp(rageValue, 0f, 1f);
		if (decrease)
		{
			rageValue -= Time.deltaTime / 10f;
		}
		if (Object.op_Implicit((Object)(object)barImage))
		{
			barImage.fillAmount = rageValue;
			if (!maxRage)
			{
				((Graphic)barImage).color = baseRageColor;
			}
			else
			{
				((Graphic)barImage).color = inRageColor;
			}
		}
		if (Object.op_Implicit((Object)(object)currentRage) && Object.op_Implicit((Object)(object)fullRage))
		{
			if (!maxRage)
			{
				sharedStringBuilder.Clear().Append(Mathf.RoundToInt(rageValue * 100f));
				((TMP_Text)currentRage).SetText(sharedStringBuilder);
				((TMP_Text)fullRage).SetText("100", true);
			}
			else
			{
				((TMP_Text)currentRage).SetText("", true);
				((TMP_Text)fullRage).SetText("", true);
			}
		}
		if (!Object.op_Implicit((Object)(object)self.targetBodyObject) || !((Object)(object)self.targetBodyObject == (Object)(object)((Component)this).gameObject) || !Object.op_Implicit((Object)(object)self.mainUIPanel) || Object.op_Implicit((Object)(object)rageBar))
		{
			return;
		}
		HealthBar componentInChildren = self.mainUIPanel.GetComponentInChildren<HealthBar>();
		if (!Object.op_Implicit((Object)(object)componentInChildren) || !Object.op_Implicit((Object)(object)((Component)componentInChildren).gameObject))
		{
			return;
		}
		Image[] componentsInChildren = ((Component)componentInChildren).gameObject.GetComponentsInChildren<Image>();
		if (!barSetupDone)
		{
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				if (componentsInChildren.Length == 5)
				{
					barSetupDone = true;
				}
			}
		}
		if (!barSetupDone)
		{
			return;
		}
		rageBar = Object.Instantiate<GameObject>(((Component)componentInChildren).gameObject, ((Component)componentInChildren).gameObject.transform.parent);
		((Object)rageBar).name = "RageBar";
		Object.Destroy((Object)(object)rageBar.GetComponent<HealthBar>());
		TextMeshProUGUI[] componentsInChildren2 = rageBar.GetComponentsInChildren<TextMeshProUGUI>();
		for (int j = 0; j < componentsInChildren2.Length; j++)
		{
			if (Object.op_Implicit((Object)(object)componentsInChildren2[j]) && Object.op_Implicit((Object)(object)((Component)componentsInChildren2[j]).gameObject))
			{
				if (((Object)((Component)componentsInChildren2[j]).gameObject).name == "CurrentHealthText")
				{
					currentRage = componentsInChildren2[j];
					((TMP_Text)currentRage).text = "0";
				}
				if (((Object)((Component)componentsInChildren2[j]).gameObject).name == "FullHealthText")
				{
					fullRage = componentsInChildren2[j];
					((TMP_Text)fullRage).text = "100";
				}
			}
		}
		Image[] componentsInChildren3 = rageBar.GetComponentsInChildren<Image>();
		for (int k = 0; k < componentsInChildren3.Length; k++)
		{
			if (Object.op_Implicit((Object)(object)componentsInChildren3[k]) && Object.op_Implicit((Object)(object)((Component)componentsInChildren3[k]).gameObject))
			{
				if ((Object)(object)componentsInChildren3[k] != (Object)(object)componentsInChildren3[3] && (Object)(object)componentsInChildren3[k] != (Object)(object)componentsInChildren3[0])
				{
					Object.Destroy((Object)(object)((Component)componentsInChildren3[k]).gameObject);
				}
				if ((Object)(object)componentsInChildren3[k] == (Object)(object)componentsInChildren3[3])
				{
					barImage = componentsInChildren3[k];
					((Graphic)barImage).color = baseRageColor;
					barImage.type = (Type)3;
					barImage.fillMethod = (FillMethod)0;
					barImage.fillCenter = false;
				}
			}
		}
	}

	private void OnDisable()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected O, but got Unknown
		HUD.Update -= new hook_Update(HUD_Update);
		if (Object.op_Implicit((Object)(object)rageBar))
		{
			Object.Destroy((Object)(object)rageBar);
		}
	}
}
