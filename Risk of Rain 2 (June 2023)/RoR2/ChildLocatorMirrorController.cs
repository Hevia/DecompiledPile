using System.Collections.Generic;
using UnityEngine;

namespace RoR2;

public class ChildLocatorMirrorController : MonoBehaviour
{
	private class MirrorPair
	{
		public Transform referenceTransform;

		public Transform targetTransform;
	}

	[SerializeField]
	[Tooltip("The ChildLocator we are using are a reference to GET the transform information")]
	private ChildLocator _referenceLocator;

	[SerializeField]
	[Tooltip("The ChildLocator we are targeting to SET the transform information")]
	private ChildLocator _targetLocator;

	private List<MirrorPair> mirrorPairs = new List<MirrorPair>();

	public ChildLocator referenceLocator
	{
		get
		{
			return _referenceLocator;
		}
		set
		{
			_referenceLocator = value;
			mirrorPairs.Clear();
			if (Object.op_Implicit((Object)(object)_referenceLocator) && Object.op_Implicit((Object)(object)_targetLocator))
			{
				RebuildMirrorPairs();
			}
		}
	}

	public ChildLocator targetLocator
	{
		get
		{
			return _targetLocator;
		}
		set
		{
			_targetLocator = value;
			mirrorPairs.Clear();
			if (Object.op_Implicit((Object)(object)_referenceLocator) && Object.op_Implicit((Object)(object)_targetLocator))
			{
				RebuildMirrorPairs();
			}
		}
	}

	private void Start()
	{
		if (Object.op_Implicit((Object)(object)_referenceLocator) && Object.op_Implicit((Object)(object)_targetLocator))
		{
			RebuildMirrorPairs();
		}
	}

	private void FixedUpdate()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		foreach (MirrorPair mirrorPair in mirrorPairs)
		{
			if (Object.op_Implicit((Object)(object)mirrorPair.referenceTransform) && Object.op_Implicit((Object)(object)mirrorPair.targetTransform))
			{
				mirrorPair.targetTransform.position = mirrorPair.referenceTransform.position;
				mirrorPair.targetTransform.rotation = mirrorPair.referenceTransform.rotation;
			}
		}
	}

	private void RebuildMirrorPairs()
	{
		mirrorPairs.Clear();
		for (int i = 0; i < _targetLocator.Count; i++)
		{
			string childName = _targetLocator.FindChildName(i);
			Transform val = _targetLocator.FindChild(i);
			Transform val2 = _referenceLocator.FindChild(childName);
			if (Object.op_Implicit((Object)(object)val) && Object.op_Implicit((Object)(object)val2))
			{
				mirrorPairs.Add(new MirrorPair
				{
					targetTransform = val,
					referenceTransform = val2
				});
			}
		}
	}
}
