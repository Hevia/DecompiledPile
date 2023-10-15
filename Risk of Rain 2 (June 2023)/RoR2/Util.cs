using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using Facepunch.Steamworks;
using JetBrains.Annotations;
using Rewired;
using RoR2.Networking;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace RoR2;

public static class Util
{
	private struct EasyTargetCandidate
	{
		public Transform transform;

		public float score;

		public float distance;
	}

	public const string attackSpeedRtpcName = "attackSpeed";

	private static readonly string strBackslash = "\\";

	private static readonly string strBackslashBackslash = strBackslash + strBackslash;

	private static readonly string strQuote = "\"";

	private static readonly string strBackslashQuote = strBackslash + strQuote;

	private static readonly Regex backlashSearch = new Regex(strBackslashBackslash);

	private static readonly Regex quoteSearch = new Regex(strQuote);

	public static readonly DateTime dateZero = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

	private static readonly StringBuilder sharedStringBuilder = new StringBuilder();

	private static readonly Stack<string> sharedStringStack = new Stack<string>();

	private static readonly string cloneString = "(Clone)";

	public static void CleanseBody(CharacterBody characterBody, bool removeDebuffs, bool removeBuffs, bool removeCooldownBuffs, bool removeDots, bool removeStun, bool removeNearbyProjectiles)
	{
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		if (removeDebuffs || removeBuffs || removeCooldownBuffs)
		{
			BuffIndex buffIndex = (BuffIndex)0;
			for (BuffIndex buffCount = (BuffIndex)BuffCatalog.buffCount; buffIndex < buffCount; buffIndex++)
			{
				BuffDef buffDef = BuffCatalog.GetBuffDef(buffIndex);
				if ((buffDef.isDebuff && removeDebuffs) || (buffDef.isCooldown && removeCooldownBuffs) || (!buffDef.isDebuff && !buffDef.isCooldown && removeBuffs))
				{
					characterBody.ClearTimedBuffs(buffIndex);
				}
			}
		}
		if (removeDots)
		{
			DotController.RemoveAllDots(((Component)characterBody).gameObject);
		}
		if (removeStun)
		{
			SetStateOnHurt component = ((Component)characterBody).GetComponent<SetStateOnHurt>();
			if (Object.op_Implicit((Object)(object)component))
			{
				component.Cleanse();
			}
		}
		if (!removeNearbyProjectiles)
		{
			return;
		}
		float num = 6f * 6f;
		TeamIndex teamIndex = characterBody.teamComponent.teamIndex;
		List<ProjectileController> instancesList = InstanceTracker.GetInstancesList<ProjectileController>();
		List<ProjectileController> list = new List<ProjectileController>();
		int i = 0;
		for (int count = instancesList.Count; i < count; i++)
		{
			ProjectileController projectileController = instancesList[i];
			if (!projectileController.cannotBeDeleted && projectileController.teamFilter.teamIndex != teamIndex)
			{
				Vector3 val = ((Component)projectileController).transform.position - characterBody.corePosition;
				if (((Vector3)(ref val)).sqrMagnitude < num)
				{
					list.Add(projectileController);
				}
			}
		}
		int j = 0;
		for (int count2 = list.Count; j < count2; j++)
		{
			ProjectileController projectileController2 = list[j];
			if (Object.op_Implicit((Object)(object)projectileController2))
			{
				Object.Destroy((Object)(object)((Component)projectileController2).gameObject);
			}
		}
	}

	public static WeightedSelection<DirectorCard> CreateReasonableDirectorCardSpawnList(float availableCredit, int maximumNumberToSpawnBeforeSkipping, int minimumToSpawn)
	{
		WeightedSelection<DirectorCard> monsterSelection = ClassicStageInfo.instance.monsterSelection;
		WeightedSelection<DirectorCard> weightedSelection = new WeightedSelection<DirectorCard>();
		for (int i = 0; i < monsterSelection.Count; i++)
		{
			DirectorCard value = monsterSelection.choices[i].value;
			float combatDirectorHighestEliteCostMultiplier = CombatDirector.CalcHighestEliteCostMultiplier(value.spawnCard.eliteRules);
			if (DirectorCardIsReasonableChoice(availableCredit, maximumNumberToSpawnBeforeSkipping, minimumToSpawn, value, combatDirectorHighestEliteCostMultiplier))
			{
				weightedSelection.AddChoice(value, monsterSelection.choices[i].weight);
			}
		}
		return weightedSelection;
	}

	public static bool DirectorCardIsReasonableChoice(float availableCredit, int maximumNumberToSpawnBeforeSkipping, int minimumToSpawn, DirectorCard card, float combatDirectorHighestEliteCostMultiplier)
	{
		float num = (float)(card.cost * maximumNumberToSpawnBeforeSkipping) * ((card.spawnCard as CharacterSpawnCard).noElites ? 1f : combatDirectorHighestEliteCostMultiplier);
		if (card.IsAvailable() && (float)card.cost * (float)minimumToSpawn <= availableCredit)
		{
			return num / 2f > availableCredit;
		}
		return false;
	}

	public static CharacterBody HurtBoxColliderToBody(Collider collider)
	{
		HurtBox component = ((Component)collider).GetComponent<HurtBox>();
		if (Object.op_Implicit((Object)(object)component) && Object.op_Implicit((Object)(object)component.healthComponent))
		{
			return component.healthComponent.body;
		}
		return null;
	}

	public static float ConvertAmplificationPercentageIntoReductionPercentage(float amplificationPercentage)
	{
		return (1f - 100f / (100f + amplificationPercentage)) * 100f;
	}

