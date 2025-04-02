using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CS2MenuManager.API.Class;

namespace ResolutionMenu;

public class Config : BasePluginConfig
{
    public string MenuType { get; set; } = "ScreenMenu";
    public string[] Commands { get; set; } = ["css_resolution"];
}

public class ResolutionMenu : BasePlugin, IPluginConfig<Config>
{
    public override string ModuleName => "CS2MenuManager-Resolution Menu";
    public override string ModuleVersion => CS2MenuManager.ProjectInfo.Version;
    public override string ModuleAuthor => CS2MenuManager.ProjectInfo.Author;

    public Config Config { get; set; } = new();

    public void OnConfigParsed(Config config)
    {
        if (config.Commands.Length == 0)
            throw new ArgumentException("Commands array cannot be empty!");

        if (string.IsNullOrWhiteSpace(config.MenuType))
            throw new ArgumentException("MenuType cannot be empty!");

        foreach (string command in config.Commands)
            AddCommand(command, "ScreenMenu-ResolutionMenu", OnCommand);

        Config = config;
    }

    public void OnCommand(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null)
            return;

        ResolutionManager.ResolutionMenuByType(Config.MenuType, player, this, null)
            .Display(player, 0);
    }
}