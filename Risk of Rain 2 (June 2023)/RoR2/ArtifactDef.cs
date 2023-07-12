using System;
using RoR2.ExpansionManagement;
using UnityEngine;
using UnityEngine.Serialization;

namespace RoR2;

[CreateAssetMenu(menuName = "RoR2/ArtifactDef")]
public class ArtifactDef : ScriptableObject
{
	public string nameToken;

	public string descriptionToken;

	public UnlockableDef unlockableDef;

	public ExpansionDef requiredExpansion;

	public Sprite smallIconSelectedSprite;

	public Sprite smallIconDeselectedSprite;

	[FormerlySerializedAs("worldModelPrefab")]
	public GameObject pickupModelPrefab;

	private string _cachedName;

	public ArtifactIndex artifactIndex { get; set; }

	[Obsolete(".name should not be used. Use .cachedName instead. If retrieving the value from the engine is absolutely necessary, cast to ScriptableObject first.", true)]
	public string name
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public string cachedName
	{
		get
		{
			return _cachedName;
		}
		set
		{
			((Object)this).name = value;
			_cachedName = value;
		}
	}

	public static void AttemptGrant(ref PickupDef.GrantContext context)
	{
		ArtifactDef artifactDef = ArtifactCatalog.GetArtifactDef(PickupCatalog.GetPickupDef(context.controller.pickupIndex).artifactIndex);
		Run.instance.GrantUnlockToAllParticipatingPlayers(artifactDef.unlockableDef);
		context.shouldNotify = true;
		context.shouldDestroy = true;
	}

	public virtual PickupDef CreatePickupDef()
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		PickupDef obj = new PickupDef
		{
			internalName = "ArtifactIndex." + cachedName,
			artifactIndex = artifactIndex,
			displayPrefab = pickupModelPrefab,
			nameToken = nameToken,
			baseColor = Color32.op_Implicit(ColorCatalog.GetColor(ColorCatalog.ColorIndex.Artifact))
		};
		obj.darkColor = obj.baseColor;
		obj.unlockableDef = unlockableDef;
		obj.interactContextToken = "ITEM_PICKUP_CONTEXT";
		obj.iconTexture = (Texture)(object)(Object.op_Implicit((Object)(object)smallIconSelectedSprite) ? smallIconSelectedSprite.texture : null);
		obj.iconSprite = (Object.op_Implicit((Object)(object)smallIconSelectedSprite) ? smallIconSelectedSprite : null);
		obj.attemptGrant = AttemptGrant;
		return obj;
	}

	private void Awake()
	{
		_cachedName = ((Object)this).name;
	}

	private void OnValidate()
	{
		_cachedName = ((Object)this).name;
	}
}
