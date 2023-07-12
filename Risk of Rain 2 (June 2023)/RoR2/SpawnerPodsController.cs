using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Serialization;

namespace RoR2;

[RequireComponent(typeof(CharacterBody))]
public class SpawnerPodsController : MonoBehaviour
{
	[FormerlySerializedAs("maxSpawnTimer")]
	public float incubationDuration;

	public GameObject spawnEffect;

	public GameObject hatchEffect;

	[FormerlySerializedAs("dissolveTime")]
	public float dissolveDuration;

	public string podSpawnSound = "";

	public string podHatchSound = "";

	private CharacterMaster ownerMaster;

	private CharacterBody characterBody;

	private void Awake()
	{
		characterBody = ((Component)this).GetComponent<CharacterBody>();
	}

	private void Start()
	{
		ownerMaster = characterBody.master.minionOwnership.ownerMaster;
		if (NetworkServer.active)
		{
			Deployable component = ((Component)this).GetComponent<Deployable>();
			if (Object.op_Implicit((Object)(object)ownerMaster))
			{
				ownerMaster.AddDeployable(component, DeployableSlot.ParentPodAlly);
			}
		}
	}

	public void UndeployKill()
	{
		characterBody.healthComponent.Suicide();
	}

	public void Dissolve()
	{
		PrintController printController = ((Component)characterBody.modelLocator.modelTransform).gameObject.AddComponent<PrintController>();
		printController.printTime = dissolveDuration;
		((Behaviour)printController).enabled = true;
		printController.startingPrintHeight = 99999f;
		printController.maxPrintHeight = 99999f;
		printController.startingPrintBias = 0.95f;
		printController.maxPrintBias = 1.95f;
		printController.animateFlowmapPower = true;
		printController.startingFlowmapPower = 1.14f;
		printController.maxFlowmapPower = 30f;
		printController.disableWhenFinished = false;
		printController.printCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
	}
}
