using System;
using System.Collections.Generic;
using System.Linq;
using RoR2.Navigation;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace RoR2;

public class MeteorStormController : MonoBehaviour
{
	private class Meteor
	{
		public Vector3 impactPosition;

		public float startTime;

		public bool didTravelEffect;

		public bool valid = true;
	}

	private class MeteorWave
	{
		private readonly CharacterBody[] targets;

		private int currentStep;

		private float hitChance = 0.4f;

		private readonly Vector3 center;

		public float timer;

		private NodeGraphSpider nodeGraphSpider;

		public MeteorWave(CharacterBody[] targets, Vector3 center)
		{
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			this.targets = new CharacterBody[targets.Length];
			targets.CopyTo(this.targets, 0);
			Util.ShuffleArray(targets);
			this.center = center;
			nodeGraphSpider = new NodeGraphSpider(SceneInfo.instance.groundNodes, HullMask.Human);
			nodeGraphSpider.AddNodeForNextStep(SceneInfo.instance.groundNodes.FindClosestNode(center, HullClassification.Human));
			int i = 0;
			for (int num = 20; i < num; i++)
			{
				if (!nodeGraphSpider.PerformStep())
				{
					break;
				}
			}
		}

		public Meteor GetNextMeteor()
		{
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_008a: Unknown result type (might be due to invalid IL or missing references)
			//IL_009f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00af: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00de: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
			if (currentStep >= targets.Length)
			{
				return null;
			}
			CharacterBody characterBody = targets[currentStep];
			Meteor meteor = new Meteor();
			if (Object.op_Implicit((Object)(object)characterBody) && Random.value < hitChance)
			{
				meteor.impactPosition = characterBody.corePosition;
				Vector3 val = meteor.impactPosition + Vector3.up * 6f;
				Vector3 onUnitSphere = Random.onUnitSphere;
				onUnitSphere.y = -1f;
				RaycastHit val2 = default(RaycastHit);
				if (Physics.Raycast(val, onUnitSphere, ref val2, 12f, LayerMask.op_Implicit(LayerIndex.world.mask), (QueryTriggerInteraction)1))
				{
					meteor.impactPosition = ((RaycastHit)(ref val2)).point;
				}
				else if (Physics.Raycast(meteor.impactPosition, Vector3.down, ref val2, float.PositiveInfinity, LayerMask.op_Implicit(LayerIndex.world.mask), (QueryTriggerInteraction)1))
				{
					meteor.impactPosition = ((RaycastHit)(ref val2)).point;
				}
			}
			else if (nodeGraphSpider.collectedSteps.Count != 0)
			{
				int index = Random.Range(0, nodeGraphSpider.collectedSteps.Count);
				SceneInfo.instance.groundNodes.GetNodePosition(nodeGraphSpider.collectedSteps[index].node, out meteor.impactPosition);
			}
			else
			{
				meteor.valid = false;
			}
			meteor.startTime = Run.instance.time;
			currentStep++;
			return meteor;
		}
	}

	public int waveCount;

	public float waveMinInterval;

	public float waveMaxInterval;

	public GameObject warningEffectPrefab;

	public GameObject travelEffectPrefab;

	public float travelEffectDuration;

	public GameObject impactEffectPrefab;

	public float impactDelay;

	public float blastDamageCoefficient;

	public float blastRadius;

	public float blastForce;

	[NonSerialized]
	public GameObject owner;

	[NonSerialized]
	public float ownerDamage;

	[NonSerialized]
	public bool isCrit;

	public UnityEvent onDestroyEvents;

	private List<Meteor> meteorList;

	private List<MeteorWave> waveList;

	private int wavesPerformed;

	private float waveTimer;

	private void Start()
	{
		if (NetworkServer.active)
		{
			meteorList = new List<Meteor>();
			waveList = new List<MeteorWave>();
		}
	}

	private void FixedUpdate()
	{
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			return;
		}
		waveTimer -= Time.fixedDeltaTime;
		if (waveTimer <= 0f && wavesPerformed < waveCount)
		{
			wavesPerformed++;
			waveTimer = Random.Range(waveMinInterval, waveMaxInterval);
			MeteorWave item = new MeteorWave(CharacterBody.readOnlyInstancesList.ToArray(), ((Component)this).transform.position);
			waveList.Add(item);
		}
		for (int num = waveList.Count - 1; num >= 0; num--)
		{
			MeteorWave meteorWave = waveList[num];
			meteorWave.timer -= Time.fixedDeltaTime;
			if (meteorWave.timer <= 0f)
			{
				meteorWave.timer = Random.Range(0.05f, 1f);
				Meteor nextMeteor = meteorWave.GetNextMeteor();
				if (nextMeteor == null)
				{
					waveList.RemoveAt(num);
				}
				else if (nextMeteor.valid)
				{
					meteorList.Add(nextMeteor);
					EffectManager.SpawnEffect(warningEffectPrefab, new EffectData
					{
						origin = nextMeteor.impactPosition,
						scale = blastRadius
					}, transmit: true);
				}
			}
		}
		float num2 = Run.instance.time - impactDelay;
		float num3 = num2 - travelEffectDuration;
		for (int num4 = meteorList.Count - 1; num4 >= 0; num4--)
		{
			Meteor meteor = meteorList[num4];
			if (meteor.startTime < num3 && !meteor.didTravelEffect)
			{
				DoMeteorEffect(meteor);
			}
			if (meteor.startTime < num2)
			{
				meteorList.RemoveAt(num4);
				DetonateMeteor(meteor);
			}
		}
		if (wavesPerformed == waveCount && meteorList.Count == 0)
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
	}

	private void OnDestroy()
	{
		onDestroyEvents.Invoke();
	}

	private void DoMeteorEffect(Meteor meteor)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		meteor.didTravelEffect = true;
		if (Object.op_Implicit((Object)(object)travelEffectPrefab))
		{
			EffectManager.SpawnEffect(travelEffectPrefab, new EffectData
			{
				origin = meteor.impactPosition
			}, transmit: true);
		}
	}

	private void DetonateMeteor(Meteor meteor)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		EffectData effectData = new EffectData
		{
			origin = meteor.impactPosition
		};
		EffectManager.SpawnEffect(impactEffectPrefab, effectData, transmit: true);
		BlastAttack blastAttack = new BlastAttack();
		blastAttack.inflictor = ((Component)this).gameObject;
		blastAttack.baseDamage = blastDamageCoefficient * ownerDamage;
		blastAttack.baseForce = blastForce;
		blastAttack.attackerFiltering = AttackerFiltering.AlwaysHit;
		blastAttack.crit = isCrit;
		blastAttack.falloffModel = BlastAttack.FalloffModel.Linear;
		blastAttack.attacker = owner;
		blastAttack.bonusForce = Vector3.zero;
		blastAttack.damageColorIndex = DamageColorIndex.Item;
		blastAttack.position = meteor.impactPosition;
		blastAttack.procChainMask = default(ProcChainMask);
		blastAttack.procCoefficient = 1f;
		blastAttack.teamIndex = TeamIndex.None;
		blastAttack.radius = blastRadius;
		blastAttack.Fire();
	}
}
