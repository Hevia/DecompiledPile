using System;
using System.Collections.Generic;
using EntityStates.Missions.Goldshores;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RoR2.UI;

public class ObjectivePanelController : MonoBehaviour
{
	public struct ObjectiveSourceDescriptor : IEquatable<ObjectiveSourceDescriptor>
	{
		public Object source;

		public CharacterMaster master;

		public Type objectiveType;

		public override int GetHashCode()
		{
			return (((((source != (Object)null) ? ((object)source).GetHashCode() : 0) * 397) ^ (((Object)(object)master != (Object)null) ? ((object)master).GetHashCode() : 0)) * 397) ^ ((objectiveType != null) ? objectiveType.GetHashCode() : 0);
		}

		public static bool Equals(ObjectiveSourceDescriptor a, ObjectiveSourceDescriptor b)
		{
			if (a.source == b.source && (Object)(object)a.master == (Object)(object)b.master)
			{
				return a.objectiveType == b.objectiveType;
			}
			return false;
		}

		public bool Equals(ObjectiveSourceDescriptor other)
		{
			if (source == other.source && (Object)(object)master == (Object)(object)other.master)
			{
				return objectiveType == other.objectiveType;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}
			if (obj is ObjectiveSourceDescriptor)
			{
				return Equals((ObjectiveSourceDescriptor)obj);
			}
			return false;
		}
	}

	public class ObjectiveTracker
	{
		public ObjectiveSourceDescriptor sourceDescriptor;

		public ObjectivePanelController owner;

		public bool isRelevant;

		protected Image checkbox;

		protected TextMeshProUGUI label;

		protected string cachedString;

		protected string baseToken = "";

		protected bool retired;

		public GameObject stripObject { get; private set; }

		protected virtual bool shouldConsiderComplete => retired;

		public void SetStrip(GameObject stripObject)
		{
			this.stripObject = stripObject;
			label = ((Component)stripObject.transform.Find("Label")).GetComponent<TextMeshProUGUI>();
			checkbox = ((Component)stripObject.transform.Find("Checkbox")).GetComponent<Image>();
			UpdateStrip();
		}

		public string GetString()
		{
			if (IsDirty())
			{
				cachedString = GenerateString();
			}
			return cachedString;
		}

		protected virtual string GenerateString()
		{
			return Language.GetString(baseToken);
		}

		protected virtual bool IsDirty()
		{
			return cachedString == null;
		}

		public void Retire()
		{
			retired = true;
			OnRetired();
			UpdateStrip();
		}

		protected virtual void OnRetired()
		{
		}

		public virtual void UpdateStrip()
		{
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
			if (Object.op_Implicit((Object)(object)label))
			{
				((TMP_Text)label).text = GetString();
				((Graphic)label).color = (retired ? Color.gray : Color.white);
				if (retired)
				{
					TextMeshProUGUI obj = label;
					((TMP_Text)obj).fontStyle = (FontStyles)(((TMP_Text)obj).fontStyle | 0x40);
				}
			}
			if (Object.op_Implicit((Object)(object)checkbox))
			{
				bool flag = shouldConsiderComplete;
				checkbox.sprite = (flag ? owner.checkboxSuccessSprite : owner.checkboxActiveSprite);
				((Graphic)checkbox).color = (flag ? Color.yellow : Color.white);
			}
		}

		public static ObjectiveTracker Instantiate(ObjectiveSourceDescriptor sourceDescriptor)
		{
			if (sourceDescriptor.objectiveType != null && sourceDescriptor.objectiveType.IsSubclassOf(typeof(ObjectiveTracker)))
			{
				ObjectiveTracker obj = (ObjectiveTracker)Activator.CreateInstance(sourceDescriptor.objectiveType);
				obj.sourceDescriptor = sourceDescriptor;
				return obj;
			}
			Debug.LogFormat("Bad objectiveType {0}", new object[1] { sourceDescriptor.objectiveType?.FullName });
			return null;
		}
	}

	private class FindTeleporterObjectiveTracker : ObjectiveTracker
	{
		public FindTeleporterObjectiveTracker()
		{
			baseToken = "OBJECTIVE_FIND_TELEPORTER";
		}
	}

	private class ActivateGoldshoreBeaconTracker : ObjectiveTracker
	{
		private int cachedActiveBeaconCount = -1;

