using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HG;
using RoR2.Projectile;
using UnityEngine;

namespace RoR2;

public class SphereSearch
{
	private struct Candidate
	{
		public Collider collider;

		public HurtBox hurtBox;

		public Vector3 position;

		public Vector3 difference;

		public float distanceSqr;

		public Transform root;

		public ProjectileController projectileController;

		public EntityLocator entityLocator;

		public static bool HurtBoxHealthComponentIsValid(Candidate candidate)
		{
			return Object.op_Implicit((Object)(object)candidate.hurtBox.healthComponent);
		}
	}

	private struct SearchData
	{
		private Candidate[] candidatesBuffer;

		private int[] candidatesMapping;

		private int candidatesCount;

		private bool hurtBoxesLoaded;

		private bool rootsLoaded;

		private bool projectileControllersLoaded;

		private bool entityLocatorsLoaded;

		private bool filteredByHurtBoxes;

		private bool filteredByHurtBoxHealthComponents;

		private bool filteredByProjectileControllers;

		private bool filteredByEntityLocators;

		public static readonly SearchData empty = new SearchData
		{
			candidatesBuffer = Array.Empty<Candidate>(),
			candidatesMapping = Array.Empty<int>(),
			candidatesCount = 0,
			hurtBoxesLoaded = false
		};

		public SearchData(Candidate[] candidatesBuffer)
		{
			this.candidatesBuffer = candidatesBuffer;
			candidatesMapping = new int[candidatesBuffer.Length];
			candidatesCount = candidatesBuffer.Length;
			for (int i = 0; i < candidatesBuffer.Length; i++)
			{
				candidatesMapping[i] = i;
			}
			hurtBoxesLoaded = false;
			rootsLoaded = false;
			projectileControllersLoaded = false;
			entityLocatorsLoaded = false;
			filteredByHurtBoxes = false;
			filteredByHurtBoxHealthComponents = false;
			filteredByProjectileControllers = false;
			filteredByEntityLocators = false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private ref Candidate GetCandidate(int i)
		{
			return ref candidatesBuffer[candidatesMapping[i]];
		}

		private void RemoveCandidate(int i)
		{
			ArrayUtils.ArrayRemoveAt<int>(candidatesMapping, ref candidatesCount, i, 1);
		}

		public void LoadHurtBoxes()
		{
			if (!hurtBoxesLoaded)
			{
				for (int i = 0; i < candidatesCount; i++)
				{
					ref Candidate candidate = ref GetCandidate(i);
					candidate.hurtBox = ((Component)candidate.collider).GetComponent<HurtBox>();
				}
				hurtBoxesLoaded = true;
			}
		}

		public void LoadRoots()
		{
			if (!rootsLoaded)
			{
				for (int i = 0; i < candidatesCount; i++)
				{
					ref Candidate candidate = ref GetCandidate(i);
					candidate.root = ((Component)candidate.collider).transform.root;
				}
				rootsLoaded = true;
			}
		}

		public void LoadProjectileControllers()
		{
			if (!projectileControllersLoaded)
			{
				LoadRoots();
				for (int i = 0; i < candidatesCount; i++)
				{
					ref Candidate candidate = ref GetCandidate(i);
					candidate.projectileController = (Object.op_Implicit((Object)(object)candidate.root) ? ((Component)candidate.root).GetComponent<ProjectileController>() : null);
				}
				projectileControllersLoaded = true;
			}
		}

		public void LoadColliderEntityLocators()
		{
			if (!entityLocatorsLoaded)
			{
				for (int i = 0; i < candidatesCount; i++)
				{
					ref Candidate candidate = ref GetCandidate(i);
					candidate.entityLocator = ((Component)candidate.collider).GetComponent<EntityLocator>();
				}
				entityLocatorsLoaded = true;
			}
		}

		public void FilterByProjectileControllers()
		{
			if (filteredByProjectileControllers)
			{
				return;
			}
			LoadProjectileControllers();
			for (int num = candidatesCount - 1; num >= 0; num--)
			{
				if (!Object.op_Implicit((Object)(object)GetCandidate(num).projectileController))
				{
					RemoveCandidate(num);
				}
			}
			filteredByProjectileControllers = true;
		}

		public void FilterByHurtBoxes()
		{
			if (filteredByHurtBoxes)
			{
				return;
			}
			LoadHurtBoxes();
			for (int num = candidatesCount - 1; num >= 0; num--)
			{
				if (!Object.op_Implicit((Object)(object)GetCandidate(num).hurtBox))
				{
					RemoveCandidate(num);
				}
			}
			filteredByHurtBoxes = true;
		}

		public void FilterByHurtBoxHealthComponents()
		{
			if (filteredByHurtBoxHealthComponents)
			{
				return;
			}
			FilterByHurtBoxes();
			for (int num = candidatesCount - 1; num >= 0; num--)
			{
				if (!Object.op_Implicit((Object)(object)GetCandidate(num).hurtBox.healthComponent))
				{
					RemoveCandidate(num);
				}
			}
			filteredByHurtBoxHealthComponents = true;
		}

		public void FilterByHurtBoxTeam(TeamMask teamMask)
		{
			FilterByHurtBoxes();
			for (int num = candidatesCount - 1; num >= 0; num--)
			{
				if (!teamMask.HasTeam(GetCandidate(num).hurtBox.teamIndex))
				{
					RemoveCandidate(num);
				}
			}
		}

		public void FilterByHurtBoxEntitiesDistinct()
		{
			FilterByHurtBoxHealthComponents();
			for (int num = candidatesCount - 1; num >= 0; num--)
			{
				ref Candidate candidate = ref GetCandidate(num);
				for (int num2 = num - 1; num2 >= 0; num2--)
				{
					ref Candidate candidate2 = ref GetCandidate(num2);
					if (candidate.hurtBox.healthComponent == candidate2.hurtBox.healthComponent)
					{
						RemoveCandidate(num);
						break;
					}
				}
			}
		}

		public void FilterByColliderEntities()
		{
			if (filteredByEntityLocators)
			{
				return;
			}
			LoadColliderEntityLocators();
			for (int num = candidatesCount - 1; num >= 0; num--)
			{
				ref Candidate candidate = ref GetCandidate(num);
				if (!Object.op_Implicit((Object)(object)candidate.entityLocator) || !Object.op_Implicit((Object)(object)candidate.entityLocator.entity))
				{
					RemoveCandidate(num);
				}
			}
			filteredByEntityLocators = true;
		}

		public void FilterByColliderEntitiesDistinct()
		{
			FilterByColliderEntities();
			for (int num = candidatesCount - 1; num >= 0; num--)
			{
				ref Candidate candidate = ref GetCandidate(num);
				for (int num2 = num - 1; num2 >= 0; num2--)
				{
					ref Candidate candidate2 = ref GetCandidate(num2);
					if (candidate.entityLocator.entity == candidate2.entityLocator.entity)
					{
						RemoveCandidate(num);
						break;
					}
				}
			}
		}

		public void OrderByDistance()
		{
			if (candidatesCount == 0)
			{
				return;
			}
			bool flag = true;
			while (flag)
			{
				flag = false;
				ref Candidate reference = ref GetCandidate(0);
				int i = 1;
				for (int num = candidatesCount - 1; i < num; i++)
				{
					ref Candidate candidate = ref GetCandidate(i);
					if (reference.distanceSqr > candidate.distanceSqr)
					{
						Util.Swap(ref candidatesMapping[i - 1], ref candidatesMapping[i]);
						flag = true;
					}
					else
					{
						reference = ref candidate;
					}
				}
			}
		}

		public HurtBox[] GetHurtBoxes()
		{
			FilterByHurtBoxes();
			HurtBox[] array = new HurtBox[candidatesCount];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = GetCandidate(i).hurtBox;
			}
			return array;
		}

