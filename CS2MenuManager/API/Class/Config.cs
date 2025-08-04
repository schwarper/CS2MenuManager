using System.Reflection;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Utils;
using Tomlyn;
using Tomlyn.Model;

namespace CS2MenuManager.API.Class;

internal static class ConfigManager
{
    public class Cfg
    {
        public bool ForceConfigSettings { get; set; } = true;
        public ButtonsKey Buttons { get; set; } = new();
        public Sound Sound { get; set; } = new();
        public MySQL MySQL { get; set; } = new();
        public ChatMenuSettings ChatMenu { get; set; } = new();
        public CenterHtmlMenuSettings CenterHtmlMenu { get; set; } = new();
        public WasdMenuSettings WasdMenu { get; set; } = new();
        public string DefaultMenuType { get; set; } = "WasdMenu";
        public Dictionary<string, Dictionary<string, string>> Lang { get; set; } = [];
    }

    public class ButtonsKey
    {
        public string ScrollUp { get; set; } = "W";
        public string ScrollDown { get; set; } = "S";
        public string Select { get; set; } = "E";
        public string Prev { get; set; } = "Shift";
        public string Exit { get; set; } = "Tab";
    }

    public class Sound
    {
        public string Select { get; set; } = "";
        public string Exit { get; set; } = "";
        public string ScrollUp { get; set; } = "";
        public string ScrollDown { get; set; } = "";
    }

    public class MySQL
    {
        public string Host { get; set; } = "";
        public string Name { get; set; } = "";
        public string User { get; set; } = "";
        public string Pass { get; set; } = "";
        public uint Port { get; set; } = 3306;
    }

    public class ChatMenuSettings
    {
        public char TitleColor { get; set; } = ChatColors.Yellow;
        public char EnabledColor { get; set; } = ChatColors.Green;
        public char DisabledColor { get; set; } = ChatColors.Grey;
        public char PrevPageColor { get; set; } = ChatColors.Yellow;
        public char NextPageColor { get; set; } = ChatColors.Yellow;
        public char ExitColor { get; set; } = ChatColors.Red;
    }

    public class CenterHtmlMenuSettings
    {
        public string TitleColor { get; set; } = "yellow";
        public string EnabledColor { get; set; } = "green";
        public string DisabledColor { get; set; } = "grey";
        public string PrevPageColor { get; set; } = "yellow";
        public string NextPageColor { get; set; } = "yellow";
        public string ExitColor { get; set; } = "red";
        public bool InlinePageOptions { get; set; } = true;
        public int MaxTitleLength { get; set; }
        public int MaxOptionLength { get; set; }
    }

    public class WasdMenuSettings
    {
        public string TitleColor { get; set; } = "green";
        public string ScrollUpDownKeyColor { get; set; } = "cyan";
        public string SelectKeyColor { get; set; } = "green";
        public string PrevKeyColor { get; set; } = "orange";
        public string ExitKeyColor { get; set; } = "red";
        public string SelectedOptionColor { get; set; } = "orange";
        public string OptionColor { get; set; } = "white";
        public string DisabledOptionColor { get; set; } = "grey";
        public string ArrowColor { get; set; } = "purple";
        public bool FreezePlayer { get; set; }
    }

    public static Cfg Config { get; set; } = new();
    private static readonly string ConfigFilePath;

    static ConfigManager()
    {
        string assemblyName = Assembly.GetExecutingAssembly().GetName().Name ?? string.Empty;
        ConfigFilePath = Path.Combine(
            Server.GameDirectory,
            "csgo",
            "addons",
            "counterstrikesharp",
            "shared",
            assemblyName,
            "config.toml"
        );
    }

    public static void LoadConfig()
    {
        if (!File.Exists(ConfigFilePath))
            throw new FileNotFoundException($"Configuration file not found: {ConfigFilePath}");

        string configText = File.ReadAllText(ConfigFilePath);
        TomlTable model = Toml.ToModel(configText);

        LoadForceConfig(model);
        LoadButtonsConfig(model);
        LoadSoundConfig(model);
        LoadMySQLConfig(model);
        LoadChatMenuConfig(model);
        LoadCenterHtmlMenuConfig(model);
        LoadWasdMenuConfig(model);
        LoadLanguages(model);
        LoadDefaultMenuType(model);

        Database.CreateDatabase();
    }

