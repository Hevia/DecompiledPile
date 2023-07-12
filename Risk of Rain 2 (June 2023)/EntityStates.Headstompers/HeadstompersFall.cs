using System;
using System.Linq;
using RoR2;
using UnityEngine;

namespace EntityStates.Headstompers;

public class HeadstompersFall : BaseHeadstompersState
{
	private float stopwatch;

	public static float maxFallDuration = 0f;

	public static float maxFallSpeed = 30f;

	public static float maxDistance = 30f;

	public static float initialFallSpeed = 10f;

	public static float accelerationY = 40f;

	public static float minimumRadius = 5f;

	public static float maximumRadius = 100f;

	public static float minimumDamageCoefficient = 10f;

	public static float maximumDamageCoefficient = 100f;

	public static float seekCone = 20f;

	public static float springboardSpeed = 30f;

	private Transform seekTransform;

	private bool seekLost;

	private CharacterMotor onHitGroundProvider;

	private float initialY;

	public override void OnEnter()
	{
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		if (!base.isAuthority)
		{
			return;
		}
		if (Object.op_Implicit((Object)(object)body))
		{
			TeamMask allButNeutral = TeamMask.allButNeutral;
			TeamIndex objectTeam = TeamComponent.GetObjectTeam(bodyGameObject);
			if (objectTeam != TeamIndex.None)
			{
				allButNeutral.RemoveTeam(objectTeam);
			}
			BullseyeSearch obj = new BullseyeSearch
			{
				filterByLoS = true,
				maxDistanceFilter = 300f,
				maxAngleFilter = seekCone,
				searchOrigin = body.footPosition,
				searchDirection = Vector3.down,
				sortMode = BullseyeSearch.SortMode.Angle,
				teamMaskFilter = allButNeutral,
				viewer = body
			};
			initialY = body.footPosition.y;
			obj.RefreshCandidates();
			HurtBox hurtBox = obj.GetResults().FirstOrDefault();
			seekTransform = ((hurtBox != null) ? ((Component)hurtBox).transform : null);
		}
		SetOnHitGroundProviderAuthority(bodyMotor);
		if (Object.op_Implicit((Object)(object)bodyMotor))
		{
			bodyMotor.velocity.y = Mathf.Min(bodyMotor.velocity.y, 0f - initialFallSpeed);
		}
	}

	private void SetOnHitGroundProviderAuthority(CharacterMotor newOnHitGroundProvider)
	{
		if (onHitGroundProvider != null)
		{
			onHitGroundProvider.onHitGroundAuthority -= OnMotorHitGroundAuthority;
		}
		onHitGroundProvider = newOnHitGroundProvider;
		if (onHitGroundProvider != null)
		{
			onHitGroundProvider.onHitGroundAuthority += OnMotorHitGroundAuthority;
		}
	}

	public override void OnExit()
	{
		SetOnHitGroundProviderAuthority(null);
		base.OnExit();
	}

	private void OnMotorHitGroundAuthority(ref CharacterMotor.HitGroundInfo hitGroundInfo)
	{
		DoStompExplosionAuthority();
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.isAuthority)
		{
			FixedUpdateAuthority();
		}
	}

	private void FixedUpdateAuthority()
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		stopwatch += Time.deltaTime;
		if (base.isGrounded)
		{
			DoStompExplosionAuthority();
		}
		else if (stopwatch >= maxFallDuration)
		{
			outer.SetNextState(new HeadstompersCooldown());
		}
		else
		{
			if (!Object.op_Implicit((Object)(object)bodyMotor))
			{
				return;
			}
			Vector3 velocity = bodyMotor.velocity;
			if (velocity.y > 0f - maxFallSpeed)
			{
				velocity.y = Mathf.MoveTowards(velocity.y, 0f - maxFallSpeed, accelerationY * Time.deltaTime);
			}
			if (Object.op_Implicit((Object)(object)seekTransform) && !seekLost)
			{
				Vector3 val = seekTransform.position - body.footPosition;
				Vector3 normalized = ((Vector3)(ref val)).normalized;
				if (Vector3.Dot(Vector3.down, normalized) >= Mathf.Cos(seekCone * (MathF.PI / 180f)))
				{
					if (velocity.y < 0f)
					{
						Vector3 val2 = normalized * (0f - velocity.y);
						val2.y = 0f;
						Vector3 val3 = velocity;
						val3.y = 0f;
						val3 = val2;
						velocity.x = val3.x;
						velocity.z = val3.z;
					}
				}
				else
				{
					seekLost = true;
				}
			}
			bodyMotor.velocity = velocity;
		}
	}

	private void DoStompExplosionAuthority()
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)body))
		{
			Inventory inventory = body.inventory;
			if (((!Object.op_Implicit((Object)(object)inventory) || inventory.GetItemCount(RoR2Content.Items.FallBoots) != 0) ? 1 : 0) > (false ? 1 : 0))
			{
				bodyMotor.velocity = Vector3.zero;
				float num = Mathf.Max(0f, initialY - body.footPosition.y);
				if (num > 0f)
				{
					Debug.Log((object)$"Fallboots distance: {num}");
					float num2 = Mathf.InverseLerp(0f, maxDistance, num);
					float num3 = Mathf.Lerp(minimumDamageCoefficient, maximumDamageCoefficient, num2);
					float num4 = Mathf.Lerp(minimumRadius, maximumRadius, num2);
					BlastAttack obj = new BlastAttack
					{
						attacker = ((Component)body).gameObject,
						inflictor = ((Component)body).gameObject
					};
					obj.teamIndex = TeamComponent.GetObjectTeam(obj.attacker);
					obj.position = body.footPosition;
					obj.procCoefficient = 1f;
					obj.radius = num4;
					obj.baseForce = 200f * num3;
					obj.bonusForce = Vector3.up * 2000f;
					obj.baseDamage = body.damage * num3;
					obj.falloffModel = BlastAttack.FalloffModel.SweetSpot;
					obj.crit = Util.CheckRoll(body.crit, body.master);
					obj.damageColorIndex = DamageColorIndex.Item;
					obj.attackerFiltering = AttackerFiltering.NeverHitSelf;
					obj.Fire();
					EffectData effectData = new EffectData();
					effectData.origin = body.footPosition;
					effectData.scale = num4;
					EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/BootShockwave"), effectData, transmit: true);
				}
			}
		}
		SetOnHitGroundProviderAuthority(null);
		outer.SetNextState(new HeadstompersCooldown());
	}
}
