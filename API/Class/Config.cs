using CounterStrikeSharp.API;
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
        public WasdMenuSettings WasdMenu { get; set; } = new();
        public ScreenMenu ScreenMenu { get; set; } = new();
        public Dictionary<string, Resolution> Resolutions { get; set; } = [];
        public Dictionary<string, Dictionary<string, string>> Lang { get; set; } = [];
    }

    public class ButtonsKey
    {
        public string ScrollUp { get; set; } = string.Empty;
        public string ScrollDown { get; set; } = string.Empty;
        public string Select { get; set; } = string.Empty;
        public string Prev { get; set; } = string.Empty;
        public string Exit { get; set; } = string.Empty;
    }

    public class Sound
    {
        public string Select { get; set; } = string.Empty;
        public string Exit { get; set; } = string.Empty;
        public string ScrollUp { get; set; } = string.Empty;
        public string ScrollDown { get; set; } = string.Empty;
    }

    public class WasdMenuSettings
    {
        public string TitleColor { get; set; } = string.Empty;
        public string ScrollUpDownKeyColor { get; set; } = string.Empty;
        public string SelectKeyColor { get; set; } = string.Empty;
        public string PrevKeyColor { get; set; } = string.Empty;
        public string ExitKeyColor { get; set; } = string.Empty;
        public string SelectedOptionColor { get; set; } = string.Empty;
        public string OptionColor { get; set; } = string.Empty;
        public string DisabledOptionColor { get; set; } = string.Empty;
        public string ArrowColor { get; set; } = string.Empty;
        public bool FreezePlayer { get; set; } = false;
    }

    public class ScreenMenu
    {
        public string TextColor { get; set; } = string.Empty;
        public float PositionX { get; set; }
        public float PositionY { get; set; }
        public bool Background { get; set; }
        public float BackgroundHeight { get; set; }
        public float BackgroundWidth { get; set; }
        public string Font { get; set; } = string.Empty;
        public int Size { get; set; }
        public bool FreezePlayer { get; set; }
        public bool ShowResolutionsOption { get; set; }
    }

    public static Cfg Config { get; set; } = new();
    private static readonly string ConfigFilePath;
    private static bool _isSet = false;

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
        if (_isSet)
            return;

        if (!File.Exists(ConfigFilePath))
            throw new FileNotFoundException($"Configuration file not found: {ConfigFilePath}");

        _isSet = true;
        string configText = File.ReadAllText(ConfigFilePath);
        TomlTable model = Toml.ToModel(configText);

        TomlTable buttons = (TomlTable)model["Buttons"];
        Config.Buttons.ScrollUp = buttons["ScrollUp"].ToString()!;
        Config.Buttons.ScrollDown = buttons["ScrollDown"].ToString()!;
        Config.Buttons.Select = buttons["Select"].ToString()!;
        Config.Buttons.Prev = buttons["Prev"].ToString()!;
        Config.Buttons.Exit = buttons["Exit"].ToString()!;

        TomlTable soundTable = (TomlTable)model["Sound"];
        Config.Sound.Select = soundTable["Select"].ToString()!;
        Config.Sound.Exit = soundTable["Exit"].ToString()!;
        Config.Sound.ScrollUp = soundTable["ScrollUp"].ToString()!;
        Config.Sound.ScrollDown = soundTable["ScrollDown"].ToString()!;

        TomlTable wasdTable = (TomlTable)model["WasdMenu"];
        Config.WasdMenu.TitleColor = wasdTable["TitleColor"].ToString()!;
        Config.WasdMenu.ScrollUpDownKeyColor = wasdTable["ScrollUpDownKeyColor"].ToString()!;
        Config.WasdMenu.SelectKeyColor = wasdTable["SelectKeyColor"].ToString()!;
        Config.WasdMenu.PrevKeyColor = wasdTable["PrevKeyColor"].ToString()!;
        Config.WasdMenu.ExitKeyColor = wasdTable["ExitKeyColor"].ToString()!;
        Config.WasdMenu.SelectedOptionColor = wasdTable["SelectedOptionColor"].ToString()!;
        Config.WasdMenu.OptionColor = wasdTable["OptionColor"].ToString()!;
        Config.WasdMenu.DisabledOptionColor = wasdTable["DisabledOptionColor"].ToString()!;
        Config.WasdMenu.ArrowColor = wasdTable["ArrowColor"].ToString()!;
        Config.WasdMenu.FreezePlayer = bool.Parse(wasdTable["FreezePlayer"]?.ToString() ?? "false");

        TomlTable screenTable = (TomlTable)model["ScreenMenu"];
        Config.ScreenMenu.TextColor = screenTable["TextColor"].ToString()!;
        Config.ScreenMenu.PositionX = float.Parse(screenTable["PositionX"].ToString()!);
        Config.ScreenMenu.PositionY = float.Parse(screenTable["PositionY"].ToString()!);
        Config.ScreenMenu.Background = bool.Parse(screenTable["Background"].ToString()!);
        Config.ScreenMenu.BackgroundHeight = float.Parse(screenTable["BackgroundHeight"].ToString()!);
        Config.ScreenMenu.BackgroundWidth = float.Parse(screenTable["BackgroundWidth"].ToString()!);
        Config.ScreenMenu.Font = screenTable["Font"].ToString()!;
        Config.ScreenMenu.Size = int.Parse(screenTable["Size"].ToString()!);
        Config.ScreenMenu.FreezePlayer = bool.Parse(screenTable["FreezePlayer"].ToString()!);
        Config.ScreenMenu.ShowResolutionsOption = bool.Parse(screenTable["ShowResolutionsOption"].ToString()!);

        TomlTable resolutionsTable = (TomlTable)model["Resolutions"];
        foreach (KeyValuePair<string, object> resolution in resolutionsTable)
        {
            if (resolution.Value is TomlTable innerTable)
            {
                Config.Resolutions[resolution.Key] = new Resolution()
                {
                    PositionX = float.Parse(innerTable["PositionX"].ToString()!),
                    PositionY = float.Parse(innerTable["PositionY"].ToString()!)
                };
            }
        }

        TomlTable langTable = (TomlTable)model["Lang"];
        foreach (KeyValuePair<string, object> lang in langTable)
        {
            if (lang.Value is TomlTable innerTable)
            {
                Dictionary<string, string> innerDict = Config.Lang.GetValueOrDefault(lang.Key) ?? (Config.Lang[lang.Key] = []);

                foreach (KeyValuePair<string, object> item in innerTable)
                {
                    innerDict[item.Key] = item.Value?.ToString() ?? string.Empty;
                }
            }
        }
    }
}