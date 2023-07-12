using UnityEngine;
using UnityEngine.Networking;

namespace RoR2.Projectile;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(ProjectileController))]
public class ProjectileCharacterController : MonoBehaviour
{
	private Vector3 downVector;

	public float velocity;

	public float lifetime = 5f;

	private float timer;

	private ProjectileController projectileController;

	private CharacterController characterController;

	private void Awake()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		downVector = Vector3.down * 3f;
		projectileController = ((Component)this).GetComponent<ProjectileController>();
		characterController = ((Component)this).GetComponent<CharacterController>();
	}

	private void FixedUpdate()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		if (NetworkServer.active || projectileController.isPrediction)
		{
			characterController.Move((((Component)this).transform.forward + downVector) * (velocity * Time.fixedDeltaTime));
		}
		if (NetworkServer.active)
		{
			timer += Time.fixedDeltaTime;
			if (timer > lifetime)
			{
				Object.Destroy((Object)(object)((Component)this).gameObject);
			}
		}
	}
}
