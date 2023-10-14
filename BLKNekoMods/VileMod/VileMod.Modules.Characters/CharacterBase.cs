using System;
using EntityStates;
using HG;
using RoR2;
using RoR2.ContentManagement;
using UnityEngine;

namespace VileMod.Modules.Characters;

internal abstract class CharacterBase
{
	public abstract string prefabBodyName { get; }

	public abstract BodyInfo bodyInfo { get; set; }

	public abstract CustomRendererInfo[] customRendererInfos { get; set; }

	public abstract Type characterMainState { get; }

	public abstract Type characterDeathState { get; }

	public virtual Type characterSpawnState { get; }

	public virtual ItemDisplaysBase itemDisplays { get; } = null;


	public virtual GameObject bodyPrefab { get; set; }

	public virtual CharacterBody prefabCharacterBody { get; set; }

	public virtual CharacterModel prefabCharacterModel { get; set; }

	public string fullBodyName => prefabBodyName + "Body";

	public virtual void Initialize()
	{
		InitializeCharacter();
	}

	public virtual void InitializeCharacter()
	{
		InitializeCharacterBodyAndModel();
		InitializeCharacterMaster();
		InitializeEntityStateMachine();
		InitializeSkills();
		InitializeHitboxes();
		InitializeHurtboxes();
		InitializeSkins();
		InitializeItemDisplays();
		InitializeDoppelganger("Merc");
	}

	protected virtual void InitializeCharacterBodyAndModel()
	{
		bodyPrefab = Prefabs.CreateBodyPrefab(prefabBodyName + "Body", "mdl" + prefabBodyName, bodyInfo);
		prefabCharacterBody = bodyPrefab.GetComponent<CharacterBody>();
		InitializeCharacterModel();
	}

	protected virtual void InitializeCharacterModel()
	{
		prefabCharacterModel = Prefabs.SetupCharacterModel(bodyPrefab, customRendererInfos);
	}

	protected virtual void InitializeCharacterMaster()
	{
	}

	protected virtual void InitializeEntityStateMachine()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		bodyPrefab.GetComponent<EntityStateMachine>().mainStateType = new SerializableEntityStateType(characterMainState);
		bodyPrefab.GetComponent<CharacterDeathBehavior>().deathState = new SerializableEntityStateType(characterDeathState);
		bodyPrefab.GetComponent<EntityStateMachine>().initialStateType = new SerializableEntityStateType(characterSpawnState);
		Content.AddEntityState(characterMainState);
	}

	public abstract void InitializeSkills();

	public virtual void InitializeHitboxes()
	{
	}

	public virtual void InitializeHurtboxes()
	{
		Prefabs.SetupHurtBoxes(bodyPrefab);
	}

	public virtual void InitializeSkins()
	{
	}

	public virtual void InitializeDoppelganger(string clone)
	{
		Prefabs.CreateGenericDoppelganger(bodyPrefab, bodyInfo.bodyName + "MonsterMaster", clone);
	}

	public virtual void InitializeItemDisplays()
	{
		ItemDisplayRuleSet val = ScriptableObject.CreateInstance<ItemDisplayRuleSet>();
		((Object)val).name = "idrs" + prefabBodyName;
		prefabCharacterModel.itemDisplayRuleSet = val;
		if (itemDisplays != null)
		{
			ContentManager.onContentPacksAssigned += SetItemDisplays;
		}
	}

	public void SetItemDisplays(ReadOnlyArray<ReadOnlyContentPack> obj)
	{
		itemDisplays.SetItemDisplays(prefabCharacterModel.itemDisplayRuleSet);
	}
}
