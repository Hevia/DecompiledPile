using UnityEngine;

namespace RoR2;

public class MonsterCounter : MonoBehaviour
{
	private int enemyList;

	private int CountEnemies()
	{
		return TeamComponent.GetTeamMembers(TeamIndex.Monster).Count;
	}

	private void Update()
	{
		enemyList = CountEnemies();
	}

	private void OnGUI()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		GUI.Label(new Rect(12f, 160f, 200f, 25f), "Living Monsters: " + enemyList);
	}
}
