using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace EntityStates.LaserTurbine;

public class AimState : LaserTurbineBaseState
{
	public static float targetAcquisitionRadius;

	private bool foundTarget;

	protected override bool shouldFollow => false;

	public override void OnEnter()
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		if (!base.isAuthority)
		{
			return;
		}
		TeamMask enemyTeams = TeamMask.GetEnemyTeams(base.ownerBody.teamComponent.teamIndex);
		HurtBox[] hurtBoxes = new SphereSearch
		{
			radius = targetAcquisitionRadius,
			mask = LayerIndex.entityPrecise.mask,
			origin = base.transform.position,
			queryTriggerInteraction = (QueryTriggerInteraction)0
		}.RefreshCandidates().FilterCandidatesByHurtBoxTeam(enemyTeams).OrderCandidatesByDistance()
			.FilterCandidatesByDistinctHurtBoxEntities()
			.GetHurtBoxes();
		float blastRadius = FireMainBeamState.secondBombPrefab.GetComponent<ProjectileImpactExplosion>().blastRadius;
		int num = -1;
		int num2 = 0;
		for (int i = 0; i < hurtBoxes.Length; i++)
		{
			HurtBox[] hurtBoxes2 = new SphereSearch
			{
				radius = blastRadius,
				mask = LayerIndex.entityPrecise.mask,
				origin = ((Component)hurtBoxes[i]).transform.position,
				queryTriggerInteraction = (QueryTriggerInteraction)0
			}.RefreshCandidates().FilterCandidatesByHurtBoxTeam(enemyTeams).FilterCandidatesByDistinctHurtBoxEntities()
				.GetHurtBoxes();
			if (hurtBoxes2.Length > num2)
			{
				num = i;
				num2 = hurtBoxes2.Length;
			}
		}
		if (num != -1)
		{
			base.simpleRotateToDirection.targetRotation = Quaternion.LookRotation(((Component)hurtBoxes[num]).transform.position - base.transform.position);
			foundTarget = true;
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.isAuthority)
		{
			if (foundTarget)
			{
				outer.SetNextState(new ChargeMainBeamState());
			}
			else
			{
				outer.SetNextState(new ReadyState());
			}
		}
	}
}
