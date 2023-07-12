using System.Collections.ObjectModel;
using RoR2;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace EntityStates.RoboBallBoss.Weapon;

public class DeployMinions : BaseState
{
	public static float baseDuration = 3.5f;

	public static string attackSoundString;

	public static string summonSoundString;

	public static int maxSummonCount = 5;

	public static float summonDuration = 3.26f;

	public static string summonMuzzleString;

	public static string spawnCard;

	private Animator animator;

	private Transform modelTransform;

	private ChildLocator childLocator;

	private float duration;

	private float summonInterval;

	private float summonTimer;

	private int summonCount;

	private bool isSummoning;

	public override void OnEnter()
	{
		base.OnEnter();
		animator = GetModelAnimator();
		modelTransform = GetModelTransform();
		childLocator = ((Component)modelTransform).GetComponent<ChildLocator>();
		duration = baseDuration;
		PlayCrossfade("Gesture, Additive", "DeployMinions", "DeployMinions.playbackRate", duration, 0.1f);
		Util.PlaySound(attackSoundString, base.gameObject);
		summonInterval = summonDuration / (float)maxSummonCount;
	}

	private Transform FindTargetClosest(Vector3 point, TeamIndex enemyTeam)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		ReadOnlyCollection<TeamComponent> teamMembers = TeamComponent.GetTeamMembers(enemyTeam);
		float num = 99999f;
		Transform result = null;
		for (int i = 0; i < teamMembers.Count; i++)
		{
			float num2 = Vector3.SqrMagnitude(((Component)teamMembers[i]).transform.position - point);
			if (num2 < num)
			{
				num = num2;
				result = ((Component)teamMembers[i]).transform;
			}
		}
		return result;
	}

	private void SummonMinion()
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Expected O, but got Unknown
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Expected O, but got Unknown
		if (!Object.op_Implicit((Object)(object)base.characterBody) || !Object.op_Implicit((Object)(object)base.characterBody.master) || base.characterBody.master.GetDeployableCount(DeployableSlot.RoboBallMini) >= base.characterBody.master.GetDeployableSameSlotLimit(DeployableSlot.RoboBallMini))
		{
			return;
		}
		Util.PlaySound(summonSoundString, base.gameObject);
		if (NetworkServer.active)
		{
			Vector3 position = FindModelChild(summonMuzzleString).position;
			DirectorSpawnRequest directorSpawnRequest = new DirectorSpawnRequest(LegacyResourcesAPI.Load<SpawnCard>($"SpawnCards/CharacterSpawnCards/{spawnCard}"), new DirectorPlacementRule
			{
				placementMode = DirectorPlacementRule.PlacementMode.Direct,
				minDistance = 0f,
				maxDistance = 0f,
				position = position
			}, RoR2Application.rng);
			directorSpawnRequest.summonerBodyObject = base.gameObject;
			GameObject val = DirectorCore.instance.TrySpawnObject(directorSpawnRequest);
			if (Object.op_Implicit((Object)(object)val))
			{
				CharacterMaster component = val.GetComponent<CharacterMaster>();
				val.GetComponent<Inventory>().SetEquipmentIndex(base.characterBody.inventory.currentEquipmentIndex);
				Deployable deployable = val.AddComponent<Deployable>();
				deployable.onUndeploy = new UnityEvent();
				deployable.onUndeploy.AddListener(new UnityAction(component.TrueKill));
				base.characterBody.master.AddDeployable(deployable, DeployableSlot.RoboBallMini);
			}
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (Object.op_Implicit((Object)(object)animator))
		{
			bool flag = animator.GetFloat("DeployMinions.active") > 0.9f;
			if (isSummoning)
			{
				summonTimer += Time.fixedDeltaTime;
				if (NetworkServer.active && summonTimer > 0f && summonCount < maxSummonCount)
				{
					summonCount++;
					summonTimer -= summonInterval;
					SummonMinion();
				}
			}
			isSummoning = flag;
		}
		if (base.fixedAge >= duration && base.isAuthority)
		{
			outer.SetNextStateToMain();
		}
	}

	public override InterruptPriority GetMinimumInterruptPriority()
	{
		return InterruptPriority.PrioritySkill;
	}
}
