using System.Collections;
using Generics.Dynamics;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public class AnimationEvents : MonoBehaviour
{
	public GameObject soundCenter;

	private GameObject bodyObject;

	private CharacterModel characterModel;

	private ChildLocator childLocator;

	private EntityLocator entityLocator;

	private Renderer meshRenderer;

	private ModelLocator modelLocator;

	private float printHeight;

	private float printTime;

	private void Start()
	{
		childLocator = ((Component)this).GetComponent<ChildLocator>();
		entityLocator = ((Component)this).GetComponent<EntityLocator>();
		meshRenderer = ((Component)this).GetComponentInChildren<Renderer>();
		characterModel = ((Component)this).GetComponent<CharacterModel>();
		if (Object.op_Implicit((Object)(object)characterModel) && Object.op_Implicit((Object)(object)characterModel.body))
		{
			bodyObject = ((Component)characterModel.body).gameObject;
			modelLocator = bodyObject.GetComponent<ModelLocator>();
		}
	}

	public void UpdateIKState(AnimationEvent animationEvent)
	{
		((Component)childLocator.FindChild(animationEvent.stringParameter)).GetComponent<IIKTargetBehavior>()?.UpdateIKState(animationEvent.intParameter);
	}

	public void PlaySound(string soundString)
	{
		Util.PlaySound(soundString, Object.op_Implicit((Object)(object)soundCenter) ? soundCenter : bodyObject);
	}

	public void NormalizeToFloor()
	{
		if (Object.op_Implicit((Object)(object)modelLocator))
		{
			modelLocator.normalizeToFloor = true;
		}
	}

	public void SetIK(AnimationEvent animationEvent)
	{
		if (Object.op_Implicit((Object)(object)modelLocator) && Object.op_Implicit((Object)(object)modelLocator.modelTransform))
		{
			bool enabled = ((animationEvent.intParameter != 0) ? true : false);
			InverseKinematics component = ((Component)modelLocator.modelTransform).GetComponent<InverseKinematics>();
			StriderLegController component2 = ((Component)modelLocator.modelTransform).GetComponent<StriderLegController>();
			if (Object.op_Implicit((Object)(object)component))
			{
				((Behaviour)component).enabled = enabled;
			}
			if (Object.op_Implicit((Object)(object)component2))
			{
				((Behaviour)component2).enabled = enabled;
			}
		}
	}

	public void RefreshAllIK()
	{
		IKTargetPassive[] componentsInChildren = ((Component)this).GetComponentsInChildren<IKTargetPassive>();
		foreach (IKTargetPassive obj in componentsInChildren)
		{
			obj.UpdateIKTargetPosition();
			obj.UpdateYOffset();
		}
	}

	public void CreateEffect(AnimationEvent animationEvent)
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Expected O, but got Unknown
		Transform val = ((Component)this).transform;
		int num = -1;
		if (!string.IsNullOrEmpty(animationEvent.stringParameter))
		{
			num = childLocator.FindChildIndex(animationEvent.stringParameter);
			if (num != -1)
			{
				val = childLocator.FindChild(num);
			}
		}
		bool transmit = animationEvent.intParameter != 0;
		EffectData effectData = new EffectData();
		effectData.origin = val.position;
		effectData.SetChildLocatorTransformReference(bodyObject, num);
		EffectManager.SpawnEffect((GameObject)animationEvent.objectReferenceParameter, effectData, transmit);
	}

	public void CreatePrefab(AnimationEvent animationEvent)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = (GameObject)animationEvent.objectReferenceParameter;
		string stringParameter = animationEvent.stringParameter;
		int intParameter = animationEvent.intParameter;
		if (Object.op_Implicit((Object)(object)childLocator))
		{
			Transform val2 = childLocator.FindChild(stringParameter);
			if (Object.op_Implicit((Object)(object)val2))
			{
				if (intParameter == 0)
				{
					Object.Instantiate<GameObject>(val, val2.position, Quaternion.identity);
				}
				else
				{
					Object.Instantiate<GameObject>(val, val2.position, val2.rotation).transform.parent = val2;
				}
			}
			else if (Object.op_Implicit((Object)(object)val))
			{
				Object.Instantiate<GameObject>(val, ((Component)this).transform.position, ((Component)this).transform.rotation);
			}
		}
		else if (Object.op_Implicit((Object)(object)val))
		{
			Object.Instantiate<GameObject>(val, ((Component)this).transform.position, ((Component)this).transform.rotation);
		}
	}

	public void ItemDrop()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Expected O, but got Unknown
		if (NetworkServer.active && Object.op_Implicit((Object)(object)entityLocator))
		{
			IChestBehavior component = entityLocator.entity.GetComponent<IChestBehavior>();
			if (Object.op_Implicit((Object)(Component)component))
			{
				component.ItemDrop();
			}
			else
			{
				Debug.Log((object)"Parent has no item drops!");
			}
		}
	}

	public void BeginPrint(AnimationEvent animationEvent)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Expected O, but got Unknown
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Expected O, but got Unknown
		if (Object.op_Implicit((Object)(object)meshRenderer))
		{
			Material material = (Material)animationEvent.objectReferenceParameter;
			float floatParameter = animationEvent.floatParameter;
			float maxPrintHeight = animationEvent.intParameter;
			meshRenderer.material = material;
			printTime = 0f;
			MaterialPropertyBlock printPropertyBlock = new MaterialPropertyBlock();
			((MonoBehaviour)this).StartCoroutine(startPrint(floatParameter, maxPrintHeight, printPropertyBlock));
		}
	}

	private IEnumerator startPrint(float maxPrintTime, float maxPrintHeight, MaterialPropertyBlock printPropertyBlock)
	{
		if (Object.op_Implicit((Object)(object)meshRenderer))
		{
			while (printHeight < maxPrintHeight)
			{
				printTime += Time.deltaTime;
				printHeight = printTime / maxPrintTime * maxPrintHeight;
				meshRenderer.GetPropertyBlock(printPropertyBlock);
				printPropertyBlock.Clear();
				printPropertyBlock.SetFloat("_SliceHeight", printHeight);
				meshRenderer.SetPropertyBlock(printPropertyBlock);
				yield return (object)new WaitForEndOfFrame();
			}
		}
	}

	public void SetChildEnable(AnimationEvent animationEvent)
	{
		string stringParameter = animationEvent.stringParameter;
		bool active = animationEvent.intParameter > 0;
		if (Object.op_Implicit((Object)(object)childLocator))
		{
			Transform val = childLocator.FindChild(stringParameter);
			if (Object.op_Implicit((Object)(object)val))
			{
				((Component)val).gameObject.SetActive(active);
			}
		}
	}

	public void SwapMaterial(AnimationEvent animationEvent)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Expected O, but got Unknown
		Material material = (Material)animationEvent.objectReferenceParameter;
		if (Object.op_Implicit((Object)(object)meshRenderer))
		{
			meshRenderer.material = material;
		}
	}
}
