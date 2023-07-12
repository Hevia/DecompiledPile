using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public class MasterSuicideOnTimer : MonoBehaviour
{
	public float lifeTimer;

	[Tooltip("Reset the timer if we're within this radius of the owner")]
	public float timerResetDistanceToOwner;

	public MinionOwnership minionOwnership;

	private CharacterBody body;

	private CharacterBody ownerBody;

	private float timer;

	private bool hasDied;

	private CharacterMaster characterMaster;

	private void Start()
	{
		characterMaster = ((Component)this).GetComponent<CharacterMaster>();
		if (Object.op_Implicit((Object)(object)minionOwnership) && Object.op_Implicit((Object)(object)minionOwnership.ownerMaster))
		{
			ownerBody = minionOwnership.ownerMaster.GetBody();
		}
	}

	private void FixedUpdate()
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			return;
		}
		if (Object.op_Implicit((Object)(object)ownerBody))
		{
			if (!Object.op_Implicit((Object)(object)body))
			{
				body = characterMaster.GetBody();
			}
			if (Object.op_Implicit((Object)(object)body))
			{
				Vector3 val = ((Component)ownerBody).transform.position - ((Component)body).transform.position;
				if (((Vector3)(ref val)).sqrMagnitude < timerResetDistanceToOwner * timerResetDistanceToOwner)
				{
					timer = 0f;
				}
			}
		}
		timer += Time.fixedDeltaTime;
		if (!(timer >= lifeTimer) || hasDied)
		{
			return;
		}
		GameObject bodyObject = characterMaster.GetBodyObject();
		if (Object.op_Implicit((Object)(object)bodyObject))
		{
			HealthComponent component = bodyObject.GetComponent<HealthComponent>();
			if (Object.op_Implicit((Object)(object)component))
			{
				component.Suicide();
				hasDied = true;
			}
		}
	}
}
