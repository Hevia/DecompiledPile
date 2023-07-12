using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace EntityStates.Engi.SpiderMine;

public class BaseSpiderMineState : BaseState
{
	[SerializeField]
	public string enterSoundString;

	[SerializeField]
	public string childLocatorStringToEnable;

	protected ProjectileStickOnImpact projectileStickOnImpact { get; private set; }

	protected ProjectileTargetComponent projectileTargetComponent { get; private set; }

	protected ProjectileGhostController projectileGhostController { get; private set; }

	protected virtual bool shouldStick => false;

	public override void OnEnter()
	{
		base.OnEnter();
		projectileStickOnImpact = GetComponent<ProjectileStickOnImpact>();
		projectileTargetComponent = GetComponent<ProjectileTargetComponent>();
		projectileGhostController = base.projectileController.ghost;
		if (Object.op_Implicit((Object)(object)base.modelLocator) && Object.op_Implicit((Object)(object)projectileGhostController))
		{
			base.modelLocator.modelBaseTransform = ((Component)projectileGhostController).transform;
			base.modelLocator.modelTransform = base.modelLocator.modelBaseTransform.Find("mdlEngiSpiderMine");
		}
		if (((Behaviour)projectileStickOnImpact).enabled != shouldStick)
		{
			((Behaviour)projectileStickOnImpact).enabled = shouldStick;
		}
		Transform val = FindModelChild(childLocatorStringToEnable);
		if (Object.op_Implicit((Object)(object)val))
		{
			((Component)val).gameObject.SetActive(true);
		}
		Util.PlaySound(enterSoundString, base.gameObject);
	}

	protected void EmitDustEffect()
	{
		if (Object.op_Implicit((Object)(object)projectileGhostController))
		{
			((Component)((Component)projectileGhostController).transform.Find("Ring")).GetComponent<ParticleSystem>().Play();
		}
	}
}
