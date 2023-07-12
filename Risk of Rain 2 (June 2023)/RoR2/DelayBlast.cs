using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

[RequireComponent(typeof(TeamFilter))]
public class DelayBlast : MonoBehaviour
{
	[HideInInspector]
	public Vector3 position;

	[HideInInspector]
	public GameObject attacker;

	[HideInInspector]
	public GameObject inflictor;

	[HideInInspector]
	public float baseDamage;

	[HideInInspector]
	public bool crit;

	[HideInInspector]
	public float baseForce;

	[HideInInspector]
	public float radius;

	[HideInInspector]
	public Vector3 bonusForce;

	[HideInInspector]
	public float maxTimer;

	[HideInInspector]
	public DamageColorIndex damageColorIndex;

	[HideInInspector]
	public BlastAttack.FalloffModel falloffModel;

	[HideInInspector]
	public DamageType damageType;

	[HideInInspector]
	public float procCoefficient = 1f;

	public GameObject explosionEffect;

	public GameObject delayEffect;

	public float timerStagger;

	private float timer;

	private bool hasSpawnedDelayEffect;

	private TeamFilter teamFilter;

	private void Awake()
	{
		teamFilter = ((Component)this).GetComponent<TeamFilter>();
		if (!NetworkServer.active)
		{
			((Behaviour)this).enabled = false;
		}
	}

	private void FixedUpdate()
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		if (NetworkServer.active)
		{
			timer += Time.fixedDeltaTime;
			if (Object.op_Implicit((Object)(object)delayEffect) && !hasSpawnedDelayEffect && timer > timerStagger)
			{
				hasSpawnedDelayEffect = true;
				EffectManager.SpawnEffect(delayEffect, new EffectData
				{
					origin = ((Component)this).transform.position,
					rotation = Util.QuaternionSafeLookRotation(((Component)this).transform.forward),
					scale = radius
				}, transmit: true);
			}
			if (timer >= maxTimer + timerStagger)
			{
				Detonate();
			}
		}
	}

	public void Detonate()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		EffectManager.SpawnEffect(explosionEffect, new EffectData
		{
			origin = ((Component)this).transform.position,
			rotation = Util.QuaternionSafeLookRotation(((Component)this).transform.forward),
			scale = radius
		}, transmit: true);
		BlastAttack blastAttack = new BlastAttack();
		blastAttack.position = position;
		blastAttack.baseDamage = baseDamage;
		blastAttack.baseForce = baseForce;
		blastAttack.bonusForce = bonusForce;
		blastAttack.radius = radius;
		blastAttack.attacker = attacker;
		blastAttack.inflictor = inflictor;
		blastAttack.teamIndex = teamFilter.teamIndex;
		blastAttack.crit = crit;
		blastAttack.damageColorIndex = damageColorIndex;
		blastAttack.damageType = damageType;
		blastAttack.falloffModel = falloffModel;
		blastAttack.procCoefficient = procCoefficient;
		blastAttack.Fire();
		Object.Destroy((Object)(object)((Component)this).gameObject);
	}
}
