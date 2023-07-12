using System.Collections.Generic;
using System.Collections.ObjectModel;
using RoR2;
using RoR2.Audio;
using RoR2.UI;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.VoidCamp;

public class Idle : EntityState
{
	private class VoidCampObjectiveTracker : ObjectivePanelController.ObjectiveTracker
	{
		protected override string GenerateString()
		{
			int count = TeamComponent.GetTeamMembers(TeamIndex.Void).Count;
			return string.Format(Language.GetString("OBJECTIVE_VOIDCAMP"), count);
		}

		protected override bool IsDirty()
		{
			return true;
		}
	}

	[SerializeField]
	public string baseAnimationLayerName;

	[SerializeField]
	public string baseAnimationStateName;

	[SerializeField]
	public string additiveAnimationLayerName;

	[SerializeField]
	public string additiveAnimationStateName;

	[SerializeField]
	public float clearCheckRate;

	[SerializeField]
	public float initialClearCheckDelay;

	[SerializeField]
	public LoopSoundDef loopSoundDef;

	[SerializeField]
	public int indicatorMaxTeamCountThreshold;

	[SerializeField]
	public GameObject indicatorPrefab;

	private LoopSoundManager.SoundLoopPtr loopPtr;

	private bool hasEnabledIndicators;

	private HashSet<NetworkInstanceId> indicatedNetIds;

	private float countdown;

	public override void OnEnter()
	{
		base.OnEnter();
		PlayAnimation(baseAnimationLayerName, baseAnimationStateName);
		PlayAnimation(additiveAnimationLayerName, additiveAnimationStateName);
		loopPtr = LoopSoundManager.PlaySoundLoopLocal(base.gameObject, loopSoundDef);
		countdown = initialClearCheckDelay;
		indicatedNetIds = new HashSet<NetworkInstanceId>();
		((Behaviour)GetComponent<OutsideInteractableLocker>()).enabled = true;
		ObjectivePanelController.collectObjectiveSources += OnCollectObjectiveSources;
	}

	private void OnCollectObjectiveSources(CharacterMaster master, List<ObjectivePanelController.ObjectiveSourceDescriptor> objectiveSourcesList)
	{
		objectiveSourcesList.Add(new ObjectivePanelController.ObjectiveSourceDescriptor
		{
			master = master,
			objectiveType = typeof(VoidCampObjectiveTracker),
			source = (Object)(object)base.gameObject
		});
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		countdown -= Time.fixedDeltaTime;
		if (!(countdown < 0f))
		{
			return;
		}
		countdown = clearCheckRate;
		ReadOnlyCollection<TeamComponent> teamMembers = TeamComponent.GetTeamMembers(TeamIndex.Void);
		int count = teamMembers.Count;
		if (count <= 0)
		{
			outer.SetNextState(new Deactivate());
		}
		else
		{
			if (!hasEnabledIndicators && count > indicatorMaxTeamCountThreshold)
			{
				return;
			}
			hasEnabledIndicators = true;
			foreach (TeamComponent item in teamMembers)
			{
				if (Object.op_Implicit((Object)(object)item) && Object.op_Implicit((Object)(object)item.body) && Object.op_Implicit((Object)(object)item.body.master))
				{
					RequestIndicatorForMaster(item.body.master);
				}
			}
		}
	}

	public override void OnExit()
	{
		LoopSoundManager.StopSoundLoopLocal(loopPtr);
		((Behaviour)GetComponent<OutsideInteractableLocker>()).enabled = false;
		ObjectivePanelController.collectObjectiveSources -= OnCollectObjectiveSources;
		base.OnExit();
	}

	protected void RequestIndicatorForMaster(CharacterMaster master)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		if (indicatedNetIds.Contains(((NetworkBehaviour)master).netId))
		{
			return;
		}
		GameObject bodyObject = master.GetBodyObject();
		if (Object.op_Implicit((Object)(object)bodyObject))
		{
			TeamComponent component = bodyObject.GetComponent<TeamComponent>();
			if (Object.op_Implicit((Object)(object)component))
			{
				indicatedNetIds.Add(((NetworkBehaviour)master).netId);
				component.RequestDefaultIndicator(indicatorPrefab);
			}
		}
	}
}
