using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Facepunch.Steamworks;
using JetBrains.Annotations;
using RoR2.ConVar;
using SimpleJSON;
using UnityEngine;

namespace RoR2;

public class Language
{
	private struct SteamLanguageDef
	{
		public readonly string englishName;

		public readonly string nativeName;

		public readonly string apiName;

		public readonly string webApiName;

		public SteamLanguageDef(string englishName, string nativeName, string apiName, string webApiName)
		{
			this.englishName = englishName;
			this.nativeName = nativeName;
			this.apiName = apiName;
			this.webApiName = webApiName;
		}
	}

	public class LanguageConVar : BaseConVar
	{
		private static readonly string platformString = "platform";

		public static LanguageConVar instance = new LanguageConVar("language", ConVarFlags.Archive, platformString, "Which language to use.");

		private string internalValue = platformString;

		public LanguageConVar(string name, ConVarFlags flags, string defaultValue, string helpText)
			: base(name, flags, defaultValue, helpText)
		{
		}

		public override void SetString(string newValue)
		{
			if (!IsAnyLanguageLoaded())
			{
				internalValue = newValue;
				return;
			}
			if (string.Equals(newValue, "EN_US", StringComparison.Ordinal) || FindLanguageByName(newValue) == null)
			{
				newValue = platformString;
			}
			internalValue = newValue;
			if (string.Equals(internalValue, platformString, StringComparison.Ordinal))
			{
				newValue = GetPlatformLanguageName() ?? "en";
			}
			SetCurrentLanguage(newValue);
		}

		public override string GetString()
		{
			return internalValue;
		}
	}

	private static readonly Dictionary<string, Language> languagesByName = new Dictionary<string, Language>(StringComparer.OrdinalIgnoreCase);

	private static bool isDummyStringOverrideEnabled = false;

	private static string dummyString = ".•.";

	private readonly Dictionary<string, string> stringsByToken = new Dictionary<string, string>();

	private Language fallbackLanguage;

	private string[] folders = Array.Empty<string>();

	private bool foundManifest;

	public readonly string name;

	private static readonly Dictionary<string, SteamLanguageDef> steamLanguageTable = new Dictionary<string, SteamLanguageDef>(StringComparer.OrdinalIgnoreCase)
	{
		["arabic"] = new SteamLanguageDef("Arabic", "العربية", "arabic", "ar"),
		["bulgarian"] = new SteamLanguageDef("Bulgarian", "български език", "bulgarian", "bg"),
		["schinese"] = new SteamLanguageDef("Chinese (Simplified)", "简体中文", "schinese", "zh-CN"),
		["tchinese"] = new SteamLanguageDef("Chinese (Traditional)", "繁體中文", "tchinese", "zh-TW"),
		["czech"] = new SteamLanguageDef("Czech", "čeština", "czech", "cs"),
		["danish"] = new SteamLanguageDef("Danish", "Dansk", "danish", "da"),
		["dutch"] = new SteamLanguageDef("Dutch", "Nederlands", "dutch", "nl"),
		["english"] = new SteamLanguageDef("English", "English", "english", "en"),
		["finnish"] = new SteamLanguageDef("Finnish", "Suomi", "finnish", "fi"),
		["french"] = new SteamLanguageDef("French", "Français", "french", "fr"),
		["german"] = new SteamLanguageDef("German", "Deutsch", "german", "de"),
		["greek"] = new SteamLanguageDef("Greek", "Ελληνικά", "greek", "el"),
		["hungarian"] = new SteamLanguageDef("Hungarian", "Magyar", "hungarian", "hu"),
		["italian"] = new SteamLanguageDef("Italian", "Italiano", "italian", "it"),
		["japanese"] = new SteamLanguageDef("Japanese", "日本語", "japanese", "ja"),
		["koreana"] = new SteamLanguageDef("Korean", "한국어", "koreana", "ko"),
		["korean"] = new SteamLanguageDef("Korean", "한국어", "korean", "ko"),
		["norwegian"] = new SteamLanguageDef("Norwegian", "Norsk", "norwegian", "no"),
		["polish"] = new SteamLanguageDef("Polish", "Polski", "polish", "pl"),
		["portuguese"] = new SteamLanguageDef("Portuguese", "Português", "portuguese", "pt"),
		["brazilian"] = new SteamLanguageDef("Portuguese-Brazil", "Português-Brasil", "brazilian", "pt-BR"),
		["romanian"] = new SteamLanguageDef("Romanian", "Română", "romanian", "ro"),
		["russian"] = new SteamLanguageDef("Russian", "Русский", "russian", "ru"),
		["spanish"] = new SteamLanguageDef("Spanish-Spain", "Español-España", "spanish", "es"),
		["latam"] = new SteamLanguageDef("Spanish-Latin America", "Español-Latinoamérica", "latam", "es-419"),
		["swedish"] = new SteamLanguageDef("Swedish", "Svenska", "swedish", "sv"),
		["thai"] = new SteamLanguageDef("Thai", "ไทย", "thai", "th"),
		["turkish"] = new SteamLanguageDef("Turkish", "Türkçe", "turkish", "tr"),
		["ukrainian"] = new SteamLanguageDef("Ukrainian", "Українська", "ukrainian", "uk"),
		["vietnamese"] = new SteamLanguageDef("Vietnamese", "Tiếng Việt", "vietnamese", "vn")
	};