    private static void LoadForceConfig(TomlTable model)
    {
        if (model.TryGetValue("ForceConfigSettings", out object forceConfigObj) &&
            forceConfigObj is TomlTable forceConfig)
        {
            Config.ForceConfigSettings = forceConfig.GetValueOrDefault("ForceConfigSettings", Config.ForceConfigSettings);
        }
    }

    private static void LoadButtonsConfig(TomlTable model)
    {
        if (!model.TryGetValue("Buttons", out object buttonsObj) || buttonsObj is not TomlTable buttons)
            return;

        Config.Buttons.ScrollUp = buttons.GetValueOrDefault("ScrollUp", Config.Buttons.ScrollUp);
        Config.Buttons.ScrollDown = buttons.GetValueOrDefault("ScrollDown", Config.Buttons.ScrollDown);
        Config.Buttons.Select = buttons.GetValueOrDefault("Select", Config.Buttons.Select);
        Config.Buttons.Prev = buttons.GetValueOrDefault("Prev", Config.Buttons.Prev);
        Config.Buttons.Exit = buttons.GetValueOrDefault("Exit", Config.Buttons.Exit);
    }

    private static void LoadSoundConfig(TomlTable model)
    {
        if (!model.TryGetValue("Sound", out object soundObj) || soundObj is not TomlTable sound)
            return;

        Config.Sound.Select = sound.GetValueOrDefault("Select", Config.Sound.Select);
        Config.Sound.Exit = sound.GetValueOrDefault("Exit", Config.Sound.Exit);
        Config.Sound.ScrollUp = sound.GetValueOrDefault("ScrollUp", Config.Sound.ScrollUp);
        Config.Sound.ScrollDown = sound.GetValueOrDefault("ScrollDown", Config.Sound.ScrollDown);
    }

    private static void LoadMySQLConfig(TomlTable model)
    {
        if (!model.TryGetValue("MySQL", out object mysqlObj) || mysqlObj is not TomlTable mysql)
            return;

        Config.MySQL.Host = mysql.GetValueOrDefault("Host", Config.MySQL.Host);
        Config.MySQL.Name = mysql.GetValueOrDefault("Name", Config.MySQL.Name);
        Config.MySQL.User = mysql.GetValueOrDefault("User", Config.MySQL.User);
        Config.MySQL.Pass = mysql.GetValueOrDefault("Pass", Config.MySQL.Pass);
        Config.MySQL.Port = mysql.GetValueOrDefault("Port", Config.MySQL.Port);
    }

    private static void LoadChatMenuConfig(TomlTable model)
    {
        if (!model.TryGetValue("ChatMenu", out object chatMenuObj) || chatMenuObj is not TomlTable chatMenu)
            return;

        Config.ChatMenu.TitleColor = chatMenu.GetValueOrDefault("TitleColor", Config.ChatMenu.TitleColor.ToString()).GetChatColor();
        Config.ChatMenu.EnabledColor = chatMenu.GetValueOrDefault("EnabledColor", Config.ChatMenu.EnabledColor.ToString()).GetChatColor();
        Config.ChatMenu.DisabledColor = chatMenu.GetValueOrDefault("DisabledColor", Config.ChatMenu.DisabledColor.ToString()).GetChatColor();
        Config.ChatMenu.PrevPageColor = chatMenu.GetValueOrDefault("PrevPageColor", Config.ChatMenu.PrevPageColor.ToString()).GetChatColor();
        Config.ChatMenu.NextPageColor = chatMenu.GetValueOrDefault("NextPageColor", Config.ChatMenu.NextPageColor.ToString()).GetChatColor();
        Config.ChatMenu.ExitColor = chatMenu.GetValueOrDefault("ExitColor", Config.ChatMenu.ExitColor.ToString()).GetChatColor();
    }

