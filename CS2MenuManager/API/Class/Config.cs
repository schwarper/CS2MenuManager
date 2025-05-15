using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Utils;
using System.Drawing;
using System.Reflection;
using Tomlyn;
using Tomlyn.Model;
using static CS2MenuManager.API.Class.ResolutionManager;

namespace CS2MenuManager.API.Class;

internal static class ConfigManager
{
    public class Cfg
    {
        public ButtonsKey Buttons { get; set; } = new();
        public Sound Sound { get; set; } = new();
        public MySQL MySQL { get; set; } = new();
        public ChatMenuSettings ChatMenu { get; set; } = new();
        public CenterHtmlMenuSettings CenterHtmlMenu { get; set; } = new();
        public WasdMenuSettings WasdMenu { get; set; } = new();
        public ScreenMenu ScreenMenu { get; set; } = new();
        public string DefaultMenuType { get; set; } = "ScreenMenu";
        public Dictionary<string, Resolution> Resolutions { get; set; } = [];
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
        public int MaxTitleLength { get; set; } = 0;
        public int MaxOptionLength { get; set; } = 0;
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
        public bool FreezePlayer { get; set; } = false;
    }

    public class ScreenMenu
    {
        public Color TextColor { get; set; } = Color.FromArgb(232, 155, 27);
        public Color DisabledTextColor { get; set; } = Color.FromArgb(231, 210, 177);
        public float PositionX { get; set; } = -5.5f;
        public float PositionY { get; set; } = 0.0f;
        public string Font { get; set; } = "Tahoma Bold";
        public int Size { get; set; } = 32;
        public bool FreezePlayer { get; set; } = false;
        public bool ShowResolutionsOption { get; set; } = true;
        public string MenuType { get; set; } = "Both";
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

        LoadButtonsConfig(model);
        LoadSoundConfig(model);
        LoadMySQLConfig(model);
        LoadChatMenuConfig(model);
        LoadCenterHtmlMenuConfig(model);
        LoadWasdMenuConfig(model);
        LoadScreenMenuConfig(model);
        LoadResolutions(model);
        LoadLanguages(model);
        LoadDefaultMenuType(model);

        Database.CreateDatabase();
    }

    private static void LoadButtonsConfig(TomlTable model)
    {
        if (model.TryGetValue("Buttons", out object? buttonsObj) && buttonsObj is TomlTable buttons)
        {
            Config.Buttons.ScrollUp = buttons.GetValueOrDefault("ScrollUp", Config.Buttons.ScrollUp);
            Config.Buttons.ScrollDown = buttons.GetValueOrDefault("ScrollDown", Config.Buttons.ScrollDown);
            Config.Buttons.Select = buttons.GetValueOrDefault("Select", Config.Buttons.Select);
            Config.Buttons.Prev = buttons.GetValueOrDefault("Prev", Config.Buttons.Prev);
            Config.Buttons.Exit = buttons.GetValueOrDefault("Exit", Config.Buttons.Exit);
        }
    }

    private static void LoadSoundConfig(TomlTable model)
    {
        if (model.TryGetValue("Sound", out object? soundObj) && soundObj is TomlTable sound)
        {
            Config.Sound.Select = sound.GetValueOrDefault("Select", Config.Sound.Select);
            Config.Sound.Exit = sound.GetValueOrDefault("Exit", Config.Sound.Exit);
            Config.Sound.ScrollUp = sound.GetValueOrDefault("ScrollUp", Config.Sound.ScrollUp);
            Config.Sound.ScrollDown = sound.GetValueOrDefault("ScrollDown", Config.Sound.ScrollDown);
        }
    }

    private static void LoadMySQLConfig(TomlTable model)
    {
        if (model.TryGetValue("MySQL", out object? mysqlObj) && mysqlObj is TomlTable mysql)
        {
            Config.MySQL.Host = mysql.GetValueOrDefault("Host", Config.MySQL.Host);
            Config.MySQL.Name = mysql.GetValueOrDefault("Name", Config.MySQL.Name);
            Config.MySQL.User = mysql.GetValueOrDefault("User", Config.MySQL.User);
            Config.MySQL.Pass = mysql.GetValueOrDefault("Pass", Config.MySQL.Pass);
            Config.MySQL.Port = mysql.GetValueOrDefault("Port", Config.MySQL.Port);
        }
    }

