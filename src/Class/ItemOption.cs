using CounterStrikeSharp.API.Core;

namespace CS2MenuManager;

public class ItemOption(string display, DisableOption option, Action<CCSPlayerController, ItemOption>? onSelect)
{
    public string Text { get; set; } = display;
    public DisableOption DisableOption { get; set; } = option;
    public Action<CCSPlayerController, ItemOption>? OnSelect { get; set; } = onSelect;
    public PostSelectAction PostSelectAction { get; set; } = PostSelectAction.Close;
}