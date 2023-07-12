using System;
using UnityEngine;

namespace RoR2;

public class BlueprintController : MonoBehaviour
{
	[NonSerialized]
	public bool ok;

	public Material okMaterial;

	public Material invalidMaterial;

	public Renderer[] renderers;

	private Transform transform;

	private void Awake()
	{
		transform = ((Component)this).transform;
	}

	private void Update()
	{
		Material sharedMaterial = (ok ? okMaterial : invalidMaterial);
		for (int i = 0; i < renderers.Length; i++)
		{
			renderers[i].sharedMaterial = sharedMaterial;
		}
	}

	public void PushState(Vector3 position, Quaternion rotation, bool ok)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		transform.position = position;
		transform.rotation = rotation;
		this.ok = ok;
	}
}
