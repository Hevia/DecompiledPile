using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using UnityEngine;

namespace RoR2;

[RequireComponent(typeof(Collider))]
public class HurtBox : MonoBehaviour
{
	public enum DamageModifier
	{
		Normal,
		[Obsolete]
		SniperTarget,
		Weak,
		Barrier
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct EntityEqualityComparer : IEqualityComparer<HurtBox>
	{
		public bool Equals(HurtBox a, HurtBox b)
		{
			return HurtBoxesShareEntity(a, b);
		}

		public int GetHashCode(HurtBox hurtBox)
		{
			return ((object)FindEntityObject(hurtBox)).GetHashCode();
		}
	}

	[Tooltip("The health component to which this hurtbox belongs.")]
	public HealthComponent healthComponent;

	[Tooltip("Whether or not this hurtbox is considered a bullseye. Do not change this at runtime!")]
	public bool isBullseye;

	[Tooltip("Whether or not this hurtbox is considered a sniper target. Do not change this at runtime!")]
	public bool isSniperTarget;

	public DamageModifier damageModifier;

	[SerializeField]
	[HideInInspector]
	public HurtBoxGroup hurtBoxGroup;

	[SerializeField]
	[HideInInspector]
	public short indexInGroup = -1;

	private bool isInBullseyeList;

	private bool isInSniperTargetList;

	private static readonly List<HurtBox> bullseyesList = new List<HurtBox>();

	public static readonly ReadOnlyCollection<HurtBox> readOnlyBullseyesList = bullseyesList.AsReadOnly();

	private static readonly List<HurtBox> sniperTargetsList = new List<HurtBox>();

	public static readonly float sniperTargetRadius = 1f;

	public static readonly float sniperTargetRadiusSqr = sniperTargetRadius * sniperTargetRadius;

	public TeamIndex teamIndex { get; set; } = TeamIndex.None;


	public Collider collider { get; private set; }

	public float volume { get; private set; }

	public Vector3 randomVolumePoint => Util.RandomColliderVolumePoint(collider);

	public static IReadOnlyList<HurtBox> readOnlySniperTargetsList => sniperTargetsList;

	private void Awake()
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		collider = ((Component)this).GetComponent<Collider>();
		collider.isTrigger = false;
		Rigidbody val = ((Component)this).GetComponent<Rigidbody>();
		if (!Object.op_Implicit((Object)(object)val))
		{
			val = ((Component)this).gameObject.AddComponent<Rigidbody>();
		}
		val.isKinematic = true;
		Vector3 lossyScale = ((Component)this).transform.lossyScale;
		volume = lossyScale.x * 2f * (lossyScale.y * 2f) * (lossyScale.z * 2f);
	}

	private void OnEnable()
	{
		if (isBullseye)
		{
			bullseyesList.Add(this);
			isInBullseyeList = true;
		}
		if (isSniperTarget)
		{
			sniperTargetsList.Add(this);
			isInSniperTargetList = true;
		}
	}

	private void OnDisable()
	{
		if (isInSniperTargetList)
		{
			isInSniperTargetList = false;
			sniperTargetsList.Remove(this);
		}
		if (isInBullseyeList)
		{
			isInBullseyeList = false;
			bullseyesList.Remove(this);
		}
	}

	public static GameObject FindEntityObject([NotNull] HurtBox hurtBox)
	{
		if (!Object.op_Implicit((Object)(object)hurtBox.healthComponent))
		{
			return null;
		}
		return ((Component)hurtBox.healthComponent).gameObject;
	}

	public static bool HurtBoxesShareEntity([NotNull] HurtBox a, [NotNull] HurtBox b)
	{
		return FindEntityObject(a) == FindEntityObject(b);
	}
}
