using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace RoR2.Projectile;

[RequireComponent(typeof(ProjectileController))]
public class ProjectileFuse : MonoBehaviour
{
	public float fuse;

	public UnityEvent onFuse;

	private void Awake()
	{
		if (!NetworkServer.active)
		{
			((Behaviour)this).enabled = false;
		}
	}

	private void FixedUpdate()
	{
		fuse -= Time.fixedDeltaTime;
		if (fuse <= 0f)
		{
			((Behaviour)this).enabled = false;
			onFuse.Invoke();
		}
	}
}