	public static Vector3 ClosestPointOnLine(Vector3 vA, Vector3 vB, Vector3 vPoint)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = vPoint - vA;
		Vector3 val2 = vB - vA;
		Vector3 normalized = ((Vector3)(ref val2)).normalized;
		float num = Vector3.Distance(vA, vB);
		float num2 = Vector3.Dot(normalized, val);
		if (num2 <= 0f)
		{
			return vA;
		}
		if (num2 >= num)
		{
			return vB;
		}
		Vector3 val3 = normalized * num2;
		return vA + val3;
	}

	public static CharacterBody TryToCreateGhost(CharacterBody targetBody, CharacterBody ownerBody, int duration)
	{
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			Debug.LogWarning((object)"[Server] function 'RoR2.CharacterBody RoR2.Util::TryToCreateGhost(RoR2.CharacterBody, RoR2.CharacterBody, int)' called on client");
			return null;
		}
		if (!Object.op_Implicit((Object)(object)targetBody))
		{
			return null;
		}
		GameObject bodyPrefab = BodyCatalog.FindBodyPrefab(targetBody);
		if (!Object.op_Implicit((Object)(object)bodyPrefab))
		{
			return null;
		}
		CharacterMaster characterMaster = MasterCatalog.allAiMasters.FirstOrDefault((CharacterMaster master) => (Object)(object)master.bodyPrefab == (Object)(object)bodyPrefab);
		if (!Object.op_Implicit((Object)(object)characterMaster))
		{
			return null;
		}
		MasterSummon obj = new MasterSummon
		{
			masterPrefab = ((Component)characterMaster).gameObject,
			ignoreTeamMemberLimit = false,
			position = targetBody.footPosition
		};
		CharacterDirection component = ((Component)targetBody).GetComponent<CharacterDirection>();
		obj.rotation = (Object.op_Implicit((Object)(object)component) ? Quaternion.Euler(0f, component.yaw, 0f) : ((Component)targetBody).transform.rotation);
		obj.summonerBodyObject = (Object.op_Implicit((Object)(object)ownerBody) ? ((Component)ownerBody).gameObject : null);
		obj.inventoryToCopy = targetBody.inventory;
		obj.useAmbientLevel = null;
		obj.preSpawnSetupCallback = (Action<CharacterMaster>)Delegate.Combine(obj.preSpawnSetupCallback, new Action<CharacterMaster>(PreSpawnSetup));
		CharacterMaster characterMaster2 = obj.Perform();
		if (!Object.op_Implicit((Object)(object)characterMaster2))
		{
			return null;
		}
		CharacterBody body = characterMaster2.GetBody();
		if (Object.op_Implicit((Object)(object)body))
		{
			EntityStateMachine[] components = ((Component)body).GetComponents<EntityStateMachine>();
			foreach (EntityStateMachine obj2 in components)
			{
				obj2.initialStateType = obj2.mainStateType;
			}
		}
		return body;
		void PreSpawnSetup(CharacterMaster newMaster)
		{
			newMaster.inventory.GiveItem(RoR2Content.Items.Ghost);
			newMaster.inventory.GiveItem(RoR2Content.Items.HealthDecay, duration);
			newMaster.inventory.GiveItem(RoR2Content.Items.BoostDamage, 150);
		}
	}

	public static float OnHitProcDamage(float damageThatProccedIt, float damageStat, float damageCoefficient)
	{
		float num = damageThatProccedIt * damageCoefficient;
		return Mathf.Max(1f, num);
	}

	public static float OnKillProcDamage(float baseDamage, float damageCoefficient)
	{
		return baseDamage * damageCoefficient;
	}

	public static Quaternion QuaternionSafeLookRotation(Vector3 forward)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		Quaternion result = Quaternion.identity;
		if (((Vector3)(ref forward)).sqrMagnitude > Mathf.Epsilon)
		{
			result = Quaternion.LookRotation(forward);
		}
		return result;
	}

	public static Quaternion QuaternionSafeLookRotation(Vector3 forward, Vector3 upwards)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		Quaternion result = Quaternion.identity;
		if (((Vector3)(ref forward)).sqrMagnitude > Mathf.Epsilon)
		{
			result = Quaternion.LookRotation(forward, upwards);
		}
		return result;
	}

	public static bool HasParameterOfType(Animator animator, string name, AnimatorControllerParameterType type)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		AnimatorControllerParameter[] parameters = animator.parameters;
		foreach (AnimatorControllerParameter val in parameters)
		{
			if (val.type == type && val.name == name)
			{
				return true;
			}
		}
		return false;
	}

	public static uint PlaySound(string soundString, GameObject gameObject)
	{
		if (string.IsNullOrEmpty(soundString))
		{
			return 0u;
		}
		return AkSoundEngine.PostEvent(soundString, gameObject);
	}

	public static uint PlaySound(string soundString, GameObject gameObject, string RTPCstring, float RTPCvalue)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		uint num = PlaySound(soundString, gameObject);
		if (num != 0)
		{
			AkSoundEngine.SetRTPCValueByPlayingID(RTPCstring, RTPCvalue, num);
		}
		return num;
	}

	public static uint PlayAttackSpeedSound(string soundString, GameObject gameObject, float attackSpeedStat)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		uint num = PlaySound(soundString, gameObject);
		if (num != 0)
		{
			float num2 = CalculateAttackSpeedRtpcValue(attackSpeedStat);
			AkSoundEngine.SetRTPCValueByPlayingID("attackSpeed", num2, num);
		}
		return num;
	}

	public static float CalculateAttackSpeedRtpcValue(float attackSpeedStat)
	{
		float num = Mathf.Log(attackSpeedStat, 2f);
		return 1200f * num / 96f + 50f;
	}

	public static void RotateAwayFromWalls(float raycastLength, int raycastCount, Vector3 raycastOrigin, Transform referenceTransform)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		float num = 360f / (float)raycastCount;
		float num2 = 0f;
		float num3 = 0f;
		RaycastHit val2 = default(RaycastHit);
		for (int i = 0; i < raycastCount; i++)
		{
			Vector3 val = Quaternion.Euler(0f, num * (float)i, 0f) * Vector3.forward;
			float num4 = raycastLength;
			if (Physics.Raycast(raycastOrigin, val, ref val2, raycastLength, LayerMask.op_Implicit(LayerIndex.world.mask)))
			{
				num4 = ((RaycastHit)(ref val2)).distance;
			}
			if (((RaycastHit)(ref val2)).distance > num3)
			{
				num2 = num * (float)i;
				num3 = num4;
			}
		}
		referenceTransform.Rotate(Vector3.up, num2, (Space)1);
	}

	public static string GetActionDisplayString(ActionElementMap actionElementMap)
	{
		if (actionElementMap == null)
		{
			return "";
		}
		return actionElementMap.elementIdentifierName switch
		{
			"Left Mouse Button" => "M1", 
			"Right Mouse Button" => "M2", 
			"Left Shift" => "Shift", 
			_ => actionElementMap.elementIdentifierName, 
		};
	}

	public static float AngleSigned(Vector3 v1, Vector3 v2, Vector3 n)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		return Mathf.Atan2(Vector3.Dot(n, Vector3.Cross(v1, v2)), Vector3.Dot(v1, v2)) * 57.29578f;
	}

	public static float Remap(float value, float inMin, float inMax, float outMin, float outMax)
	{
		return outMin + (value - inMin) / (inMax - inMin) * (outMax - outMin);
	}

	public static bool HasAnimationParameter(string paramName, Animator animator)
	{
		AnimatorControllerParameter[] parameters = animator.parameters;
		for (int i = 0; i < parameters.Length; i++)
		{
			if (parameters[i].name == paramName)
			{
				return true;
			}
		}
		return false;
	}

	public static bool HasAnimationParameter(int paramHash, Animator animator)
	{
		int i = 0;
		for (int parameterCount = animator.parameterCount; i < parameterCount; i++)
		{
			if (animator.GetParameter(i).nameHash == paramHash)
			{
				return true;
			}
		}
		return false;
	}

	public static bool CheckRoll(float percentChance, float luck = 0f, CharacterMaster effectOriginMaster = null)
	{
		if (percentChance <= 0f)
		{
			return false;
		}
		int num = Mathf.CeilToInt(Mathf.Abs(luck));
		float num2 = Random.Range(0f, 100f);
		float num3 = num2;
		for (int i = 0; i < num; i++)
		{
			float num4 = Random.Range(0f, 100f);
			num2 = ((luck > 0f) ? Mathf.Min(num2, num4) : Mathf.Max(num2, num4));
		}
		if (num2 <= percentChance)
		{
			if (num3 > percentChance && Object.op_Implicit((Object)(object)effectOriginMaster))
			{
				GameObject bodyObject = effectOriginMaster.GetBodyObject();
				if (Object.op_Implicit((Object)(object)bodyObject))
				{
					CharacterBody component = bodyObject.GetComponent<CharacterBody>();
					if (Object.op_Implicit((Object)(object)component))
					{
						component.wasLucky = true;
					}
				}
			}
			return true;
		}
		return false;
	}

	public static bool CheckRoll(float percentChance, CharacterMaster master)
	{
		return CheckRoll(percentChance, Object.op_Implicit((Object)(object)master) ? master.luck : 0f, master);
	}

	public static float EstimateSurfaceDistance(Collider a, Collider b)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		Bounds bounds = a.bounds;
		Vector3 center = ((Bounds)(ref bounds)).center;
		bounds = b.bounds;
		Vector3 center2 = ((Bounds)(ref bounds)).center;
		RaycastHit val = default(RaycastHit);
		Vector3 val2 = ((!b.Raycast(new Ray(center, center2 - center), ref val, float.PositiveInfinity)) ? b.ClosestPointOnBounds(center) : ((RaycastHit)(ref val)).point);
		Vector3 val3 = ((!a.Raycast(new Ray(center2, center - center2), ref val, float.PositiveInfinity)) ? a.ClosestPointOnBounds(center2) : ((RaycastHit)(ref val)).point);
		return Vector3.Distance(val2, val3);
	}

	public static bool HasEffectiveAuthority(GameObject gameObject)
	{
		if (Object.op_Implicit((Object)(object)gameObject))
		{
			return HasEffectiveAuthority(gameObject.GetComponent<NetworkIdentity>());
		}
		return false;
	}

	public static bool HasEffectiveAuthority(NetworkIdentity networkIdentity)
	{
		if (Object.op_Implicit((Object)(object)networkIdentity))
		{
			if (!networkIdentity.hasAuthority)
			{
				if (NetworkServer.active)
				{
					return networkIdentity.clientAuthorityOwner == null;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	public static float CalculateSphereVolume(float radius)
	{
		return 4.1887903f * radius * radius * radius;
	}

	public static float CalculateCylinderVolume(float radius, float height)
	{
		return MathF.PI * radius * radius * height;
	}

	public static float CalculateColliderVolume(Collider collider)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		Vector3 lossyScale = ((Component)collider).transform.lossyScale;
		float num = lossyScale.x * lossyScale.y * lossyScale.z;
		float num2 = 0f;
		if (collider is BoxCollider)
		{
			Vector3 size = ((BoxCollider)collider).size;
			num2 = size.x * size.y * size.z;
		}
		else if (collider is SphereCollider)
		{
			num2 = CalculateSphereVolume(((SphereCollider)collider).radius);
		}
		else if (collider is CapsuleCollider)
		{
			CapsuleCollider val = (CapsuleCollider)collider;
			float radius = val.radius;
			float num3 = CalculateSphereVolume(radius);
			float num4 = Mathf.Max(val.height - num3, 0f);
			float num5 = MathF.PI * radius * radius * num4;
			num2 = num3 + num5;
		}
		else if (collider is CharacterController)
		{
			CharacterController val2 = (CharacterController)collider;
			float radius2 = val2.radius;
			float num6 = CalculateSphereVolume(radius2);
			float num7 = Mathf.Max(val2.height - num6, 0f);
			float num8 = MathF.PI * radius2 * radius2 * num7;
			num2 = num6 + num8;
		}
		return num2 * num;
	}

	public static Vector3 RandomColliderVolumePoint(Collider collider)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Expected O, but got Unknown
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Expected O, but got Unknown
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_0217: Unknown result type (might be due to invalid IL or missing references)
		//IL_0220: Unknown result type (might be due to invalid IL or missing references)
		//IL_0230: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Unknown result type (might be due to invalid IL or missing references)
		//IL_0245: Unknown result type (might be due to invalid IL or missing references)
		//IL_0248: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0252: Unknown result type (might be due to invalid IL or missing references)
		//IL_0264: Unknown result type (might be due to invalid IL or missing references)
		//IL_026b: Expected O, but got Unknown
		//IL_02b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0317: Unknown result type (might be due to invalid IL or missing references)
		//IL_0320: Unknown result type (might be due to invalid IL or missing references)
		//IL_032c: Unknown result type (might be due to invalid IL or missing references)
		//IL_032f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0334: Unknown result type (might be due to invalid IL or missing references)
		//IL_0339: Unknown result type (might be due to invalid IL or missing references)
		//IL_033b: Unknown result type (might be due to invalid IL or missing references)
		//IL_033c: Unknown result type (might be due to invalid IL or missing references)
		Transform transform = ((Component)collider).transform;
		Vector3 val = Vector3.zero;
		if (collider is BoxCollider)
		{
			BoxCollider val2 = (BoxCollider)collider;
			Vector3 size = val2.size;
			Vector3 center = val2.center;
			((Vector3)(ref val))._002Ector(center.x + Random.Range(size.x * -0.5f, size.x * 0.5f), center.y + Random.Range(size.y * -0.5f, size.y * 0.5f), center.z + Random.Range(size.z * -0.5f, size.z * 0.5f));
		}
		else if (collider is SphereCollider)
		{
			SphereCollider val3 = (SphereCollider)collider;
			val = val3.center + Random.insideUnitSphere * val3.radius;
		}
		else if (collider is CapsuleCollider)
		{
			CapsuleCollider val4 = (CapsuleCollider)collider;
			float radius = val4.radius;
			float num = Mathf.Max(val4.height - radius, 0f);
			float num2 = CalculateSphereVolume(radius);
			float num3 = CalculateCylinderVolume(radius, num);
			float num4 = num2 + num3;
			if (Random.Range(0f, num4) <= num2)
			{
				val = Random.insideUnitSphere * radius;
				float num5 = ((float)Random.Range(0, 2) * 2f - 1f) * num * 0.5f;
				switch (val4.direction)
				{
				case 0:
					val.x += num5;
					break;
				case 1:
					val.y += num5;
					break;
				case 2:
					val.z += num5;
					break;
				}
			}
			else
			{
				Vector2 val5 = Random.insideUnitCircle * radius;
				float num6 = Random.Range(num * -0.5f, num * 0.5f);
				switch (val4.direction)
				{
				case 0:
					((Vector3)(ref val))._002Ector(num6, val5.x, val5.y);
					break;
				case 1:
					((Vector3)(ref val))._002Ector(val5.x, num6, val5.y);
					break;
				case 2:
					((Vector3)(ref val))._002Ector(val5.x, val5.y, num6);
					break;
				}
			}
			val += val4.center;
		}
		else if (collider is CharacterController)
		{
			CharacterController val6 = (CharacterController)collider;
			float radius2 = val6.radius;
			float num7 = Mathf.Max(val6.height - radius2, 0f);
			float num8 = CalculateSphereVolume(radius2);
			float num9 = CalculateCylinderVolume(radius2, num7);
			float num10 = num8 + num9;
			if (Random.Range(0f, num10) <= num8)
			{
				val = Random.insideUnitSphere * radius2;
				float num11 = ((float)Random.Range(0, 2) * 2f - 1f) * num7 * 0.5f;
				val.y += num11;
			}
			else
			{
				Vector2 val7 = Random.insideUnitCircle * radius2;
				float num12 = Random.Range(num7 * -0.5f, num7 * 0.5f);
				((Vector3)(ref val))._002Ector(val7.x, num12, val7.y);
			}
			val += val6.center;
		}
		return transform.TransformPoint(val);
	}

	public static CharacterBody GetFriendlyEasyTarget(CharacterBody casterBody, Ray aimRay, float maxDistance, float maxDeviation = 20f)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		TeamIndex teamIndex = TeamIndex.Neutral;
		TeamComponent component = ((Component)casterBody).GetComponent<TeamComponent>();
		if (Object.op_Implicit((Object)(object)component))
		{
			teamIndex = component.teamIndex;
		}
		ReadOnlyCollection<TeamComponent> teamMembers = TeamComponent.GetTeamMembers(teamIndex);
		Vector3 origin = aimRay.origin;
		Vector3 direction = aimRay.direction;
		List<EasyTargetCandidate> candidatesList = new List<EasyTargetCandidate>(teamMembers.Count);
		List<int> list = new List<int>(teamMembers.Count);
		float num = Mathf.Cos(maxDeviation * (MathF.PI / 180f));
		for (int i = 0; i < teamMembers.Count; i++)
		{
			Transform transform = ((Component)teamMembers[i]).transform;
			Vector3 val = transform.position - origin;
			float magnitude = ((Vector3)(ref val)).magnitude;
			float num2 = Vector3.Dot(val * (1f / magnitude), direction);
			CharacterBody component2 = ((Component)transform).GetComponent<CharacterBody>();
			if (num2 >= num && (Object)(object)component2 != (Object)(object)casterBody)
			{
				float num3 = 1f / magnitude;
				float score = num2 + num3;
				candidatesList.Add(new EasyTargetCandidate
				{
					transform = transform,
					score = score,
					distance = magnitude
				});
				list.Add(list.Count);
			}
		}
		list.Sort(delegate(int a, int b)
		{
			float score2 = candidatesList[a].score;
			float score3 = candidatesList[b].score;
			return (score2 != score3) ? ((!(score2 > score3)) ? 1 : (-1)) : 0;
		});
		for (int j = 0; j < list.Count; j++)
		{
			int index = list[j];
			CharacterBody component3 = ((Component)candidatesList[index].transform).GetComponent<CharacterBody>();
			if (Object.op_Implicit((Object)(object)component3) && (Object)(object)component3 != (Object)(object)casterBody)
			{
				return component3;
			}
		}
		return null;
	}

	public static CharacterBody GetEnemyEasyTarget(CharacterBody casterBody, Ray aimRay, float maxDistance, float maxDeviation = 20f)
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		TeamIndex teamIndex = TeamIndex.Neutral;
		TeamComponent component = ((Component)casterBody).GetComponent<TeamComponent>();
		if (Object.op_Implicit((Object)(object)component))
		{
			teamIndex = component.teamIndex;
		}
		List<TeamComponent> list = new List<TeamComponent>();
		for (TeamIndex teamIndex2 = TeamIndex.Neutral; teamIndex2 < TeamIndex.Count; teamIndex2++)
		{
			if (teamIndex2 != teamIndex)
			{
				list.AddRange(TeamComponent.GetTeamMembers(teamIndex2));
			}
		}
		Vector3 origin = aimRay.origin;
		Vector3 direction = aimRay.direction;
		List<EasyTargetCandidate> candidatesList = new List<EasyTargetCandidate>(list.Count);
		List<int> list2 = new List<int>(list.Count);
		float num = Mathf.Cos(maxDeviation * (MathF.PI / 180f));
		for (int i = 0; i < list.Count; i++)
		{
			Transform transform = ((Component)list[i]).transform;
			Vector3 val = transform.position - origin;
			float magnitude = ((Vector3)(ref val)).magnitude;
			float num2 = Vector3.Dot(val * (1f / magnitude), direction);
			CharacterBody component2 = ((Component)transform).GetComponent<CharacterBody>();
			if (num2 >= num && (Object)(object)component2 != (Object)(object)casterBody && magnitude < maxDistance)
			{
				float num3 = 1f / magnitude;
				float score = num2 + num3;
				candidatesList.Add(new EasyTargetCandidate
				{
					transform = transform,
					score = score,
					distance = magnitude
				});
				list2.Add(list2.Count);
			}
		}
		list2.Sort(delegate(int a, int b)
		{
			float score2 = candidatesList[a].score;
			float score3 = candidatesList[b].score;
			return (score2 != score3) ? ((!(score2 > score3)) ? 1 : (-1)) : 0;
		});
		for (int j = 0; j < list2.Count; j++)
		{
			int index = list2[j];
			CharacterBody component3 = ((Component)candidatesList[index].transform).GetComponent<CharacterBody>();
			if (Object.op_Implicit((Object)(object)component3) && (Object)(object)component3 != (Object)(object)casterBody)
			{
				return component3;
			}
		}
		return null;
	}

	public static float GetBodyPrefabFootOffset(GameObject prefab)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		CapsuleCollider component = prefab.GetComponent<CapsuleCollider>();
		if (Object.op_Implicit((Object)(object)component))
		{
			return component.height * 0.5f - component.center.y;
		}
		return 0f;
	}

	public static void ShuffleList<T>(List<T> list)
	{
		for (int num = list.Count - 1; num > 0; num--)
		{
			int index = Random.Range(0, num + 1);
			T value = list[num];
			list[num] = list[index];
			list[index] = value;
		}
	}

	public static void ShuffleList<T>(List<T> list, Xoroshiro128Plus rng)
	{
		for (int num = list.Count - 1; num > 0; num--)
		{
			int index = rng.RangeInt(0, num + 1);
			T value = list[num];
			list[num] = list[index];
			list[index] = value;
		}
	}

	public static void ShuffleArray<T>(T[] array)
	{
		for (int num = array.Length - 1; num > 0; num--)
		{
			int num2 = Random.Range(0, num + 1);
			T val = array[num];
			array[num] = array[num2];
			array[num2] = val;
		}
	}

	public static void ShuffleArray<T>(T[] array, Xoroshiro128Plus rng)
	{
		for (int num = array.Length - 1; num > 0; num--)
		{
			int num2 = rng.RangeInt(0, num + 1);
			T val = array[num];
			array[num] = array[num2];
			array[num2] = val;
		}
	}

	public static Transform FindNearest(Vector3 position, List<Transform> transformsList, float range = float.PositiveInfinity)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		Transform result = null;
		float num = range * range;
		for (int i = 0; i < transformsList.Count; i++)
		{
			Vector3 val = transformsList[i].position - position;
			float sqrMagnitude = ((Vector3)(ref val)).sqrMagnitude;
			if (sqrMagnitude < num)
			{
				num = sqrMagnitude;
				result = transformsList[i];
			}
		}
		return result;
	}

	public static Vector3 ApplySpread(Vector3 aimDirection, float minSpread, float maxSpread, float spreadYawScale, float spreadPitchScale, float bonusYaw = 0f, float bonusPitch = 0f)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		Vector3 up = Vector3.up;
		Vector3 val = Vector3.Cross(up, aimDirection);
		float num = Random.Range(minSpread, maxSpread);
		float num2 = Random.Range(0f, 360f);
		Vector3 val2 = Quaternion.Euler(0f, 0f, num2) * (Quaternion.Euler(num, 0f, 0f) * Vector3.forward);
		float y = val2.y;
		val2.y = 0f;
		float num3 = (Mathf.Atan2(val2.z, val2.x) * 57.29578f - 90f + bonusYaw) * spreadYawScale;
		float num4 = (Mathf.Atan2(y, ((Vector3)(ref val2)).magnitude) * 57.29578f + bonusPitch) * spreadPitchScale;
		return Quaternion.AngleAxis(num3, up) * (Quaternion.AngleAxis(num4, val) * aimDirection);
	}

	public static string GenerateColoredString(string str, Color32 color)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		return string.Format(CultureInfo.InvariantCulture, "<color=#{0:X2}{1:X2}{2:X2}>{3}</color>", color.r, color.g, color.b, str);
	}

	public static bool GuessRenderBounds(GameObject gameObject, out Bounds bounds)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		Renderer[] componentsInChildren = gameObject.GetComponentsInChildren<Renderer>();
		if (componentsInChildren.Length != 0)
		{
			bounds = componentsInChildren[0].bounds;
			for (int i = 1; i < componentsInChildren.Length; i++)
			{
				((Bounds)(ref bounds)).Encapsulate(componentsInChildren[i].bounds);
			}
			return true;
		}
		bounds = new Bounds(gameObject.transform.position, Vector3.zero);
		return false;
	}

	public static bool GuessRenderBoundsMeshOnly(GameObject gameObject, out Bounds bounds)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		Renderer[] array = (from renderer in gameObject.GetComponentsInChildren<Renderer>()
			where renderer is MeshRenderer || renderer is SkinnedMeshRenderer
			select renderer).ToArray();
		if (array.Length != 0)
		{
			bounds = array[0].bounds;
			for (int i = 1; i < array.Length; i++)
			{
				((Bounds)(ref bounds)).Encapsulate(array[i].bounds);
			}
			return true;
		}
		bounds = new Bounds(gameObject.transform.position, Vector3.zero);
		return false;
	}

	public static GameObject FindNetworkObject(NetworkInstanceId networkInstanceId)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		if (NetworkServer.active)
		{
			return NetworkServer.FindLocalObject(networkInstanceId);
		}
		return ClientScene.FindLocalObject(networkInstanceId);
	}

	public static string GetGameObjectHierarchyName(GameObject gameObject)
	{
		int num = 0;
		Transform val = gameObject.transform;
		while (Object.op_Implicit((Object)(object)val))
		{
			num++;
			val = val.parent;
		}
		string[] array = new string[num];
		Transform val2 = gameObject.transform;
		while (Object.op_Implicit((Object)(object)val2))
		{
			array[--num] = ((Object)((Component)val2).gameObject).name;
			val2 = val2.parent;
		}
		return string.Join("/", array);
	}

	public static string GetBestBodyName(GameObject bodyObject)
	{
		CharacterBody characterBody = null;
		string text = "???";
		if (Object.op_Implicit((Object)(object)bodyObject))
		{
			characterBody = bodyObject.GetComponent<CharacterBody>();
		}
		if (Object.op_Implicit((Object)(object)characterBody))
		{
			text = characterBody.GetUserName();
		}
		else
		{
			IDisplayNameProvider component = bodyObject.GetComponent<IDisplayNameProvider>();
			if (component != null)
			{
				text = component.GetDisplayName();
			}
		}
		string text2 = text;
		if (Object.op_Implicit((Object)(object)characterBody))
		{
			if (characterBody.isElite)
			{
				BuffIndex[] eliteBuffIndices = BuffCatalog.eliteBuffIndices;
				foreach (BuffIndex buffIndex in eliteBuffIndices)
				{
					if (characterBody.HasBuff(buffIndex))
					{
						text2 = Language.GetStringFormatted(BuffCatalog.GetBuffDef(buffIndex).eliteDef.modifierToken, text2);
					}
				}
			}
			if (Object.op_Implicit((Object)(object)characterBody.inventory))
			{
				if (characterBody.inventory.GetItemCount(RoR2Content.Items.InvadingDoppelganger) > 0)
				{
					text2 = Language.GetStringFormatted("BODY_MODIFIER_DOPPELGANGER", text2);
				}
				if (characterBody.inventory.GetItemCount(DLC1Content.Items.GummyCloneIdentifier) > 0)
				{
					text2 = Language.GetStringFormatted("BODY_MODIFIER_GUMMYCLONE", text2);
				}
			}
		}
		return text2;
	}

	public static string GetBestBodyNameColored(GameObject bodyObject)
	{
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)bodyObject))
		{
			CharacterBody component = bodyObject.GetComponent<CharacterBody>();
			if (Object.op_Implicit((Object)(object)component))
			{
				CharacterMaster master = component.master;
				if (Object.op_Implicit((Object)(object)master))
				{
					PlayerCharacterMasterController component2 = ((Component)master).GetComponent<PlayerCharacterMasterController>();
					if (Object.op_Implicit((Object)(object)component2))
					{
						GameObject networkUserObject = component2.networkUserObject;
						if (Object.op_Implicit((Object)(object)networkUserObject))
						{
							NetworkUser component3 = networkUserObject.GetComponent<NetworkUser>();
							if (Object.op_Implicit((Object)(object)component3))
							{
								return GenerateColoredString(component3.userName, component3.userColor);
							}
						}
					}
				}
			}
			IDisplayNameProvider component4 = bodyObject.GetComponent<IDisplayNameProvider>();
			if (component4 != null)
			{
				return component4.GetDisplayName();
			}
		}
		return "???";
	}

	public static string GetBestMasterName(CharacterMaster characterMaster)
	{
		if (Object.op_Implicit((Object)(object)characterMaster))
		{
			string text = characterMaster.playerCharacterMasterController?.networkUser?.userName;
			if (text == null)
			{
				GameObject bodyPrefab = characterMaster.bodyPrefab;
				string text2 = ((bodyPrefab != null) ? bodyPrefab.GetComponent<CharacterBody>().baseNameToken : null);
				if (text2 != null)
				{
					text = Language.GetString(text2);
				}
			}
			return text;
		}
		return "Null Master";
	}

	public static NetworkUser LookUpBodyNetworkUser(GameObject bodyObject)
	{
		if (Object.op_Implicit((Object)(object)bodyObject))
		{
			return LookUpBodyNetworkUser(bodyObject.GetComponent<CharacterBody>());
		}
		return null;
	}

	public static NetworkUser LookUpBodyNetworkUser(CharacterBody characterBody)
	{
		if (Object.op_Implicit((Object)(object)characterBody))
		{
			CharacterMaster master = characterBody.master;
			if (Object.op_Implicit((Object)(object)master))
			{
				PlayerCharacterMasterController component = ((Component)master).GetComponent<PlayerCharacterMasterController>();
				if (Object.op_Implicit((Object)(object)component))
				{
					GameObject networkUserObject = component.networkUserObject;
					if (Object.op_Implicit((Object)(object)networkUserObject))
					{
						NetworkUser component2 = networkUserObject.GetComponent<NetworkUser>();
						if (Object.op_Implicit((Object)(object)component2))
						{
							return component2;
						}
					}
				}
			}
		}
		return null;
	}

	private static bool HandleCharacterPhysicsCastResults(GameObject bodyObject, Ray ray, RaycastHit[] hits, out RaycastHit hitInfo)
	{
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		int num = -1;
		float num2 = float.PositiveInfinity;
		for (int i = 0; i < hits.Length; i++)
		{
			float distance = ((RaycastHit)(ref hits[i])).distance;
			if (!(distance < num2))
			{
				continue;
			}
			HurtBox component = ((Component)((RaycastHit)(ref hits[i])).collider).GetComponent<HurtBox>();
			if (Object.op_Implicit((Object)(object)component))
			{
				HealthComponent healthComponent = component.healthComponent;
				if (Object.op_Implicit((Object)(object)healthComponent) && (Object)(object)((Component)healthComponent).gameObject == (Object)(object)bodyObject)
				{
					continue;
				}
			}
			if (distance == 0f)
			{
				hitInfo = hits[i];
				((RaycastHit)(ref hitInfo)).point = ((Ray)(ref ray)).origin;
				return true;
			}
			num = i;
			num2 = distance;
		}
		if (num == -1)
		{
			hitInfo = default(RaycastHit);
			return false;
		}
		hitInfo = hits[num];
		return true;
	}

	public static bool CharacterRaycast(GameObject bodyObject, Ray ray, out RaycastHit hitInfo, float maxDistance, LayerMask layerMask, QueryTriggerInteraction queryTriggerInteraction)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		RaycastHit[] hits = Physics.RaycastAll(ray, maxDistance, LayerMask.op_Implicit(layerMask), queryTriggerInteraction);
		return HandleCharacterPhysicsCastResults(bodyObject, ray, hits, out hitInfo);
	}

	public static bool CharacterSpherecast(GameObject bodyObject, Ray ray, float radius, out RaycastHit hitInfo, float maxDistance, LayerMask layerMask, QueryTriggerInteraction queryTriggerInteraction)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		RaycastHit[] hits = Physics.SphereCastAll(ray, radius, maxDistance, LayerMask.op_Implicit(layerMask), queryTriggerInteraction);
		return HandleCharacterPhysicsCastResults(bodyObject, ray, hits, out hitInfo);
	}

	public static bool ConnectionIsLocal([NotNull] NetworkConnection conn)
	{
		if (conn is SteamNetworkConnection)
		{
			return false;
		}
		if (conn is EOSNetworkConnection)
		{
			return false;
		}
		return ((object)conn).GetType() != typeof(NetworkConnection);
	}

	public static string EscapeRichTextForTextMeshPro(string rtString)
	{
		string text = rtString;
		text = text.Replace("<", "</noparse><noparse><</noparse><noparse>");
		return "<noparse>" + text + "</noparse>";
	}

	public static string EscapeQuotes(string str)
	{
		str = backlashSearch.Replace(str, strBackslashBackslash);
		str = quoteSearch.Replace(str, strBackslashQuote);
		return str;
	}

	public static string RGBToHex(Color32 rgb)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		return string.Format(CultureInfo.InvariantCulture, "{0:X2}{1:X2}{2:X2}", rgb.r, rgb.g, rgb.b);
	}

	public static Vector2 Vector3XZToVector2XY(Vector3 vector3)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2(vector3.x, vector3.z);
	}

	public static Vector2 Vector3XZToVector2XY(ref Vector3 vector3)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2(vector3.x, vector3.z);
	}

	public static void Vector3XZToVector2XY(Vector3 vector3, out Vector2 vector2)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		vector2.x = vector3.x;
		vector2.y = vector3.z;
	}

	public static void Vector3XZToVector2XY(ref Vector3 vector3, out Vector2 vector2)
	{
		vector2.x = vector3.x;
		vector2.y = vector3.z;
	}

	public static Vector2 RotateVector2(Vector2 vector2, float degrees)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Sin(degrees * (MathF.PI / 180f));
		float num2 = Mathf.Cos(degrees * (MathF.PI / 180f));
		return new Vector2(num2 * vector2.x - num * vector2.y, num * vector2.x + num2 * vector2.y);
	}

	public static Quaternion SmoothDampQuaternion(Quaternion current, Quaternion target, ref float currentVelocity, float smoothTime, float maxSpeed, float deltaTime)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		float num = Quaternion.Angle(current, target);
		num = Mathf.SmoothDamp(0f, num, ref currentVelocity, smoothTime, maxSpeed, deltaTime);
		return Quaternion.RotateTowards(current, target, num);
	}

	public static Quaternion SmoothDampQuaternion(Quaternion current, Quaternion target, ref float currentVelocity, float smoothTime)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		float num = Quaternion.Angle(current, target);
		num = Mathf.SmoothDamp(0f, num, ref currentVelocity, smoothTime);
		return Quaternion.RotateTowards(current, target, num);
	}

	public static HurtBox FindBodyMainHurtBox(CharacterBody characterBody)
	{
		return characterBody.mainHurtBox;
	}

	public static HurtBox FindBodyMainHurtBox(GameObject bodyObject)
	{
		CharacterBody component = bodyObject.GetComponent<CharacterBody>();
		if (Object.op_Implicit((Object)(object)component))
		{
			return FindBodyMainHurtBox(component);
		}
		return null;
	}

	public static Vector3 GetCorePosition(CharacterBody characterBody)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return characterBody.corePosition;
	}

	public static Vector3 GetCorePosition(GameObject bodyObject)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		CharacterBody component = bodyObject.GetComponent<CharacterBody>();
		if (Object.op_Implicit((Object)(object)component))
		{
			return GetCorePosition(component);
		}
		return bodyObject.transform.position;
	}

	public static Transform GetCoreTransform(GameObject bodyObject)
	{
		CharacterBody component = bodyObject.GetComponent<CharacterBody>();
		if (Object.op_Implicit((Object)(object)component))
		{
			return component.coreTransform;
		}
		return bodyObject.transform;
	}

	public static float SphereRadiusToVolume(float radius)
	{
		return 4.1887903f * (radius * radius * radius);
	}

	public static float SphereVolumeToRadius(float volume)
	{
		return Mathf.Pow(3f * volume / (MathF.PI * 4f), 1f / 3f);
	}

	public static void CopyList<T>(List<T> src, List<T> dest)
	{
		dest.Clear();
		foreach (T item in src)
		{
			dest.Add(item);
		}
	}

	public static float ScanCharacterAnimationClipForMomentOfRootMotionStop(GameObject characterPrefab, string clipName, string rootBoneNameInChildLocator)
	{
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = Object.Instantiate<GameObject>(characterPrefab);
		Transform modelTransform = val.GetComponent<ModelLocator>().modelTransform;
		Transform val2 = ((Component)modelTransform).GetComponent<ChildLocator>().FindChild(rootBoneNameInChildLocator);
		AnimationClip val3 = ((Component)modelTransform).GetComponent<Animator>().runtimeAnimatorController.animationClips.FirstOrDefault((AnimationClip c) => ((Object)c).name == clipName);
		float result = 1f;
		val3.SampleAnimation(val, 0f);
		Vector3 val4 = val2.position;
		for (float num = 0.1f; num < 1f; num += 0.1f)
		{
			val3.SampleAnimation(val, num);
			Vector3 position = val2.position;
			Vector3 val5 = position - val4;
			if (((Vector3)(ref val5)).magnitude == 0f)
			{
				result = num;
				break;
			}
			val4 = position;
		}
		Object.Destroy((Object)(object)val);
		return result;
	}

	public static void DebugCross(Vector3 position, float radius, Color color, float duration)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		Debug.DrawLine(position - Vector3.right * radius, position + Vector3.right * radius, color, duration);
		Debug.DrawLine(position - Vector3.up * radius, position + Vector3.up * radius, color, duration);
		Debug.DrawLine(position - Vector3.forward * radius, position + Vector3.forward * radius, color, duration);
	}

	public static void DebugBounds(Bounds bounds, Color color, float duration)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		Vector3 min = ((Bounds)(ref bounds)).min;
		Vector3 max = ((Bounds)(ref bounds)).max;
		Vector3 val = default(Vector3);
		((Vector3)(ref val))._002Ector(min.x, min.y, min.z);
		Vector3 val2 = default(Vector3);
		((Vector3)(ref val2))._002Ector(min.x, min.y, max.z);
		Vector3 val3 = default(Vector3);
		((Vector3)(ref val3))._002Ector(min.x, max.y, min.z);
		Vector3 val4 = default(Vector3);
		((Vector3)(ref val4))._002Ector(min.x, max.y, max.z);
		Vector3 val5 = default(Vector3);
		((Vector3)(ref val5))._002Ector(max.x, min.y, min.z);
		Vector3 val6 = default(Vector3);
		((Vector3)(ref val6))._002Ector(max.x, min.y, max.z);
		Vector3 val7 = default(Vector3);
		((Vector3)(ref val7))._002Ector(max.x, max.y, min.z);
		Vector3 val8 = new Vector3(max.x, max.y, max.z);
		Debug.DrawLine(val, val2, color, duration);
		Debug.DrawLine(val, val5, color, duration);
		Debug.DrawLine(val, val3, color, duration);
		Debug.DrawLine(val3, val4, color, duration);
		Debug.DrawLine(val3, val7, color, duration);
		Debug.DrawLine(val8, val4, color, duration);
		Debug.DrawLine(val8, val7, color, duration);
		Debug.DrawLine(val8, val6, color, duration);
		Debug.DrawLine(val6, val5, color, duration);
		Debug.DrawLine(val6, val2, color, duration);
		Debug.DrawLine(val2, val4, color, duration);
		Debug.DrawLine(val5, val7, color, duration);
	}

	public static bool PositionIsValid(Vector3 value)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		float f = value.x + value.y + value.z;
		if (!float.IsInfinity(f))
		{
			return !float.IsNaN(f);
		}
		return false;
	}

	public static void Swap<T>(ref T a, ref T b)
	{
		T val = a;
		a = b;
		b = val;
	}

	public static DateTime UnixTimeStampToDateTimeUtc(uint unixTimeStamp)
	{
		DateTime dateTime = dateZero;
		return dateTime.AddSeconds(unixTimeStamp).ToUniversalTime();
	}

	public static double GetCurrentUnixEpochTimeInSeconds()
	{
		return Client.Instance.Utils.GetServerRealTime();
	}

	public static bool IsValid(Object o)
	{
		return Object.op_Implicit(o);
	}

	public static string BuildPrefabTransformPath(Transform root, Transform transform, bool appendCloneSuffix = false, bool includeRoot = false)
	{
		if (!Object.op_Implicit((Object)(object)root))
		{
			throw new ArgumentException("Value provided as 'root' is an invalid object.", "root");
		}
		if (!Object.op_Implicit((Object)(object)transform))
		{
			throw new ArgumentException("Value provided as 'transform' is an invalid object.", "transform");
		}
		try
		{
			for (Transform val = transform; val != root; val = val.parent)
			{
				if (!Object.op_Implicit((Object)(object)val))
				{
					string arg = TakeResult();
					throw new ArgumentException($"\"{arg}\" is not a child of \"{root}\".");
				}
				if (sharedStringStack.Count > 0)
				{
					sharedStringStack.Push("/");
				}
				sharedStringStack.Push(((Object)((Component)val).gameObject).name);
			}
			if (includeRoot)
			{
				if (sharedStringStack.Count > 0)
				{
					sharedStringStack.Push("/");
				}
				sharedStringStack.Push(((Object)((Component)root).gameObject).name);
			}
			return TakeResult();
		}
		finally
		{
			sharedStringBuilder.Clear();
			sharedStringStack.Clear();
		}
		string TakeResult()
		{
			while (sharedStringStack.Count > 0)
			{
				sharedStringBuilder.Append(sharedStringStack.Pop());
			}
			if (appendCloneSuffix)
			{
				sharedStringBuilder.Append(cloneString);
			}
			return sharedStringBuilder.Take();
		}
	}

	public static int GetItemCountForTeam(TeamIndex teamIndex, ItemIndex itemIndex, bool requiresAlive, bool requiresConnected = true)
	{
		int num = 0;
		ReadOnlyCollection<CharacterMaster> readOnlyInstancesList = CharacterMaster.readOnlyInstancesList;
		int i = 0;
		for (int count = readOnlyInstancesList.Count; i < count; i++)
		{
			CharacterMaster characterMaster = readOnlyInstancesList[i];
			if (characterMaster.teamIndex == teamIndex && (!requiresAlive || characterMaster.hasBody) && (!requiresConnected || !Object.op_Implicit((Object)(object)characterMaster.playerCharacterMasterController) || characterMaster.playerCharacterMasterController.isConnected))
			{
				num += characterMaster.inventory.GetItemCount(itemIndex);
			}
		}
		return num;
	}

	public static int GetItemCountGlobal(ItemIndex itemIndex, bool requiresAlive, bool requiresConnected = true)
	{
		int num = 0;
		ReadOnlyCollection<CharacterMaster> readOnlyInstancesList = CharacterMaster.readOnlyInstancesList;
		int i = 0;
		for (int count = readOnlyInstancesList.Count; i < count; i++)
		{
			CharacterMaster characterMaster = readOnlyInstancesList[i];
			if ((!requiresAlive || characterMaster.hasBody) && (!requiresConnected || !Object.op_Implicit((Object)(object)characterMaster.playerCharacterMasterController) || characterMaster.playerCharacterMasterController.isConnected))
			{
				num += characterMaster.inventory.GetItemCount(itemIndex);
			}
		}
		return num;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void NullifyIfInvalid<T>(ref T objRef) where T : Object
	{
		if (objRef != null && !Object.op_Implicit((Object)(object)objRef))
		{
			objRef = default(T);
		}
	}

	public static bool IsPrefab(GameObject gameObject)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)gameObject))
		{
			Scene scene = gameObject.scene;
			return !((Scene)(ref scene)).IsValid();
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static uint IntToUintPlusOne(int value)
	{
		return (uint)(value + 1);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int UintToIntMinusOne(uint value)
	{
		return (int)(value - 1);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ulong LongToUlongPlusOne(long value)
	{
		return (ulong)(value + 1);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static long UlongToLongMinusOne(ulong value)
	{
		return (long)(value - 1);
	}

	public static float GetExpAdjustedDropChancePercent(float baseChancePercent, GameObject characterBodyObject)
	{
		int num = 0;
		DeathRewards component = characterBodyObject.GetComponent<DeathRewards>();
		if (Object.op_Implicit((Object)(object)component))
		{
			num = component.spawnValue;
		}
		return baseChancePercent * Mathf.Log((float)num + 1f, 2f);
	}

	public static bool IsDontDestroyOnLoad(GameObject go)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		Scene scene = go.scene;
		return ((Scene)(ref scene)).name == "DontDestroyOnLoad";
	}
}
