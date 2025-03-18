using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace CS2MenuManager;

public class ChatMenu(string title) : BaseMenu(title)
{
    public char TitleColor { get; set; } = ChatColors.Yellow;
    public char EnabledColor { get; set; } = ChatColors.Green;
    public char DisabledColor { get; set; } = ChatColors.Grey;
    public char PrevPageColor { get; set; } = ChatColors.Yellow;
    public char NextPageColor { get; set; } = ChatColors.Yellow;
    public char CloseColor { get; set; } = ChatColors.Red;

    public override void Display(CCSPlayerController player, int time = 0)
    {
        MenuTime = time;
        MenuManager.OpenMenu(player, this, (p, m) => new ChatMenuInstance(p, m));
    }
}
public class ChatMenuInstance(CCSPlayerController player, ChatMenu menu) : BaseMenuInstance(player, menu)
{
    public override void Display()
    {
        if (Menu is not ChatMenu chatMenu)
            return;

        Player.PrintToChat($" {chatMenu.EnabledColor} {chatMenu.Title}");
        Player.PrintToChat("---");

        int keyOffset = 1;
        int maxIndex = Math.Min(CurrentOffset + MenuItemsPerPage, Menu.ItemOptions.Count);
        for (int i = CurrentOffset; i < maxIndex; i++)
        {
            ItemOption option = Menu.ItemOptions[i];
            char color = option.DisableOption == DisableOption.None ? chatMenu.EnabledColor : chatMenu.DisabledColor;

            switch (option.DisableOption)
            {
                case DisableOption.None:
                case DisableOption.DisableShowNumber:
                    Player.PrintToChat($" {color} !{keyOffset++} {ChatColors.Default}{option.Text}");
                    break;
                case DisableOption.DisableHideNumber:
                    keyOffset++;
                    Player.PrintToChat($" {color}{option.Text}");
                    break;
            }
        }

        if (HasPrevButton)
        {
            Player.PrintToChat($" {chatMenu.PrevPageColor}!7 {ChatColors.Default}-> {Player.Localizer("Prev")}");
        }

        if (HasNextButton)
        {
            Player.PrintToChat($" {chatMenu.NextPageColor}!8 {ChatColors.Default}-> {Player.Localizer("Next")}");
        }

        if (Menu.ExitButton)
        {
            Player.PrintToChat($" {chatMenu.CloseColor}!9 {ChatColors.Default}->  {Player.Localizer("Exit")}");
        }
    }
}