using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Dynamic Bone/Dynamic Bone")]
public class DynamicBone : MonoBehaviour
{
	public enum UpdateMode
	{
		Normal,
		AnimatePhysics,
		UnscaledTime
	}

	public enum FreezeAxis
	{
		None,
		X,
		Y,
		Z
	}

	private class Particle
	{
		public Transform m_Transform;

		public int m_ParentIndex = -1;

		public float m_Damping;

		public float m_Elasticity;

		public float m_Stiffness;

		public float m_Inert;

		public float m_Radius;

		public float m_BoneLength;

		public Vector3 m_Position = Vector3.zero;

		public Vector3 m_PrevPosition = Vector3.zero;

		public Vector3 m_EndOffset = Vector3.zero;

		public Vector3 m_InitLocalPosition = Vector3.zero;

		public Quaternion m_InitLocalRotation = Quaternion.identity;
	}

	public Transform m_Root;

	public float m_UpdateRate = 60f;

	public UpdateMode m_UpdateMode;

	[Range(0f, 1f)]
	public float m_Damping = 0.1f;

	public AnimationCurve m_DampingDistrib;

	[Range(0f, 1f)]
	public float m_Elasticity = 0.1f;

	public AnimationCurve m_ElasticityDistrib;

	[Range(0f, 1f)]
	public float m_Stiffness = 0.1f;

	public AnimationCurve m_StiffnessDistrib;

	[Range(0f, 1f)]
	public float m_Inert;

	public AnimationCurve m_InertDistrib;

	public float m_Radius;

	public AnimationCurve m_RadiusDistrib;

	public float m_EndLength;

	public Vector3 m_EndOffset = Vector3.zero;

	public Vector3 m_Gravity = Vector3.zero;

	public Vector3 m_Force = Vector3.zero;

	public List<DynamicBoneCollider> m_Colliders;

	public List<Transform> m_Exclusions;

	public FreezeAxis m_FreezeAxis;

	public bool m_DistantDisable;

	public Transform m_ReferenceObject;

	public float m_DistanceToObject = 20f;

	private Vector3 m_LocalGravity = Vector3.zero;

	private Vector3 m_ObjectMove = Vector3.zero;

	private Vector3 m_ObjectPrevPosition = Vector3.zero;

	private float m_BoneTotalLength;

	private float m_ObjectScale = 1f;

	private float m_Time;

	private float m_Weight = 1f;

	private bool m_DistantDisabled;

	private List<Particle> m_Particles = new List<Particle>();

	private void Start()
	{
		SetupParticles();
	}

	private void FixedUpdate()
	{
		if (m_UpdateMode == UpdateMode.AnimatePhysics)
		{
			PreUpdate();
		}
	}

	private void Update()
	{
		if (m_UpdateMode != UpdateMode.AnimatePhysics)
		{
			PreUpdate();
		}
	}

	private void LateUpdate()
	{
		if (m_DistantDisable)
		{
			CheckDistance();
		}
		if (m_Weight > 0f && (!m_DistantDisable || !m_DistantDisabled))
		{
			float deltaTime = Time.deltaTime;
			UpdateDynamicBones(deltaTime);
		}
	}

	private void PreUpdate()
	{
		if (m_Weight > 0f && (!m_DistantDisable || !m_DistantDisabled))
		{
			InitTransforms();
		}
	}

