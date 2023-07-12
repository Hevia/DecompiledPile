using RoR2;
using UnityEngine;

namespace EntityStates.BrotherMonster;

public class StaggerEnter : StaggerBaseState
{
	public static GameObject effectPrefab;

	public static string effectMuzzleString;

	public override EntityState nextState => new StaggerLoop();

	public override void OnEnter()
	{
		base.OnEnter();
		PlayAnimation("Body", "StaggerEnter", "Stagger.playbackRate", duration);
		if (Object.op_Implicit((Object)(object)effectPrefab))
		{
			EffectManager.SimpleMuzzleFlash(effectPrefab, base.gameObject, effectMuzzleString, transmit: false);
		}
	}
}
