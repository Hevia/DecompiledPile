using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using RoR2.UI;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

[DisallowMultipleComponent]
public class TeamComponent : NetworkBehaviour, ILifeBehavior
{
	public bool hideAllyCardDisplay;

	[SerializeField]
	private TeamIndex _teamIndex = TeamIndex.None;

	private TeamIndex oldTeamIndex = TeamIndex.None;

	private GameObject defaultIndicatorPrefab;

	private GameObject indicator;

	private static readonly Queue<TeamComponent> indicatorSetupQueue;

	private static List<TeamComponent>[] teamsList;

	private static ReadOnlyCollection<TeamComponent>[] readonlyTeamsList;

	private static ReadOnlyCollection<TeamComponent> emptyTeamMembers;

	public CharacterBody body { get; private set; }

	public TeamIndex teamIndex
	{
		get
		{
			return _teamIndex;
		}
		set
		{
			if (_teamIndex != value)
			{
				_teamIndex = value;
				if (Application.isPlaying)
				{
					((NetworkBehaviour)this).SetDirtyBit(1u);
					OnChangeTeam(value);
				}
			}
		}
	}

	public static event Action<TeamComponent, TeamIndex> onJoinTeamGlobal;

	public static event Action<TeamComponent, TeamIndex> onLeaveTeamGlobal;

	private static bool TeamIsValid(TeamIndex teamIndex)
	{
		if (teamIndex >= TeamIndex.Neutral)
		{
			return teamIndex < TeamIndex.Count;
		}
		return false;
	}

	private void OnChangeTeam(TeamIndex newTeamIndex)
	{
		OnLeaveTeam(oldTeamIndex);
		OnJoinTeam(newTeamIndex);
	}

	private void OnLeaveTeam(TeamIndex oldTeamIndex)
	{
		if (TeamIsValid(oldTeamIndex))
		{
			teamsList[(int)oldTeamIndex].Remove(this);
		}
		TeamComponent.onLeaveTeamGlobal?.Invoke(this, oldTeamIndex);
	}

	private void OnJoinTeam(TeamIndex newTeamIndex)
	{
		if (TeamIsValid(newTeamIndex))
		{
			teamsList[(int)newTeamIndex].Add(this);
		}
		indicatorSetupQueue.Enqueue(this);
		HurtBox[] array = ((!Object.op_Implicit((Object)(object)body)) ? null : body.hurtBoxGroup?.hurtBoxes);
		if (array != null)
		{
			HurtBox[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].teamIndex = newTeamIndex;
			}
		}
		oldTeamIndex = newTeamIndex;
		TeamComponent.onJoinTeamGlobal?.Invoke(this, newTeamIndex);
	}

	private static void ProcessIndicatorSetupRequests()
	{
		while (indicatorSetupQueue.Count > 0)
		{
			TeamComponent teamComponent = indicatorSetupQueue.Dequeue();
			if (Object.op_Implicit((Object)(object)teamComponent))
			{
				try
				{
					teamComponent.SetupIndicator();
				}
				catch (Exception ex)
				{
					Debug.LogError((object)ex);
				}
			}
		}
	}

	[SystemInitializer(new Type[] { })]
	private static void Init()
	{
		RoR2Application.onFixedUpdate += ProcessIndicatorSetupRequests;
	}

	public void RequestDefaultIndicator(GameObject newIndicatorPrefab)
	{
		defaultIndicatorPrefab = newIndicatorPrefab;
		indicatorSetupQueue.Enqueue(this);
	}

	private void SetupIndicator()
	{
		if (Object.op_Implicit((Object)(object)indicator) || !Object.op_Implicit((Object)(object)body))
		{
			return;
		}
		CharacterMaster master = body.master;
		bool flag = Object.op_Implicit((Object)(object)master) && master.isBoss;
		GameObject val = defaultIndicatorPrefab;
		if (Object.op_Implicit((Object)(object)master) && teamIndex == TeamIndex.Player)
		{
			val = LegacyResourcesAPI.Load<GameObject>(body.isPlayerControlled ? "Prefabs/PositionIndicators/PlayerPositionIndicator" : "Prefabs/PositionIndicators/NPCPositionIndicator");
		}
		else if (flag)
		{
			val = LegacyResourcesAPI.Load<GameObject>("Prefabs/PositionIndicators/BossPositionIndicator");
		}
		if (Object.op_Implicit((Object)(object)indicator))
		{
			Object.Destroy((Object)(object)indicator);
			indicator = null;
		}
		if (Object.op_Implicit((Object)(object)val))
		{
			indicator = Object.Instantiate<GameObject>(val, ((Component)this).transform);
			indicator.GetComponent<PositionIndicator>().targetTransform = body.coreTransform;
			Nameplate component = indicator.GetComponent<Nameplate>();
			if (Object.op_Implicit((Object)(object)component))
			{
				component.SetBody(body);
			}
		}
	}

	static TeamComponent()
	{
		indicatorSetupQueue = new Queue<TeamComponent>();
		emptyTeamMembers = new List<TeamComponent>().AsReadOnly();
		teamsList = new List<TeamComponent>[5];
		readonlyTeamsList = new ReadOnlyCollection<TeamComponent>[teamsList.Length];
		for (int i = 0; i < teamsList.Length; i++)
		{
			teamsList[i] = new List<TeamComponent>();
			readonlyTeamsList[i] = teamsList[i].AsReadOnly();
		}
	}

	private void Awake()
	{
		body = ((Component)this).GetComponent<CharacterBody>();
	}

	public void Start()
	{
		SetupIndicator();
		if (oldTeamIndex != teamIndex)
		{
			OnChangeTeam(teamIndex);
		}
	}

	private void OnDestroy()
	{
		teamIndex = TeamIndex.None;
	}

	public void OnDeathStart()
	{
		((Behaviour)this).enabled = false;
	}

	public override bool OnSerialize(NetworkWriter writer, bool initialState)
	{
		writer.Write(teamIndex);
		if (!initialState)
		{
			return ((NetworkBehaviour)this).syncVarDirtyBits != 0;
		}
		return true;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		teamIndex = reader.ReadTeamIndex();
	}

	public static ReadOnlyCollection<TeamComponent> GetTeamMembers(TeamIndex teamIndex)
	{
		if (!TeamIsValid(teamIndex))
		{
			return emptyTeamMembers;
		}
		return readonlyTeamsList[(int)teamIndex];
	}

	public static TeamIndex GetObjectTeam(GameObject gameObject)
	{
		if (Object.op_Implicit((Object)(object)gameObject))
		{
			TeamComponent component = gameObject.GetComponent<TeamComponent>();
			if (Object.op_Implicit((Object)(object)component))
			{
				return component.teamIndex;
			}
		}
		return TeamIndex.None;
	}

	private void UNetVersion()
	{
	}

	public override void PreStartClient()
	{
	}
}
