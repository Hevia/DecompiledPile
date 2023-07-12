using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using HG;
using HG.Reflection;
using RoR2.ConVar;
using UnityEngine;
using UnityEngine.Networking;
using Zio;

namespace RoR2;

public class Console : MonoBehaviour
{
	public struct Log
	{
		public string message;

		public string stackTrace;

		public LogType logType;
	}

	public delegate void LogReceivedDelegate(Log log);

	private class Lexer
	{
		private enum TokenType
		{
			Identifier,
			NestedString
		}

		private string srcString;

		private int readIndex;

		private StringBuilder stringBuilder = new StringBuilder();

		public Lexer(string srcString)
		{
			this.srcString = srcString;
			readIndex = 0;
		}

		private static bool IsIgnorableCharacter(char character)
		{
			if (!IsSeparatorCharacter(character) && !IsQuoteCharacter(character) && !IsIdentifierCharacter(character))
			{
				return character != '/';
			}
			return false;
		}

		private static bool IsSeparatorCharacter(char character)
		{
			if (character != ';')
			{
				return character == '\n';
			}
			return true;
		}

		private static bool IsQuoteCharacter(char character)
		{
			if (character != '\'')
			{
				return character == '"';
			}
			return true;
		}

		private static bool IsIdentifierCharacter(char character)
		{
			if (!char.IsLetterOrDigit(character) && character != '_' && character != '.' && character != '-')
			{
				return character == ':';
			}
			return true;
		}

		private bool TrimComment()
		{
			if (readIndex >= srcString.Length)
			{
				return false;
			}
			if (srcString[readIndex] == '/')
			{
				if (readIndex + 1 < srcString.Length)
				{
					char c = srcString[readIndex + 1];
					if (c == '/')
					{
						while (readIndex < srcString.Length)
						{
							if (srcString[readIndex] == '\n')
							{
								readIndex++;
								return true;
							}
							readIndex++;
						}
						return true;
					}
					if (c == '*')
					{
						while (readIndex < srcString.Length - 1)
						{
							if (srcString[readIndex] == '*' && srcString[readIndex + 1] == '/')
							{
								readIndex += 2;
								return true;
							}
							readIndex++;
						}
						return true;
					}
				}
				readIndex++;
			}
			return false;
		}

		private void TrimWhitespace()
		{
			while (readIndex < srcString.Length && IsIgnorableCharacter(srcString[readIndex]))
			{
				readIndex++;
			}
		}

		private void TrimUnused()
		{
			do
			{
				TrimWhitespace();
			}
			while (TrimComment());
		}

		private static int UnescapeNext(string srcString, int startPos, out char result)
		{
			result = '\\';
			int num = startPos;
			num++;
			if (num < srcString.Length)
			{
				char c = srcString[num];
				switch (c)
				{
				case '"':
				case '\'':
				case '\\':
					result = c;
					return 2;
				case 'n':
					result = '\n';
					return 2;
				}
			}
			return 1;
		}

		public string NextToken()
		{
			TrimUnused();
			if (readIndex == srcString.Length)
			{
				return null;
			}
			TokenType tokenType = TokenType.Identifier;
			char c = srcString[readIndex];
			char c2 = '\0';
			if (IsQuoteCharacter(c))
			{
				tokenType = TokenType.NestedString;
				c2 = c;
				readIndex++;
			}
			else if (IsSeparatorCharacter(c))
			{
				readIndex++;
				return ";";
			}
			char result;
			for (; readIndex < srcString.Length; stringBuilder.Append(result), readIndex++)
			{
				result = srcString[readIndex];
				switch (tokenType)
				{
				case TokenType.Identifier:
					if (IsIdentifierCharacter(result))
					{
						continue;
					}
					break;
				case TokenType.NestedString:
					if (result == '\\')
					{
						readIndex += UnescapeNext(srcString, readIndex, out result) - 1;
						continue;
					}
					if (result != c2)
					{
						continue;
					}
					readIndex++;
					break;
				default:
					continue;
				}
				break;
			}
			string result2 = stringBuilder.ToString();
			stringBuilder.Length = 0;
			return result2;
		}