	public string selfName { get; private set; } = string.Empty;


	public Sprite iconSprite { get; private set; }

	public bool stringsLoaded { get; private set; }

	public bool hasEntries => stringsByToken.Count > 0;

	public static string currentLanguageName { get; private set; } = "";


	public static Language currentLanguage { get; private set; } = null;


	public static Language english { get; private set; }

	public static event Action onCurrentLanguageChanged;

	public static event Action<List<string>> collectLanguageRootFolders;

	private Language()
	{
	}

	private Language(string name)
	{
		this.name = name;
	}

	private void SetFolders([NotNull] IEnumerable<string> newFolders)
	{
		folders = newFolders.ToArray();
		foundManifest = false;
		for (int num = folders.Length - 1; num >= 0; num--)
		{
			string path = folders[num];
			if (Directory.Exists(path))
			{
				string text = Directory.EnumerateFiles(path, "language.json").FirstOrDefault();
				foundManifest |= text != null;
				if (text != null)
				{
					LoadManifest(text);
				}
				string text2 = Directory.EnumerateFiles(path, "icon.png").FirstOrDefault();
				if (text2 != null)
				{
					SetIconSprite(BuildSpriteFromTextureFile(text2));
					break;
				}
			}
		}
	}

	private void SetIconSprite(Sprite newIconSprite)
	{
		if (Object.op_Implicit((Object)(object)iconSprite))
		{
			Object.Destroy((Object)(object)iconSprite.texture);
			Object.Destroy((Object)(object)iconSprite);
		}
		iconSprite = newIconSprite;
	}

	private void LoadManifest(string file)
	{
		using Stream stream = File.Open(file, FileMode.Open, FileAccess.Read);
		using StreamReader streamReader = new StreamReader(stream, Encoding.UTF8);
		JSONNode val = JSON.Parse(streamReader.ReadToEnd());
		if (!(val != (object)null))
		{
			return;
		}
		JSONNode val2 = val["language"];
		if (val2 != (object)null)
		{
			string text = JSONNode.op_Implicit(val2["selfname"]);
			if (text != null)
			{
				selfName = text;
			}
		}
	}

	[NotNull]
	public string GetLocalizedStringByToken([NotNull] string token)
	{
		if (isDummyStringOverrideEnabled)
		{
			return dummyString;
		}
		if (stringsByToken.TryGetValue(token, out var value))
		{
			return value;
		}
		if (fallbackLanguage != null)
		{
			return fallbackLanguage.GetLocalizedStringByToken(token);
		}
		return token;
	}

	[NotNull]
	public string GetLocalizedFormattedStringByToken([NotNull] string token, params object[] args)
	{
		return string.Format(GetLocalizedStringByToken(token), args);
	}

	public void SetStringByToken([NotNull] string token, [NotNull] string localizedString)
	{
		stringsByToken[token] = localizedString;
	}

	public void SetStringsByTokens([NotNull] IEnumerable<KeyValuePair<string, string>> tokenPairs)
	{
		foreach (KeyValuePair<string, string> tokenPair in tokenPairs)
		{
			SetStringByToken(tokenPair.Key, tokenPair.Value);
		}
	}

