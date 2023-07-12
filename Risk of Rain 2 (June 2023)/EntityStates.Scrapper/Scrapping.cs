using RoR2;
using UnityEngine;

namespace EntityStates.Scrapper;

public class Scrapping : ScrapperBaseState
{
	public static string enterSoundString;

	public static string exitSoundString;

	public static float duration;

	protected override bool enableInteraction => false;

	public override void OnEnter()
	{
		base.OnEnter();
		((Component)FindModelChild("ScrappingEffect")).gameObject.SetActive(true);
		Util.PlaySound(enterSoundString, base.gameObject);
		PlayAnimation("Base", "Scrapping", "Scrapping.playbackRate", duration);
	}

	public override void OnExit()
	{
		((Component)FindModelChild("ScrappingEffect")).gameObject.SetActive(false);
		Util.PlaySound(exitSoundString, base.gameObject);
		base.OnExit();
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (base.fixedAge > duration)
		{
			outer.SetNextState(new ScrappingToIdle());
		}
	}
}
