using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

namespace RoR2;

public class BullseyeSearch
{
	private struct CandidateInfo
	{
		[StructLayout(LayoutKind.Sequential, Size = 1)]
		public struct EntityEqualityComparer : IEqualityComparer<CandidateInfo>
		{
			public bool Equals(CandidateInfo a, CandidateInfo b)
			{
				return a.hurtBox.healthComponent == b.hurtBox.healthComponent;
			}

			public int GetHashCode(CandidateInfo obj)
			{
				return ((object)obj.hurtBox.healthComponent).GetHashCode();
			}
		}

		public HurtBox hurtBox;

		public Vector3 position;

		public float dot;

		public float distanceSqr;
	}

	public enum SortMode
	{
		None,
		Distance,
		Angle,
		DistanceAndAngle
	}

	private delegate CandidateInfo Selector(HurtBox hurtBox);

	public CharacterBody viewer;

	public Vector3 searchOrigin;

	public Vector3 searchDirection;

	private float minThetaDot = -1f;

	private float maxThetaDot = 1f;

	public float minDistanceFilter;

	public float maxDistanceFilter = float.PositiveInfinity;

	public TeamMask teamMaskFilter = TeamMask.allButNeutral;

	public bool filterByLoS = true;

	public bool filterByDistinctEntity;

	public QueryTriggerInteraction queryTriggerInteraction;

	public SortMode sortMode = SortMode.Distance;

	private IEnumerable<CandidateInfo> candidatesEnumerable;

	public float minAngleFilter
	{
		set
		{
			maxThetaDot = Mathf.Cos(Mathf.Clamp(value, 0f, 180f) * (MathF.PI / 180f));
		}
	}

	public float maxAngleFilter
	{
		set
		{
			minThetaDot = Mathf.Cos(Mathf.Clamp(value, 0f, 180f) * (MathF.PI / 180f));
		}
	}

	private bool filterByDistance
	{
		get
		{
			if (!(minDistanceFilter > 0f) && !(maxDistanceFilter < float.PositiveInfinity))
			{
				if (Object.op_Implicit((Object)(object)viewer))
				{
					return viewer.visionDistance < float.PositiveInfinity;
				}
				return false;
			}
			return true;
		}
	}

	private bool filterByAngle
	{
		get
		{
			if (!(minThetaDot > -1f))
			{
				return maxThetaDot < 1f;
			}
			return true;
		}
	}

	private Func<HurtBox, CandidateInfo> GetSelector()
	{
		bool getDot = filterByAngle;
		bool getDistanceSqr = filterByDistance;
		getDistanceSqr |= sortMode == SortMode.Distance || sortMode == SortMode.DistanceAndAngle;
		getDot |= sortMode == SortMode.Angle || sortMode == SortMode.DistanceAndAngle;
		bool getDifference = getDot || getDistanceSqr;
		bool getPosition = getDot || getDistanceSqr || filterByLoS;
		return delegate(HurtBox hurtBox)
		{
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			CandidateInfo candidateInfo = default(CandidateInfo);
			candidateInfo.hurtBox = hurtBox;
			CandidateInfo result = candidateInfo;
			if (getPosition)
			{
				result.position = ((Component)hurtBox).transform.position;
			}
			Vector3 val = default(Vector3);
			if (getDifference)
			{
				val = result.position - searchOrigin;
			}
			if (getDot)
			{
				result.dot = Vector3.Dot(searchDirection, ((Vector3)(ref val)).normalized);
			}
			if (getDistanceSqr)
			{
				result.distanceSqr = ((Vector3)(ref val)).sqrMagnitude;
			}
			return result;
		};
	}