	public void LoadStrings()
	{
		if (!stringsLoaded)
		{
			stringsLoaded = true;
			List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
			LoadAllTokensFromFolders(folders, list);
			SetStringsByTokens(list);
		}
	}

	public void UnloadStrings()
	{
		if (stringsLoaded)
		{
			stringsLoaded = false;
			stringsByToken.Clear();
		}
	}

	public bool TokenIsRegistered([NotNull] string token)
	{
		return stringsByToken.ContainsKey(token);
	}

	[CanBeNull]
	public static Language FindLanguageByName([NotNull] string languageName)
	{
		if (languagesByName.TryGetValue(languageName, out var value))
		{
			return value;
		}
		return null;
	}

	[NotNull]
	public static string GetString([NotNull] string token, [NotNull] string language)
	{
		return FindLanguageByName(language)?.GetLocalizedStringByToken(token) ?? token;
	}

	[NotNull]
	public static string GetString([NotNull] string token)
	{
		return currentLanguage?.GetLocalizedStringByToken(token) ?? token;
	}

	[NotNull]
	public static string GetStringFormatted([NotNull] string token, params object[] args)
	{
		return currentLanguage?.GetLocalizedFormattedStringByToken(token, args) ?? string.Format(token, args);
	}

	public static bool IsTokenInvalid([NotNull] string token)
	{
		return !(currentLanguage?.TokenIsRegistered(token) ?? false);
	}

	public static IEnumerable<Language> GetAllLanguages()
	{
		return languagesByName.Values;
	}

	[NotNull]
	private static Language GetOrCreateLanguage([NotNull] string languageName)
	{
		if (!languagesByName.TryGetValue(languageName, out var value))
		{
			value = (languagesByName[languageName] = new Language(languageName));
		}
		return value;
	}

	private static void LoadAllTokensFromFolders([NotNull] IEnumerable<string> folders, [NotNull] List<KeyValuePair<string, string>> output)
	{
		foreach (string folder in folders)
		{
			LoadAllTokensFromFolder(folder, output);
		}
	}

	private static void LoadAllTokensFromFolder([NotNull] string folder, [NotNull] List<KeyValuePair<string, string>> output)
	{
		PlatformSystems.textDataManager.GetLocFiles(folder, delegate(string[] contents)
		{
			int num = contents.Length;
			for (int i = 0; i < num; i++)
			{
				LoadTokensFromData(contents[i], output);
			}
		});
	}

	private static void LoadTokensFromData([NotNull] string contents, [NotNull] List<KeyValuePair<string, string>> output)
	{
		JSONNode val = JSON.Parse(contents);
		if (!(val != (object)null))
		{
			return;
		}
		JSONNode val2 = val["strings"];
		if (!(val2 != (object)null))
		{
			return;
		}
		foreach (string key in val2.Keys)
		{
			output.Add(new KeyValuePair<string, string>(key, val2[key].Value));
		}
	}

	private static void LoadTokensFromFile([NotNull] string file, [NotNull] List<KeyValuePair<string, string>> output)
	{
		using Stream stream = File.Open(file, FileMode.Open, FileAccess.Read);
		using StreamReader streamReader = new StreamReader(stream, Encoding.UTF8);
		JSONNode val = JSON.Parse(streamReader.ReadToEnd());
		if (!(val != (object)null))
		{
			return;
		}
		JSONNode val2 = val["strings"];
		if (!(val2 != (object)null))
		{
			return;
		}
		foreach (string key in val2.Keys)
		{
			output.Add(new KeyValuePair<string, string>(key, val2[key].Value));
		}
	}

	[NotNull]
	private static List<string> GetLanguageRootFolders()
	{
		List<string> list = new List<string>();
		try
		{
			Language.collectLanguageRootFolders?.Invoke(list);
		}
		catch (Exception ex)
		{
			Debug.LogErrorFormat("Encountered error loading language folders: {0}", new object[1] { ex });
		}
		return list;
	}

	private static void BuildLanguagesFromFolders()
	{
		IGrouping<string, string>[] array = GetLanguageRootFolders().SelectMany((string languageRootFolder) => Directory.EnumerateDirectories(languageRootFolder)).GroupBy<string, string>((string languageRootFolder) => new DirectoryInfo(languageRootFolder).Name, StringComparer.OrdinalIgnoreCase).ToArray();
		foreach (IGrouping<string, string> grouping in array)
		{
			Language orCreateLanguage = GetOrCreateLanguage(grouping.Key);
			orCreateLanguage.SetFolders(grouping);
			if (!orCreateLanguage.foundManifest)
			{
				languagesByName.Remove(grouping.Key);
			}
		}
	}

