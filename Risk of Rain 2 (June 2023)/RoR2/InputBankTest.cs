using UnityEngine;

namespace RoR2;

[RequireComponent(typeof(CharacterBody))]
public class InputBankTest : MonoBehaviour
{
	private struct CachedRaycastInfo
	{
		public float time;

		public float fixedTime;

		public bool didHit;

		public RaycastHit hitInfo;

		public float maxDistance;

		public static readonly CachedRaycastInfo empty = new CachedRaycastInfo
		{
			time = float.NegativeInfinity,
			fixedTime = float.NegativeInfinity,
			didHit = false,
			maxDistance = 0f
		};
	}

	public struct ButtonState
	{
		public bool down;

		public bool wasDown;

		public bool hasPressBeenClaimed;

		public bool justReleased
		{
			get
			{
				if (!down)
				{
					return wasDown;
				}
				return false;
			}
		}

		public bool justPressed
		{
			get
			{
				if (down)
				{
					return !wasDown;
				}
				return false;
			}
		}

		public void PushState(bool newState)
		{
			hasPressBeenClaimed &= newState;
			wasDown = down;
			down = newState;
		}
	}

	private CharacterBody characterBody;

	private Vector3 _aimDirection;

	private float lastRaycastTime = float.NegativeInfinity;

	private float lastFixedRaycastTime = float.NegativeInfinity;

	private bool didLastRaycastHit;

	private RaycastHit lastHitInfo;

	private float lastMaxDistance;

	private CachedRaycastInfo cachedRaycast = CachedRaycastInfo.empty;

	public Vector3 moveVector;

	public ButtonState skill1;

	public ButtonState skill2;

	public ButtonState skill3;

	public ButtonState skill4;

	public ButtonState interact;

	public ButtonState jump;

	public ButtonState sprint;

	public ButtonState activateEquipment;

	public ButtonState ping;

	public Vector3 aimDirection
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			if (!(_aimDirection != Vector3.zero))
			{
				return ((Component)this).transform.forward;
			}
			return _aimDirection;
		}
		set
		{
			//IL_0003: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			_aimDirection = ((Vector3)(ref value)).normalized;
		}
	}

	public Vector3 aimOrigin
	{
		get
		{
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			if (!Object.op_Implicit((Object)(object)characterBody.aimOriginTransform))
			{
				return ((Component)this).transform.position;
			}
			return characterBody.aimOriginTransform.position;
		}
	}

	public int emoteRequest { get; set; } = -1;


	public Ray GetAimRay()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return new Ray(aimOrigin, aimDirection);
	}

	public bool GetAimRaycast(float maxDistance, out RaycastHit hitInfo)
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		float time = Time.time;
		float fixedTime = Time.fixedTime;
		if (!cachedRaycast.time.Equals(time) || !cachedRaycast.fixedTime.Equals(fixedTime) || (!(cachedRaycast.maxDistance >= maxDistance) && !cachedRaycast.didHit))
		{
			float extraRaycastDistance = 0f;
			Ray val = CameraRigController.ModifyAimRayIfApplicable(GetAimRay(), ((Component)this).gameObject, out extraRaycastDistance);
			cachedRaycast = CachedRaycastInfo.empty;
			cachedRaycast.time = time;
			cachedRaycast.fixedTime = fixedTime;
			cachedRaycast.maxDistance = maxDistance;
			cachedRaycast.didHit = Util.CharacterRaycast(((Component)this).gameObject, val, maxDistance: maxDistance + extraRaycastDistance, layerMask: LayerMask.op_Implicit(LayerMask.op_Implicit(LayerIndex.world.mask) | LayerMask.op_Implicit(LayerIndex.entityPrecise.mask)), hitInfo: out cachedRaycast.hitInfo, queryTriggerInteraction: (QueryTriggerInteraction)1);
		}
		bool flag = cachedRaycast.didHit;
		hitInfo = cachedRaycast.hitInfo;
		if (flag && ((RaycastHit)(ref hitInfo)).distance > maxDistance)
		{
			flag = false;
			hitInfo = default(RaycastHit);
		}
		return flag;
	}

	public bool CheckAnyButtonDown()
	{
		if (!skill1.down && !skill2.down && !skill3.down && !skill4.down && !interact.down && !jump.down && !sprint.down && !activateEquipment.down)
		{
			return ping.down;
		}
		return true;
	}

	private void Awake()
	{
		characterBody = ((Component)this).GetComponent<CharacterBody>();
	}

	private void Start()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		if (characterBody.hasEffectiveAuthority)
		{
			if (Object.op_Implicit((Object)(object)characterBody.characterDirection))
			{
				aimDirection = characterBody.characterDirection.forward;
			}
			else
			{
				aimDirection = ((Component)this).transform.forward;
			}
		}
	}
}
