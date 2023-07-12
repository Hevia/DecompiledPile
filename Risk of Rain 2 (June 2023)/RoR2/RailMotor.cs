using RoR2.Navigation;
using UnityEngine;

namespace RoR2;

[RequireComponent(typeof(CharacterBody))]
public class RailMotor : MonoBehaviour
{
	public Vector3 inputMoveVector;

	public Vector3 rootMotion;

	private Animator modelAnimator;

	private InputBankTest inputBank;

	private NodeGraph railGraph;

	private NodeGraph.NodeIndex nodeA;

	private NodeGraph.NodeIndex nodeB;

	private NodeGraph.LinkIndex currentLink;

	private CharacterBody characterBody;

	private CharacterDirection characterDirection;

	private float linkLerp;

	private Vector3 projectedMoveVector;

	private Vector3 nodeAPosition;

	private Vector3 nodeBPosition;

	private Vector3 linkVector;

	private float linkLength;

	private float currentMoveSpeed;

	private bool useRootMotion;

	private void Start()
	{
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		characterDirection = ((Component)this).GetComponent<CharacterDirection>();
		inputBank = ((Component)this).GetComponent<InputBankTest>();
		characterBody = ((Component)this).GetComponent<CharacterBody>();
		railGraph = SceneInfo.instance.railNodes;
		ModelLocator component = ((Component)this).GetComponent<ModelLocator>();
		if (Object.op_Implicit((Object)(object)component))
		{
			modelAnimator = ((Component)component.modelTransform).GetComponent<Animator>();
		}
		nodeA = railGraph.FindClosestNode(((Component)this).transform.position, characterBody.hullClassification);
		NodeGraph.LinkIndex[] activeNodeLinks = railGraph.GetActiveNodeLinks(nodeA);
		currentLink = activeNodeLinks[0];
		UpdateNodeAndLinkInfo();
		useRootMotion = characterBody.rootMotionInMainState;
	}

	private void UpdateNodeAndLinkInfo()
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		nodeA = railGraph.GetLinkStartNode(currentLink);
		nodeB = railGraph.GetLinkEndNode(currentLink);
		railGraph.GetNodePosition(nodeA, out nodeAPosition);
		railGraph.GetNodePosition(nodeB, out nodeBPosition);
		linkVector = nodeBPosition - nodeAPosition;
		linkLength = ((Vector3)(ref linkVector)).magnitude;
	}

	private void FixedUpdate()
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		UpdateNodeAndLinkInfo();
		if (Object.op_Implicit((Object)(object)inputBank))
		{
			bool flag = false;
			if (((Vector3)(ref inputMoveVector)).sqrMagnitude > 0f)
			{
				flag = true;
				characterDirection.moveVector = linkVector;
				if (linkLerp == 0f || linkLerp == 1f)
				{
					NodeGraph.NodeIndex nodeIndex = ((linkLerp != 0f) ? nodeB : nodeA);
					NodeGraph.LinkIndex[] activeNodeLinks = railGraph.GetActiveNodeLinks(nodeIndex);
					float num = -1f;
					NodeGraph.LinkIndex linkIndex = currentLink;
					Debug.DrawRay(((Component)this).transform.position, inputMoveVector, Color.green);
					NodeGraph.LinkIndex[] array = activeNodeLinks;
					Vector3 val2 = default(Vector3);
					foreach (NodeGraph.LinkIndex linkIndex2 in array)
					{
						NodeGraph.NodeIndex linkStartNode = railGraph.GetLinkStartNode(linkIndex2);
						NodeGraph.NodeIndex linkEndNode = railGraph.GetLinkEndNode(linkIndex2);
						if (!(linkStartNode != nodeIndex))
						{
							railGraph.GetNodePosition(linkStartNode, out var position);
							railGraph.GetNodePosition(linkEndNode, out var position2);
							Vector3 val = position2 - position;
							((Vector3)(ref val2))._002Ector(val.x, 0f, val.z);
							Debug.DrawRay(position, val, Color.red);
							float num2 = Vector3.Dot(inputMoveVector, val2);
							if (num2 > num)
							{
								num = num2;
								linkIndex = linkIndex2;
							}
						}
					}
					if (linkIndex != currentLink)
					{
						currentLink = linkIndex;
						UpdateNodeAndLinkInfo();
						linkLerp = 0f;
					}
				}
			}
			modelAnimator.SetBool("isMoving", flag);
			if (useRootMotion)
			{
				TravelLink();
			}
			else
			{
				TravelLink();
			}
		}
		((Component)this).transform.position = Vector3.Lerp(nodeAPosition, nodeBPosition, linkLerp);
	}

	private void TravelLink()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		projectedMoveVector = Vector3.Project(inputMoveVector, linkVector);
		projectedMoveVector = ((Vector3)(ref projectedMoveVector)).normalized;
		if (characterBody.rootMotionInMainState)
		{
			currentMoveSpeed = ((Vector3)(ref rootMotion)).magnitude / Time.fixedDeltaTime;
			rootMotion = Vector3.zero;
		}
		else
		{
			float num = ((!(((Vector3)(ref projectedMoveVector)).sqrMagnitude > 0f)) ? 0f : (characterBody.moveSpeed * ((Vector3)(ref inputMoveVector)).magnitude));
			currentMoveSpeed = Mathf.MoveTowards(currentMoveSpeed, num, characterBody.acceleration * Time.fixedDeltaTime);
		}
		if (currentMoveSpeed > 0f)
		{
			Vector3 val = projectedMoveVector * currentMoveSpeed;
			float num2 = currentMoveSpeed / linkLength * Mathf.Sign(Vector3.Dot(val, linkVector)) * Time.fixedDeltaTime;
			linkLerp = Mathf.Clamp01(linkLerp + num2);
		}
	}

	private void OnDrawGizmosSelected()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		Gizmos.color = Color.green;
		Gizmos.DrawSphere(nodeAPosition, 0.5f);
		Gizmos.color = Color.yellow;
		Gizmos.DrawSphere(nodeBPosition, 0.5f);
		Gizmos.DrawLine(nodeAPosition, nodeBPosition);
	}
}
