using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Utils;
using System.Reflection;
using Tomlyn;
using Tomlyn.Model;
using static CS2MenuManager.API.Class.ResolutionManager;

namespace CS2MenuManager.API.Class;

internal static class ConfigManager
{
    public class Cfg
    {
        public ButtonsKey Buttons { get; set; } = new()
        {
            ScrollUp = "W",
            ScrollDown = "S",
            Select = "E",
            Prev = "Shift",
            Exit = "Tab"
        };
        public Sound Sound { get; set; } = new();
        public ChatMenuSettings ChatMenu { get; set; } = new()
        {
            TitleColor = ChatColors.Yellow,
            EnabledColor = ChatColors.Green,
            DisabledColor = ChatColors.Grey,
            PrevPageColor = ChatColors.Yellow,
            NextPageColor = ChatColors.Yellow,
            ExitColor = ChatColors.Red
        };
        public CenterHtmlMenuSettings CenterHtmlMenu { get; set; } = new()
        {
            TitleColor = "yellow",
            EnabledColor = "green",
            DisabledColor = "grey",
            PrevPageColor = "yellow",
            NextPageColor = "yellow",
            ExitColor = "red",
            InlinePageOptions = true,
            MaxTitleLength = 0,
            MaxOptionLength = 0
        };
        public WasdMenuSettings WasdMenu { get; set; } = new()
        {
            TitleColor = "green",
            ScrollUpDownKeyColor = "cyan",
            SelectKeyColor = "green",
            PrevKeyColor = "orange",
            ExitKeyColor = "red",
            SelectedOptionColor = "orange",
            OptionColor = "white",
            DisabledOptionColor = "grey",
            ArrowColor = "purple",
            FreezePlayer = false
        };
        public ScreenMenu ScreenMenu { get; set; } = new()
        {
            TextColor = "orange",
            PositionX = -5.5f,
            PositionY = 0.0f,
            Background = true,
            BackgroundHeight = 0,
            BackgroundWidth = 0.2f,
            Font = "Arial Bold",
            Size = 32,
            FreezePlayer = false,
            ShowResolutionsOption = true,
            MenuType = "Both"
        };
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

    public class ChatMenuSettings
    {
        public char TitleColor { get; set; }
        public char EnabledColor { get; set; }
        public char DisabledColor { get; set; }
        public char PrevPageColor { get; set; }
        public char NextPageColor { get; set; }
        public char ExitColor { get; set; }
    }

    public class CenterHtmlMenuSettings
    {
        public string TitleColor { get; set; } = string.Empty;
        public string EnabledColor { get; set; } = string.Empty;
        public string DisabledColor { get; set; } = string.Empty;
        public string PrevPageColor { get; set; } = string.Empty;
        public string NextPageColor { get; set; } = string.Empty;
        public string ExitColor { get; set; } = string.Empty;
        public bool InlinePageOptions { get; set; }
        public int MaxTitleLength { get; set; }
        public int MaxOptionLength { get; set; }
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
        public bool FreezePlayer { get; set; }
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
        public string MenuType { get; set; } = string.Empty;
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

        model.SetIfPresent("Buttons.ScrollUp", (string value) => Config.Buttons.ScrollUp = value);
        model.SetIfPresent("Buttons.ScrollDown", (string value) => Config.Buttons.ScrollDown = value);
        model.SetIfPresent("Buttons.Select", (string value) => Config.Buttons.Select = value);
        model.SetIfPresent("Buttons.Prev", (string value) => Config.Buttons.Prev = value);
        model.SetIfPresent("Buttons.Exit", (string value) => Config.Buttons.Exit = value);

        model.SetIfPresent("Sound.Select", (string value) => Config.Sound.Select = value);
        model.SetIfPresent("Sound.Exit", (string value) => Config.Sound.Exit = value);
        model.SetIfPresent("Sound.ScrollUp", (string value) => Config.Sound.ScrollUp = value);
        model.SetIfPresent("Sound.ScrollDown", (string value) => Config.Sound.ScrollDown = value);

        model.SetIfPresent("ChatMenu.TitleColor", (string value) => Config.ChatMenu.TitleColor = value.GetChatColor());
        model.SetIfPresent("ChatMenu.EnabledColor", (string value) => Config.ChatMenu.EnabledColor = value.GetChatColor());
        model.SetIfPresent("ChatMenu.DisabledColor", (string value) => Config.ChatMenu.DisabledColor = value.GetChatColor());
        model.SetIfPresent("ChatMenu.PrevPageColor", (string value) => Config.ChatMenu.PrevPageColor = value.GetChatColor());
        model.SetIfPresent("ChatMenu.NextPageColor", (string value) => Config.ChatMenu.NextPageColor = value.GetChatColor());
        model.SetIfPresent("ChatMenu.ExitColor", (string value) => Config.ChatMenu.ExitColor = value.GetChatColor());