		private int cachedRequiredBeaconCount = -1;

		private GoldshoresMissionController missionController => sourceDescriptor.source as GoldshoresMissionController;

		public ActivateGoldshoreBeaconTracker()
		{
			baseToken = "OBJECTIVE_GOLDSHORES_ACTIVATE_BEACONS";
		}

		private bool UpdateCachedValues()
		{
			int beaconsActive = missionController.beaconsActive;
			int beaconCount = missionController.beaconCount;
			if (beaconsActive != cachedActiveBeaconCount || beaconCount != cachedRequiredBeaconCount)
			{
				cachedActiveBeaconCount = beaconsActive;
				cachedRequiredBeaconCount = beaconCount;
				return true;
			}
			return false;
		}

		protected override string GenerateString()
		{
			UpdateCachedValues();
			return string.Format(Language.GetString(baseToken), cachedActiveBeaconCount, cachedRequiredBeaconCount);
		}

		protected override bool IsDirty()
		{
			if (!Object.op_Implicit((Object)(object)(sourceDescriptor.source as GoldshoresMissionController)))
			{
				return true;
			}
			return UpdateCachedValues();
		}
	}

	private class ClearArena : ObjectiveTracker
	{
		public ClearArena()
		{
			baseToken = "OBJECTIVE_CLEAR_ARENA";
		}

		protected override string GenerateString()
		{
			ArenaMissionController instance = ArenaMissionController.instance;
			return string.Format(Language.GetString(baseToken), instance.clearedRounds, instance.totalRoundsMax);
		}

		protected override bool IsDirty()
		{
			return true;
		}
	}

	private class DestroyTimeCrystals : ObjectiveTracker
	{
		public DestroyTimeCrystals()
		{
			baseToken = "OBJECTIVE_WEEKLYRUN_DESTROY_CRYSTALS";
		}

		protected override string GenerateString()
		{
			WeeklyRun weeklyRun = Run.instance as WeeklyRun;
			return string.Format(Language.GetString(baseToken), weeklyRun.crystalsKilled, weeklyRun.crystalsRequiredToKill);
		}

		protected override bool IsDirty()
		{
			return true;
		}
	}

	private class FinishTeleporterObjectiveTracker : ObjectiveTracker
	{
		public FinishTeleporterObjectiveTracker()
		{
			baseToken = "OBJECTIVE_FINISH_TELEPORTER";
		}
	}

	private class StripExitAnimation
	{
		public float t;

		private readonly float originalHeight;

		public readonly ObjectiveTracker objectiveTracker;

		private readonly LayoutElement layoutElement;

		private readonly CanvasGroup canvasGroup;

		public StripExitAnimation(ObjectiveTracker objectiveTracker)
		{
			if (Object.op_Implicit((Object)(object)objectiveTracker.stripObject))
			{
				this.objectiveTracker = objectiveTracker;
				layoutElement = objectiveTracker.stripObject.GetComponent<LayoutElement>();
				canvasGroup = objectiveTracker.stripObject.GetComponent<CanvasGroup>();
				originalHeight = layoutElement.minHeight;
			}
		}

		public void SetT(float newT)
		{
			if (Object.op_Implicit((Object)(object)objectiveTracker.stripObject))
			{
				t = newT;
				float alpha = Mathf.Clamp01(Util.Remap(t, 0.5f, 0.75f, 1f, 0f));
				canvasGroup.alpha = alpha;
				float num = Mathf.Clamp01(Util.Remap(t, 0.75f, 1f, 1f, 0f));
				num *= num;
				layoutElement.minHeight = num * originalHeight;
				layoutElement.preferredHeight = layoutElement.minHeight;
				layoutElement.flexibleHeight = 0f;
			}
		}
	}

	private class StripEnterAnimation
	{
		public float t;

		private readonly float finalHeight;

		public readonly ObjectiveTracker objectiveTracker;

		private readonly LayoutElement layoutElement;

		private readonly CanvasGroup canvasGroup;

		public StripEnterAnimation(ObjectiveTracker objectiveTracker)
		{
			if (Object.op_Implicit((Object)(object)objectiveTracker.stripObject))
			{
				this.objectiveTracker = objectiveTracker;
				layoutElement = objectiveTracker.stripObject.GetComponent<LayoutElement>();
				canvasGroup = objectiveTracker.stripObject.GetComponent<CanvasGroup>();
				finalHeight = layoutElement.minHeight;
			}
		}

