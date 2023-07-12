using System.Collections.Generic;
using HG;

namespace RoR2.Achievements;

[RegisterAchievement("CaptainVisitSeveralStages", "Skills.Captain.CaptainSupplyDropEquipmentRestock", "CompleteMainEnding", null)]
public class CaptainVisitSeveralStagesAchievement : BaseAchievement
{
	private static readonly int requirement = 10;

	private List<SceneDef> visitedScenes;

	protected override BodyIndex LookUpRequiredBodyIndex()
	{
		return BodyCatalog.FindBodyIndex("CaptainBody");
	}

	protected override void OnBodyRequirementMet()
	{
		base.OnBodyRequirementMet();
		visitedScenes = CollectionPool<SceneDef, List<SceneDef>>.RentCollection();
		SceneCatalog.onMostRecentSceneDefChanged += HandleMostRecentSceneDefChanged;
	}

	protected override void OnBodyRequirementBroken()
	{
		SceneCatalog.onMostRecentSceneDefChanged -= HandleMostRecentSceneDefChanged;
		visitedScenes = CollectionPool<SceneDef, List<SceneDef>>.ReturnCollection(visitedScenes);
		base.OnBodyRequirementBroken();
	}

	private void HandleMostRecentSceneDefChanged(SceneDef newSceneDef)
	{
		if (!visitedScenes.Contains(newSceneDef))
		{
			visitedScenes.Add(newSceneDef);
		}
		if (visitedScenes.Count >= requirement)
		{
			Grant();
		}
	}
}