		public Queue<string> GetTokens()
		{
			Queue<string> queue = new Queue<string>();
			for (string text = NextToken(); text != null; text = NextToken())
			{
				queue.Enqueue(text);
			}
			queue.Enqueue(";");
			return queue;
		}
	}

	private class Substring
	{
		public string srcString;

		public int startIndex;

		public int length;

		public int endIndex => startIndex + length;

		public string str => srcString.Substring(startIndex, length);

		public Substring nextToken => new Substring
		{
			srcString = srcString,
			startIndex = startIndex + length,
			length = 0
		};
	}

	private class ConCommand
	{
		public ConVarFlags flags;

		public ConCommandDelegate action;

		public string helpText;
	}

	public delegate void ConCommandDelegate(ConCommandArgs args);

	public readonly struct CmdSender
	{
		public readonly LocalUser localUser;

		public readonly NetworkUser networkUser;

		public CmdSender(LocalUser localUser)
		{
			this.localUser = localUser;
			networkUser = localUser?.currentNetworkUser;
		}

		public CmdSender(NetworkUser networkUser)
		{
			localUser = networkUser?.localUser;
			this.networkUser = networkUser;
		}

		public static implicit operator CmdSender(LocalUser localUser)
		{
			return new CmdSender(localUser);
		}

		public static implicit operator CmdSender(NetworkUser networkUser)
		{
			return new CmdSender(networkUser);
		}
	}

	private enum SystemConsoleType
	{
		None,
		Attach,
		Alloc
	}

	public class AutoComplete
	{
		private struct MatchInfo
		{
			public string str;

			public int similarity;
		}

		private List<string> searchableStrings = new List<string>();

		private string searchString;

		public List<string> resultsList = new List<string>();

		public AutoComplete(Console console)
		{
			HashSet<string> hashSet = new HashSet<string>();
			for (int i = 0; i < userCmdHistory.Count; i++)
			{
				hashSet.Add(userCmdHistory[i]);
			}
			foreach (KeyValuePair<string, BaseConVar> allConVar in console.allConVars)
			{
				hashSet.Add(allConVar.Key);
			}
			foreach (KeyValuePair<string, ConCommand> item in console.concommandCatalog)
			{
				hashSet.Add(item.Key);
			}
			foreach (string item2 in hashSet)
			{
				searchableStrings.Add(item2);
			}
			searchableStrings.Sort();
		}

		public bool SetSearchString(string newSearchString)
		{
			newSearchString = newSearchString.ToLower(CultureInfo.InvariantCulture);
			if (newSearchString == searchString)
			{
				return false;
			}
			searchString = newSearchString;
			List<MatchInfo> list = new List<MatchInfo>();
			for (int i = 0; i < searchableStrings.Count; i++)
			{
				string text = searchableStrings[i];
				int num = Math.Min(text.Length, searchString.Length);
				int j;
				for (j = 0; j < num && char.ToLower(text[j]) == searchString[j]; j++)
				{
				}
				if (j > 1)
				{
					list.Add(new MatchInfo
					{
						str = text,
						similarity = j
					});
				}
			}
			list.Sort(delegate(MatchInfo a, MatchInfo b)
			{
				if (a.similarity == b.similarity)
				{
					return string.CompareOrdinal(a.str, b.str);
				}
				return (a.similarity <= b.similarity) ? 1 : (-1);
			});
			resultsList = new List<string>();
			for (int k = 0; k < list.Count; k++)
			{
				resultsList.Add(list[k].str);
			}
			return true;
		}
	}

	public class CheatsConVar : BaseConVar
	{
		public static readonly CheatsConVar instance = new CheatsConVar("cheats", ConVarFlags.ExecuteOnServer, "0", "Enable cheats. Achievements, unlock progression, and stat tracking will be disabled until the application is restarted.");

		private bool _boolValue;

		public bool boolValue
		{
			get
			{
				return _boolValue;
			}
			private set
			{
				if (_boolValue)
				{
					sessionCheatsEnabled = true;
				}
			}
		}