    private static void LoadChatMenuConfig(TomlTable model)
    {
        if (model.TryGetValue("ChatMenu", out object? chatMenuObj) && chatMenuObj is TomlTable chatMenu)
        {
            Config.ChatMenu.TitleColor = chatMenu.GetValueOrDefault("TitleColor", Config.ChatMenu.TitleColor.ToString()).GetChatColor();
            Config.ChatMenu.EnabledColor = chatMenu.GetValueOrDefault("EnabledColor", Config.ChatMenu.EnabledColor.ToString()).GetChatColor();
            Config.ChatMenu.DisabledColor = chatMenu.GetValueOrDefault("DisabledColor", Config.ChatMenu.DisabledColor.ToString()).GetChatColor();
            Config.ChatMenu.PrevPageColor = chatMenu.GetValueOrDefault("PrevPageColor", Config.ChatMenu.PrevPageColor.ToString()).GetChatColor();
            Config.ChatMenu.NextPageColor = chatMenu.GetValueOrDefault("NextPageColor", Config.ChatMenu.NextPageColor.ToString()).GetChatColor();
            Config.ChatMenu.ExitColor = chatMenu.GetValueOrDefault("ExitColor", Config.ChatMenu.ExitColor.ToString()).GetChatColor();
        }
    }

    private static void LoadCenterHtmlMenuConfig(TomlTable model)
    {
        if (model.TryGetValue("CenterHtmlMenu", out object? centerHtmlObj) && centerHtmlObj is TomlTable centerHtml)
        {
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
    }

    private static void LoadWasdMenuConfig(TomlTable model)
    {
        if (model.TryGetValue("WasdMenu", out object? wasdMenuObj) && wasdMenuObj is TomlTable wasdMenu)
        {
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
    }

    private static void LoadScreenMenuConfig(TomlTable model)
    {
        if (model.TryGetValue("ScreenMenu", out object? screenMenuObj) && screenMenuObj is TomlTable screenMenu)
        {
            Config.ScreenMenu.PositionX = screenMenu.GetValueOrDefault("PositionX", Config.ScreenMenu.PositionX);
            Config.ScreenMenu.PositionY = screenMenu.GetValueOrDefault("PositionY", Config.ScreenMenu.PositionY);
            Config.ScreenMenu.Font = screenMenu.GetValueOrDefault("Font", Config.ScreenMenu.Font);
            Config.ScreenMenu.Size = screenMenu.GetValueOrDefault("Size", Config.ScreenMenu.Size);
            Config.ScreenMenu.FreezePlayer = screenMenu.GetValueOrDefault("FreezePlayer", Config.ScreenMenu.FreezePlayer);
            Config.ScreenMenu.ShowResolutionsOption = screenMenu.GetValueOrDefault("ShowResolutionsOption", Config.ScreenMenu.ShowResolutionsOption);
            Config.ScreenMenu.MenuType = screenMenu.GetValueOrDefault("MenuType", Config.ScreenMenu.MenuType);

            if (screenMenu.TryGetValue("TextColor", out object? textColorObj))
            {
                string textColor = textColorObj.ToString()!;
                Config.ScreenMenu.TextColor = textColor.StartsWith('#') ?
                    textColor.HexToColor() :
                    Color.FromName(textColor);
            }

            if (screenMenu.TryGetValue("DisabledTextColor", out object? disabledTextColorObj))
            {
                string disabledTextColor = disabledTextColorObj.ToString()!;
                Config.ScreenMenu.DisabledTextColor = disabledTextColor.StartsWith('#') ?
                    disabledTextColor.HexToColor() :
                    Color.FromName(disabledTextColor);
            }
        }
    }

    private static void LoadResolutions(TomlTable model)
    {
        if (model.TryGetValue("Resolutions", out object? resolutionsObj) && resolutionsObj is TomlTable resolutions)
        {
            foreach (KeyValuePair<string, object> resolution in resolutions)
            {
                if (resolution.Value is TomlTable resolutionTable)
                {
                    Config.Resolutions[resolution.Key] = new Resolution
                    {
                        PositionX = resolutionTable.GetValueOrDefault("PositionX", 0f),
                        PositionY = resolutionTable.GetValueOrDefault("PositionY", 0f)
                    };
                }
            }
        }
    }

    private static void LoadLanguages(TomlTable model)
    {
        if (model.TryGetValue("Lang", out object? langObj) && langObj is TomlTable langTable)
        {
            foreach (KeyValuePair<string, object> lang in langTable)
            {
                if (lang.Value is TomlTable translations)
                {
                    Dictionary<string, string> langDict = [];
                    foreach (KeyValuePair<string, object> translation in translations)
                    {
                        langDict[translation.Key] = translation.Value?.ToString() ?? string.Empty;
                    }
                    Config.Lang[lang.Key] = langDict;
                }
            }
        }
    }

    private static void LoadDefaultMenuType(TomlTable model)
    {
        if (model.TryGetValue("DefaultMenuType", out object? menuTypeObj))
        {
            string menuType = menuTypeObj.ToString()!;
            if (MenuManager.MenuTypesList.ContainsKey(menuType))
            {
                Config.DefaultMenuType = menuType;
            }
        }
    }
}