using System;
using HG;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2.Projectile;

[RequireComponent(typeof(ProjectileController))]
public class ProjectileExplosion : MonoBehaviour
{
	protected ProjectileController projectileController;

	protected ProjectileDamage projectileDamage;

	protected bool alive = true;

	[Header("Main Properties")]
	public BlastAttack.FalloffModel falloffModel = BlastAttack.FalloffModel.Linear;

	public float blastRadius;

	[Tooltip("The percentage of the damage, proc coefficient, and force of the initial projectile. Ranges from 0-1")]
	public float blastDamageCoefficient;

	public float blastProcCoefficient = 1f;

	public AttackerFiltering blastAttackerFiltering;

	public Vector3 bonusBlastForce;

	public bool canRejectForce = true;

	public HealthComponent projectileHealthComponent;

	public GameObject explosionEffect;

	[Obsolete("This sound will not play over the network. Provide the sound via the prefab referenced by explosionEffect instead.", false)]
	[ShowFieldObsolete]
	[Tooltip("This sound will not play over the network. Provide the sound via the prefab referenced by explosionEffect instead.")]
	public string explosionSoundString;

	[Header("Child Properties")]
	[Tooltip("Does this projectile release children on death?")]
	public bool fireChildren;

	public GameObject childrenProjectilePrefab;

	public int childrenCount;

	[Tooltip("What percentage of our damage does the children get?")]
	public float childrenDamageCoefficient;

	[ShowFieldObsolete]
	[Tooltip("How to randomize the orientation of children")]
	public Vector3 minAngleOffset;

	[ShowFieldObsolete]
	public Vector3 maxAngleOffset;

	public float minRollDegrees;

	public float rangeRollDegrees;

	public float minPitchDegrees;

	public float rangePitchDegrees;

	[Tooltip("useLocalSpaceForChildren is unused by ProjectileImpactExplosion")]
	public bool useLocalSpaceForChildren;

	[Header("DoT Properties")]
	[Tooltip("If true, applies a DoT given the following properties")]
	public bool applyDot;

	public DotController.DotIndex dotIndex = DotController.DotIndex.None;

	[Tooltip("Duration in seconds of the DoT.  Unused if calculateTotalDamage is true.")]
	public float dotDuration;

	[Tooltip("Multiplier on the per-tick damage")]
	public float dotDamageMultiplier = 1f;

	[Tooltip("If true, we cap the numer of DoT stacks for this attacker.")]
	public bool applyMaxStacksFromAttacker;

	[Tooltip("The maximum number of stacks that we can apply for this attacker")]
	public uint maxStacksFromAttacker = uint.MaxValue;

	[Tooltip("If true, we disregard the duration and instead specify the total damage.")]
	public bool calculateTotalDamage;

	[Tooltip("totalDamage = totalDamageMultiplier * attacker's damage")]
	public float totalDamageMultiplier;

	protected virtual void Awake()
	{
		projectileController = ((Component)this).GetComponent<ProjectileController>();
		projectileDamage = ((Component)this).GetComponent<ProjectileDamage>();
	}

	public void Detonate()
	{
		if (NetworkServer.active)
		{
			DetonateServer();
		}
		Object.Destroy((Object)(object)((Component)this).gameObject);
	}