		public CheatsConVar(string name, ConVarFlags flags, string defaultValue, string helpText)
			: base(name, flags, defaultValue, helpText)
		{
		}

		public override void SetString(string newValue)
		{
			if (TextSerialization.TryParseInvariant(newValue, out int result))
			{
				boolValue = result != 0;
			}
		}

		public override string GetString()
		{
			if (!boolValue)
			{
				return "0";
			}
			return "1";
		}
	}

	public static List<Log> logs = new List<Log>();

	private Dictionary<string, string> vstrs = new Dictionary<string, string>();

	private Dictionary<string, ConCommand> concommandCatalog = new Dictionary<string, ConCommand>();

	private Dictionary<string, BaseConVar> allConVars;

	private List<BaseConVar> archiveConVars;

	public static List<string> userCmdHistory = new List<string>();

	private const int VK_RETURN = 13;

	private const int WM_KEYDOWN = 256;

	private static byte[] inputStreamBuffer = new byte[256];

	private static Queue<string> stdInQueue = new Queue<string>();

	private static Thread stdInReaderThread = null;

	private static SystemConsoleType systemConsoleType = SystemConsoleType.None;

	private static readonly StringBuilder sharedStringBuilder = new StringBuilder();

	private const string configFolder = "/Config/";

	private const string archiveConVarsPath = "/Config/config.cfg";

	private static IntConVar maxMessages = new IntConVar("max_messages", ConVarFlags.Archive, "25", "Maximum number of messages that can be held in the console log.");

	public static Console instance { get; private set; }

	public static bool sessionCheatsEnabled { get; private set; } = false;


	public static event LogReceivedDelegate onLogReceived;

	public static event Action onClear;

