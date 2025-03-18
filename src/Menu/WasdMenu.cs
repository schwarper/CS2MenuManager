using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using System.Text;
using static CounterStrikeSharp.API.Core.Listeners;
using static CS2MenuManager.Buttons;
using static CS2MenuManager.ConfigManager;
using static CS2MenuManager.CS2MenuManager;

namespace CS2MenuManager;

public class WasdMenu(string title) : BaseMenu(title)
{
    public string TitleColor { get; set; } = "green";
    public string ScrollUpDownColor { get; set; } = "cyan";
    public string ExitColor { get; set; } = "purple";
    public string SelectedOptionColor { get; set; } = "orange";
    public string OptionColor { get; set; } = "white";
    public string DisabledOptionColor { get; set; } = "grey";
    public bool FreezePlayer { get; set; } = Config.WasdMenu.FreezePlayer;

    public override void Display(CCSPlayerController player, int time = 0)
    {
        MenuTime = time;
        MenuManager.OpenMenu(player, this, (p, m) => new WasdMenuInstance(p, m));
    }
}
public class WasdMenuInstance : BaseMenuInstance
{
    public int CurrentChoiceIndex;
    public override int NumPerPage => 5;
    public string DisplayString = "";
    public PlayerButtons OldButton;

    public WasdMenuInstance(CCSPlayerController player, IMenu menu) : base(player, menu)
    {
        var firstEnabledOption = Menu.ItemOptions
            .Select((option, index) => new { Option = option, Index = index })
            .FirstOrDefault(x => x.Option.DisableOption == DisableOption.None);

        CurrentChoiceIndex = firstEnabledOption != null ? firstEnabledOption.Index : throw new ArgumentException("No non-disabled menu option found.");

        RemoveOnTickListener();
        Plugin.RegisterListener<OnTick>(OnTick);

        if (((WasdMenu)Menu).FreezePlayer)
            Player.Freeze();
    }

    public void OnTick()
    {
        PlayerButtons button = Player.Buttons;

        Dictionary<string, Action> Mapping = new()
        {
            { Config.Buttons.ScrollUp, ScrollUp },
            { Config.Buttons.ScrollDown, ScrollDown },
            { Config.Buttons.Select, Choose }
        };

        foreach (KeyValuePair<string, Action> kvp in Mapping)
        {
            if (ButtonMapping.TryGetValue(kvp.Key, out PlayerButtons buttonmappingButton))
            {
                if ((button & buttonmappingButton) == 0 && (OldButton & buttonmappingButton) != 0)
                {
                    kvp.Value.Invoke();
                    break;
                }
            }
        }

        if ((button & PlayerButtons.Moveleft) == 0 && (OldButton & PlayerButtons.Moveleft) != 0)
        {
            if (PrevMenu == null)
                Close();
            else
                PrevSubMenu();
        }

        if (((long)button & 8589934592) == 8589934592)
        {
            Close();
            return;
        }

        OldButton = button;

        if (DisplayString != "")
        {
            Player.PrintToCenterHtml(DisplayString);
        }
    }

