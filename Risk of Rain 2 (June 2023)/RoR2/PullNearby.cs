using System.Collections.Generic;
using UnityEngine;

namespace RoR2;

public class PullNearby : MonoBehaviour
{
	public float pullRadius;

	public float pullDuration;

	public AnimationCurve pullStrengthCurve;

	public bool pullOnStart;

	public int maximumPullCount = int.MaxValue;

	private List<CharacterBody> victimBodyList = new List<CharacterBody>();

	private bool pulling;

	private TeamFilter teamFilter;

	private float fixedAge;

	private void Start()
	{
		teamFilter = ((Component)this).GetComponent<TeamFilter>();
		if (pullOnStart)
		{
			InitializePull();
		}
	}

	private void FixedUpdate()
	{
		fixedAge += Time.fixedDeltaTime;
		if (!(fixedAge > pullDuration))
		{
			UpdatePull(Time.fixedDeltaTime);
		}
	}

	private void UpdatePull(float deltaTime)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		if (!pulling)
		{
			return;
		}
		for (int i = 0; i < victimBodyList.Count; i++)
		{
			CharacterBody characterBody = victimBodyList[i];
			Vector3 val = ((Component)this).transform.position - characterBody.corePosition;
			float num = pullStrengthCurve.Evaluate(((Vector3)(ref val)).magnitude / pullRadius);
			Vector3 val2 = ((Vector3)(ref val)).normalized * num * deltaTime;
			CharacterMotor component = ((Component)characterBody).GetComponent<CharacterMotor>();
			if (Object.op_Implicit((Object)(object)component))
			{
				component.rootMotion += val2;
				continue;
			}
			Rigidbody component2 = ((Component)characterBody).GetComponent<Rigidbody>();
			if (Object.op_Implicit((Object)(object)component2))
			{
				component2.velocity += val2;
			}
		}
	}

	public void InitializePull()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		if (pulling)
		{
			return;
		}
		pulling = true;
		Collider[] array = Physics.OverlapSphere(((Component)this).transform.position, pullRadius, LayerMask.op_Implicit(LayerIndex.defaultLayer.mask));
		int i = 0;
		int num = 0;
		for (; i < array.Length; i++)
		{
			if (num >= maximumPullCount)
			{
				break;
			}
			HealthComponent component = ((Component)array[i]).GetComponent<HealthComponent>();
			if (Object.op_Implicit((Object)(object)component))
			{
				TeamComponent component2 = ((Component)component).GetComponent<TeamComponent>();
				bool flag = false;
				if (Object.op_Implicit((Object)(object)component2) && Object.op_Implicit((Object)(object)teamFilter))
				{
					flag = component2.teamIndex == teamFilter.teamIndex;
				}
				if (!flag)
				{
					AddToList(((Component)component).gameObject);
					num++;
				}
			}
		}
	}

	private void AddToList(GameObject affectedObject)
	{
		CharacterBody component = affectedObject.GetComponent<CharacterBody>();
		if (!victimBodyList.Contains(component))
		{
			victimBodyList.Add(component);
		}
	}
}