	private static bool IsAnyLanguageLoaded()
	{
		return languagesByName.Count > 0;
	}

	private static void SetCurrentLanguage([NotNull] string newCurrentLanguageName)
	{
		Debug.LogFormat("Setting current language to \"{0}\"", new object[1] { newCurrentLanguageName });
		if (currentLanguage != english)
		{
			currentLanguage?.UnloadStrings();
		}
		currentLanguageName = newCurrentLanguageName;
		currentLanguage = FindLanguageByName(currentLanguageName);
		if (currentLanguage == null && string.Compare(currentLanguageName, "en", StringComparison.OrdinalIgnoreCase) != 0)
		{
			Debug.LogFormat("Could not load files for language \"{0}\". Falling back to \"en\".", new object[1] { newCurrentLanguageName });
			currentLanguageName = "en";
			currentLanguage = FindLanguageByName(currentLanguageName);
		}
		currentLanguage?.LoadStrings();
		Language.onCurrentLanguageChanged?.Invoke();
	}

	public static void Init()
	{
		BuildLanguagesFromFolders();
		if (LanguageConVar.instance != null)
		{
			LanguageConVar.instance.SetString(LanguageConVar.instance.GetString());
		}
		english = GetOrCreateLanguage("en");
		english.LoadStrings();
		foreach (Language allLanguage in GetAllLanguages())
		{
			if (allLanguage != english)
			{
				allLanguage.fallbackLanguage = english;
			}
		}
	}

	private static Sprite BuildSpriteFromTextureFile(string file)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		Texture2D val = new Texture2D(2, 2, (TextureFormat)5, false, false);
		((Texture)val).wrapMode = (TextureWrapMode)1;
		ImageConversion.LoadImage(val, File.ReadAllBytes(file), true);
		return Sprite.Create(val, new Rect(0f, 0f, (float)((Texture)val).width, (float)((Texture)val).height), new Vector2(0.5f, 0.5f), 1f, 1u, (SpriteMeshType)0, Vector4.zero);
	}

	[CanBeNull]
	public static string GetPlatformLanguageName()
	{
		Client instance = Client.Instance;
		string text = ((instance != null) ? instance.CurrentLanguage : null);
		if (text == null)
		{
			return null;
		}
		if (steamLanguageTable.TryGetValue(text, out var value))
		{
			return value.webApiName;
		}
		return null;
	}

	[ConCommand(commandName = "language_reload", flags = ConVarFlags.None, helpText = "Reloads the current language.")]
	public static void CCLanguageReload(ConCommandArgs args)
	{
		SetCurrentLanguage(currentLanguageName);
	}

	[ConCommand(commandName = "language_dump_to_json", flags = ConVarFlags.None, helpText = "Combines all files for the given language into a single JSON file.")]
	private static void CCLanguageDumpToJson(ConCommandArgs args)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected O, but got Unknown
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Expected O, but got Unknown
		//IL_0052: Expected O, but got Unknown
		string argString = args.GetArgString(0);
		Language obj = FindLanguageByName(argString) ?? throw new ConCommandException($"'{argString}' is not a valid language name.");
		List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
		LoadAllTokensFromFolders(obj.folders, list);
		new StringBuilder();
		JSONNode val = (JSONNode)new JSONObject();
		JSONObject val2 = new JSONObject();
		JSONNode val3 = (JSONNode)val2;
		val["strings"] = (JSONNode)val2;
		foreach (KeyValuePair<string, string> item in list)
		{
			val3[item.Key] = JSONNode.op_Implicit(item.Value);
		}
		File.WriteAllText("output.json", val.ToString(1), Encoding.UTF8);
	}

	[ConCommand(commandName = "language_dummy_strings", flags = ConVarFlags.None, helpText = "Toggles use of a dummy string for all text")]
	private static void CCLanguageDummyStringsToggle(ConCommandArgs args)
	{
		isDummyStringOverrideEnabled = !isDummyStringOverrideEnabled;
		Debug.Log((object)$"isDummyStringOverrideEnabled={isDummyStringOverrideEnabled}");
	}
}
