using CounterStrikeSharp.API.Core;

namespace CS2MenuManager;

public class ConsoleMenu(string title) : BaseMenu(title)
{
    public override void Display(CCSPlayerController player, int time = 0)
    {
        MenuTime = time;
        MenuManager.OpenMenu(player, this, (p, m) => new ConsoleMenuInstance(p, m));
    }
}
public class ConsoleMenuInstance(CCSPlayerController player, IMenu menu) : BaseMenuInstance(player, menu)
{
    public override void Display()
    {
        Player.PrintToConsole(Menu.Title);
        Player.PrintToConsole("---");

        int keyOffset = 1;
        int maxIndex = Math.Min(CurrentOffset + MenuItemsPerPage, Menu.ItemOptions.Count);
        for (int i = CurrentOffset; i < maxIndex; i++)
        {
            ItemOption option = Menu.ItemOptions[i];

            switch (option.DisableOption)
            {
                case DisableOption.None:
                    Player.PrintToConsole($"css_{keyOffset++} {option.Text}");
                    break;
                case DisableOption.DisableShowNumber:
                    Player.PrintToConsole($"css_{keyOffset++} {option.Text} [{Player.Localizer("Disabled")}]");
                    break;
                case DisableOption.DisableHideNumber:
                    keyOffset++;
                    Player.PrintToConsole(option.Text);
                    break;
            }
        }

        if (HasPrevButton)
        {
            Player.PrintToConsole($"css_7 -> {Player.Localizer("Prev")}");
        }

        if (HasNextButton)
        {
            Player.PrintToConsole($"css_8 -> {Player.Localizer("Next")}");
        }

        if (Menu.ExitButton)
        {
            Player.PrintToConsole($"css_9 -> {Player.Localizer("Exit")}");
        }
    }
}