		public void SetT(float newT)
		{
			if (Object.op_Implicit((Object)(object)objectiveTracker.stripObject))
			{
				t = newT;
				float alpha = Mathf.Clamp01(Util.Remap(1f - t, 0.5f, 0.75f, 1f, 0f));
				canvasGroup.alpha = alpha;
				float num = Mathf.Clamp01(Util.Remap(1f - t, 0.75f, 1f, 1f, 0f));
				num *= num;
				layoutElement.minHeight = num * finalHeight;
				layoutElement.preferredHeight = layoutElement.minHeight;
				layoutElement.flexibleHeight = 0f;
			}
		}
	}

	public RectTransform objectiveTrackerContainer;

	public GameObject objectiveTrackerPrefab;

	public Sprite checkboxActiveSprite;

	public Sprite checkboxSuccessSprite;

	public Sprite checkboxFailSprite;

	private CharacterMaster currentMaster;

	private readonly List<ObjectiveTracker> objectiveTrackers = new List<ObjectiveTracker>();

	private Dictionary<ObjectiveSourceDescriptor, ObjectiveTracker> objectiveSourceToTrackerDictionary = new Dictionary<ObjectiveSourceDescriptor, ObjectiveTracker>(EqualityComparer<ObjectiveSourceDescriptor>.Default);

	private readonly List<ObjectiveSourceDescriptor> objectiveSourceDescriptors = new List<ObjectiveSourceDescriptor>();

	private readonly List<StripExitAnimation> exitAnimations = new List<StripExitAnimation>();

	private readonly List<StripEnterAnimation> enterAnimations = new List<StripEnterAnimation>();

	public static event Action<CharacterMaster, List<ObjectiveSourceDescriptor>> collectObjectiveSources;

	public void SetCurrentMaster(CharacterMaster newMaster)
	{
		if (!((Object)(object)newMaster == (Object)(object)currentMaster))
		{
			for (int num = objectiveTrackers.Count - 1; num >= 0; num--)
			{
				Object.Destroy((Object)(object)objectiveTrackers[num].stripObject);
			}
			objectiveTrackers.Clear();
			objectiveSourceToTrackerDictionary.Clear();
			currentMaster = newMaster;
			RefreshObjectiveTrackers();
		}
	}

	private void AddObjectiveTracker(ObjectiveTracker objectiveTracker)
	{
		GameObject val = Object.Instantiate<GameObject>(objectiveTrackerPrefab, (Transform)(object)objectiveTrackerContainer);
		val.SetActive(true);
		objectiveTracker.owner = this;
		objectiveTracker.SetStrip(val);
		objectiveTrackers.Add(objectiveTracker);
		objectiveSourceToTrackerDictionary.Add(objectiveTracker.sourceDescriptor, objectiveTracker);
		AddEnterAnimation(objectiveTracker);
	}

	private void RemoveObjectiveTracker(ObjectiveTracker objectiveTracker)
	{
		objectiveTrackers.Remove(objectiveTracker);
		objectiveSourceToTrackerDictionary.Remove(objectiveTracker.sourceDescriptor);
		objectiveTracker.Retire();
		AddExitAnimation(objectiveTracker);
	}

	private void RefreshObjectiveTrackers()
	{
		foreach (ObjectiveTracker objectiveTracker2 in objectiveTrackers)
		{
			objectiveTracker2.isRelevant = false;
		}
		if (Object.op_Implicit((Object)(object)currentMaster))
		{
			GetObjectiveSources(currentMaster, objectiveSourceDescriptors);
			foreach (ObjectiveSourceDescriptor objectiveSourceDescriptor in objectiveSourceDescriptors)
			{
				if (objectiveSourceToTrackerDictionary.TryGetValue(objectiveSourceDescriptor, out var value))
				{
					value.isRelevant = true;
					continue;
				}
				ObjectiveTracker objectiveTracker = ObjectiveTracker.Instantiate(objectiveSourceDescriptor);
				objectiveTracker.isRelevant = true;
				AddObjectiveTracker(objectiveTracker);
			}
		}
		for (int num = objectiveTrackers.Count - 1; num >= 0; num--)
		{
			if (!objectiveTrackers[num].isRelevant)
			{
				RemoveObjectiveTracker(objectiveTrackers[num]);
			}
		}
		foreach (ObjectiveTracker objectiveTracker3 in objectiveTrackers)
		{
			objectiveTracker3.UpdateStrip();
		}
	}

