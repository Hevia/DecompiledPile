using RoR2;
using RoR2.CharacterSpeech;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Missions.BrotherEncounter;

public abstract class BrotherEncounterPhaseBaseState : BrotherEncounterBaseState
{
	[SerializeField]
	public float durationBeforeEnablingCombatEncounter;

	[SerializeField]
	public GameObject speechControllerPrefab;

	protected ScriptedCombatEncounter phaseScriptedCombatEncounter;

	protected GameObject phaseControllerObject;

	protected GameObject phaseControllerSubObjectContainer;

	protected BossGroup phaseBossGroup;

	private bool hasSpawned;

	private bool finishedServer;

	private const float minimumDurationPerPhase = 2f;

	private Run.FixedTimeStamp healthBarShowTime = Run.FixedTimeStamp.positiveInfinity;

	protected abstract EntityState nextState { get; }

	protected abstract string phaseControllerChildString { get; }

	protected virtual float healthBarShowDelay => 0f;

	public override void OnEnter()
	{
		base.OnEnter();
		if (Object.op_Implicit((Object)(object)PhaseCounter.instance))
		{
			PhaseCounter.instance.GoToNextPhase();
		}
		if (Object.op_Implicit((Object)(object)childLocator))
		{
			phaseControllerObject = ((Component)childLocator.FindChild(phaseControllerChildString)).gameObject;
			if (Object.op_Implicit((Object)(object)phaseControllerObject))
			{
				phaseScriptedCombatEncounter = phaseControllerObject.GetComponent<ScriptedCombatEncounter>();
				phaseBossGroup = phaseControllerObject.GetComponent<BossGroup>();
				phaseControllerSubObjectContainer = ((Component)phaseControllerObject.transform.Find("PhaseObjects")).gameObject;
				phaseControllerSubObjectContainer.SetActive(true);
			}
			GameObject val = ((Component)childLocator.FindChild("AllPhases")).gameObject;
			if (Object.op_Implicit((Object)(object)val))
			{
				val.SetActive(true);
			}
		}
		healthBarShowTime = Run.FixedTimeStamp.now + healthBarShowDelay;
		if (Object.op_Implicit((Object)(object)DirectorCore.instance))
		{
			CombatDirector[] components = ((Component)DirectorCore.instance).GetComponents<CombatDirector>();
			for (int i = 0; i < components.Length; i++)
			{
				((Behaviour)components[i]).enabled = false;
			}
		}
		if (NetworkServer.active && phaseScriptedCombatEncounter != null)
		{
			phaseScriptedCombatEncounter.combatSquad.onMemberAddedServer += OnMemberAddedServer;
		}
	}

	public override void OnExit()
	{
		if (phaseScriptedCombatEncounter != null)
		{
			phaseScriptedCombatEncounter.combatSquad.onMemberAddedServer -= OnMemberAddedServer;
		}
		if (Object.op_Implicit((Object)(object)phaseControllerSubObjectContainer))
		{
			phaseControllerSubObjectContainer.SetActive(false);
		}
		if (Object.op_Implicit((Object)(object)phaseBossGroup))
		{
			phaseBossGroup.shouldDisplayHealthBarOnHud = false;
		}
		base.OnExit();
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		phaseBossGroup.shouldDisplayHealthBarOnHud = healthBarShowTime.hasPassed;
		if (!hasSpawned)
		{
			if (base.fixedAge > durationBeforeEnablingCombatEncounter)
			{
				BeginEncounter();
			}
		}
		else if (NetworkServer.active && !finishedServer && base.fixedAge > 2f + durationBeforeEnablingCombatEncounter && Object.op_Implicit((Object)(object)phaseScriptedCombatEncounter) && phaseScriptedCombatEncounter.combatSquad.memberCount == 0)
		{
			finishedServer = true;
			outer.SetNextState(nextState);
		}
	}

	protected void BeginEncounter()
	{
		hasSpawned = true;
		PreEncounterBegin();
		if (NetworkServer.active)
		{
			phaseScriptedCombatEncounter.BeginEncounter();
		}
	}

	protected virtual void PreEncounterBegin()
	{
	}

	protected virtual void OnMemberAddedServer(CharacterMaster master)
	{
		if (Object.op_Implicit((Object)(object)speechControllerPrefab))
		{
			Object.Instantiate<GameObject>(speechControllerPrefab, ((Component)master).transform).GetComponent<CharacterSpeechController>().characterMaster = master;
		}
	}
}
