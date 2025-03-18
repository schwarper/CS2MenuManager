using CounterStrikeSharp.API.Core;
using System.Text;
using static CounterStrikeSharp.API.Core.Listeners;
using static CS2MenuManager.CS2MenuManager;

namespace CS2MenuManager;

public class CenterHtmlMenu(string title) : BaseMenu(title)
{
    public string TitleColor { get; set; } = "yellow";
    public string EnabledColor { get; set; } = "green";
    public string DisabledColor { get; set; } = "grey";
    public string PrevPageColor { get; set; } = "yellow";
    public string NextPageColor { get; set; } = "yellow";
    public string CloseColor { get; set; } = "red";

    public override void Display(CCSPlayerController player, int time = 0)
    {
        MenuTime = time;
        MenuManager.OpenMenu(player, this, (p, m) => new CenterHtmlMenuInstance(p, m));
    }
}
public class CenterHtmlMenuInstance : BaseMenuInstance
{
    public override int NumPerPage => 5;
    protected override int MenuItemsPerPage => (Menu.ExitButton ? 0 : 1) + ((HasPrevButton && HasNextButton) ? NumPerPage - 1 : NumPerPage);

    public CenterHtmlMenuInstance(CCSPlayerController player, IMenu menu) : base(player, menu)
    {
        Plugin.RegisterListener<OnTick>(Display);
    }

    public override void Display()
    {
        if (Menu is not CenterHtmlMenu centerHtmlMenu) return;

        StringBuilder builder = new();
        builder.Append($"<b><font color='{centerHtmlMenu.TitleColor}'>{centerHtmlMenu.Title}</font></b>");
        builder.AppendLine("<br>");

        int keyOffset = 1;
        int maxIndex = Math.Min(CurrentOffset + MenuItemsPerPage, Menu.ItemOptions.Count);
        for (int i = CurrentOffset; i < maxIndex; i++)
        {
            ItemOption option = centerHtmlMenu.ItemOptions[i];
            string color = option.DisableOption == DisableOption.None ? centerHtmlMenu.EnabledColor : centerHtmlMenu.DisabledColor;

            switch (option.DisableOption)
            {
                case DisableOption.None:
                case DisableOption.DisableShowNumber:
                    builder.Append($"<font color='{color}'>!{keyOffset++}</font> {option.Text}");
                    break;
                case DisableOption.DisableHideNumber:
                    keyOffset++;
                    builder.Append($"<font color='{color}'>{option.Text}</font>");
                    break;
            }

            builder.AppendLine("<br>");
        }

        if (HasPrevButton)
        {
            builder.AppendFormat($"<font color='{centerHtmlMenu.PrevPageColor}'>!7</font> &#60;- {Player.Localizer("Prev")}");
            builder.AppendLine("<br>");
        }

        if (HasNextButton)
        {
            builder.AppendFormat($"<font color='{centerHtmlMenu.NextPageColor}'>!8</font> -> {Player.Localizer("Next")}");
            builder.AppendLine("<br>");
        }

        if (centerHtmlMenu.ExitButton)
        {
            builder.AppendFormat($"<font color='{centerHtmlMenu.CloseColor}'>!9</font> -> {Player.Localizer("Exit")}");
            builder.AppendLine("<br>");
        }

        Player.PrintToCenterHtml(builder.ToString());
    }

    public override void Close()
    {
        base.Close();
        Plugin.RemoveListener<OnTick>(Display);
        Player.PrintToCenterHtml(" ");
    }
}