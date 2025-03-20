using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CS2MenuManager.API.Enum;
using CS2MenuManager.API.Menu;

namespace Test;

public class Test : BasePlugin
{
    public override string ModuleName => "";
    public override string ModuleVersion => "0.0.1";

    [ConsoleCommand("css_m")]
    public void Testx(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null) return;

        player.PrintToChat("Menüü aç");
        ChatSubMenu(player).Display(player);
    }

    private ChatMenu ChatSubMenu(CCSPlayerController player)
    {
        ChatMenu menu = new("Chat Menu", this);
        menu.AddItem("Item 1", (p, o) => { p.PrintToChat("Chosen item 1"); });
        menu.AddItem("Item 2", DisableOption.DisableShowNumber);
        menu.AddItem("Item 3", DisableOption.DisableHideNumber);

        menu.PrevMenu = ConsoleSubMenu();
        return menu;
    }

    private ConsoleMenu ConsoleSubMenu()
    {
        ConsoleMenu menu = new("Console Menu", this)
        {
            MenuTime = 0
        };

        menu.AddItem("Item 1", (p, o) => { p.PrintToChat("Chosen item 1"); });
        menu.AddItem("Item 2", DisableOption.DisableShowNumber);
        menu.AddItem("Item 3", DisableOption.DisableHideNumber);

        menu.PrevMenu = CenterSubMenu();
        return menu;
    }

    private CenterHtmlMenu CenterSubMenu()
    {
        CenterHtmlMenu menu = new("CenterHtmlMenu Menu", this)
        {
            MenuTime = 0
        };

        menu.AddItem("Item 1", (p, o) => { p.PrintToChat("Chosen item 1"); });
        menu.AddItem("Item 2", (p, o) => { p.PrintToChat("Chosen item 2"); });
        menu.AddItem("Item 3", (p, o) => { p.PrintToChat("Chosen item 3"); });
        menu.AddItem("Item 4", (p, o) => { p.PrintToChat("Chosen item 4"); });
        menu.AddItem("Item 5X", DisableOption.DisableShowNumber);
        menu.AddItem("Item 6X", DisableOption.DisableHideNumber);
        menu.AddItem("Item 7X", DisableOption.DisableShowNumber);
        menu.AddItem("Item 8", (p, o) => { p.PrintToChat("Chosen item 8"); });

        menu.PrevMenu = WasdSubMenu();
        return menu;
    }

    private WasdMenu WasdSubMenu()
    {
        WasdMenu menu = new("WasdMenu Menu", this)
        {
            MenuTime = 0
        };

        menu.AddItem("Item 1", (p, o) => { p.PrintToChat("Chosen item 1"); });
        menu.AddItem("Item 2", (p, o) => { p.PrintToChat("Chosen item 2"); });
        menu.AddItem("Item 3", (p, o) => { p.PrintToChat("Chosen item 3"); });
        menu.AddItem("Item 4", (p, o) => { p.PrintToChat("Chosen item 4"); });
        menu.AddItem("Item 5X", DisableOption.DisableShowNumber);
        menu.AddItem("Item 6X", DisableOption.DisableHideNumber);
        menu.AddItem("Item 7X", DisableOption.DisableShowNumber);
        menu.AddItem("Item 8", (p, o) => { p.PrintToChat("Chosen item 8"); });

        menu.PrevMenu = ScreenSubMenu();
        return menu;
    }

    private ScreenMenu ScreenSubMenu()
    {
        ScreenMenu menu = new("ScreenMenu Menu", this)
        {
            MenuTime = 10
        };

        menu.AddItem("Item 1", (p, o) => { p.PrintToChat("Chosen item 1"); });
        menu.AddItem("Item 2", (p, o) => { p.PrintToChat("Chosen item 2"); });
        menu.AddItem("Item 3", (p, o) => { p.PrintToChat("Chosen item 3"); });
        menu.AddItem("Item 4", (p, o) => { p.PrintToChat("Chosen item 4"); });
        menu.AddItem("Item 5X", DisableOption.DisableShowNumber);
        menu.AddItem("Item 6X", DisableOption.DisableHideNumber);
        menu.AddItem("Item 7X", DisableOption.DisableShowNumber);
        menu.AddItem("Item 8", (p, o) => { p.PrintToChat("Chosen item 8"); });

        menu.PrevMenu = menu;
        return menu;
    }
}