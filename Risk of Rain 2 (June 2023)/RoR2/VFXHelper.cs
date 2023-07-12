using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace RoR2;

public class VFXHelper : IDisposable
{
	private class VFXTransformController : MonoBehaviour
	{
		public Transform followedTransform;

		public bool usePosition;

		public bool useRotation;

		public bool useScale;

		protected Transform transform { get; private set; }

		private void Awake()
		{
			transform = ((Component)this).transform;
		}

		private void LateUpdate()
		{
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			if (Object.op_Implicit((Object)(object)followedTransform))
			{
				if (usePosition)
				{
					transform.position = followedTransform.position;
				}
				if (useRotation)
				{
					transform.rotation = followedTransform.rotation;
				}
				if (useScale)
				{
					transform.localScale = followedTransform.lossyScale;
				}
			}
		}
	}

	private static readonly Stack<VFXHelper> pool = new Stack<VFXHelper>();

	private GameObject _vfxPrefabReference;

	private object _vfxPrefabAddress;

	public Transform followedTransform;

	public bool useFollowedTransformPosition = true;

	public bool useFollowedTransformRotation = true;

	public bool useFollowedTransformScale = true;

	public GameObject vfxPrefabReference
	{
		get
		{
			return _vfxPrefabReference;
		}
		set
		{
			_vfxPrefabReference = value;
			_vfxPrefabAddress = null;
		}
	}

	public object vfxPrefabAddress
	{
		get
		{
			return _vfxPrefabAddress;
		}
		set
		{
			_vfxPrefabReference = null;
			_vfxPrefabAddress = null;
		}
	}

	public GameObject vfxInstance { get; private set; }

	public Transform vfxInstanceTransform { get; private set; }

	private VFXTransformController vfxInstanceTransformController { get; set; }

	public bool enabled
	{
		get
		{
			return Object.op_Implicit((Object)(object)vfxInstance);
		}
		set
		{
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0078: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			//IL_0086: Unknown result type (might be due to invalid IL or missing references)
			//IL_0091: Unknown result type (might be due to invalid IL or missing references)
			//IL_0092: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
			if (Object.op_Implicit((Object)(object)vfxInstance) == value)
			{
				return;
			}
			if (value)
			{
				vfxInstance = null;
				vfxInstanceTransform = null;
				Vector3 val = Vector3.zero;
				Quaternion val2 = Quaternion.identity;
				Vector3 val3 = Vector3.one;
				if (Object.op_Implicit((Object)(object)followedTransform))
				{
					val = (useFollowedTransformPosition ? followedTransform.position : val);
					val2 = (useFollowedTransformRotation ? followedTransform.rotation : val2);
					val3 = (useFollowedTransformScale ? followedTransform.lossyScale : val3);
				}
				if (vfxPrefabAddress != null)
				{
					InstantiationParameters val4 = default(InstantiationParameters);
					((InstantiationParameters)(ref val4))._002Ector(val, val2, (Transform)null);
					vfxInstance = Addressables.InstantiateAsync(vfxPrefabAddress, val4, true).WaitForCompletion();
				}
				else if (Object.op_Implicit((Object)(object)vfxPrefabReference))
				{
					vfxInstance = Object.Instantiate<GameObject>(vfxPrefabReference, val, val2);
				}
				if (Object.op_Implicit((Object)(object)vfxInstance))
				{
					vfxInstanceTransform = vfxInstance.transform;
					vfxInstanceTransform.localScale = val3;
					vfxInstanceTransformController = vfxInstance.AddComponent<VFXTransformController>();
					vfxInstanceTransformController.usePosition = useFollowedTransformPosition;
					vfxInstanceTransformController.useRotation = useFollowedTransformRotation;
					vfxInstanceTransformController.useScale = useFollowedTransformScale;
				}
			}
			else
			{
				VfxKillBehavior.KillVfxObject(vfxInstance);
				vfxInstance = null;
				vfxInstanceTransform = null;
				vfxInstanceTransformController = null;
			}
		}
	}

	private VFXHelper()
	{
	}

	public static VFXHelper Rent()
	{
		if (pool.Count <= 0)
		{
			return new VFXHelper();
		}
		return pool.Pop();
	}

	public static VFXHelper Return(VFXHelper instance)
	{
		instance.Dispose();
		pool.Push(instance);
		return null;
	}

	public void Dispose()
	{
		enabled = false;
		vfxPrefabAddress = null;
		vfxPrefabReference = null;
		followedTransform = null;
		useFollowedTransformPosition = true;
		useFollowedTransformRotation = true;
		useFollowedTransformScale = true;
	}
}
