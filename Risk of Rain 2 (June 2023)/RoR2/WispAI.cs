using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace RoR2;

public class WispAI : MonoBehaviour
{
	private struct TargetSearchCandidate
	{
		public Transform transform;

		public Vector3 positionDiff;

		public float sqrDistance;
	}

	[Tooltip("The character to control.")]
	public GameObject body;

	[Tooltip("The enemy to target.")]
	public Transform targetTransform;

	[Tooltip("The skill to activate for a ranged attack.")]
	public GenericSkill fireSkill;

	[Tooltip("How close the character must be to the enemy to use a ranged attack.")]
	public float fireRange;

	private CharacterDirection bodyDirectionComponent;

	private CharacterMotor bodyMotorComponent;

	private static List<TargetSearchCandidate> candidateList = new List<TargetSearchCandidate>();

	private void Awake()
	{
		bodyDirectionComponent = body.GetComponent<CharacterDirection>();
		bodyMotorComponent = body.GetComponent<CharacterMotor>();
	}

	private void FixedUpdate()
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)body))
		{
			return;
		}
		if (!Object.op_Implicit((Object)(object)targetTransform))
		{
			targetTransform = SearchForTarget();
		}
		if (Object.op_Implicit((Object)(object)targetTransform))
		{
			Vector3 val = targetTransform.position - body.transform.position;
			bodyMotorComponent.moveDirection = val;
			bodyDirectionComponent.moveVector = Vector3.Lerp(bodyDirectionComponent.moveVector, val, Time.deltaTime);
			if (Object.op_Implicit((Object)(object)fireSkill) && ((Vector3)(ref val)).sqrMagnitude < fireRange * fireRange)
			{
				fireSkill.ExecuteIfReady();
			}
		}
	}

	private Transform SearchForTarget()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = body.transform.position;
		Vector3 forward = bodyDirectionComponent.forward;
		ReadOnlyCollection<TeamComponent> teamMembers = TeamComponent.GetTeamMembers(TeamIndex.Player);
		for (int i = 0; i < teamMembers.Count; i++)
		{
			Transform transform = ((Component)teamMembers[i]).transform;
			Vector3 val = transform.position - position;
			if (Vector3.Dot(forward, val) > 0f)
			{
				candidateList.Add(new TargetSearchCandidate
				{
					transform = transform,
					positionDiff = val,
					sqrDistance = ((Vector3)(ref val)).sqrMagnitude
				});
			}
		}
		candidateList.Sort((TargetSearchCandidate a, TargetSearchCandidate b) => (!(a.sqrDistance < b.sqrDistance)) ? ((a.sqrDistance != b.sqrDistance) ? 1 : 0) : (-1));
		Transform result = null;
		for (int j = 0; j < candidateList.Count; j++)
		{
			if (!Physics.Raycast(position, candidateList[j].positionDiff, Mathf.Sqrt(candidateList[j].sqrDistance), LayerMask.op_Implicit(LayerIndex.world.mask), (QueryTriggerInteraction)1))
			{
				result = candidateList[j].transform;
				break;
			}
		}
		candidateList.Clear();
		return result;
	}
}
