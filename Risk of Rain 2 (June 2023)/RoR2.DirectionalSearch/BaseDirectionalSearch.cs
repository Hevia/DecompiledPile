using System;
using System.Collections.Generic;
using HG;
using UnityEngine;

namespace RoR2.DirectionalSearch;

public class BaseDirectionalSearch<TSource, TSelector, TCandidateFilter> where TSource : class where TSelector : IGenericWorldSearchSelector<TSource> where TCandidateFilter : IGenericDirectionalSearchFilter<TSource>
{
	private struct CandidateInfo
	{
		public TSource source;

		public Vector3 position;

		public Vector3 diff;

		public float distance;

		public float dot;

		public GameObject entity;
	}

	public Vector3 searchOrigin;

	public Vector3 searchDirection;

	private float minThetaDot = -1f;

	private float maxThetaDot = 1f;

	public float minDistanceFilter;

	public float maxDistanceFilter = float.PositiveInfinity;

	public SortMode sortMode = SortMode.Distance;

	public bool filterByLoS = true;

	public bool filterByDistinctEntity;

	private CandidateInfo[] candidateInfoList = Array.Empty<CandidateInfo>();

	private int candidateCount;

	protected TSelector selector;

	protected TCandidateFilter candidateFilter;

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

	public BaseDirectionalSearch(TSelector selector, TCandidateFilter candidateFilter)
	{
		this.selector = selector;
		this.candidateFilter = candidateFilter;
	}

	public TSource SearchCandidatesForSingleTarget<TSourceEnumerable>(TSourceEnumerable sourceEnumerable) where TSourceEnumerable : IEnumerable<TSource>
	{
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_025d: Unknown result type (might be due to invalid IL or missing references)
		//IL_026f: Unknown result type (might be due to invalid IL or missing references)
		//IL_027f: Unknown result type (might be due to invalid IL or missing references)
		ArrayUtils.Clear<CandidateInfo>(candidateInfoList, ref candidateCount);
		float num = minDistanceFilter * minDistanceFilter;
		float num2 = maxDistanceFilter * maxDistanceFilter;
		foreach (TSource item in sourceEnumerable)
		{
			CandidateInfo candidateInfo = default(CandidateInfo);
			candidateInfo.source = item;
			CandidateInfo candidateInfo2 = candidateInfo;
			Transform transform = selector.GetTransform(item);
			if (!Object.op_Implicit((Object)(object)transform))
			{
				continue;
			}
			candidateInfo2.position = transform.position;
			candidateInfo2.diff = candidateInfo2.position - searchOrigin;
			float sqrMagnitude = ((Vector3)(ref candidateInfo2.diff)).sqrMagnitude;
			if (!(sqrMagnitude < num) && !(sqrMagnitude > num2))
			{
				candidateInfo2.distance = Mathf.Sqrt(sqrMagnitude);
				candidateInfo2.dot = ((candidateInfo2.distance == 0f) ? 0f : Vector3.Dot(searchDirection, candidateInfo2.diff / candidateInfo2.distance));
				if (!(candidateInfo2.dot < minThetaDot) && !(candidateInfo2.dot > maxThetaDot))
				{
					candidateInfo2.entity = selector.GetRootObject(item);
					ArrayUtils.ArrayAppend<CandidateInfo>(ref candidateInfoList, ref candidateCount, ref candidateInfo2);
				}
			}
		}
		for (int num3 = candidateCount - 1; num3 >= 0; num3--)
		{
			if (!candidateFilter.PassesFilter(candidateInfoList[num3].source))
			{
				ArrayUtils.ArrayRemoveAt<CandidateInfo>(candidateInfoList, ref candidateCount, num3, 1);
			}
		}
		Array.Sort(candidateInfoList, GetSorter());
		if (filterByDistinctEntity)
		{
			for (int num4 = candidateCount - 1; num4 >= 0; num4--)
			{
				ref CandidateInfo reference = ref candidateInfoList[num4];
				for (int i = 0; i < num4; i++)
				{
					ref CandidateInfo reference2 = ref candidateInfoList[i];
					if ((Object)(object)reference.entity == (Object)(object)reference2.entity)
					{
						ArrayUtils.ArrayRemoveAt<CandidateInfo>(candidateInfoList, ref candidateCount, num4, 1);
						break;
					}
				}
			}
		}
		TSource result = null;
		if (filterByLoS)
		{
			RaycastHit val = default(RaycastHit);
			for (int j = 0; j < candidateCount; j++)
			{
				if (!Physics.Linecast(searchOrigin, candidateInfoList[j].position, ref val, LayerMask.op_Implicit(LayerIndex.world.mask), (QueryTriggerInteraction)1))
				{
					result = candidateInfoList[j].source;
					break;
				}
			}
		}
		else if (candidateCount > 0)
		{
			result = candidateInfoList[0].source;
		}
		ArrayUtils.Clear<CandidateInfo>(candidateInfoList, ref candidateCount);
		return result;
	}

	private static int DistanceToInversePriority(CandidateInfo a, CandidateInfo b)
	{
		return a.distance.CompareTo(b.distance);
	}

	private static int AngleToInversePriority(CandidateInfo a, CandidateInfo b)
	{
		return (0f - a.dot).CompareTo(0f - b.dot);
	}

	private static int DistanceAndAngleToInversePriority(CandidateInfo a, CandidateInfo b)
	{
		return ((0f - a.dot) * a.distance).CompareTo((0f - b.dot) * b.distance);
	}

	private Comparison<CandidateInfo> GetSorter()
	{
		return sortMode switch
		{
			SortMode.Distance => DistanceToInversePriority, 
			SortMode.Angle => AngleToInversePriority, 
			SortMode.DistanceAndAngle => DistanceAndAngleToInversePriority, 
			_ => null, 
		};
	}
}
