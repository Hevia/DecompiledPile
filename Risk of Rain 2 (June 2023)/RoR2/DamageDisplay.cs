using System.Collections.Generic;
using System.Collections.ObjectModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RoR2;

public class DamageDisplay : MonoBehaviour
{
	private static List<DamageDisplay> instancesList;

	private static ReadOnlyCollection<DamageDisplay> _readOnlyInstancesList;

	public TextMeshPro textMeshComponent;

	public AnimationCurve magnitudeCurve;

	public float maxLife = 3f;

	public float gravity = 9.81f;

	public float magnitude = 3f;

	public float offset = 20f;

	private Vector3 velocity;

	public float textMagnitude = 0.01f;

	private float vel;

	private float life;

	private float scale = 1f;

	[HideInInspector]
	public Color baseColor = Color.white;

	[HideInInspector]
	public Color baseOutlineColor = Color.gray;

	private GameObject victim;

	private GameObject attacker;

	private TeamIndex victimTeam;

	private TeamIndex attackerTeam;

	private bool crit;

	private bool heal;

	private Vector3 internalPosition;

	public static ReadOnlyCollection<DamageDisplay> readOnlyInstancesList => _readOnlyInstancesList;

	static DamageDisplay()
	{
		instancesList = new List<DamageDisplay>();
		_readOnlyInstancesList = new ReadOnlyCollection<DamageDisplay>(instancesList);
		UICamera.onUICameraPreCull += OnUICameraPreCull;
		RoR2Application.onUpdate += UpdateAll;
	}

	private void Start()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		velocity = Vector3.Normalize(Vector3.up + new Vector3(Random.Range(0f - offset, offset), 0f, Random.Range(0f - offset, offset))) * magnitude;
		instancesList.Add(this);
		internalPosition = ((Component)this).transform.position;
	}

	private void OnDestroy()
	{
		instancesList.Remove(this);
	}

	public void SetValues(GameObject victim, GameObject attacker, float damage, bool crit, DamageColorIndex damageColorIndex)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		victimTeam = TeamIndex.Neutral;
		attackerTeam = TeamIndex.Neutral;
		scale = 1f;
		this.victim = victim;
		this.attacker = attacker;
		this.crit = crit;
		baseColor = DamageColor.FindColor(damageColorIndex);
		string text = Mathf.CeilToInt(Mathf.Abs(damage)).ToString();
		heal = damage < 0f;
		if (heal)
		{
			damage = 0f - damage;
			((Component)this).transform.parent = victim.transform;
			text = "+" + text;
			baseColor = DamageColor.FindColor(DamageColorIndex.Heal);
			baseOutlineColor = baseColor * Color.gray;
		}
		if (Object.op_Implicit((Object)(object)victim))
		{
			TeamComponent component = victim.GetComponent<TeamComponent>();
			if (Object.op_Implicit((Object)(object)component))
			{
				victimTeam = component.teamIndex;
			}
		}
		if (Object.op_Implicit((Object)(object)attacker))
		{
			TeamComponent component2 = attacker.GetComponent<TeamComponent>();
			if (Object.op_Implicit((Object)(object)component2))
			{
				attackerTeam = component2.teamIndex;
			}
		}
		if (crit)
		{
			text += "!";
			baseOutlineColor = Color.red;
		}
		((TMP_Text)textMeshComponent).text = text;
		UpdateMagnitude();
	}

	private void UpdateMagnitude()
	{
		float fontSize = magnitudeCurve.Evaluate(life / maxLife) * textMagnitude * scale;
		((TMP_Text)textMeshComponent).fontSize = fontSize;
	}

	private static void UpdateAll()
	{
		for (int num = instancesList.Count - 1; num >= 0; num--)
		{
			instancesList[num].DoUpdate();
		}
	}

	private void DoUpdate()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		UpdateMagnitude();
		life += Time.deltaTime;
		if (life >= maxLife)
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
			return;
		}
		velocity += gravity * Vector3.down * Time.deltaTime;
		internalPosition += velocity * Time.deltaTime;
	}

	private static void OnUICameraPreCull(UICamera uiCamera)
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = null;
		TeamIndex teamIndex = TeamIndex.Neutral;
		Camera camera = uiCamera.camera;
		Camera sceneCam = uiCamera.cameraRigController.sceneCam;
		val = uiCamera.cameraRigController.target;
		teamIndex = uiCamera.cameraRigController.targetTeamIndex;
		for (int i = 0; i < instancesList.Count; i++)
		{
			DamageDisplay damageDisplay = instancesList[i];
			Color val2 = Color.white;
			if (!damageDisplay.heal)
			{
				if (teamIndex == damageDisplay.victimTeam)
				{
					((Color)(ref val2))._002Ector(0.5568628f, 0.29411766f, 0.6039216f);
				}
				else if (teamIndex == damageDisplay.attackerTeam && (Object)(object)val != (Object)(object)damageDisplay.attacker)
				{
					val2 = Color.gray;
				}
			}
			((Graphic)damageDisplay.textMeshComponent).color = Color.Lerp(Color.white, damageDisplay.baseColor * val2, damageDisplay.life / 0.2f);
			((TMP_Text)damageDisplay.textMeshComponent).outlineColor = Color32.op_Implicit(Color.Lerp(Color.white, damageDisplay.baseOutlineColor * val2, damageDisplay.life / 0.2f));
			Vector3 val3 = damageDisplay.internalPosition;
			Vector3 val4 = sceneCam.WorldToScreenPoint(val3);
			val4.z = ((val4.z > 0f) ? 1f : (-1f));
			Vector3 position = camera.ScreenToWorldPoint(val4);
			((Component)damageDisplay).transform.position = position;
		}
	}
}
