using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public class FogDamageController : NetworkBehaviour
{
	[Tooltip("Used to control which teams to damage.  If it's null, it damages ALL teams")]
	[SerializeField]
	private TeamFilter teamFilter;

	[SerializeField]
	[Tooltip("If true, it damages all OTHER teams than the one specified.  If false, it damages the specified team.")]
	private bool invertTeamFilter;

	[SerializeField]
	[Tooltip("The period in seconds in between each tick")]
	private float tickPeriodSeconds;

	[SerializeField]
	[Tooltip("The fraction of combined health to deduct per second.  Note that damage is actually applied per tick, not per second.")]
	[Range(0f, 1f)]
	private float healthFractionPerSecond;

	[SerializeField]
	[Tooltip("The coefficient to increase the damage by, for every tick they take outside the zone.")]
	private float healthFractionRampCoefficientPerSecond;

	[Tooltip("The buff to apply when in danger, i.e not in the safe zone.")]
	[SerializeField]
	private BuffDef dangerBuffDef;

	[SerializeField]
	private float dangerBuffDuration;

	[Tooltip("An initial list of safe zones behaviors which protect bodies from the fog")]
	[SerializeField]
	private BaseZoneBehavior[] initialSafeZones;

	private float dictionaryValidationTimer;

	private float damageTimer;

	private List<IZone> safeZones = new List<IZone>();

	private Dictionary<CharacterBody, int> characterBodyToStacks = new Dictionary<CharacterBody, int>();

	private void Start()
	{
		BaseZoneBehavior[] array = initialSafeZones;
		foreach (BaseZoneBehavior zone in array)
		{
			AddSafeZone(zone);
		}
	}

	public void AddSafeZone(IZone zone)
	{
		safeZones.Add(zone);
	}

	public void RemoveSafeZone(IZone zone)
	{
		safeZones.Remove(zone);
	}

	private void FixedUpdate()
	{
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			return;
		}
		damageTimer += Time.fixedDeltaTime;
		dictionaryValidationTimer += Time.fixedDeltaTime;
		if (dictionaryValidationTimer > 60f)
		{
			dictionaryValidationTimer = 0f;
			CharacterBody[] array = new CharacterBody[characterBodyToStacks.Keys.Count];
			characterBodyToStacks.Keys.CopyTo(array, 0);
			for (int i = 0; i < array.Length; i++)
			{
				if (!Object.op_Implicit((Object)(object)array[i]))
				{
					characterBodyToStacks.Remove(array[i]);
				}
			}
		}
		while (damageTimer > tickPeriodSeconds)
		{
			damageTimer -= tickPeriodSeconds;
			if (Object.op_Implicit((Object)(object)teamFilter))
			{
				if (invertTeamFilter)
				{
					for (TeamIndex teamIndex = TeamIndex.Neutral; teamIndex < TeamIndex.Count; teamIndex++)
					{
						if (teamIndex != teamFilter.teamIndex && teamIndex != TeamIndex.None && teamIndex != 0)
						{
							EvaluateTeam(teamIndex);
						}
					}
				}
				else
				{
					EvaluateTeam(teamFilter.teamIndex);
				}
			}
			else
			{
				for (TeamIndex teamIndex2 = TeamIndex.Neutral; teamIndex2 < TeamIndex.Count; teamIndex2++)
				{
					EvaluateTeam(teamIndex2);
				}
			}
			foreach (KeyValuePair<CharacterBody, int> characterBodyToStack in characterBodyToStacks)
			{
				CharacterBody key = characterBodyToStack.Key;
				if (Object.op_Implicit((Object)(object)key) && Object.op_Implicit((Object)(object)((Component)key).transform) && Object.op_Implicit((Object)(object)key.healthComponent))
				{
					int num = characterBodyToStack.Value - 1;
					float num2 = healthFractionPerSecond * (1f + (float)num * healthFractionRampCoefficientPerSecond * tickPeriodSeconds) * tickPeriodSeconds * key.healthComponent.fullCombinedHealth;
					if (num2 > 0f)
					{
						key.healthComponent.TakeDamage(new DamageInfo
						{
							damage = num2,
							position = key.corePosition,
							damageType = (DamageType.BypassArmor | DamageType.BypassBlock),
							damageColorIndex = DamageColorIndex.Void
						});
					}
					if (Object.op_Implicit((Object)(object)dangerBuffDef))
					{
						key.AddTimedBuff(dangerBuffDef, dangerBuffDuration);
					}
				}
			}
		}
	}

	public IEnumerable<CharacterBody> GetAffectedBodies()
	{
		TeamIndex currentTeam2;
		if (Object.op_Implicit((Object)(object)teamFilter))
		{
			if (invertTeamFilter)
			{
				currentTeam2 = TeamIndex.Neutral;
				while (currentTeam2 < TeamIndex.Count)
				{
					IEnumerable<CharacterBody> affectedBodiesOnTeam = GetAffectedBodiesOnTeam(currentTeam2);
					foreach (CharacterBody item in affectedBodiesOnTeam)
					{
						yield return item;
					}
					TeamIndex teamIndex = currentTeam2 + 1;
					currentTeam2 = teamIndex;
				}
				yield break;
			}
			IEnumerable<CharacterBody> affectedBodiesOnTeam2 = GetAffectedBodiesOnTeam(teamFilter.teamIndex);
			foreach (CharacterBody item2 in affectedBodiesOnTeam2)
			{
				yield return item2;
			}
			yield break;
		}
		currentTeam2 = TeamIndex.Neutral;
		while (currentTeam2 < TeamIndex.Count)
		{
			IEnumerable<CharacterBody> affectedBodiesOnTeam3 = GetAffectedBodiesOnTeam(currentTeam2);
			foreach (CharacterBody item3 in affectedBodiesOnTeam3)
			{
				yield return item3;
			}
			TeamIndex teamIndex = currentTeam2 + 1;
			currentTeam2 = teamIndex;
		}
	}

	public IEnumerable<CharacterBody> GetAffectedBodiesOnTeam(TeamIndex teamIndex)
	{
		foreach (TeamComponent teamMember in TeamComponent.GetTeamMembers(teamIndex))
		{
			CharacterBody body = teamMember.body;
			bool flag = false;
			foreach (IZone safeZone in safeZones)
			{
				if (safeZone.IsInBounds(((Component)teamMember).transform.position))
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				yield return body;
			}
		}
	}

	private void EvaluateTeam(TeamIndex teamIndex)
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		foreach (TeamComponent teamMember in TeamComponent.GetTeamMembers(teamIndex))
		{
			CharacterBody body = teamMember.body;
			bool flag = characterBodyToStacks.ContainsKey(body);
			bool flag2 = false;
			foreach (IZone safeZone in safeZones)
			{
				if (safeZone.IsInBounds(((Component)teamMember).transform.position))
				{
					flag2 = true;
					break;
				}
			}
			if (flag2)
			{
				if (flag)
				{
					characterBodyToStacks.Remove(body);
				}
			}
			else if (!flag)
			{
				characterBodyToStacks.Add(body, 1);
			}
			else
			{
				characterBodyToStacks[body]++;
			}
		}
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		bool result = default(bool);
		return result;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
	}

	public override void PreStartClient()
	{
	}
}
