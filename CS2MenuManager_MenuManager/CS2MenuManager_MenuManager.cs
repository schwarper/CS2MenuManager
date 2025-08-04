using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CS2MenuManager.API.Class;
using CS2MenuManager.API.Menu;

namespace CS2MenuManager_MenuManager;

public class Config : BasePluginConfig
{
    public string[] Commands { get; set; } = ["css_mm", "css_menumanager"];
    public string[] ChangeMenuTypeCommands { get; set; } = ["css_menutype", "css_ct"];
    public MenuManager_Config MenuManagerConfig { get; set; } = new();

    public class MenuManager_Config
    {
        public string Flag { get; set; } = "@css/root";
        public string[] ReloadConfigCommands { get; set; } = ["css_mm_reload_config"];
    }
}

public class CS2MenuManager_Menu : BasePlugin, IPluginConfig<Config>
{
    public override string ModuleName => "CS2MenuManager-MenuManager";
    public override string ModuleVersion => CS2MenuManager.ProjectInfo.Version;
    public override string ModuleAuthor => CS2MenuManager.ProjectInfo.Author;

    public Config Config { get; set; } = new();

    public void OnConfigParsed(Config config)
    {
        MenuManager.ReloadConfig();

        foreach (string command in config.Commands)
            AddCommand(command, "CS2MenuManager-MenuManager", Command_MenuManager);

        foreach (string command in config.ChangeMenuTypeCommands)
            AddCommand(command, "CS2MenuManager-ChangeMenuType-Menu", Command_ChangeMenuType);

        foreach (string command in config.MenuManagerConfig.ReloadConfigCommands)
            AddCommand(command, "CS2MenuManager-ReloadConfig", Command_ReloadConfig);

        Config = config;
    }

    public void Command_MenuManager(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null)
            return;

        PlayerMenu menu = new(Localizer.ForPlayer(player, "MenuManager Title"), this);

        menu.AddItem(Localizer.ForPlayer(player, "Change Menu Type"), (p, _) =>
        {
            MenuTypeManager.MenuTypeMenuByType(typeof(PlayerMenu), p, this, menu)
                .Display(player, 0);
        });

        menu.Display(player, 0);
    }

    public void Command_ChangeMenuType(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null)
            return;

        MenuTypeManager.MenuTypeMenuByType(typeof(PlayerMenu), player, this, null)
            .Display(player, 0);
    }

    public void Command_ReloadConfig(CCSPlayerController? player, CommandInfo info)
    {
        string flag = Config.MenuManagerConfig.Flag;
        if (string.IsNullOrWhiteSpace(flag) || AdminManager.PlayerHasPermissions(player, flag))
            return;

        MenuManager.ReloadConfig();
    }
}