    private static void LoadCenterHtmlMenuConfig(TomlTable model)
    {
        if (!model.TryGetValue("CenterHtmlMenu", out object centerHtmlObj) || centerHtmlObj is not TomlTable centerHtml)
            return;

        Config.CenterHtmlMenu.TitleColor = centerHtml.GetValueOrDefault("TitleColor", Config.CenterHtmlMenu.TitleColor);
        Config.CenterHtmlMenu.EnabledColor = centerHtml.GetValueOrDefault("EnabledColor", Config.CenterHtmlMenu.EnabledColor);
        Config.CenterHtmlMenu.DisabledColor = centerHtml.GetValueOrDefault("DisabledColor", Config.CenterHtmlMenu.DisabledColor);
        Config.CenterHtmlMenu.PrevPageColor = centerHtml.GetValueOrDefault("PrevPageColor", Config.CenterHtmlMenu.PrevPageColor);
        Config.CenterHtmlMenu.NextPageColor = centerHtml.GetValueOrDefault("NextPageColor", Config.CenterHtmlMenu.NextPageColor);
        Config.CenterHtmlMenu.ExitColor = centerHtml.GetValueOrDefault("ExitColor", Config.CenterHtmlMenu.ExitColor);
        Config.CenterHtmlMenu.InlinePageOptions = centerHtml.GetValueOrDefault("InlinePageOptions", Config.CenterHtmlMenu.InlinePageOptions);
        Config.CenterHtmlMenu.MaxTitleLength = centerHtml.GetValueOrDefault("MaxTitleLength", Config.CenterHtmlMenu.MaxTitleLength);
        Config.CenterHtmlMenu.MaxOptionLength = centerHtml.GetValueOrDefault("MaxOptionLength", Config.CenterHtmlMenu.MaxOptionLength);
    }

    private static void LoadWasdMenuConfig(TomlTable model)
    {
        if (!model.TryGetValue("WasdMenu", out object wasdMenuObj) || wasdMenuObj is not TomlTable wasdMenu)
            return;

        Config.WasdMenu.TitleColor = wasdMenu.GetValueOrDefault("TitleColor", Config.WasdMenu.TitleColor);
        Config.WasdMenu.ScrollUpDownKeyColor = wasdMenu.GetValueOrDefault("ScrollUpDownKeyColor", Config.WasdMenu.ScrollUpDownKeyColor);
        Config.WasdMenu.SelectKeyColor = wasdMenu.GetValueOrDefault("SelectKeyColor", Config.WasdMenu.SelectKeyColor);
        Config.WasdMenu.PrevKeyColor = wasdMenu.GetValueOrDefault("PrevKeyColor", Config.WasdMenu.PrevKeyColor);
        Config.WasdMenu.ExitKeyColor = wasdMenu.GetValueOrDefault("ExitKeyColor", Config.WasdMenu.ExitKeyColor);
        Config.WasdMenu.SelectedOptionColor = wasdMenu.GetValueOrDefault("SelectedOptionColor", Config.WasdMenu.SelectedOptionColor);
        Config.WasdMenu.OptionColor = wasdMenu.GetValueOrDefault("OptionColor", Config.WasdMenu.OptionColor);
        Config.WasdMenu.DisabledOptionColor = wasdMenu.GetValueOrDefault("DisabledOptionColor", Config.WasdMenu.DisabledOptionColor);
        Config.WasdMenu.ArrowColor = wasdMenu.GetValueOrDefault("ArrowColor", Config.WasdMenu.ArrowColor);
        Config.WasdMenu.FreezePlayer = wasdMenu.GetValueOrDefault("FreezePlayer", Config.WasdMenu.FreezePlayer);
    }

    private static void LoadLanguages(TomlTable model)
    {
        if (!model.TryGetValue("Lang", out object langObj) || langObj is not TomlTable langTable)
            return;

        foreach (KeyValuePair<string, object> lang in langTable)
        {
            if (lang.Value is not TomlTable translations)
                continue;

            Dictionary<string, string> langDict = [];
            foreach (KeyValuePair<string, object> translation in translations)
            {
                langDict[translation.Key] = translation.Value.ToString() ?? string.Empty;
            }
            Config.Lang[lang.Key] = langDict;
        }
    }

    private static void LoadDefaultMenuType(TomlTable model)
    {
        if (!model.TryGetValue("DefaultMenuType", out object menuTypeObj))
            return;

        string menuType = menuTypeObj.ToString()!;
        if (MenuManager.MenuTypesList.ContainsKey(menuType))
            Config.DefaultMenuType = menuType;
    }
}