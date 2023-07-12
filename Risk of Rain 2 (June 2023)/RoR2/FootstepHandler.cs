using UnityEngine;

namespace RoR2;

[RequireComponent(typeof(ChildLocator))]
public class FootstepHandler : MonoBehaviour
{
	public string baseFootstepString;

	public string baseFootliftString;

	public string sprintFootstepOverrideString;

	public string sprintFootliftOverrideString;

	public bool enableFootstepDust;

	public GameObject footstepDustPrefab;

	private ChildLocator childLocator;

	private Inventory bodyInventory;

	private Animator animator;

	private Transform footstepDustInstanceTransform;

	private ParticleSystem footstepDustInstanceParticleSystem;

	private ShakeEmitter footstepDustInstanceShakeEmitter;

	private CharacterBody body;

	private void Start()
	{
		childLocator = ((Component)this).GetComponent<ChildLocator>();
		if (Object.op_Implicit((Object)(object)((Component)this).GetComponent<CharacterModel>()))
		{
			body = ((Component)this).GetComponent<CharacterModel>().body;
			bodyInventory = (Object.op_Implicit((Object)(object)body) ? body.inventory : null);
		}
		animator = ((Component)this).GetComponent<Animator>();
		if (enableFootstepDust)
		{
			footstepDustInstanceTransform = Object.Instantiate<GameObject>(footstepDustPrefab, ((Component)this).transform).transform;
			footstepDustInstanceParticleSystem = ((Component)footstepDustInstanceTransform).GetComponent<ParticleSystem>();
			footstepDustInstanceShakeEmitter = ((Component)footstepDustInstanceTransform).GetComponent<ShakeEmitter>();
		}
	}

	public void Footstep(AnimationEvent animationEvent)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Expected O, but got Unknown
		AnimatorClipInfo animatorClipInfo = animationEvent.animatorClipInfo;
		if ((double)((AnimatorClipInfo)(ref animatorClipInfo)).weight > 0.5)
		{
			Footstep(animationEvent.stringParameter, (GameObject)animationEvent.objectReferenceParameter);
		}
	}

	public void Footstep(string childName, GameObject footstepEffect)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0208: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_021d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)body))
		{
			return;
		}
		Transform val = childLocator.FindChild(childName);
		int childIndex = childLocator.FindChildIndex(childName);
		if (Object.op_Implicit((Object)(object)val))
		{
			Color val2 = Color.gray;
			RaycastHit val3 = default(RaycastHit);
			Vector3 position = val.position;
			position.y += 1.5f;
			Debug.DrawRay(position, Vector3.down);
			if (Physics.Raycast(new Ray(position, Vector3.down), ref val3, 4f, LayerMask.op_Implicit(LayerIndex.world.mask) | LayerMask.op_Implicit(LayerIndex.water.mask), (QueryTriggerInteraction)2))
			{
				if (Object.op_Implicit((Object)(object)bodyInventory) && bodyInventory.GetItemCount(RoR2Content.Items.Hoof) > 0 && childName == "FootR")
				{
					Util.PlaySound("Play_item_proc_hoof", ((Component)body).gameObject);
				}
				if (Object.op_Implicit((Object)(object)footstepEffect))
				{
					EffectData effectData = new EffectData();
					effectData.origin = ((RaycastHit)(ref val3)).point;
					effectData.rotation = Util.QuaternionSafeLookRotation(((RaycastHit)(ref val3)).normal);
					effectData.SetChildLocatorTransformReference(((Component)body).gameObject, childIndex);
					EffectManager.SpawnEffect(footstepEffect, effectData, transmit: false);
				}
				SurfaceDef objectSurfaceDef = SurfaceDefProvider.GetObjectSurfaceDef(((RaycastHit)(ref val3)).collider, ((RaycastHit)(ref val3)).point);
				bool flag = false;
				if (Object.op_Implicit((Object)(object)objectSurfaceDef))
				{
					val2 = objectSurfaceDef.approximateColor;
					if (Object.op_Implicit((Object)(object)objectSurfaceDef.footstepEffectPrefab))
					{
						EffectManager.SpawnEffect(objectSurfaceDef.footstepEffectPrefab, new EffectData
						{
							origin = ((RaycastHit)(ref val3)).point,
							scale = body.radius
						}, transmit: false);
						flag = true;
					}
					if (!string.IsNullOrEmpty(objectSurfaceDef.materialSwitchString))
					{
						AkSoundEngine.SetSwitch("material", objectSurfaceDef.materialSwitchString, ((Component)body).gameObject);
					}
				}
				else
				{
					Debug.LogFormat("{0} is missing surface def", new object[1] { ((Component)((RaycastHit)(ref val3)).collider).gameObject });
				}
				if (Object.op_Implicit((Object)(object)footstepDustInstanceTransform) && !flag)
				{
					footstepDustInstanceTransform.position = ((RaycastHit)(ref val3)).point;
					MainModule main = footstepDustInstanceParticleSystem.main;
					((MainModule)(ref main)).startColor = MinMaxGradient.op_Implicit(val2);
					footstepDustInstanceParticleSystem.Play();
					if (Object.op_Implicit((Object)(object)footstepDustInstanceShakeEmitter))
					{
						footstepDustInstanceShakeEmitter.StartShake();
					}
				}
			}
			Util.PlaySound((!string.IsNullOrEmpty(sprintFootstepOverrideString) && body.isSprinting) ? sprintFootstepOverrideString : baseFootstepString, ((Component)body).gameObject);
		}
		else
		{
			Debug.LogWarningFormat("Object {0} lacks ChildLocator entry \"{1}\" to handle Footstep event!", new object[2]
			{
				((Object)((Component)this).gameObject).name,
				childName
			});
		}
	}

	public void Footlift(AnimationEvent animationEvent)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		AnimatorClipInfo animatorClipInfo = animationEvent.animatorClipInfo;
		if ((double)((AnimatorClipInfo)(ref animatorClipInfo)).weight > 0.5)
		{
			Footlift();
		}
	}

	public void Footlift()
	{
		Util.PlaySound((!string.IsNullOrEmpty(sprintFootliftOverrideString) && body.isSprinting) ? sprintFootliftOverrideString : baseFootliftString, ((Component)body).gameObject);
	}
}
