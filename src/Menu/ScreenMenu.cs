using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using System.Text;
using static CounterStrikeSharp.API.Core.Listeners;
using static CS2MenuManager.ConfigManager;
using static CS2MenuManager.CS2MenuManager;
using static CS2MenuManager.Library;

namespace CS2MenuManager;

public class ScreenMenu(string title) : BaseMenu(title)
{
    public override void Display(CCSPlayerController player, int time = 0)
    {
        MenuTime = time;
        MenuManager.OpenMenu(player, this, (p, m) => new ScreenMenuInstance(p, m));
    }
}
public class ScreenMenuInstance : BaseMenuInstance
{
    public int CurrentChoiceIndex = 0;
    public override int NumPerPage => 5;
    public PlayerButtons OldButton;
    public CPointWorldText? WorldText;

    public ScreenMenuInstance(CCSPlayerController player, IMenu menu) : base(player, menu)
    {
        var firstEnabledOption = Menu.ItemOptions
            .Select((option, index) => new { Option = option, Index = index })
            .FirstOrDefault(x => x.Option.DisableOption == DisableOption.None);

        CurrentChoiceIndex = firstEnabledOption != null ? firstEnabledOption.Index : throw new ArgumentException("No non-disabled menu option found.");

        Plugin.RegisterListener<OnTick>(OnTick);
        Plugin.RegisterListener<CheckTransmit>(OnCheckTransmit);
        Plugin.RegisterListener<OnEntityDeleted>(OnEntityDeleted);

        if (Config.ScreenMenu.FreezePlayer)
            player.Freeze();
    }

    public override void Display()
    {
        StringBuilder builder = new();
        builder.AppendLine(" ");
        builder.AppendLine(Menu.Title);
        builder.AppendLine(" ");

        int keyOffset = 1;
        int maxIndex = Math.Min(CurrentOffset + MenuItemsPerPage, Menu.ItemOptions.Count);
        for (int i = CurrentOffset; i < maxIndex; i++)
        {
            ItemOption option = Menu.ItemOptions[i];
            builder.AppendLine(i == CurrentChoiceIndex ? $"> {keyOffset++}. {option.Text}" : $"  {keyOffset++}. {option.Text}");
        }

        if (Menu.ItemOptions.Count > MenuItemsPerPage)
            builder.AppendLine(" ");

        builder.AppendLine(" ");
        builder.AppendLine(Player.Localizer("Scroll", Config.Buttons.ScrollUp, Config.Buttons.ScrollDown));
        builder.AppendLine(Player.Localizer("Select", Config.Buttons.Select));
        builder.AppendLine(" ");

        if (WorldText == null || !WorldText.IsValid)
        {
            WorldText = CreateWorldText(builder.ToString());

            if (WorldText == null || !WorldText.IsValid)
            {
                Close();
            }
        }
        else
        {
            WorldText.MessageText = builder.ToString();
            Utilities.SetStateChanged(WorldText, "CPointWorldText", "m_messageText");
        }
    }

    public override void Close()
    {
        base.Close();
        Plugin.RemoveListener<OnTick>(OnTick);
        Plugin.RemoveListener<CheckTransmit>(OnCheckTransmit);
        Plugin.RemoveListener<OnEntityDeleted>(OnEntityDeleted);

        if (WorldText != null && WorldText.IsValid)
            WorldText.Remove();

        if (Config.ScreenMenu.FreezePlayer)
            Player.Unfreeze();

        if (!string.IsNullOrEmpty(Config.Sound.Exit))
            Player.ExecuteClientCommand($"play {Config.Sound.Exit}");
    }

    public void OnTick()
    {
        PlayerButtons button = Player.Buttons;
        Dictionary<string, Action> mapping = new()
        {
            { Config.Buttons.ScrollUp, ScrollUp },
            { Config.Buttons.ScrollDown, ScrollDown },
            { Config.Buttons.Select, Choose }
        };

        foreach (KeyValuePair<string, Action> kvp in mapping)
        {
            if (Buttons.ButtonMapping.TryGetValue(kvp.Key, out PlayerButtons buttonMappingButton))
            {
                if ((button & buttonMappingButton) == 0 && (OldButton & buttonMappingButton) != 0)
                {
                    kvp.Value.Invoke();
                    break;
                }
            }
        }

        if ((button & PlayerButtons.Moveleft) == 0 && (OldButton & PlayerButtons.Moveleft) != 0)
        {
            if (PrevMenu == null) Close();
            else PrevSubMenu();
        }

        if (((long)button & 8589934592) == 8589934592)
        {
            Close();
            return;
        }

        OldButton = button;

        if (WorldText != null)
        {
            VectorData? vectorData = Player.FindVectorData();
            if (vectorData == null)
            {
                Close();
                return;
            }

            CCSGOViewModel? viewModel = Player.EnsureCustomView();
            if (viewModel == null)
            {
                Close();
                return;
            }

            WorldText.Teleport(vectorData.Position, vectorData.Angle, null);
            WorldText.AcceptInput("SetParent", viewModel, null, "!activator");
        }
    }

    public void Choose()
    {
        if (CurrentChoiceIndex < 0 || CurrentChoiceIndex >= Menu.ItemOptions.Count) return;

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

    public void ScrollDown()
    {
        int startIndex = CurrentChoiceIndex;
        if (startIndex == Menu.ItemOptions.Count - 1) return;

        do
        {
            CurrentChoiceIndex = (CurrentChoiceIndex + 1) % Menu.ItemOptions.Count;
            if (CurrentChoiceIndex == startIndex) return;
        } while (Menu.ItemOptions[CurrentChoiceIndex].DisableOption != DisableOption.None);

        if (CurrentChoiceIndex > CurrentOffset + MenuItemsPerPage - 1)
            NextPage();
        else
            Display();

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
            if (CurrentChoiceIndex == startIndex) return;
        } while (Menu.ItemOptions[CurrentChoiceIndex].DisableOption != DisableOption.None);

        if (CurrentChoiceIndex < CurrentOffset)
            PrevPage();
        else
            Display();

        if (!string.IsNullOrEmpty(Config.Sound.ScrollUp))
            Player.ExecuteClientCommand($"play {Config.Sound.ScrollUp}");
    }

    public void OnCheckTransmit(CCheckTransmitInfoList infoList)
    {
        if (WorldText == null) return;
        foreach ((CCheckTransmitInfo info, CCSPlayerController? player) in infoList)
        {
            if (player == null || Player == player) continue;
            info.TransmitAlways.Remove(WorldText);
        }
    }

    public void OnEntityDeleted(CEntityInstance entity)
    {
        if (WorldText != null && WorldText.IsValid && WorldText == entity)
            Close();
    }
}