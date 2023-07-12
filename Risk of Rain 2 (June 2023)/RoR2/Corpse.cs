using System;
using System.Collections.Generic;
using RoR2.ConVar;
using UnityEngine;

namespace RoR2;

public class Corpse : MonoBehaviour
{
	private class CorpsesMaxConVar : BaseConVar
	{
		private static CorpsesMaxConVar instance = new CorpsesMaxConVar("corpses_max", ConVarFlags.Archive | ConVarFlags.Engine, "25", "The maximum number of corpses allowed.");

		private CorpsesMaxConVar(string name, ConVarFlags flags, string defaultValue, string helpText)
			: base(name, flags, defaultValue, helpText)
		{
		}

		public override void SetString(string newValue)
		{
			if (TextSerialization.TryParseInvariant(newValue, out int result))
			{
				maxCorpses = result;
			}
		}

		public override string GetString()
		{
			return TextSerialization.ToStringInvariant(maxCorpses);
		}
	}

	public enum DisposalMode
	{
		Hard,
		OutOfSight
	}

	private class CorpseDisposalConVar : BaseConVar
	{
		private static CorpseDisposalConVar instance = new CorpseDisposalConVar("corpses_disposal", ConVarFlags.Archive | ConVarFlags.Engine, null, "The corpse disposal mode. Choices are Hard and OutOfSight.");

		private CorpseDisposalConVar(string name, ConVarFlags flags, string defaultValue, string helpText)
			: base(name, flags, defaultValue, helpText)
		{
		}

		public override void SetString(string newValue)
		{
			try
			{
				DisposalMode disposalMode = (DisposalMode)Enum.Parse(typeof(DisposalMode), newValue, ignoreCase: true);
				if (disposalMode == Corpse.disposalMode)
				{
					return;
				}
				Corpse.disposalMode = disposalMode;
				if (disposalMode == DisposalMode.Hard || disposalMode != DisposalMode.OutOfSight)
				{
					return;
				}
				foreach (Corpse instances in instancesList)
				{
					instances.CollectRenderers();
				}
			}
			catch (ArgumentException)
			{
				Console.ShowHelpText(name);
			}
		}

		public override string GetString()
		{
			return disposalMode.ToString();
		}
	}

	private static readonly List<Corpse> instancesList = new List<Corpse>();

	private Renderer[] renderers;

	private static int maxCorpses = 25;

	private static DisposalMode disposalMode = DisposalMode.OutOfSight;

	private static int maxChecksPerUpdate = 3;

	private static int currentCheckIndex = 0;

	private void CollectRenderers()
	{
		if (renderers == null)
		{
			renderers = ((Component)this).GetComponentsInChildren<Renderer>();
		}
	}

	private void OnEnable()
	{
		instancesList.Add(this);
		if (disposalMode == DisposalMode.OutOfSight)
		{
			CollectRenderers();
		}
	}

	private void OnDisable()
	{
		instancesList.Remove(this);
	}

	[RuntimeInitializeOnLoadMethod(/*Could not decode attribute arguments.*/)]
	private static void StaticInit()
	{
		RoR2Application.onUpdate += StaticUpdate;
	}

	private static void IncrementCurrentCheckIndex()
	{
		currentCheckIndex++;
		if (currentCheckIndex >= instancesList.Count)
		{
			currentCheckIndex = 0;
		}
	}

	private static bool CheckCorpseOutOfSight(Corpse corpse)
	{
		Renderer[] array = corpse.renderers;
		foreach (Renderer val in array)
		{
			if (Object.op_Implicit((Object)(object)val) && val.isVisible)
			{
				return false;
			}
		}
		return true;
	}

	private static void StaticUpdate()
	{
		if (maxCorpses < 0)
		{
			return;
		}
		int num = instancesList.Count - maxCorpses;
		int num2 = Math.Min(Math.Min(num, maxChecksPerUpdate), instancesList.Count);
		switch (disposalMode)
		{
		case DisposalMode.Hard:
		{
			for (int num3 = num - 1; num3 >= 0; num3--)
			{
				DestroyCorpse(instancesList[num3]);
			}
			break;
		}
		case DisposalMode.OutOfSight:
		{
			for (int i = 0; i < num2; i++)
			{
				IncrementCurrentCheckIndex();
				if (CheckCorpseOutOfSight(instancesList[currentCheckIndex]))
				{
					DestroyCorpse(instancesList[currentCheckIndex]);
				}
			}
			break;
		}
		}
	}

	private static void DestroyCorpse(Corpse corpse)
	{
		if (Object.op_Implicit((Object)(object)corpse))
		{
			Object.Destroy((Object)(object)((Component)corpse).gameObject);
		}
	}
}
