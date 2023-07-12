using UnityEngine;

namespace RoR2.EntitlementManagement;

[CreateAssetMenu(menuName = "RoR2/EntitlementDef")]
public class EntitlementDef : ScriptableObject
{
	[HideInInspector]
	[SerializeField]
	public EntitlementIndex entitlementIndex = EntitlementIndex.None;

	[Tooltip("The user-facing display name of this entitlement.")]
	public string nameToken;

	public uint steamAppId;

	[Tooltip("This is an EOS Item Id, not Offer Id.")]
	public string eosItemId;

	private void OnDisable()
	{
		entitlementIndex = EntitlementIndex.None;
	}
}