	protected void DetonateServer()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)explosionEffect))
		{
			EffectManager.SpawnEffect(explosionEffect, new EffectData
			{
				origin = ((Component)this).transform.position,
				scale = blastRadius
			}, transmit: true);
		}
		if (Object.op_Implicit((Object)(object)projectileDamage))
		{
			BlastAttack blastAttack = new BlastAttack();
			blastAttack.position = ((Component)this).transform.position;
			blastAttack.baseDamage = projectileDamage.damage * blastDamageCoefficient;
			blastAttack.baseForce = projectileDamage.force * blastDamageCoefficient;
			blastAttack.radius = blastRadius;
			blastAttack.attacker = (Object.op_Implicit((Object)(object)projectileController.owner) ? projectileController.owner.gameObject : null);
			blastAttack.inflictor = ((Component)this).gameObject;
			blastAttack.teamIndex = projectileController.teamFilter.teamIndex;
			blastAttack.crit = projectileDamage.crit;
			blastAttack.procChainMask = projectileController.procChainMask;
			blastAttack.procCoefficient = projectileController.procCoefficient * blastProcCoefficient;
			blastAttack.bonusForce = bonusBlastForce;
			blastAttack.falloffModel = falloffModel;
			blastAttack.damageColorIndex = projectileDamage.damageColorIndex;
			blastAttack.damageType = projectileDamage.damageType;
			blastAttack.attackerFiltering = blastAttackerFiltering;
			blastAttack.canRejectForce = canRejectForce;
			BlastAttack.Result result = blastAttack.Fire();
			OnBlastAttackResult(blastAttack, result);
		}
		if (explosionSoundString.Length > 0)
		{
			Util.PlaySound(explosionSoundString, ((Component)this).gameObject);
		}
		if (fireChildren)
		{
			for (int i = 0; i < childrenCount; i++)
			{
				FireChild();
			}
		}
	}

	protected Quaternion GetRandomChildRollPitch()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		Quaternion val = Quaternion.AngleAxis(minRollDegrees + Random.Range(0f, rangeRollDegrees), Vector3.forward);
		Quaternion val2 = Quaternion.AngleAxis(minPitchDegrees + Random.Range(0f, rangePitchDegrees), Vector3.left);
		return val * val2;
	}

	protected virtual Quaternion GetRandomDirectionForChild()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		Quaternion randomChildRollPitch = GetRandomChildRollPitch();
		if (useLocalSpaceForChildren)
		{
			return ((Component)this).transform.rotation * randomChildRollPitch;
		}
		return randomChildRollPitch;
	}

	protected void FireChild()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		Quaternion randomDirectionForChild = GetRandomDirectionForChild();
		GameObject obj = Object.Instantiate<GameObject>(childrenProjectilePrefab, ((Component)this).transform.position, randomDirectionForChild);
		ProjectileController component = obj.GetComponent<ProjectileController>();
		if (Object.op_Implicit((Object)(object)component))
		{
			component.procChainMask = projectileController.procChainMask;
			component.procCoefficient = projectileController.procCoefficient;
			component.Networkowner = projectileController.owner;
		}
		obj.GetComponent<TeamFilter>().teamIndex = ((Component)this).GetComponent<TeamFilter>().teamIndex;
		ProjectileDamage component2 = obj.GetComponent<ProjectileDamage>();
		if (Object.op_Implicit((Object)(object)component2))
		{
			component2.damage = projectileDamage.damage * childrenDamageCoefficient;
			component2.crit = projectileDamage.crit;
			component2.force = projectileDamage.force;
			component2.damageColorIndex = projectileDamage.damageColorIndex;
		}
		NetworkServer.Spawn(obj);
	}

	public void SetExplosionRadius(float newRadius)
	{
		blastRadius = newRadius;
	}

	public void SetAlive(bool newAlive)
	{
		alive = newAlive;
	}

	public bool GetAlive()
	{
		if (!NetworkServer.active)
		{
			Debug.Log((object)"Cannot get alive state. Returning false.");
			return false;
		}
		return alive;
	}

	protected virtual void OnValidate()
	{
		if (!Application.IsPlaying((Object)(object)this) && !string.IsNullOrEmpty(explosionSoundString))
		{
			Debug.LogWarningFormat((Object)(object)((Component)this).gameObject, "{0} ProjectileImpactExplosion component supplies a value in the explosionSoundString field. This will not play correctly over the network. Please move the sound to the explosion effect.", new object[1] { Util.GetGameObjectHierarchyName(((Component)this).gameObject) });
		}
	}

	protected virtual void OnBlastAttackResult(BlastAttack blastAttack, BlastAttack.Result result)
	{
		if (!applyDot)
		{
			return;
		}
		GameObject attacker = blastAttack.attacker;
		CharacterBody characterBody = ((attacker != null) ? attacker.GetComponent<CharacterBody>() : null);
		BlastAttack.HitPoint[] hitPoints = result.hitPoints;
		for (int i = 0; i < hitPoints.Length; i++)
		{
			BlastAttack.HitPoint hitPoint = hitPoints[i];
			if (Object.op_Implicit((Object)(object)hitPoint.hurtBox) && Object.op_Implicit((Object)(object)hitPoint.hurtBox.healthComponent))
			{
				InflictDotInfo inflictDotInfo = default(InflictDotInfo);
				inflictDotInfo.victimObject = ((Component)hitPoint.hurtBox.healthComponent).gameObject;
				inflictDotInfo.attackerObject = blastAttack.attacker;
				inflictDotInfo.dotIndex = dotIndex;
				inflictDotInfo.damageMultiplier = dotDamageMultiplier;
				InflictDotInfo dotInfo = inflictDotInfo;
				if (calculateTotalDamage && Object.op_Implicit((Object)(object)characterBody))
				{
					dotInfo.totalDamage = characterBody.damage * totalDamageMultiplier;
				}
				else
				{
					dotInfo.duration = dotDuration;
				}
				if (applyMaxStacksFromAttacker)
				{
					dotInfo.maxStacksFromAttacker = maxStacksFromAttacker;
				}
				if (Object.op_Implicit((Object)(object)characterBody) && Object.op_Implicit((Object)(object)characterBody.inventory))
				{
					StrengthenBurnUtils.CheckDotForUpgrade(characterBody.inventory, ref dotInfo);
				}
				DotController.InflictDot(ref dotInfo);
			}
		}
	}
}
