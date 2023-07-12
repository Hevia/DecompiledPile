using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RoR2.UI;

public class AchievementNotificationPanel : MonoBehaviour
{
	private static readonly List<AchievementNotificationPanel> instancesList = new List<AchievementNotificationPanel>();

	public Image achievementIconImage;

	public TextMeshProUGUI achievementName;

	public TextMeshProUGUI achievementDescription;

	public UnityEvent onStart;

	private void Awake()
	{
		instancesList.Add(this);
		onStart.Invoke();
	}

	private void OnDestroy()
	{
		instancesList.Remove(this);
	}

	private void Update()
	{
	}

	public void SetAchievementDef(AchievementDef achievementDef)
	{
		achievementIconImage.sprite = achievementDef.GetAchievedIcon();
		((TMP_Text)achievementName).text = Language.GetString(achievementDef.nameToken);
		((TMP_Text)achievementDescription).text = Language.GetString(achievementDef.descriptionToken);
	}

	[CanBeNull]
	private static Canvas GetUserCanvas([NotNull] LocalUser localUser)
	{
		if (Object.op_Implicit((Object)(object)Run.instance))
		{
			CameraRigController cameraRigController = localUser.cameraRigController;
			if (!Object.op_Implicit((Object)(object)cameraRigController))
			{
				return null;
			}
			HUD hud = cameraRigController.hud;
			if (!Object.op_Implicit((Object)(object)hud))
			{
				return null;
			}
			return hud.mainContainer.GetComponent<Canvas>();
		}
		return RoR2Application.instance.mainCanvas;
	}

	private static bool IsAppropriateTimeToDisplayUserAchievementNotification(LocalUser localUser)
	{
		return !Object.op_Implicit((Object)(object)GameOverController.instance);
	}

	private static void DispatchAchievementNotification(Canvas canvas, AchievementDef achievementDef)
	{
		Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/UI/AchievementNotificationPanel"), ((Component)canvas).transform).GetComponent<AchievementNotificationPanel>().SetAchievementDef(achievementDef);
		Util.PlaySound(achievementDef.GetAchievementSoundString(), ((Component)RoR2Application.instance).gameObject);
	}

	[RuntimeInitializeOnLoadMethod]
	private static void Init()
	{
		RoR2Application.onFixedUpdate += StaticFixedUpdate;
	}

	private static void StaticFixedUpdate()
	{
		foreach (LocalUser readOnlyLocalUsers in LocalUserManager.readOnlyLocalUsersList)
		{
			if (!readOnlyLocalUsers.userProfile.hasUnviewedAchievement)
			{
				continue;
			}
			Canvas canvas = GetUserCanvas(readOnlyLocalUsers);
			if ((Object)(object)canvas == (Object)null || instancesList.Any((AchievementNotificationPanel instance) => (Object)(object)((Component)instance).transform.parent == (Object)(object)((Component)canvas).transform) || !IsAppropriateTimeToDisplayUserAchievementNotification(readOnlyLocalUsers))
			{
				continue;
			}
			string text = readOnlyLocalUsers.userProfile.PopNextUnviewedAchievementName();
			if (text != null)
			{
				AchievementDef achievementDef = AchievementManager.GetAchievementDef(text);
				if (achievementDef != null)
				{
					DispatchAchievementNotification(canvas, achievementDef);
				}
			}
		}
	}
}
