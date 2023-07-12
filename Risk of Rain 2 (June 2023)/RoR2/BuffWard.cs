using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace RoR2;

[RequireComponent(typeof(TeamFilter))]
public class BuffWard : NetworkBehaviour
{
	public enum BuffWardShape
	{
		Sphere,
		VerticalTube,
		Count
	}

	[Tooltip("The shape of the area")]
	public BuffWardShape shape;

	[Tooltip("The area of effect.")]
	[SyncVar]
	public float radius;

	[Tooltip("How long between buff pulses in the area of effect.")]
	public float interval = 1f;

	[Tooltip("The child range indicator object. Will be scaled to the radius.")]
	public Transform rangeIndicator;

	[Tooltip("The buff type to grant")]
	public BuffDef buffDef;

	[Tooltip("The buff duration")]
	public float buffDuration;

	[Tooltip("Should the ward be floored on start")]
	public bool floorWard;

	[Tooltip("Does the ward disappear over time?")]
	public bool expires;

	[Tooltip("If set, applies to all teams BUT the one selected.")]
	public bool invertTeamFilter;

	public float expireDuration;

	public bool animateRadius;

	public AnimationCurve radiusCoefficientCurve;

	[Tooltip("If set, the ward will give you this amount of time to play removal effects.")]
	public float removalTime;

	private bool needsRemovalTime;

	public string removalSoundString = "";

	public UnityEvent onRemoval;

	public bool requireGrounded;

	private TeamFilter teamFilter;

	private float buffTimer;

	private float rangeIndicatorScaleVelocity;

	private float stopwatch;

	private float calculatedRadius;

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
		teamFilter = ((Component)this).GetComponent<TeamFilter>();
	}

	private void OnEnable()
	{
		if (Object.op_Implicit((Object)(object)rangeIndicator))
		{
			((Component)rangeIndicator).gameObject.SetActive(true);
		}
	}

	private void OnDisable()
	{
		if (Object.op_Implicit((Object)(object)rangeIndicator))
		{
			((Component)rangeIndicator).gameObject.SetActive(false);
		}
	}

	private void Start()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		if (removalTime > 0f)
		{
			needsRemovalTime = true;
		}
		RaycastHit val = default(RaycastHit);
		if (floorWard && Physics.Raycast(((Component)this).transform.position, Vector3.down, ref val, 500f, LayerMask.op_Implicit(LayerIndex.world.mask)))
		{
			((Component)this).transform.position = ((RaycastHit)(ref val)).point;
			((Component)this).transform.up = ((RaycastHit)(ref val)).normal;
		}
		if (Object.op_Implicit((Object)(object)rangeIndicator) && expires)
		{
			ScaleParticleSystemDuration component = ((Component)rangeIndicator).GetComponent<ScaleParticleSystemDuration>();
			if (Object.op_Implicit((Object)(object)component))
			{
				component.newDuration = expireDuration;
			}
		}
	}

	private void Update()
	{
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		calculatedRadius = (animateRadius ? (radius * radiusCoefficientCurve.Evaluate(stopwatch / expireDuration)) : radius);
		stopwatch += Time.deltaTime;
		if (expires && NetworkServer.active)
		{
			if (needsRemovalTime)
			{
				if (stopwatch >= expireDuration - removalTime)
				{
					needsRemovalTime = false;
					Util.PlaySound(removalSoundString, ((Component)this).gameObject);
					onRemoval.Invoke();
				}
			}
			else if (expireDuration <= stopwatch)
			{
				Object.Destroy((Object)(object)((Component)this).gameObject);
			}
		}
		if (Object.op_Implicit((Object)(object)rangeIndicator))
		{
			float num = Mathf.SmoothDamp(rangeIndicator.localScale.x, calculatedRadius, ref rangeIndicatorScaleVelocity, 0.2f);
			rangeIndicator.localScale = new Vector3(num, num, num);
		}
	}

	private void FixedUpdate()
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active)
		{
			return;
		}
		buffTimer -= Time.fixedDeltaTime;
		if (!(buffTimer <= 0f))
		{
			return;
		}
		buffTimer = interval;
		float radiusSqr = calculatedRadius * calculatedRadius;
		Vector3 position = ((Component)this).transform.position;
		if (invertTeamFilter)
		{
			for (TeamIndex teamIndex = TeamIndex.Neutral; teamIndex < TeamIndex.Count; teamIndex++)
			{
				if (teamIndex != teamFilter.teamIndex)
				{
					BuffTeam(TeamComponent.GetTeamMembers(teamIndex), radiusSqr, position);
				}
			}
		}
		else
		{
			BuffTeam(TeamComponent.GetTeamMembers(teamFilter.teamIndex), radiusSqr, position);
		}
	}

	private void BuffTeam(IEnumerable<TeamComponent> recipients, float radiusSqr, Vector3 currentPosition)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		if (!NetworkServer.active || !Object.op_Implicit((Object)(object)buffDef))
		{
			return;
		}
		foreach (TeamComponent recipient in recipients)
		{
			Vector3 val = ((Component)recipient).transform.position - currentPosition;
			if (shape == BuffWardShape.VerticalTube)
			{
				val.y = 0f;
			}
			if (((Vector3)(ref val)).sqrMagnitude <= radiusSqr)
			{
				CharacterBody component = ((Component)recipient).GetComponent<CharacterBody>();
				if (Object.op_Implicit((Object)(object)component) && (!requireGrounded || !Object.op_Implicit((Object)(object)component.characterMotor) || component.characterMotor.isGrounded))
				{
					component.AddTimedBuff(buffDef.buffIndex, buffDuration);
				}
			}
		}
	}

	private void OnValidate()
	{
		if (!Object.op_Implicit((Object)(object)buffDef))
		{
			Debug.LogWarningFormat((Object)(object)this, "BuffWard {0} has no buff specified.", new object[1] { this });
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