	private void CheckDistance()
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		Transform val = m_ReferenceObject;
		if ((Object)(object)val == (Object)null && Object.op_Implicit((Object)(object)Camera.main))
		{
			val = ((Component)Camera.main).transform;
		}
		if (!Object.op_Implicit((Object)(object)val))
		{
			return;
		}
		Vector3 val2 = val.position - ((Component)this).transform.position;
		bool flag = ((Vector3)(ref val2)).sqrMagnitude > m_DistanceToObject * m_DistanceToObject;
		if (flag != m_DistantDisabled)
		{
			if (!flag)
			{
				ResetParticlesPosition();
			}
			m_DistantDisabled = flag;
		}
	}

	private void OnEnable()
	{
		ResetParticlesPosition();
	}

	private void OnDisable()
	{
		InitTransforms();
	}

	private void OnValidate()
	{
		m_UpdateRate = Mathf.Max(m_UpdateRate, 0f);
		m_Damping = Mathf.Clamp01(m_Damping);
		m_Elasticity = Mathf.Clamp01(m_Elasticity);
		m_Stiffness = Mathf.Clamp01(m_Stiffness);
		m_Inert = Mathf.Clamp01(m_Inert);
		m_Radius = Mathf.Max(m_Radius, 0f);
		if (Application.isEditor && Application.isPlaying)
		{
			InitTransforms();
			SetupParticles();
		}
	}

	private void OnDrawGizmosSelected()
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		if (!((Behaviour)this).enabled || (Object)(object)m_Root == (Object)null)
		{
			return;
		}
		if (Application.isEditor && !Application.isPlaying && ((Component)this).transform.hasChanged)
		{
			InitTransforms();
			SetupParticles();
		}
		Gizmos.color = Color.white;
		for (int i = 0; i < m_Particles.Count; i++)
		{
			Particle particle = m_Particles[i];
			if (particle.m_ParentIndex >= 0)
			{
				Particle particle2 = m_Particles[particle.m_ParentIndex];
				Gizmos.DrawLine(particle.m_Position, particle2.m_Position);
			}
			if (particle.m_Radius > 0f)
			{
				Gizmos.DrawWireSphere(particle.m_Position, particle.m_Radius * m_ObjectScale);
			}
		}
	}

	public void SetWeight(float w)
	{
		if (m_Weight != w)
		{
			if (w == 0f)
			{
				InitTransforms();
			}
			else if (m_Weight == 0f)
			{
				ResetParticlesPosition();
			}
			m_Weight = w;
		}
	}

	public float GetWeight()
	{
		return m_Weight;
	}

	private void UpdateDynamicBones(float t)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)m_Root == (Object)null)
		{
			return;
		}
		m_ObjectScale = Mathf.Abs(((Component)this).transform.lossyScale.x);
		m_ObjectMove = ((Component)this).transform.position - m_ObjectPrevPosition;
		m_ObjectPrevPosition = ((Component)this).transform.position;
		int num = 1;
		if (m_UpdateRate > 0f)
		{
			float num2 = 1f / m_UpdateRate;
			m_Time += t;
			num = 0;
			while (m_Time >= num2)
			{
				m_Time -= num2;
				if (++num >= 3)
				{
					m_Time = 0f;
					break;
				}
			}
		}
		if (num > 0)
		{
			for (int i = 0; i < num; i++)
			{
				UpdateParticles1();
				UpdateParticles2();
				m_ObjectMove = Vector3.zero;
			}
		}
		else
		{
			SkipUpdateParticles();
		}
		ApplyParticlesToTransforms();
	}

	private void SetupParticles()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		m_Particles.Clear();
		if ((Object)(object)m_Root == (Object)null)
		{
			return;
		}
		m_LocalGravity = m_Root.InverseTransformDirection(m_Gravity);
		m_ObjectScale = Mathf.Abs(((Component)this).transform.lossyScale.x);
		m_ObjectPrevPosition = ((Component)this).transform.position;
		m_ObjectMove = Vector3.zero;
		m_BoneTotalLength = 0f;
		AppendParticles(m_Root, -1, 0f);
		for (int i = 0; i < m_Particles.Count; i++)
		{
			Particle particle = m_Particles[i];
			particle.m_Damping = m_Damping;
			particle.m_Elasticity = m_Elasticity;
			particle.m_Stiffness = m_Stiffness;
			particle.m_Inert = m_Inert;
			particle.m_Radius = m_Radius;
			if (m_BoneTotalLength > 0f)
			{
				float num = particle.m_BoneLength / m_BoneTotalLength;
				if (m_DampingDistrib != null && m_DampingDistrib.keys.Length != 0)
				{
					particle.m_Damping *= m_DampingDistrib.Evaluate(num);
				}
				if (m_ElasticityDistrib != null && m_ElasticityDistrib.keys.Length != 0)
				{
					particle.m_Elasticity *= m_ElasticityDistrib.Evaluate(num);
				}
				if (m_StiffnessDistrib != null && m_StiffnessDistrib.keys.Length != 0)
				{
					particle.m_Stiffness *= m_StiffnessDistrib.Evaluate(num);
				}
				if (m_InertDistrib != null && m_InertDistrib.keys.Length != 0)
				{
					particle.m_Inert *= m_InertDistrib.Evaluate(num);
				}
				if (m_RadiusDistrib != null && m_RadiusDistrib.keys.Length != 0)
				{
					particle.m_Radius *= m_RadiusDistrib.Evaluate(num);
				}
			}
			particle.m_Damping = Mathf.Clamp01(particle.m_Damping);
			particle.m_Elasticity = Mathf.Clamp01(particle.m_Elasticity);
			particle.m_Stiffness = Mathf.Clamp01(particle.m_Stiffness);
			particle.m_Inert = Mathf.Clamp01(particle.m_Inert);
			particle.m_Radius = Mathf.Max(particle.m_Radius, 0f);
		}
	}

	private void AppendParticles(Transform b, int parentIndex, float boneLength)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		Particle particle = new Particle();
		particle.m_Transform = b;
		particle.m_ParentIndex = parentIndex;
		Vector3 val;
		if (Object.op_Implicit((Object)(object)b))
		{
			val = (particle.m_Position = (particle.m_PrevPosition = b.position));
			particle.m_InitLocalPosition = b.localPosition;
			particle.m_InitLocalRotation = b.localRotation;
		}
		else
		{
			Transform transform = m_Particles[parentIndex].m_Transform;
			if (m_EndLength > 0f)
			{
				Transform parent = transform.parent;
				if (Object.op_Implicit((Object)(object)parent))
				{
					particle.m_EndOffset = transform.InverseTransformPoint(transform.position * 2f - parent.position) * m_EndLength;
				}
				else
				{
					particle.m_EndOffset = new Vector3(m_EndLength, 0f, 0f);
				}
			}
			else
			{
				particle.m_EndOffset = transform.InverseTransformPoint(((Component)this).transform.TransformDirection(m_EndOffset) + transform.position);
			}
			val = (particle.m_Position = (particle.m_PrevPosition = transform.TransformPoint(particle.m_EndOffset)));
		}
		if (parentIndex >= 0)
		{
			float num = boneLength;
			val = m_Particles[parentIndex].m_Transform.position - particle.m_Position;
			boneLength = num + ((Vector3)(ref val)).magnitude;
			particle.m_BoneLength = boneLength;
			m_BoneTotalLength = Mathf.Max(m_BoneTotalLength, boneLength);
		}
		int count = m_Particles.Count;
		m_Particles.Add(particle);
		if (!Object.op_Implicit((Object)(object)b))
		{
			return;
		}
		for (int i = 0; i < b.childCount; i++)
		{
			bool flag = false;
			if (m_Exclusions != null)
			{
				for (int j = 0; j < m_Exclusions.Count; j++)
				{
					if ((Object)(object)m_Exclusions[j] == (Object)(object)b.GetChild(i))
					{
						flag = true;
						break;
					}
				}
			}
			if (!flag)
			{
				AppendParticles(b.GetChild(i), count, boneLength);
			}
		}
		if (b.childCount == 0 && (m_EndLength > 0f || m_EndOffset != Vector3.zero))
		{
			AppendParticles(null, count, boneLength);
		}
	}

	private void InitTransforms()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < m_Particles.Count; i++)
		{
			Particle particle = m_Particles[i];
			if (Object.op_Implicit((Object)(object)particle.m_Transform))
			{
				particle.m_Transform.localPosition = particle.m_InitLocalPosition;
				particle.m_Transform.localRotation = particle.m_InitLocalRotation;
			}
		}
	}

	private void ResetParticlesPosition()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < m_Particles.Count; i++)
		{
			Particle particle = m_Particles[i];
			if (Object.op_Implicit((Object)(object)particle.m_Transform))
			{
				particle.m_Position = (particle.m_PrevPosition = particle.m_Transform.position);
				continue;
			}
			Transform transform = m_Particles[particle.m_ParentIndex].m_Transform;
			particle.m_Position = (particle.m_PrevPosition = transform.TransformPoint(particle.m_EndOffset));
		}
		m_ObjectPrevPosition = ((Component)this).transform.position;
	}

	private void UpdateParticles1()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		Vector3 gravity = m_Gravity;
		Vector3 normalized = ((Vector3)(ref m_Gravity)).normalized;
		Vector3 val = m_Root.TransformDirection(m_LocalGravity);
		Vector3 val2 = normalized * Mathf.Max(Vector3.Dot(val, normalized), 0f);
		gravity -= val2;
		gravity = (gravity + m_Force) * m_ObjectScale;
		for (int i = 0; i < m_Particles.Count; i++)
		{
			Particle particle = m_Particles[i];
			if (particle.m_ParentIndex >= 0)
			{
				Vector3 val3 = particle.m_Position - particle.m_PrevPosition;
				Vector3 val4 = m_ObjectMove * particle.m_Inert;
				particle.m_PrevPosition = particle.m_Position + val4;
				particle.m_Position += val3 * (1f - particle.m_Damping) + gravity + val4;
			}
			else
			{
				particle.m_PrevPosition = particle.m_Position;
				particle.m_Position = particle.m_Transform.position;
			}
		}
	}

	private void UpdateParticles2()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		//IL_023b: Unknown result type (might be due to invalid IL or missing references)
		//IL_024f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0255: Unknown result type (might be due to invalid IL or missing references)
		//IL_0269: Unknown result type (might be due to invalid IL or missing references)
		//IL_026f: Unknown result type (might be due to invalid IL or missing references)
		//IL_027b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0282: Unknown result type (might be due to invalid IL or missing references)
		//IL_028a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0294: Unknown result type (might be due to invalid IL or missing references)
		//IL_0299: Unknown result type (might be due to invalid IL or missing references)
		//IL_029e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_02af: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02de: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e3: Unknown result type (might be due to invalid IL or missing references)
		Plane val = default(Plane);
		for (int i = 1; i < m_Particles.Count; i++)
		{
			Particle particle = m_Particles[i];
			Particle particle2 = m_Particles[particle.m_ParentIndex];
			if (!Object.op_Implicit((Object)(object)particle2.m_Transform))
			{
				continue;
			}
			Vector3 val2;
			float magnitude;
			if (Object.op_Implicit((Object)(object)particle.m_Transform))
			{
				val2 = particle2.m_Transform.position - particle.m_Transform.position;
				magnitude = ((Vector3)(ref val2)).magnitude;
			}
			else
			{
				Matrix4x4 localToWorldMatrix = particle2.m_Transform.localToWorldMatrix;
				val2 = ((Matrix4x4)(ref localToWorldMatrix)).MultiplyVector(particle.m_EndOffset);
				magnitude = ((Vector3)(ref val2)).magnitude;
			}
			float num = Mathf.Lerp(1f, particle.m_Stiffness, m_Weight);
			if (num > 0f || particle.m_Elasticity > 0f)
			{
				Matrix4x4 localToWorldMatrix2 = particle2.m_Transform.localToWorldMatrix;
				((Matrix4x4)(ref localToWorldMatrix2)).SetColumn(3, Vector4.op_Implicit(particle2.m_Position));
				Vector3 val3 = ((!Object.op_Implicit((Object)(object)particle.m_Transform)) ? ((Matrix4x4)(ref localToWorldMatrix2)).MultiplyPoint3x4(particle.m_EndOffset) : ((Matrix4x4)(ref localToWorldMatrix2)).MultiplyPoint3x4(particle.m_Transform.localPosition));
				Vector3 val4 = val3 - particle.m_Position;
				particle.m_Position += val4 * particle.m_Elasticity;
				if (num > 0f)
				{
					val4 = val3 - particle.m_Position;
					float magnitude2 = ((Vector3)(ref val4)).magnitude;
					float num2 = magnitude * (1f - num) * 2f;
					if (magnitude2 > num2)
					{
						particle.m_Position += val4 * ((magnitude2 - num2) / magnitude2);
					}
				}
			}
			if (m_Colliders != null)
			{
				float particleRadius = particle.m_Radius * m_ObjectScale;
				for (int j = 0; j < m_Colliders.Count; j++)
				{
					DynamicBoneCollider dynamicBoneCollider = m_Colliders[j];
					if (Object.op_Implicit((Object)(object)dynamicBoneCollider) && ((Behaviour)dynamicBoneCollider).enabled)
					{
						dynamicBoneCollider.Collide(ref particle.m_Position, particleRadius);
					}
				}
			}
			if (m_FreezeAxis != 0)
			{
				switch (m_FreezeAxis)
				{
				case FreezeAxis.X:
					((Plane)(ref val)).SetNormalAndPosition(particle2.m_Transform.right, particle2.m_Position);
					break;
				case FreezeAxis.Y:
					((Plane)(ref val)).SetNormalAndPosition(particle2.m_Transform.up, particle2.m_Position);
					break;
				case FreezeAxis.Z:
					((Plane)(ref val)).SetNormalAndPosition(particle2.m_Transform.forward, particle2.m_Position);
					break;
				}
				particle.m_Position -= ((Plane)(ref val)).normal * ((Plane)(ref val)).GetDistanceToPoint(particle.m_Position);
			}
			Vector3 val5 = particle2.m_Position - particle.m_Position;
			float magnitude3 = ((Vector3)(ref val5)).magnitude;
			if (magnitude3 > 0f)
			{
				particle.m_Position += val5 * ((magnitude3 - magnitude) / magnitude3);
			}
		}
	}

	private void SkipUpdateParticles()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_0189: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < m_Particles.Count; i++)
		{
			Particle particle = m_Particles[i];
			if (particle.m_ParentIndex >= 0)
			{
				particle.m_PrevPosition += m_ObjectMove;
				particle.m_Position += m_ObjectMove;
				Particle particle2 = m_Particles[particle.m_ParentIndex];
				if (!Object.op_Implicit((Object)(object)particle2.m_Transform))
				{
					continue;
				}
				Vector3 val;
				float magnitude;
				if (Object.op_Implicit((Object)(object)particle.m_Transform))
				{
					val = particle2.m_Transform.position - particle.m_Transform.position;
					magnitude = ((Vector3)(ref val)).magnitude;
				}
				else
				{
					Matrix4x4 localToWorldMatrix = particle2.m_Transform.localToWorldMatrix;
					val = ((Matrix4x4)(ref localToWorldMatrix)).MultiplyVector(particle.m_EndOffset);
					magnitude = ((Vector3)(ref val)).magnitude;
				}
				float num = Mathf.Lerp(1f, particle.m_Stiffness, m_Weight);
				if (num > 0f)
				{
					Matrix4x4 localToWorldMatrix2 = particle2.m_Transform.localToWorldMatrix;
					((Matrix4x4)(ref localToWorldMatrix2)).SetColumn(3, Vector4.op_Implicit(particle2.m_Position));
					Vector3 val2 = ((!Object.op_Implicit((Object)(object)particle.m_Transform)) ? ((Matrix4x4)(ref localToWorldMatrix2)).MultiplyPoint3x4(particle.m_EndOffset) : ((Matrix4x4)(ref localToWorldMatrix2)).MultiplyPoint3x4(particle.m_Transform.localPosition));
					Vector3 val3 = val2 - particle.m_Position;
					float magnitude2 = ((Vector3)(ref val3)).magnitude;
					float num2 = magnitude * (1f - num) * 2f;
					if (magnitude2 > num2)
					{
						particle.m_Position += val3 * ((magnitude2 - num2) / magnitude2);
					}
				}
				Vector3 val4 = particle2.m_Position - particle.m_Position;
				float magnitude3 = ((Vector3)(ref val4)).magnitude;
				if (magnitude3 > 0f)
				{
					particle.m_Position += val4 * ((magnitude3 - magnitude) / magnitude3);
				}
			}
			else if (Object.op_Implicit((Object)(object)particle.m_Transform))
			{
				particle.m_PrevPosition = particle.m_Position;
				particle.m_Position = particle.m_Transform.position;
			}
		}
	}

	private static Vector3 MirrorVector(Vector3 v, Vector3 axis)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		return v - axis * (Vector3.Dot(v, axis) * 2f);
	}

	private void ApplyParticlesToTransforms()
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 1; i < m_Particles.Count; i++)
		{
			Particle particle = m_Particles[i];
			Particle particle2 = m_Particles[particle.m_ParentIndex];
			if (particle2.m_Transform.childCount <= 1)
			{
				Vector3 val = ((!Object.op_Implicit((Object)(object)particle.m_Transform)) ? particle.m_EndOffset : particle.m_Transform.localPosition);
				Vector3 val2 = particle.m_Position - particle2.m_Position;
				Quaternion val3 = Quaternion.FromToRotation(particle2.m_Transform.TransformDirection(val), val2);
				particle2.m_Transform.rotation = val3 * particle2.m_Transform.rotation;
			}
			if (Object.op_Implicit((Object)(object)particle.m_Transform))
			{
				particle.m_Transform.position = particle.m_Position;
			}
		}
	}
}
