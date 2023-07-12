using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.GrandParent;

public class ChannelSunStart : ChannelSunBase
{
	public static string animLayerName;

	public static string animStateName;

	public static string animPlaybackRateParam;

	public static string beginSoundName;

	public static float baseDuration;

	public static GameObject beamVfxPrefab;

	private float duration;

	private Vector3? sunSpawnPosition;

	private ParticleSystem leftHandBeamParticleSystem;

	private ParticleSystem rightHandBeamParticleSystem;

	public override void OnEnter()
	{
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		base.OnEnter();
		duration = baseDuration / attackSpeedStat;
		PlayAnimation(animLayerName, animStateName, animPlaybackRateParam, duration);
		Transform modelTransform = GetModelTransform();
		if (Object.op_Implicit((Object)(object)modelTransform))
		{
			AimAnimator component = ((Component)modelTransform).GetComponent<AimAnimator>();
			if (Object.op_Implicit((Object)(object)component))
			{
				((Behaviour)component).enabled = true;
			}
		}
		if (base.isAuthority)
		{
			sunSpawnPosition = sunSpawnPosition ?? ChannelSun.FindSunSpawnPosition(base.transform.position);
		}
		ChildLocator modelChildLocator = GetModelChildLocator();
		if (Object.op_Implicit((Object)(object)modelChildLocator) && sunSpawnPosition.HasValue)
		{
			CreateBeamEffect(modelChildLocator, ChannelSunBase.leftHandVfxTargetNameInChildLocator, sunSpawnPosition.Value, ref leftHandBeamParticleSystem);
			CreateBeamEffect(modelChildLocator, ChannelSunBase.rightHandVfxTargetNameInChildLocator, sunSpawnPosition.Value, ref rightHandBeamParticleSystem);
		}
		if (beginSoundName != null)
		{
			Util.PlaySound(beginSoundName, base.gameObject);
		}
	}

	public override void OnExit()
	{
		EndBeamEffect(ref leftHandBeamParticleSystem);
		EndBeamEffect(ref rightHandBeamParticleSystem);
		base.OnExit();
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.isAuthority && base.fixedAge >= duration)
		{
			outer.SetNextState(new ChannelSun
			{
				activatorSkillSlot = base.activatorSkillSlot,
				sunSpawnPosition = sunSpawnPosition
			});
		}
	}

	public override void OnSerialize(NetworkWriter writer)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		base.OnSerialize(writer);
		writer.Write(sunSpawnPosition.HasValue);
		if (sunSpawnPosition.HasValue)
		{
			writer.Write(sunSpawnPosition.Value);
		}
	}

	public override void OnDeserialize(NetworkReader reader)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		base.OnDeserialize(reader);
		sunSpawnPosition = null;
		if (reader.ReadBoolean())
		{
			sunSpawnPosition = reader.ReadVector3();
		}
	}

	private void CreateBeamEffect(ChildLocator childLocator, string nameInChildLocator, Vector3 targetPosition, ref ParticleSystem dest)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		Transform val = childLocator.FindChild(nameInChildLocator);
		if (Object.op_Implicit((Object)(object)val))
		{
			ChildLocator component = Object.Instantiate<GameObject>(beamVfxPrefab, val).GetComponent<ChildLocator>();
			component.FindChild("EndPoint").SetPositionAndRotation(targetPosition, Quaternion.identity);
			Transform val2 = component.FindChild("BeamParticles");
			dest = ((Component)val2).GetComponent<ParticleSystem>();
		}
		else
		{
			dest = null;
		}
	}

	private void EndBeamEffect(ref ParticleSystem particleSystem)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)particleSystem))
		{
			MainModule main = particleSystem.main;
			((MainModule)(ref main)).loop = false;
			particleSystem.Stop();
		}
		particleSystem = null;
	}
}