        model.SetIfPresent("CenterHtmlMenu.TitleColor", (string value) => Config.CenterHtmlMenu.TitleColor = value);
        model.SetIfPresent("CenterHtmlMenu.EnabledColor", (string value) => Config.CenterHtmlMenu.EnabledColor = value);
        model.SetIfPresent("CenterHtmlMenu.DisabledColor", (string value) => Config.CenterHtmlMenu.DisabledColor = value);
        model.SetIfPresent("CenterHtmlMenu.PrevPageColor", (string value) => Config.CenterHtmlMenu.PrevPageColor = value);
        model.SetIfPresent("CenterHtmlMenu.NextPageColor", (string value) => Config.CenterHtmlMenu.NextPageColor = value);
        model.SetIfPresent("CenterHtmlMenu.ExitColor", (string value) => Config.CenterHtmlMenu.ExitColor = value);
        model.SetIfPresent("CenterHtmlMenu.InlinePageOptions", (bool value) => Config.CenterHtmlMenu.InlinePageOptions = value);
        model.SetIfPresent("CenterHtmlMenu.MaxTitleLength", (int value) => Config.CenterHtmlMenu.MaxTitleLength = value);
        model.SetIfPresent("CenterHtmlMenu.MaxOptionLength", (int value) => Config.CenterHtmlMenu.MaxOptionLength = value);

        model.SetIfPresent("WasdMenu.TitleColor", (string value) => Config.WasdMenu.TitleColor = value);
        model.SetIfPresent("WasdMenu.ScrollUpDownKeyColor", (string value) => Config.WasdMenu.ScrollUpDownKeyColor = value);
        model.SetIfPresent("WasdMenu.SelectKeyColor", (string value) => Config.WasdMenu.SelectKeyColor = value);
        model.SetIfPresent("WasdMenu.PrevKeyColor", (string value) => Config.WasdMenu.PrevKeyColor = value);
        model.SetIfPresent("WasdMenu.ExitKeyColor", (string value) => Config.WasdMenu.ExitKeyColor = value);
        model.SetIfPresent("WasdMenu.SelectedOptionColor", (string value) => Config.WasdMenu.SelectedOptionColor = value);
        model.SetIfPresent("WasdMenu.OptionColor", (string value) => Config.WasdMenu.OptionColor = value);
        model.SetIfPresent("WasdMenu.DisabledOptionColor", (string value) => Config.WasdMenu.DisabledOptionColor = value);
        model.SetIfPresent("WasdMenu.ArrowColor", (string value) => Config.WasdMenu.ArrowColor = value);
        model.SetIfPresent("WasdMenu.FreezePlayer", (bool value) => Config.WasdMenu.FreezePlayer = value);

        model.SetIfPresent("ScreenMenu.TextColor", (string value) => Config.ScreenMenu.TextColor = value);
        model.SetIfPresent("ScreenMenu.PositionX", (float value) => Config.ScreenMenu.PositionX = value);
        model.SetIfPresent("ScreenMenu.PositionY", (float value) => Config.ScreenMenu.PositionY = value);
        model.SetIfPresent("ScreenMenu.Background", (bool value) => Config.ScreenMenu.Background = value);
        model.SetIfPresent("ScreenMenu.BackgroundHeight", (float value) => Config.ScreenMenu.BackgroundHeight = value);
        model.SetIfPresent("ScreenMenu.BackgroundWidth", (float value) => Config.ScreenMenu.BackgroundWidth = value);
        model.SetIfPresent("ScreenMenu.Font", (string value) => Config.ScreenMenu.Font = value);
        model.SetIfPresent("ScreenMenu.Size", (int value) => Config.ScreenMenu.Size = value);
        model.SetIfPresent("ScreenMenu.FreezePlayer", (bool value) => Config.ScreenMenu.FreezePlayer = value);
        model.SetIfPresent("ScreenMenu.ShowResolutionsOption", (bool value) => Config.ScreenMenu.ShowResolutionsOption = value);
        model.SetIfPresent("ScreenMenu.MenuType", (string value) => Config.ScreenMenu.MenuType = value);

        model.SetIfExist("Resolutions", (TomlTable table) =>
        {
            foreach (KeyValuePair<string, object> item in table)
            {
                if (item.Value is TomlTable innerTable)
                {
                    Config.Resolutions[item.Key] = new Resolution
                    {
                        PositionX = innerTable.GetValue<float>("PositionX"),
                        PositionY = innerTable.GetValue<float>("PositionY")
                    };
                }
            }
        });

        model.SetIfExist("Lang", (TomlTable table) =>
        {
            foreach (KeyValuePair<string, object> item in table)
            {
                if (item.Value is TomlTable innerTable)
                {
                    Dictionary<string, string> innerDict = [];
                    foreach (KeyValuePair<string, object> innerItem in innerTable)
                    {
                        innerDict[innerItem.Key] = innerItem.Value?.ToString() ?? string.Empty;
                    }
                    Config.Lang[item.Key] = innerDict;
                }
            }
        });
    }
}