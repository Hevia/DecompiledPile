using System.Collections.Generic;
using EntityStates.Interactables.GoldBeacon;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2;

public class GoldshoresMissionController : MonoBehaviour
{
	public Xoroshiro128Plus rng;

	public EntityStateMachine entityStateMachine;

	public GameObject beginTransitionIntoBossFightEffect;

	public GameObject exitTransitionIntoBossFightEffect;

	public Transform bossSpawnPosition;

	public List<GameObject> beaconInstanceList = new List<GameObject>();

	public int beaconsRequiredToSpawnBoss;

	public int beaconsToSpawnOnMap;

	public InteractableSpawnCard beaconSpawnCard;

	public static GoldshoresMissionController instance { get; private set; }

	public int beaconsActive => Ready.count;

	public int beaconCount => Ready.count + NotReady.count;

	private void OnEnable()
	{
		instance = SingletonHelper.Assign<GoldshoresMissionController>(instance, this);
	}

	private void OnDisable()
	{
		instance = SingletonHelper.Unassign<GoldshoresMissionController>(instance, this);
	}

	private void Start()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected O, but got Unknown
		rng = new Xoroshiro128Plus((ulong)Run.instance.stageRng.nextUint);
		beginTransitionIntoBossFightEffect.SetActive(false);
		exitTransitionIntoBossFightEffect.SetActive(false);
	}

	public void SpawnBeacons()
	{
		if (!NetworkServer.active)
		{
			return;
		}
		for (int i = 0; i < beaconsToSpawnOnMap; i++)
		{
			GameObject val = DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(beaconSpawnCard, new DirectorPlacementRule
			{
				placementMode = DirectorPlacementRule.PlacementMode.Random
			}, rng));
			if (Object.op_Implicit((Object)(object)val))
			{
				beaconInstanceList.Add(val);
			}
		}
		beaconsToSpawnOnMap = beaconInstanceList.Count;
	}

	public void BeginTransitionIntoBossfight()
	{
		beginTransitionIntoBossFightEffect.SetActive(true);
		exitTransitionIntoBossFightEffect.SetActive(false);
	}

	public void ExitTransitionIntoBossfight()
	{
		beginTransitionIntoBossFightEffect.SetActive(false);
		exitTransitionIntoBossFightEffect.SetActive(true);
	}
}