	[RuntimeInitializeOnLoadMethod(/*Could not decode attribute arguments.*/)]
	private static void RegisterLogHandler()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Expected O, but got Unknown
		Application.logMessageReceived += new LogCallback(HandleLog);
	}

	private static void HandleLog(string message, string stackTrace, LogType logType)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Invalid comparison between Unknown and I4
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		if ((int)logType == 0)
		{
			message = string.Format(CultureInfo.InvariantCulture, "<color=#FF0000>{0}</color>", message);
		}
		else if ((int)logType == 2)
		{
			message = string.Format(CultureInfo.InvariantCulture, "<color=#FFFF00>{0}</color>", message);
		}
		Log log = default(Log);
		log.message = message;
		log.stackTrace = stackTrace;
		log.logType = logType;
		Log log2 = log;
		logs.Add(log2);
		if (maxMessages.value > 0)
		{
			while (logs.Count > maxMessages.value)
			{
				logs.RemoveAt(0);
			}
		}
		if (Console.onLogReceived != null)
		{
			Console.onLogReceived(log2);
		}
	}

	private string GetVstrValue(LocalUser user, string identifier)
	{
		if (user == null)
		{
			if (vstrs.TryGetValue(identifier, out var value))
			{
				return value;
			}
			return "";
		}
		return "";
	}

	private void InitConVars()
	{
		allConVars = new Dictionary<string, BaseConVar>();
		archiveConVars = new List<BaseConVar>();
		Type[] types = typeof(BaseConVar).Assembly.GetTypes();
		foreach (Type type in types)
		{
			FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (FieldInfo fieldInfo in fields)
			{
				if (fieldInfo.FieldType.IsSubclassOf(typeof(BaseConVar)))
				{
					if (fieldInfo.IsStatic)
					{
						BaseConVar conVar = (BaseConVar)fieldInfo.GetValue(null);
						RegisterConVarInternal(conVar);
					}
					else if (type.GetCustomAttribute<CompilerGeneratedAttribute>() == null)
					{
						Debug.LogErrorFormat("ConVar defined as {0}.{1} could not be registered. ConVars must be static fields.", new object[2] { type.Name, fieldInfo.Name });
					}
				}
			}
			MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (MethodInfo methodInfo in methods)
			{
				if (methodInfo.GetCustomAttribute<ConVarProviderAttribute>() == null)
				{
					continue;
				}
				if (methodInfo.ReturnType != typeof(IEnumerable<BaseConVar>) || methodInfo.GetParameters().Length != 0)
				{
					Debug.LogErrorFormat("ConVar provider {0}.{1} does not match the signature \"static IEnumerable<ConVar.BaseConVar>()\".", new object[2] { type.Name, methodInfo.Name });
					continue;
				}
				if (!methodInfo.IsStatic)
				{
					Debug.LogErrorFormat("ConVar provider {0}.{1} could not be invoked. Methods marked with the ConVarProvider attribute must be static.", new object[2] { type.Name, methodInfo.Name });
					continue;
				}
				foreach (BaseConVar item in (IEnumerable<BaseConVar>)methodInfo.Invoke(null, Array.Empty<object>()))
				{
					RegisterConVarInternal(item);
				}
			}
		}
		foreach (KeyValuePair<string, BaseConVar> allConVar in allConVars)
		{
			try
			{
				BaseConVar value = allConVar.Value;
				if ((value.flags & ConVarFlags.Engine) != 0)
				{
					value.defaultValue = value.GetString();
				}
				else if (value.defaultValue != null)
				{
					value.AttemptSetString(value.defaultValue);
				}
			}
			catch (Exception ex)
			{
				Debug.LogError((object)ex);
			}
		}
	}

	private void RegisterConVarInternal(BaseConVar conVar)
	{
		if (conVar == null)
		{
			Debug.LogWarning((object)"Attempted to register null ConVar");
			return;
		}
		allConVars[conVar.name] = conVar;
		if ((conVar.flags & ConVarFlags.Archive) != 0)
		{
			archiveConVars.Add(conVar);
		}
	}

	public BaseConVar FindConVar(string name)
	{
		if (allConVars.TryGetValue(name, out var value))
		{
			return value;
		}
		return null;
	}

	public void SubmitCmd(NetworkUser sender, string cmd, bool recordSubmit = false)
	{
		SubmitCmd((CmdSender)sender, cmd, recordSubmit);
	}

	public void SubmitCmd(CmdSender sender, string cmd, bool recordSubmit = false)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		if (recordSubmit)
		{
			Log log = default(Log);
			log.message = string.Format(CultureInfo.InvariantCulture, "<color=#C0C0C0>] {0}</color>", cmd);
			log.stackTrace = "";
			log.logType = (LogType)3;
			Log log2 = log;
			logs.Add(log2);
			if (Console.onLogReceived != null)
			{
				Console.onLogReceived(log2);
			}
			userCmdHistory.Add(cmd);
		}
		Queue<string> tokens = new Lexer(cmd).GetTokens();
		List<string> list = new List<string>();
		bool flag = false;
		while (tokens.Count != 0)
		{
			string text = tokens.Dequeue();
			if (text == ";")
			{
				flag = false;
				if (list.Count > 0)
				{
					string concommandName = list[0].ToLower(CultureInfo.InvariantCulture);
					list.RemoveAt(0);
					RunCmd(sender, concommandName, list);
					list.Clear();
				}
			}
			else
			{
				if (flag)
				{
					text = GetVstrValue(sender.localUser, text);
					flag = false;
				}
				if (text == "vstr")
				{
					flag = true;
				}
				else
				{
					list.Add(text);
				}
			}
		}
	}

	private void ForwardCmdToServer(ConCommandArgs args)
	{
		if (Object.op_Implicit((Object)(object)args.sender))
		{
			args.sender.CallCmdSendConsoleCommand(args.commandName, args.userArgs.ToArray());
		}
	}

	public void RunClientCmd(NetworkUser sender, string concommandName, string[] args)
	{
		RunCmd(sender, concommandName, new List<string>(args));
	}

	private void RunCmd(CmdSender sender, string concommandName, List<string> userArgs)
	{
		NetworkUser networkUser = sender.networkUser;
		bool flag = networkUser != null && !((NetworkBehaviour)networkUser).isLocalPlayer;
		ConVarFlags conVarFlags = ConVarFlags.None;
		ConCommand value = null;
		BaseConVar baseConVar = null;
		if (concommandCatalog.TryGetValue(concommandName, out value))
		{
			conVarFlags = value.flags;
		}
		else
		{
			baseConVar = FindConVar(concommandName);
			if (baseConVar == null)
			{
				Debug.LogFormat("\"{0}\" is not a recognized ConCommand or ConVar.", new object[1] { concommandName });
				return;
			}
			conVarFlags = baseConVar.flags;
		}
		bool flag2 = (conVarFlags & ConVarFlags.ExecuteOnServer) != 0;
		if (!NetworkServer.active && flag2)
		{
			ForwardCmdToServer(new ConCommandArgs
			{
				sender = sender.networkUser,
				commandName = concommandName,
				userArgs = userArgs
			});
			return;
		}
		if (flag && (conVarFlags & ConVarFlags.SenderMustBeServer) != 0)
		{
			Debug.LogFormat("Blocked server-only command {0} from remote user {1}.", new object[2]
			{
				concommandName,
				sender.networkUser.userName
			});
			return;
		}
		if (flag && !flag2)
		{
			Debug.LogFormat("Blocked non-transmittable command {0} from remote user {1}.", new object[2]
			{
				concommandName,
				sender.networkUser.userName
			});
			return;
		}
		if ((conVarFlags & ConVarFlags.Cheat) != 0 && !CheatsConVar.instance.boolValue)
		{
			Debug.LogFormat("Command \"{0}\" cannot be used while cheats are disabled.", new object[1] { concommandName });
			return;
		}
		if (value != null)
		{
			try
			{
				value.action(new ConCommandArgs
				{
					sender = sender.networkUser,
					localUserSender = sender.localUser,
					commandName = concommandName,
					userArgs = userArgs
				});
				return;
			}
			catch (ConCommandException ex)
			{
				Debug.LogFormat("Command \"{0}\" failed: {1}", new object[2] { concommandName, ex.Message });
				return;
			}
		}
		if (baseConVar != null)
		{
			if (userArgs.Count > 0)
			{
				baseConVar.AttemptSetString(userArgs[0]);
				return;
			}
			Debug.LogFormat("\"{0}\" = \"{1}\"\n{2}", new object[3]
			{
				concommandName,
				baseConVar.GetString(),
				baseConVar.helpText
			});
		}
	}

	[DllImport("kernel32.dll")]
	private static extern bool AllocConsole();

	[DllImport("kernel32.dll")]
	private static extern bool FreeConsole();

	[DllImport("kernel32.dll")]
	private static extern bool AttachConsole(int processId);

	[DllImport("user32.dll")]
	private static extern bool PostMessage(IntPtr hWnd, uint msg, int wParam, int lParam);

	[DllImport("kernel32.dll")]
	private static extern IntPtr GetConsoleWindow();

	private static string ReadInputStream()
	{
		if (stdInQueue.Count > 0)
		{
			return stdInQueue.Dequeue();
		}
		return null;
	}

	private static void ThreadedInputQueue()
	{
		string item;
		while (systemConsoleType != 0 && (item = System.Console.ReadLine()) != null)
		{
			stdInQueue.Enqueue(item);
		}
	}

	private static void SetupSystemConsole()
	{
		bool flag = false;
		bool flag2 = false;
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		for (int i = 0; i < commandLineArgs.Length; i++)
		{
			if (commandLineArgs[i] == "-console")
			{
				flag = flag || true;
			}
			if (commandLineArgs[i] == "-console_detach")
			{
				flag2 = flag2 || true;
			}
		}
		if (flag)
		{
			systemConsoleType = SystemConsoleType.Attach;
			if (flag2)
			{
				systemConsoleType = SystemConsoleType.Alloc;
			}
		}
		switch (systemConsoleType)
		{
		case SystemConsoleType.Attach:
			AttachConsole(-1);
			break;
		case SystemConsoleType.Alloc:
			AllocConsole();
			break;
		}
		if (systemConsoleType != 0)
		{
			System.Console.SetIn(new StreamReader(System.Console.OpenStandardInput()));
			stdInReaderThread = new Thread(ThreadedInputQueue);
			stdInReaderThread.Start();
		}
	}

	private void Awake()
	{
		instance = this;
		SetupSystemConsole();
		InitConVars();
		foreach (ConCommandAttribute instance in SearchableAttribute.GetInstances<ConCommandAttribute>())
		{
			concommandCatalog[instance.commandName.ToLower(CultureInfo.InvariantCulture)] = new ConCommand
			{
				flags = instance.flags,
				action = (ConCommandDelegate)Delegate.CreateDelegate(typeof(ConCommandDelegate), ((SearchableAttribute)instance).target as MethodInfo),
				helpText = instance.helpText
			};
		}
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		StringBuilder stringBuilder = StringBuilderPool.RentStringBuilder();
		stringBuilder.AppendLine("Launch Parameters: ");
		for (int i = 0; i < commandLineArgs.Length; i++)
		{
			stringBuilder.Append("  arg[").AppendInt(i).Append("]=\"")
				.Append(commandLineArgs[i])
				.Append("\"")
				.AppendLine();
		}
		Debug.Log((object)stringBuilder);
		stringBuilder = StringBuilderPool.ReturnStringBuilder(stringBuilder);
		MPEventSystemManager.availability.CallWhenAvailable(LoadStartupConfigs);
	}

	private void LoadStartupConfigs()
	{
		try
		{
			SubmitCmd(null, "exec config");
			SubmitCmd(null, "exec autoexec");
		}
		catch (Exception ex)
		{
			Debug.LogError((object)ex);
		}
	}

	private void Update()
	{
		string cmd;
		while ((cmd = ReadInputStream()) != null)
		{
			SubmitCmd(null, cmd, recordSubmit: true);
		}
	}

	private void OnDestroy()
	{
		if (stdInReaderThread != null)
		{
			stdInReaderThread = null;
		}
		if (systemConsoleType != 0)
		{
			systemConsoleType = SystemConsoleType.None;
			IntPtr consoleWindow = GetConsoleWindow();
			if (consoleWindow != IntPtr.Zero)
			{
				PostMessage(consoleWindow, 256u, 13, 0);
			}
			if (stdInReaderThread != null)
			{
				stdInReaderThread.Join();
				stdInReaderThread = null;
			}
			FreeConsole();
		}
	}

	private static string LoadConfig(string fileName)
	{
		string text = sharedStringBuilder.Clear().Append("/Config/").Append(fileName)
			.Append(".cfg")
			.ToString();
		try
		{
			return PlatformSystems.textDataManager.GetConfFile(fileName, text);
		}
		catch (IOException ex)
		{
			Debug.LogFormat("Could not load config {0}: {1}", new object[2] { text, ex.Message });
		}
		return null;
	}

	public void SaveArchiveConVars()
	{
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		Debug.Log((object)"in save archive convars");
		using MemoryStream memoryStream = new MemoryStream();
		using TextWriter textWriter = new StreamWriter(memoryStream, Encoding.UTF8);
		for (int i = 0; i < archiveConVars.Count; i++)
		{
			BaseConVar baseConVar = archiveConVars[i];
			textWriter.Write(baseConVar.name);
			textWriter.Write(" ");
			textWriter.Write(baseConVar.GetString());
			textWriter.Write(";\r\n");
		}
		textWriter.Write("echo \"Loaded archived convars.\";");
		textWriter.Flush();
		RoR2Application.fileSystem.CreateDirectory(UPath.op_Implicit("/Config/"));
		try
		{
			using Stream stream = RoR2Application.fileSystem.OpenFile(UPath.op_Implicit("/Config/config.cfg"), FileMode.Create, FileAccess.Write, FileShare.None);
			if (stream != null)
			{
				stream.Write(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
				stream.Close();
			}
		}
		catch (IOException ex)
		{
			Debug.LogFormat("Failed to write archived convars: {0}", new object[1] { ex.Message });
		}
	}

	[ConCommand(commandName = "set_vstr", flags = ConVarFlags.None, helpText = "Sets the specified vstr to the specified value.")]
	private static void CCSetVstr(ConCommandArgs args)
	{
		args.CheckArgumentCount(2);
		instance.vstrs.Add(args[0], args[1]);
	}

	[ConCommand(commandName = "exec", flags = ConVarFlags.None, helpText = "Executes a named config from the \"Config/\" folder.")]
	private static void CCExec(ConCommandArgs args)
	{
		if (args.Count > 0)
		{
			string text = LoadConfig(args[0]);
			if (text != null)
			{
				instance.SubmitCmd(args.sender, text);
			}
		}
	}

	[ConCommand(commandName = "echo", flags = ConVarFlags.None, helpText = "Echoes the given text to the console.")]
	private static void CCEcho(ConCommandArgs args)
	{
		if (args.Count > 0)
		{
			Debug.Log((object)args[0]);
		}
		else
		{
			ShowHelpText(args.commandName);
		}
	}

	[ConCommand(commandName = "cvarlist", flags = ConVarFlags.None, helpText = "Print all available convars and concommands.")]
	private static void CCCvarList(ConCommandArgs args)
	{
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, BaseConVar> allConVar in instance.allConVars)
		{
			list.Add(allConVar.Key);
		}
		foreach (KeyValuePair<string, ConCommand> item in instance.concommandCatalog)
		{
			list.Add(item.Key);
		}
		list.Sort();
		Debug.Log((object)string.Join("\n", list.ToArray()));
	}

	[ConCommand(commandName = "help", flags = ConVarFlags.None, helpText = "Show help text for the named convar or concommand.")]
	private static void CCHelp(ConCommandArgs args)
	{
		if (args.Count == 0)
		{
			instance.SubmitCmd(args.sender, "find \"*\"");
		}
		else
		{
			ShowHelpText(args[0]);
		}
	}

	[ConCommand(commandName = "find", flags = ConVarFlags.None, helpText = "Find all concommands and convars with the specified substring.")]
	private static void CCFind(ConCommandArgs args)
	{
		if (args.Count == 0)
		{
			ShowHelpText("find");
			return;
		}
		string text = args[0].ToLower(CultureInfo.InvariantCulture);
		bool flag = text == "*";
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, BaseConVar> allConVar in instance.allConVars)
		{
			if (flag || allConVar.Key.ToLower(CultureInfo.InvariantCulture).Contains(text) || allConVar.Value.helpText.ToLower(CultureInfo.InvariantCulture).Contains(text))
			{
				list.Add(allConVar.Key);
			}
		}
		foreach (KeyValuePair<string, ConCommand> item in instance.concommandCatalog)
		{
			if (flag || item.Key.ToLower(CultureInfo.InvariantCulture).Contains(text) || item.Value.helpText.ToLower(CultureInfo.InvariantCulture).Contains(text))
			{
				list.Add(item.Key);
			}
		}
		list.Sort();
		string[] array = new string[list.Count];
		for (int i = 0; i < list.Count; i++)
		{
			array[i] = GetHelpText(list[i]);
		}
		Debug.Log((object)string.Join("\n", array));
	}

	[ConCommand(commandName = "clear", flags = ConVarFlags.None, helpText = "Clears the console output.")]
	private static void CCClear(ConCommandArgs args)
	{
		logs.Clear();
		Console.onClear?.Invoke();
	}

	private static string GetHelpText(string commandName)
	{
		if (instance.concommandCatalog.TryGetValue(commandName, out var value))
		{
			return string.Format(CultureInfo.InvariantCulture, "<color=#FF7F7F>\"{0}\"</color>\n- {1}", commandName, value.helpText);
		}
		BaseConVar baseConVar = instance.FindConVar(commandName);
		if (baseConVar != null)
		{
			return string.Format(CultureInfo.InvariantCulture, "<color=#FF7F7F>\"{0}\" = \"{1}\"</color>\n - {2}", commandName, baseConVar.GetString(), baseConVar.helpText);
		}
		return "";
	}

	public static void ShowHelpText(string commandName)
	{
		Debug.Log((object)GetHelpText(commandName));
	}
}