	public void RefreshCandidates()
	{
		Func<HurtBox, CandidateInfo> selector = GetSelector();
		candidatesEnumerable = HurtBox.readOnlyBullseyesList.Where((HurtBox hurtBox) => teamMaskFilter.HasTeam(hurtBox.teamIndex)).Select(selector);
		if (filterByAngle)
		{
			candidatesEnumerable = candidatesEnumerable.Where(DotOkay);
		}
		float minDistanceSqr;
		float maxDistanceSqr;
		if (filterByDistance)
		{
			float num = maxDistanceFilter;
			if (Object.op_Implicit((Object)(object)viewer))
			{
				num = Mathf.Min(num, viewer.visionDistance);
			}
			minDistanceSqr = minDistanceFilter * minDistanceFilter;
			maxDistanceSqr = num * num;
			candidatesEnumerable = candidatesEnumerable.Where(DistanceOkay);
		}
		if (filterByDistinctEntity)
		{
			candidatesEnumerable = candidatesEnumerable.Distinct(default(CandidateInfo.EntityEqualityComparer));
		}
		Func<CandidateInfo, float> sorter = GetSorter();
		if (sorter != null)
		{
			candidatesEnumerable = candidatesEnumerable.OrderBy(sorter);
		}
		bool DistanceOkay(CandidateInfo candidateInfo)
		{
			if (candidateInfo.distanceSqr >= minDistanceSqr)
			{
				return candidateInfo.distanceSqr <= maxDistanceSqr;
			}
			return false;
		}
		bool DotOkay(CandidateInfo candidateInfo)
		{
			if (minThetaDot <= candidateInfo.dot)
			{
				return candidateInfo.dot <= maxThetaDot;
			}
			return false;
		}
	}

	private Func<CandidateInfo, float> GetSorter()
	{
		return sortMode switch
		{
			SortMode.Distance => (CandidateInfo candidateInfo) => candidateInfo.distanceSqr, 
			SortMode.Angle => (CandidateInfo candidateInfo) => 0f - candidateInfo.dot, 
			SortMode.DistanceAndAngle => (CandidateInfo candidateInfo) => (0f - candidateInfo.dot) * candidateInfo.distanceSqr, 
			_ => null, 
		};
	}

	public void FilterCandidatesByHealthFraction(float minHealthFraction = 0f, float maxHealthFraction = 1f)
	{
		if (minHealthFraction > 0f)
		{
			if (maxHealthFraction < 1f)
			{
				candidatesEnumerable = candidatesEnumerable.Where(delegate(CandidateInfo v)
				{
					float combinedHealthFraction = v.hurtBox.healthComponent.combinedHealthFraction;
					return combinedHealthFraction >= minHealthFraction && combinedHealthFraction <= maxHealthFraction;
				});
			}
			else
			{
				candidatesEnumerable = candidatesEnumerable.Where((CandidateInfo v) => v.hurtBox.healthComponent.combinedHealthFraction >= minHealthFraction);
			}
		}
		else if (maxHealthFraction < 1f)
		{
			candidatesEnumerable = candidatesEnumerable.Where((CandidateInfo v) => v.hurtBox.healthComponent.combinedHealthFraction <= maxHealthFraction);
		}
	}

	public void FilterOutGameObject(GameObject gameObject)
	{
		candidatesEnumerable = candidatesEnumerable.Where((CandidateInfo v) => (Object)(object)((Component)v.hurtBox.healthComponent).gameObject != (Object)(object)gameObject);
	}

	public IEnumerable<HurtBox> GetResults()
	{
		IEnumerable<CandidateInfo> source = candidatesEnumerable;
		if (filterByLoS)
		{
			source = source.Where((CandidateInfo candidateInfo) => CheckLoS(candidateInfo.position));
		}
		if (Object.op_Implicit((Object)(object)viewer))
		{
			source = source.Where((CandidateInfo candidateInfo) => CheckVisible(((Component)candidateInfo.hurtBox.healthComponent).gameObject));
		}
		return source.Select((CandidateInfo candidateInfo) => candidateInfo.hurtBox);
	}

	private bool CheckLoS(Vector3 targetPosition)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = targetPosition - searchOrigin;
		RaycastHit val2 = default(RaycastHit);
		return !Physics.Raycast(searchOrigin, val, ref val2, ((Vector3)(ref val)).magnitude, LayerMask.op_Implicit(LayerIndex.world.mask), queryTriggerInteraction);
	}

	private bool CheckVisible(GameObject gameObject)
	{
		CharacterBody component = gameObject.GetComponent<CharacterBody>();
		if (Object.op_Implicit((Object)(object)component))
		{
			return component.GetVisibilityLevel(viewer) >= VisibilityLevel.Revealed;
		}
		return true;
	}
}
