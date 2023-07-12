using System.Runtime.InteropServices;
using UnityEngine;

namespace RoR2.DirectionalSearch;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct PickupSearchSelector : IGenericWorldSearchSelector<GenericPickupController>
{
	public Transform GetTransform(GenericPickupController source)
	{
		return ((Component)source.pickupDisplay).transform;
	}

	public GameObject GetRootObject(GenericPickupController source)
	{
		return ((Component)source).gameObject;
	}
}
