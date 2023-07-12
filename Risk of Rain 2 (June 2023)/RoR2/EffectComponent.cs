using System;
using RoR2.Orbs;
using UnityEngine;

namespace RoR2;

[DisallowMultipleComponent]
public class EffectComponent : MonoBehaviour
{
	[Tooltip("This is assigned to the prefab automatically by EffectCatalog at runtime. Do not set this value manually.")]
	[HideInInspector]
	public EffectIndex effectIndex = EffectIndex.Invalid;

	[NonSerialized]
	public EffectData effectData;

	[Tooltip("Positions the effect at the transform referenced by the effect data if available.")]
	public bool positionAtReferencedTransform;

	[Tooltip("Parents the effect to the transform object referenced by the effect data if available.")]
	public bool parentToReferencedTransform;

	[Tooltip("Causes this object to adopt the scale received in the effectdata.")]
	public bool applyScale;

	[Tooltip("The sound to play whenever this effect is dispatched, regardless of whether or not it actually ends up spawning.")]
	public string soundName;

	[Tooltip("Ignore Z scale when adopting scale values from effectdata.")]
	public bool disregardZScale;

	private bool didResolveReferencedObject;

	private GameObject referencedObject;

	private bool didResolveReferencedChildTransform;

	private Transform referencedChildTransform;

	private bool didResolveReferencedHurtBox;

	private GameObject referencedHurtBoxGameObject;

	private void Start()
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		if (effectData == null)
		{
			Debug.LogErrorFormat((Object)(object)((Component)this).gameObject, "Object {0} should not be instantiated by means other than EffectManager.SpawnEffect. This WILL result in an NRE!!! Use EffectManager.SpawnEffect or don't use EffectComponent!!!!!", new object[1] { ((Component)this).gameObject });
		}
		Transform val = null;
		if ((positionAtReferencedTransform | parentToReferencedTransform) && effectData != null)
		{
			val = effectData.ResolveChildLocatorTransformReference();
		}
		if (Object.op_Implicit((Object)(object)val))
		{
			if (positionAtReferencedTransform)
			{
				((Component)this).transform.position = val.position;
				((Component)this).transform.rotation = val.rotation;
			}
			if (parentToReferencedTransform)
			{
				((Component)this).transform.SetParent(val, true);
			}
		}
		if (applyScale && effectData != null)
		{
			float scale = effectData.scale;
			if (!disregardZScale)
			{
				((Component)this).transform.localScale = new Vector3(scale, scale, scale);
			}
			else
			{
				((Component)this).transform.localScale = new Vector3(scale, scale, ((Component)this).transform.localScale.z);
			}
		}
	}

	[ContextMenu("Attempt to Upgrade Sfx Setup")]
	private void AttemptToUpgradeSfxSetup()
	{
		AkEvent[] components = ((Component)this).GetComponents<AkEvent>();
		int num = 1281810935;
		if (components.Length == 1 && ((AkTriggerHandler)components[0]).triggerList.Count == 1 && ((AkTriggerHandler)components[0]).triggerList[0] == num)
		{
			string objectName = ((WwiseObjectReference)components[0].data.WwiseObjectReference).ObjectName;
			soundName = objectName;
			Object.DestroyImmediate((Object)(object)components[0]);
			Object.DestroyImmediate((Object)(object)((Component)this).GetComponent<AkGameObj>());
			Rigidbody component = ((Component)this).GetComponent<Rigidbody>();
			if (component.isKinematic)
			{
				Object.DestroyImmediate((Object)(object)component);
			}
			EditorUtil.SetDirty((Object)(object)this);
			EditorUtil.SetDirty((Object)(object)((Component)this).gameObject);
		}
	}

	private void OnValidate()
	{
		if (!Application.isPlaying && effectIndex != EffectIndex.Invalid)
		{
			effectIndex = EffectIndex.Invalid;
		}
		if (Application.isPlaying)
		{
			return;
		}
		bool num = Object.op_Implicit((Object)(object)((Component)this).GetComponent<AkGameObj>());
		bool flag = Object.op_Implicit((Object)(object)((Component)this).GetComponent<OrbEffect>());
		if (num && !flag)
		{
			AkEvent[] components = ((Component)this).GetComponents<AkEvent>();
			int item = 1281810935;
			if (components.Length == 1 && ((AkTriggerHandler)components[0]).triggerList.Contains(item))
			{
				Debug.LogWarningFormat((Object)(object)((Component)this).gameObject, "Effect {0} has an attached AkGameObj. You probably want to use the soundName field of its EffectComponent instead.", new object[1] { Util.GetGameObjectHierarchyName(((Component)this).gameObject) });
			}
		}
	}

	public GameObject GetReferencedObject()
	{
		if (!didResolveReferencedObject)
		{
			referencedObject = effectData.ResolveNetworkedObjectReference();
			didResolveReferencedObject = true;
		}
		return referencedObject;
	}

	public Transform GetReferencedChildTransform()
	{
		if (!didResolveReferencedChildTransform)
		{
			referencedChildTransform = effectData.ResolveChildLocatorTransformReference();
			didResolveReferencedChildTransform = true;
		}
		return referencedChildTransform;
	}

	public GameObject GetReferencedHurtBoxGameObject()
	{
		if (!didResolveReferencedHurtBox)
		{
			referencedHurtBoxGameObject = effectData.ResolveHurtBoxReference();
			didResolveReferencedHurtBox = true;
		}
		return referencedHurtBoxGameObject;
	}
}
