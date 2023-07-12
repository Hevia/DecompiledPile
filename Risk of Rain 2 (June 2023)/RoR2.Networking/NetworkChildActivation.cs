using System;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2.Networking;

public class NetworkChildActivation : NetworkBehaviour
{
	private class GameObjectActiveTracker : MonoBehaviour
	{
		public NetworkChildActivation networkChildActivation;

		public int index = -1;

		private void OnEnable()
		{
			if (Object.op_Implicit((Object)(object)networkChildActivation))
			{
				networkChildActivation.SetChildActiveState(index, active: true);
			}
		}

		private void OnDisable()
		{
			if (Object.op_Implicit((Object)(object)networkChildActivation))
			{
				networkChildActivation.SetChildActiveState(index, active: false);
			}
		}
	}

	[Tooltip("The list of child objects this object will handle activating and deactivating over the network. Client and server must have matching lists (ie DO NOT CHANGE AT RUNTIME.)")]
	public GameObject[] children = Array.Empty<GameObject>();

	private GameObjectActiveTracker[] trackers = Array.Empty<GameObjectActiveTracker>();

	private bool[] childrenActiveStates = Array.Empty<bool>();

	private static readonly uint activeStatesDirtyBit = 1u;

	private static readonly uint allDirtyBits = activeStatesDirtyBit;

	private GameObject[] trackedChildren = Array.Empty<GameObject>();

	private void BuildTrackersForChildren(GameObject[] newTrackedChildren)
	{
		if (trackedChildren == newTrackedChildren)
		{
			return;
		}
		trackedChildren = newTrackedChildren;
		for (int i = 0; i < trackers.Length; i++)
		{
			Object.Destroy((Object)(object)trackers[i]);
		}
		Array.Resize(ref trackers, newTrackedChildren.Length);
		for (int j = 0; j < newTrackedChildren.Length; j++)
		{
			GameObject val = newTrackedChildren[j];
			if (Object.op_Implicit((Object)(object)val))
			{
				GameObjectActiveTracker gameObjectActiveTracker = val.AddComponent<GameObjectActiveTracker>();
				gameObjectActiveTracker.index = j;
				gameObjectActiveTracker.networkChildActivation = this;
				trackers[j] = gameObjectActiveTracker;
				SetChildActiveState(j, val.gameObject.activeInHierarchy);
			}
		}
	}

	private void SetChildActiveState(int index, bool active)
	{
		childrenActiveStates[index] = active;
		((NetworkBehaviour)this).SetDirtyBit(activeStatesDirtyBit);
	}

	private void Awake()
	{
		Array.Resize(ref childrenActiveStates, children.Length);
		Array.Clear(childrenActiveStates, 0, childrenActiveStates.Length);
		if (NetworkServer.active)
		{
			BuildTrackersForChildren(children);
		}
	}

	private void OnDestroy()
	{
		if (NetworkServer.active)
		{
			BuildTrackersForChildren(Array.Empty<GameObject>());
		}
	}

	public override bool OnSerialize(NetworkWriter writer, bool initialState)
	{
		((NetworkBehaviour)this).OnSerialize(writer, initialState);
		uint num = (initialState ? allDirtyBits : ((NetworkBehaviour)this).syncVarDirtyBits);
		writer.WritePackedUInt32(num);
		if ((num & activeStatesDirtyBit) != 0)
		{
			writer.WriteBitArray(childrenActiveStates, childrenActiveStates.Length);
		}
		return num != 0;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		((NetworkBehaviour)this).OnDeserialize(reader, initialState);
		if ((reader.ReadPackedUInt32() & activeStatesDirtyBit) == 0)
		{
			return;
		}
		reader.ReadBitArray(childrenActiveStates);
		for (int i = 0; i < childrenActiveStates.Length; i++)
		{
			GameObject val = children[i];
			if (Object.op_Implicit((Object)(object)val))
			{
				try
				{
					val.SetActive(childrenActiveStates[i]);
				}
				catch (Exception ex)
				{
					Debug.LogError((object)ex);
				}
			}
		}
	}

	private void UNetVersion()
	{
	}

	public override void PreStartClient()
	{
	}
}