		public void GetHurtBoxes(List<HurtBox> dest)
		{
			int num = dest.Count + candidatesCount;
			if (dest.Capacity < num)
			{
				dest.Capacity = num;
			}
			for (int i = 0; i < candidatesCount; i++)
			{
				dest.Add(GetCandidate(i).hurtBox);
			}
		}

		public void GetProjectileControllers(List<ProjectileController> dest)
		{
			int num = dest.Count + candidatesCount;
			if (dest.Capacity < num)
			{
				dest.Capacity = num;
			}
			for (int i = 0; i < candidatesCount; i++)
			{
				dest.Add(GetCandidate(i).projectileController);
			}
		}

		public void GetColliders(List<Collider> dest)
		{
			int num = dest.Count + candidatesCount;
			if (dest.Capacity < num)
			{
				dest.Capacity = num;
			}
			for (int i = 0; i < candidatesCount; i++)
			{
				dest.Add(GetCandidate(i).collider);
			}
		}
	}

	public float radius;

	public Vector3 origin;

	public LayerMask mask;

	public QueryTriggerInteraction queryTriggerInteraction;

	private SearchData searchData = SearchData.empty;

	public SphereSearch ClearCandidates()
	{
		searchData = SearchData.empty;
		return this;
	}

	public SphereSearch RefreshCandidates()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		Collider[] array = Physics.OverlapSphere(origin, radius, LayerMask.op_Implicit(mask), queryTriggerInteraction);
		Candidate[] array2 = new Candidate[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			Collider val = array[i];
			ref Candidate reference = ref array2[i];
			reference.collider = val;
			MeshCollider val2;
			if ((val2 = (MeshCollider)(object)((val is MeshCollider) ? val : null)) != null && !val2.convex)
			{
				reference.position = val.ClosestPointOnBounds(origin);
			}
			else
			{
				reference.position = val.ClosestPoint(origin);
			}
			reference.difference = reference.position - origin;
			reference.distanceSqr = ((Vector3)(ref reference.difference)).sqrMagnitude;
		}
		searchData = new SearchData(array2);
		return this;
	}

	public SphereSearch OrderCandidatesByDistance()
	{
		searchData.OrderByDistance();
		return this;
	}

	public SphereSearch FilterCandidatesByHurtBoxTeam(TeamMask mask)
	{
		searchData.FilterByHurtBoxTeam(mask);
		return this;
	}

	public SphereSearch FilterCandidatesByColliderEntities()
	{
		searchData.FilterByColliderEntities();
		return this;
	}

	public SphereSearch FilterCandidatesByDistinctColliderEntities()
	{
		searchData.FilterByColliderEntitiesDistinct();
		return this;
	}

	public SphereSearch FilterCandidatesByDistinctHurtBoxEntities()
	{
		searchData.FilterByHurtBoxEntitiesDistinct();
		return this;
	}

	public SphereSearch FilterCandidatesByProjectileControllers()
	{
		searchData.FilterByProjectileControllers();
		return this;
	}

	public HurtBox[] GetHurtBoxes()
	{
		return searchData.GetHurtBoxes();
	}

	public void GetHurtBoxes(List<HurtBox> dest)
	{
		searchData.GetHurtBoxes(dest);
	}

	public void GetProjectileControllers(List<ProjectileController> dest)
	{
		searchData.GetProjectileControllers(dest);
	}

	public void GetColliders(List<Collider> dest)
	{
		searchData.GetColliders(dest);
	}
}
