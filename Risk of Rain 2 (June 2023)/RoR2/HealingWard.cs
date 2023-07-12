using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

[RequireComponent(typeof(TeamFilter))]
public class HealingWard : NetworkBehaviour
{
	[SyncVar]
	[Tooltip("The area of effect.")]
	public float radius;

	[Tooltip("How long between heal pulses in the area of effect.")]
	public float interval = 1f;

	[Tooltip("How many hit points to restore each pulse.")]
	public float healPoints;

	[Tooltip("What fraction of the healee max health to restore each pulse.")]
	public float healFraction;

	[Tooltip("The child range indicator object. Will be scaled to the radius.")]
	public Transform rangeIndicator;

	[Tooltip("Should the ward be floored on start")]
	public bool floorWard;

	private TeamFilter teamFilter;

	private float healTimer;

	private float rangeIndicatorScaleVelocity;

	public float Networkradius
	{
		get
		{
			return radius;
		}
		[param: In]
		set
		{
			((NetworkBehaviour)this).SetSyncVar<float>(value, ref radius, 1u);
		}
	}

	private void Awake()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		teamFilter = ((Component)this).GetComponent<TeamFilter>();
		RaycastHit val = default(RaycastHit);
		if (NetworkServer.active && floorWard && Physics.Raycast(((Component)this).transform.position, Vector3.down, ref val, 500f, LayerMask.op_Implicit(LayerIndex.world.mask)))
		{
			((Component)this).transform.position = ((RaycastHit)(ref val)).point;
			((Component)this).transform.up = ((RaycastHit)(ref val)).normal;
		}
	}

	private void Update()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)rangeIndicator))
		{
			float num = Mathf.SmoothDamp(rangeIndicator.localScale.x, radius, ref rangeIndicatorScaleVelocity, 0.2f);
			rangeIndicator.localScale = new Vector3(num, num, num);
		}
	}

	private void FixedUpdate()
	{
		healTimer -= Time.fixedDeltaTime;
		if (healTimer <= 0f && NetworkServer.active)
		{
			healTimer = interval;
			HealOccupants();
		}
	}

	private void HealOccupants()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		ReadOnlyCollection<TeamComponent> teamMembers = TeamComponent.GetTeamMembers(teamFilter.teamIndex);
		float num = radius * radius;
		Vector3 position = ((Component)this).transform.position;
		for (int i = 0; i < teamMembers.Count; i++)
		{
			Vector3 val = ((Component)teamMembers[i]).transform.position - position;
			if (!(((Vector3)(ref val)).sqrMagnitude <= num))
			{
				continue;
			}
			HealthComponent component = ((Component)teamMembers[i]).GetComponent<HealthComponent>();
			if (Object.op_Implicit((Object)(object)component))
			{
				float num2 = healPoints + component.fullHealth * healFraction;
				if (num2 > 0f)
				{
					component.Heal(num2, default(ProcChainMask));
				}
			}
		}
	}

	private void UNetVersion()
	{
	}

	public override bool OnSerialize(NetworkWriter writer, bool forceAll)
	{
		if (forceAll)
		{
			writer.Write(radius);
			return true;
		}
		bool flag = false;
		if ((((NetworkBehaviour)this).syncVarDirtyBits & (true ? 1u : 0u)) != 0)
		{
			if (!flag)
			{
				writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
				flag = true;
			}
			writer.Write(radius);
		}
		if (!flag)
		{
			writer.WritePackedUInt32(((NetworkBehaviour)this).syncVarDirtyBits);
		}
		return flag;
	}

	public override void OnDeserialize(NetworkReader reader, bool initialState)
	{
		if (initialState)
		{
			radius = reader.ReadSingle();
			return;
		}
		int num = (int)reader.ReadPackedUInt32();
		if (((uint)num & (true ? 1u : 0u)) != 0)
		{
			radius = reader.ReadSingle();
		}
	}

	public override void PreStartClient()
	{
	}
}