    public override void Display()
    {
        if (Menu is not WasdMenu wasdMenu)
            return;

        const string MenuSelectionLeft = "<img src='https://raw.githubusercontent.com/oqyh/cs2-Kill-Sound-GoldKingZ/main/Resources/left.gif' class=''>";
        const string MenuSelectionRight = "<img src='https://raw.githubusercontent.com/oqyh/cs2-Kill-Sound-GoldKingZ/main/Resources/right.gif' class=''>";
        string Prefix = $"<font color='{wasdMenu.TitleColor}'>";
        const string OptionsBelow = "<img src='https://raw.githubusercontent.com/oqyh/cs2-Kill-Sound-GoldKingZ/main/Resources/arrow.gif' class=''> <img src='https://raw.githubusercontent.com/oqyh/cs2-Kill-Sound-GoldKingZ/main/Resources/arrow.gif' class=''> <img src='https://raw.githubusercontent.com/oqyh/cs2-Kill-Sound-GoldKingZ/main/Resources/arrow.gif' class=''>";

        StringBuilder builder = new();
        builder.Append($"           <font color='{wasdMenu.ScrollUpDownColor}'>")
               .Append(Player.Localizer("Scroll", Config.Buttons.ScrollUp, Config.Buttons.ScrollDown))
               .Append(" - ")
               .Append(Player.Localizer("Select", Config.Buttons.Select))
               .Append($" </font> <br><font color='{wasdMenu.ExitColor}'>[ <img src='https://raw.githubusercontent.com/oqyh/cs2-Kill-Sound-GoldKingZ/main/Resources/tab.gif' class=''> - ")
               .Append(Player.Localizer("Exit"))
               .Append(" ]");

        string buttomText = builder.ToString();

        builder.Clear();
        builder.AppendLine($"{Prefix}{Menu.Title}</u><br>");

        int keyOffset = 1;
        int maxIndex = Math.Min(CurrentOffset + MenuItemsPerPage, Menu.ItemOptions.Count);
        for (int i = CurrentOffset; i < maxIndex; i++)
        {
            ItemOption option = Menu.ItemOptions[i];
            if (i == CurrentChoiceIndex)
            {
                builder.AppendLine($"{MenuSelectionLeft} <font color='{wasdMenu.SelectedOptionColor}'>{keyOffset++}. {option.Text} {MenuSelectionRight}</font> <br>");
            }
            else
            {
                switch (option.DisableOption)
                {
                    case DisableOption.None:
                    case DisableOption.DisableShowNumber:
                        builder.AppendLine($"<font color='{wasdMenu.OptionColor}'>{keyOffset++}. {option.Text}</font> <br>");
                        break;
                    case DisableOption.DisableHideNumber:
                        keyOffset++;
                        builder.AppendLine($"<font color='{wasdMenu.DisabledOptionColor}'>{option.Text}</font> <br>");
                        break;
                }
            }
        }

        if (Menu.ItemOptions.Count > MenuItemsPerPage)
        {
            builder.AppendLine(OptionsBelow);
        }

        builder.AppendLine("<br>" + $"{buttomText}<br>");
        builder.AppendLine("</div>");

        DisplayString = builder.ToString();
    }

    public override void Close()
    {
        base.Close();
        RemoveOnTickListener();
        Player.PrintToCenterHtml(" ");

        if (((WasdMenu)Menu).FreezePlayer)
            Player.Unfreeze();

        if (!string.IsNullOrEmpty(Config.Sound.Exit))
            Player.ExecuteClientCommand($"play {Config.Sound.Exit}");
    }

    private void RemoveOnTickListener()
    {
        Plugin.RemoveListener<OnTick>(OnTick);
    }

    public void Choose()
    {
        if (CurrentChoiceIndex >= 0 && CurrentChoiceIndex < Menu.ItemOptions.Count)
        {
            ItemOption option = Menu.ItemOptions[CurrentChoiceIndex];
            option.OnSelect?.Invoke(Player, option);

            if (!string.IsNullOrEmpty(Config.Sound.Select))
                Player.ExecuteClientCommand($"play {Config.Sound.Select}");

            switch (option.PostSelectAction)
            {
                case PostSelectAction.Close:
                    Close();
                    break;
                case PostSelectAction.Reset:
                    Reset();
                    break;
                case PostSelectAction.Nothing:
                    break;
            }
        }
    }

    public void ScrollDown()
    {
        int startIndex = CurrentChoiceIndex;
        if (startIndex == Menu.ItemOptions.Count - 1) return;

        do
        {
            CurrentChoiceIndex = (CurrentChoiceIndex + 1) % Menu.ItemOptions.Count;

            if (CurrentChoiceIndex == startIndex)
                return;

        } while (Menu.ItemOptions[CurrentChoiceIndex].DisableOption != DisableOption.None);

        int lastVisibleIndex = CurrentOffset + MenuItemsPerPage - 1;
        if (CurrentChoiceIndex > lastVisibleIndex)
        {
            NextPage();
        }
        else
        {
            Display();
        }

        if (!string.IsNullOrEmpty(Config.Sound.ScrollDown))
            Player.ExecuteClientCommand($"play {Config.Sound.ScrollDown}");
    }

    public void ScrollUp()
    {
        int startIndex = CurrentChoiceIndex;
        if (startIndex == 1) return;

        do
        {
            CurrentChoiceIndex = (CurrentChoiceIndex - 1 + Menu.ItemOptions.Count) % Menu.ItemOptions.Count;

            if (CurrentChoiceIndex == startIndex)
                return;

        } while (Menu.ItemOptions[CurrentChoiceIndex].DisableOption != DisableOption.None);

        if (CurrentChoiceIndex < CurrentOffset)
        {
            PrevPage();
        }
        else
        {
            Display();
        }

        if (!string.IsNullOrEmpty(Config.Sound.ScrollUp))
            Player.ExecuteClientCommand($"play {Config.Sound.ScrollUp}");
    }
}