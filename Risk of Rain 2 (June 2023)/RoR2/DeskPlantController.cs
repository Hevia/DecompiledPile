using EntityStates;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

[RequireComponent(typeof(TeamFilter))]
public class DeskPlantController : MonoBehaviour
{
	private abstract class DeskPlantBaseState : BaseState
	{
		protected DeskPlantController controller;

		protected abstract bool showSeedObject { get; }

		protected abstract bool showPlantObject { get; }

		protected abstract float CalcScale();

		public override void OnEnter()
		{
			base.OnEnter();
			controller = GetComponent<DeskPlantController>();
			controller.seedObject.SetActive(showSeedObject);
			controller.plantObject.SetActive(showPlantObject);
		}

		public override void Update()
		{
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			base.Update();
			if (showPlantObject)
			{
				float num = CalcScale();
				Vector3 val = default(Vector3);
				((Vector3)(ref val))._002Ector(num, num, num);
				Transform val2 = controller.plantObject.transform;
				if (val2.localScale != val)
				{
					val2.localScale = val;
				}
			}
		}
	}

	private class SeedState : DeskPlantBaseState
	{
		protected override bool showSeedObject => true;

		protected override bool showPlantObject => false;

		protected override float CalcScale()
		{
			return 1f;
		}

		public override void OnEnter()
		{
			base.OnEnter();
			Util.PlaySound("Play_item_proc_interstellarDeskPlant_grow", base.gameObject);
			GetComponent<Animation>().Play();
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (NetworkServer.active && base.fixedAge >= seedDuration)
			{
				outer.SetNextState(new SproutState());
			}
		}
	}

	private class SproutState : DeskPlantBaseState
	{
		protected override bool showSeedObject => false;

		protected override bool showPlantObject => true;

		protected override float CalcScale()
		{
			float num = Mathf.Clamp01(base.age / scaleDuration);
			return linearRampUp.Evaluate(num) * easeUp.Evaluate(num);
		}

		public override void OnEnter()
		{
			base.OnEnter();
			Util.PlaySound("Play_item_proc_interstellarDeskPlant_bloom", base.gameObject);
		}

		public override void OnExit()
		{
			base.OnExit();
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (NetworkServer.active && base.fixedAge >= scaleDuration)
			{
				outer.SetNextState(new MainState());
			}
		}
	}

	private class MainState : DeskPlantBaseState
	{
		private GameObject deskplantWard;

		protected override bool showSeedObject => false;

		protected override bool showPlantObject => true;

		protected override float CalcScale()
		{
			return 1f;
		}

		public override void OnEnter()
		{
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			base.OnEnter();
			if (NetworkServer.active && !Object.op_Implicit((Object)(object)deskplantWard))
			{
				deskplantWard = Object.Instantiate<GameObject>(LegacyResourcesAPI.Load<GameObject>("Prefabs/NetworkedObjects/DeskplantWard"), controller.plantObject.transform.position, Quaternion.identity);
				deskplantWard.GetComponent<TeamFilter>().teamIndex = controller.teamFilter.teamIndex;
				if (Object.op_Implicit((Object)(object)deskplantWard))
				{
					HealingWard component = deskplantWard.GetComponent<HealingWard>();
					component.healFraction = 0.05f;
					component.healPoints = 0f;
					component.Networkradius = controller.healingRadius + controller.radiusIncreasePerStack * (float)controller.itemCount;
					((Behaviour)component).enabled = true;
				}
				NetworkServer.Spawn(deskplantWard);
			}
		}

		public override void OnExit()
		{
			if (Object.op_Implicit((Object)(object)deskplantWard))
			{
				EntityState.Destroy((Object)(object)deskplantWard);
			}
			base.OnExit();
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (NetworkServer.active && base.fixedAge >= mainDuration)
			{
				outer.SetNextState(new DeathState());
			}
		}
	}

	private class DeathState : DeskPlantBaseState
	{
		protected override bool showSeedObject => false;

		protected override bool showPlantObject => true;

		protected override float CalcScale()
		{
			float num = Mathf.Clamp01(base.age / scaleDuration);
			return linearRampDown.Evaluate(num) * easeDown.Evaluate(num);
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (NetworkServer.active && base.fixedAge >= scaleDuration)
			{
				EntityState.Destroy((Object)(object)base.gameObject);
			}
		}
	}

	public GameObject plantObject;

	public GameObject seedObject;

	public GameObject groundFX;

	public float healingRadius;

	public float radiusIncreasePerStack = 5f;

	private static readonly float seedDuration = 5f;

	private static readonly float mainDuration = 10f;

	private static readonly float scaleDuration = 0.5f;

	private static readonly AnimationCurve linearRampUp = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	private static readonly AnimationCurve linearRampDown = AnimationCurve.Linear(0f, 1f, 1f, 0f);

	private static readonly AnimationCurve easeUp = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	private static readonly AnimationCurve easeDown = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

	public TeamFilter teamFilter { get; private set; }

	public int itemCount { get; set; }

	private void Awake()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		teamFilter = ((Component)this).GetComponent<TeamFilter>();
		plantObject.SetActive(false);
		seedObject.SetActive(false);
		RaycastHit val = default(RaycastHit);
		if (NetworkServer.active && Physics.Raycast(((Component)this).transform.position, Vector3.down, ref val, 500f, LayerMask.op_Implicit(LayerIndex.world.mask)))
		{
			((Component)this).transform.position = ((RaycastHit)(ref val)).point;
			((Component)this).transform.up = ((RaycastHit)(ref val)).normal;
		}
	}

	public void SeedPlanting()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		EffectManager.SimpleEffect(groundFX, ((Component)this).transform.position, Quaternion.identity, transmit: false);
	}
}