	private void GetObjectiveSources(CharacterMaster master, [NotNull] List<ObjectiveSourceDescriptor> output)
	{
		output.Clear();
		WeeklyRun weeklyRun = Run.instance as WeeklyRun;
		if (Object.op_Implicit((Object)(object)weeklyRun) && weeklyRun.crystalsRequiredToKill > weeklyRun.crystalsKilled)
		{
			output.Add(new ObjectiveSourceDescriptor
			{
				source = (Object)(object)Run.instance,
				master = master,
				objectiveType = typeof(DestroyTimeCrystals)
			});
		}
		TeleporterInteraction instance = TeleporterInteraction.instance;
		if (Object.op_Implicit((Object)(object)instance))
		{
			Type type = null;
			if (instance.isCharged && !instance.isInFinalSequence)
			{
				type = typeof(FinishTeleporterObjectiveTracker);
			}
			else if (instance.isIdle)
			{
				type = typeof(FindTeleporterObjectiveTracker);
			}
			if (type != null)
			{
				output.Add(new ObjectiveSourceDescriptor
				{
					source = (Object)(object)instance,
					master = master,
					objectiveType = type
				});
			}
		}
		if (Object.op_Implicit((Object)(object)GoldshoresMissionController.instance))
		{
			Type type2 = GoldshoresMissionController.instance.entityStateMachine.state.GetType();
			if ((type2 == typeof(ActivateBeacons) || type2 == typeof(GoldshoresBossfight)) && GoldshoresMissionController.instance.beaconsActive < GoldshoresMissionController.instance.beaconCount)
			{
				output.Add(new ObjectiveSourceDescriptor
				{
					source = (Object)(object)GoldshoresMissionController.instance,
					master = master,
					objectiveType = typeof(ActivateGoldshoreBeaconTracker)
				});
			}
		}
		if (Object.op_Implicit((Object)(object)ArenaMissionController.instance) && ArenaMissionController.instance.clearedRounds < ArenaMissionController.instance.totalRoundsMax)
		{
			output.Add(new ObjectiveSourceDescriptor
			{
				source = (Object)(object)ArenaMissionController.instance,
				master = master,
				objectiveType = typeof(ClearArena)
			});
		}
		ObjectivePanelController.collectObjectiveSources?.Invoke(master, output);
	}

	private void Update()
	{
		RefreshObjectiveTrackers();
		RunExitAnimations();
		RunEnterAnimations();
	}

	private void AddExitAnimation(ObjectiveTracker objectiveTracker)
	{
		for (int i = 0; i < enterAnimations.Count; i++)
		{
			if (enterAnimations[i].objectiveTracker == objectiveTracker)
			{
				enterAnimations.RemoveAt(i);
				break;
			}
		}
		exitAnimations.Add(new StripExitAnimation(objectiveTracker));
	}

	private void AddEnterAnimation(ObjectiveTracker objectiveTracker)
	{
		enterAnimations.Add(new StripEnterAnimation(objectiveTracker));
	}

	private void RunExitAnimations()
	{
		float deltaTime = Time.deltaTime;
		float num = 7f;
		float num2 = deltaTime / num;
		for (int num3 = exitAnimations.Count - 1; num3 >= 0; num3--)
		{
			StripExitAnimation stripExitAnimation = exitAnimations[num3];
			float num4 = Mathf.Min(stripExitAnimation.t + num2, 1f);
			exitAnimations[num3].SetT(num4);
			if (num4 >= 1f)
			{
				Object.Destroy((Object)(object)stripExitAnimation.objectiveTracker.stripObject);
				exitAnimations.RemoveAt(num3);
			}
		}
	}

	private void RunEnterAnimations()
	{
		float deltaTime = Time.deltaTime;
		float num = 7f;
		float num2 = deltaTime / num;
		for (int num3 = enterAnimations.Count - 1; num3 >= 0; num3--)
		{
			float num4 = Mathf.Min(enterAnimations[num3].t + num2, 1f);
			enterAnimations[num3].SetT(num4);
			if (num4 >= 1f)
			{
				enterAnimations.RemoveAt(num3);
			}
		}
	}
}
