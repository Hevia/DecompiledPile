using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RoR2.UI;

public class HUDBossHealthBarController : MonoBehaviour
{
	public HUD hud;

	public GameObject container;

	public Image fillRectImage;

	public Image delayRectImage;

	public TextMeshProUGUI healthLabel;

	public TextMeshProUGUI bossNameLabel;

	public TextMeshProUGUI bossSubtitleLabel;

	private BossGroup currentBossGroup;

	private float delayedTotalHealthFraction;

	private float healthFractionVelocity;

	private static readonly StringBuilder sharedStringBuilder = new StringBuilder();

	private Run.TimeStamp nextAllowedSourceUpdateTime = Run.TimeStamp.negativeInfinity;

	private bool listeningForClientDamageNotified;

	private void FixedUpdate()
	{
		List<BossGroup> instancesList = InstanceTracker.GetInstancesList<BossGroup>();
		int num = 0;
		for (int i = 0; i < instancesList.Count; i++)
		{
			if (instancesList[i].shouldDisplayHealthBarOnHud)
			{
				num++;
			}
		}
		SetListeningForClientDamageNotified(num > 1);
		if (Object.op_Implicit((Object)(object)currentBossGroup) && !currentBossGroup.shouldDisplayHealthBarOnHud)
		{
			currentBossGroup = null;
		}
		if (num > 0)
		{
			if (num != 1 && Object.op_Implicit((Object)(object)currentBossGroup))
			{
				return;
			}
			for (int j = 0; j < instancesList.Count; j++)
			{
				if (instancesList[j].shouldDisplayHealthBarOnHud)
				{
					currentBossGroup = instancesList[j];
					break;
				}
			}
		}
		else
		{
			currentBossGroup = null;
		}
	}

	private void OnDisable()
	{
		SetListeningForClientDamageNotified(newListeningForClientDamageNotified: false);
	}

	private void OnClientDamageNotified(DamageDealtMessage damageDealtMessage)
	{
		if (!nextAllowedSourceUpdateTime.hasPassed || !Object.op_Implicit((Object)(object)damageDealtMessage.victim))
		{
			return;
		}
		CharacterBody component = damageDealtMessage.victim.GetComponent<CharacterBody>();
		if (Object.op_Implicit((Object)(object)component) && component.isBoss && damageDealtMessage.attacker == hud.targetBodyObject)
		{
			BossGroup bossGroup = BossGroup.FindBossGroup(component);
			if (Object.op_Implicit((Object)(object)bossGroup) && bossGroup.shouldDisplayHealthBarOnHud)
			{
				currentBossGroup = bossGroup;
				nextAllowedSourceUpdateTime = Run.TimeStamp.now + 1f;
			}
		}
	}

	private void SetListeningForClientDamageNotified(bool newListeningForClientDamageNotified)
	{
		if (newListeningForClientDamageNotified != listeningForClientDamageNotified)
		{
			listeningForClientDamageNotified = newListeningForClientDamageNotified;
			if (listeningForClientDamageNotified)
			{
				GlobalEventManager.onClientDamageNotified += OnClientDamageNotified;
			}
			else
			{
				GlobalEventManager.onClientDamageNotified -= OnClientDamageNotified;
			}
		}
	}

	private void LateUpdate()
	{
		bool flag = Object.op_Implicit((Object)(object)currentBossGroup) && currentBossGroup.combatSquad.memberCount > 0;
		container.SetActive(flag);
		if (flag)
		{
			float totalObservedHealth = currentBossGroup.totalObservedHealth;
			float totalMaxObservedMaxHealth = currentBossGroup.totalMaxObservedMaxHealth;
			float num = ((totalMaxObservedMaxHealth == 0f) ? 0f : Mathf.Clamp01(totalObservedHealth / totalMaxObservedMaxHealth));
			delayedTotalHealthFraction = Mathf.Clamp(Mathf.SmoothDamp(delayedTotalHealthFraction, num, ref healthFractionVelocity, 0.1f, float.PositiveInfinity, Time.deltaTime), num, 1f);
			fillRectImage.fillAmount = num;
			delayRectImage.fillAmount = delayedTotalHealthFraction;
			sharedStringBuilder.Clear().AppendInt(Mathf.CeilToInt(totalObservedHealth)).Append("/")
				.AppendInt(Mathf.CeilToInt(totalMaxObservedMaxHealth));
			((TMP_Text)healthLabel).SetText(sharedStringBuilder);
			((TMP_Text)bossNameLabel).SetText(currentBossGroup.bestObservedName, true);
			((TMP_Text)bossSubtitleLabel).SetText(currentBossGroup.bestObservedSubtitle, true);
		}
	}
}
