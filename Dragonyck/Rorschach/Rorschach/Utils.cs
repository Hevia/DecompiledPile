using System;
using System.Collections;
using System.Reflection;
using EntityStates;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace Rorschach;

internal class Utils
{
	internal static EntityStateMachine NewStateMachine(GameObject target, string name)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		EntityStateMachine val = target.AddComponent<EntityStateMachine>();
		val.customName = name;
		val.initialStateType = new SerializableEntityStateType(typeof(Idle));
		val.mainStateType = new SerializableEntityStateType(typeof(Idle));
		return val;
	}

	internal static IEnumerator ExecuteDelayedAction(float time, Action action)
	{
		yield return (object)new WaitForSeconds(time);
		action();
	}

	internal static T CopyComponent<T>(T original, GameObject destination) where T : Component
	{
		Type type = ((object)original).GetType();
		Component val = destination.AddComponent(type);
		FieldInfo[] fields = type.GetFields();
		FieldInfo[] array = fields;
		foreach (FieldInfo fieldInfo in array)
		{
			fieldInfo.SetValue(val, fieldInfo.GetValue(original));
		}
		return (T)(object)((val is T) ? val : null);
	}

	public static Sprite CreateSpriteFromTexture(Texture2D texture)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)texture))
		{
			return Sprite.Create(texture, new Rect(0f, 0f, (float)((Texture)texture).width, (float)((Texture)texture).height), new Vector2(0.5f, 0.5f));
		}
		return null;
	}

	public static GameObject FindInActiveObjectByName(string name)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Invalid comparison between Unknown and I4
		Transform[] array = Resources.FindObjectsOfTypeAll<Transform>();
		for (int i = 0; i < array.Length; i++)
		{
			if ((int)((Object)array[i]).hideFlags == 0 && ((Object)array[i]).name == name)
			{
				return ((Component)array[i]).gameObject;
			}
		}
		return null;
	}

	public static GameObject CreateHitbox(string name, Transform parent, Vector3 scale)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Expected O, but got Unknown
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = new GameObject(name);
		val.transform.SetParent(parent);
		val.transform.localPosition = new Vector3(0f, 1f, 3f);
		val.transform.localRotation = Quaternion.identity;
		val.transform.localScale = scale;
		HitBoxGroup val2 = ((Component)parent).gameObject.AddComponent<HitBoxGroup>();
		HitBox val3 = val.AddComponent<HitBox>();
		val.layer = LayerIndex.projectile.intVal;
		val2.hitBoxes = (HitBox[])(object)new HitBox[1] { val3 };
		val2.groupName = name;
		return val;
	}

	internal static void RegisterEffect(GameObject effect, float duration, string soundName = "")
	{
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		EffectComponent val = effect.GetComponent<EffectComponent>();
		if (!Object.op_Implicit((Object)(object)val))
		{
			val = effect.AddComponent<EffectComponent>();
		}
		if (!Object.op_Implicit((Object)(object)effect.GetComponent<DestroyOnTimer>()))
		{
			effect.AddComponent<DestroyOnTimer>().duration = duration;
		}
		if (!Object.op_Implicit((Object)(object)effect.GetComponent<NetworkIdentity>()))
		{
			effect.AddComponent<NetworkIdentity>();
		}
		if (!Object.op_Implicit((Object)(object)effect.GetComponent<VFXAttributes>()))
		{
			effect.AddComponent<VFXAttributes>().vfxPriority = (VFXPriority)2;
		}
		val.applyScale = false;
		val.effectIndex = (EffectIndex)(-1);
		val.parentToReferencedTransform = true;
		val.positionAtReferencedTransform = true;
		val.soundName = soundName;
		ContentAddition.AddEffect(effect);
	}

	public static Material InstantiateMaterial(Color color, Texture tex, Color emColor, float emPower, Texture emTex, float normStr, Texture normTex)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		Material val = Object.Instantiate<Material>(LegacyResourcesAPI.Load<GameObject>("Prefabs/CharacterBodies/CommandoBody").GetComponentInChildren<CharacterModel>().baseRendererInfos[0].defaultMaterial);
		if (Object.op_Implicit((Object)(object)val))
		{
			val.SetColor("_Color", color);
			val.SetTexture("_MainTex", tex);
			val.SetColor("_EmColor", emColor);
			val.SetFloat("_EmPower", emPower);
			val.SetTexture("_EmTex", emTex);
			val.SetFloat("_NormalStrength", 1f);
			val.SetTexture("_NormalTex", normTex);
			return val;
		}
		return val;
	}

	public static Material FindMaterial(string name)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Invalid comparison between Unknown and I4
		Material[] array = Resources.FindObjectsOfTypeAll<Material>();
		for (int i = 0; i < array.Length; i++)
		{
			if ((int)((Object)array[i]).hideFlags == 0 && ((Object)array[i]).name == name)
			{
				return array[i];
			}
		}
		return null;
	}
}
