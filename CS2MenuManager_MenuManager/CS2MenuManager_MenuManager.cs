using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Commands;
using CS2MenuManager.API.Class;
using CS2MenuManager.API.Menu;

namespace CS2MenuManager_MenuManager;

public class Config : BasePluginConfig
{
    public string[] Commands { get; set; } = ["css_mm", "css_menumanager"];
    public string[] ChangeResolutionCommands { get; set; } = ["css_resolution", "css_cr"];
    public string[] ChangeMenuTypeCommands { get; set; } = ["css_menutype", "css_ct"];
}

public class CS2MenuManager_Menu : BasePlugin, IPluginConfig<Config>
{
    public override string ModuleName => "CS2MenuManager-MenuManager";
    public override string ModuleVersion => CS2MenuManager.ProjectInfo.Version;
    public override string ModuleAuthor => CS2MenuManager.ProjectInfo.Author;

    public Config Config { get; set; } = new();

    public void OnConfigParsed(Config config)
    {
        foreach (string command in config.Commands)
            AddCommand(command, "CS2MenuManager-MenuManager", Command_MenuManager);

        foreach (string command in config.ChangeResolutionCommands)
            AddCommand(command, "CS2MenuManager-ChangeResolution-Menu", Command_ChangeResolution);

        foreach (string command in config.ChangeMenuTypeCommands)
            AddCommand(command, "CS2MenuManager-ChangeMenuType-Menu", Command_ChangeMenuType);

        Config = config;
    }

    public void Command_MenuManager(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null)
            return;

        PlayerMenu menu = new(Localizer.ForPlayer(player, "MenuManager Title"), this);

        menu.AddItem(Localizer.ForPlayer(player, "Change Resolution"), (p, o) =>
        {
            ResolutionManager.ResolutionMenuByType(typeof(PlayerMenu), player, this, menu)
                .Display(player, 0);
        });

        menu.AddItem(Localizer.ForPlayer(player, "Change Menu Type"), (p, o) =>
        {
            MenuTypeManager.MenuTypeMenuByType(typeof(PlayerMenu), player, this, menu)
                .Display(player, 0);
        });

        menu.Display(player, 0);
    }

    public void Command_ChangeResolution(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null)
            return;

        ResolutionManager.ResolutionMenuByType(typeof(PlayerMenu), player, this, null)
            .Display(player, 0);
    }

    public void Command_ChangeMenuType(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null)
            return;

        MenuTypeManager.MenuTypeMenuByType(typeof(PlayerMenu), player, this, null)
            .Display(player, 0);
    }

    [ConsoleCommand("css_testme")]
    public void OnTestMe(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null)
            return;

        PlayerMenu menu = new("Test Player Menu", this)
        {
            ScreenMenu_ShowResolutionsOption = true,
            WasdMenu_ExitKeyColor = "Pink",
            ScreenMenu_SelectKey = "R",
            ScreenMenu_ScrollUpKey = "D"
        };

        menu.AddItem("Test Item 1", (p, o) => { p.PrintToChat("Test Item 1 selected"); });
        menu.AddItem("Test Item 2", (p, o) => { p.PrintToChat("Test Item 2 selected"); });
        menu.AddItem("Test Item 3", (p, o) => { p.PrintToChat("Test Item 3 selected"); });
        menu.AddItem("Test Item 4", (p, o) => { p.PrintToChat("Test Item 4 selected"); });
        menu.AddItem("Test Item 5", (p, o) => { p.PrintToChat("Test Item 5 selected"); });

        menu.Display(player, 0);
    }
}