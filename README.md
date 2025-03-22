CS2MenuManager is a flexible and user-friendly menu system developed for Counter-Strike 2 using the CounterStrikeSharp library. This project provides server administrators and developers with the ability to create customisable menus. It is easy to use for players and easy to configure and extend for administrators.

If you would like to donate or need assistance with the plugin, feel free to contact me via Discord, either privately or on my server.

Discord nickname: schwarper

Discord link : [Discord server](https://discord.gg/4zQfUzjk36)

# Nuget
[![NuGet Badge](https://img.shields.io/nuget/v/CS2MenuManager)](https://www.nuget.org/packages/CS2MenuManager)

# Installation 

1. Download:
* Download the latest release from [GitHub Releases](https://github.com/schwarper/CS2MenuManager/releases) or [Nuget](https://www.nuget.org/packages/CS2MenuManager).
2. Install the files:
* Extract the contents of the downloaded ZIP file to the `addons/counterstrikesharp/shared` folder.
3. Configure the settings:
* Adjust the settings in the `config.toml` file in the `addons/counterstrikesharp/shared/CS2MenuManager/` directory.
4. Restart server:
* Restart your server for the changes to take effect. You will need to use this API in your plugins.

# Usage

## Creating Menus
You can create any type of menu. All menu types have a similar structure. Here's an example of how to create a Chat Menu:

Supported menus: `ChatMenu`, `ConsoleMenu`, `CenterHtmlMenu`, `WasdMenu`, `ScreenMenu`, `PanoramaVote`
```csharp
ChatMenu menu = new("Title", this);

menu.AddItem("Option 1", (p, o) =>
{
    p.PrintToChat("You selected option 1");
});

menu.AddItem("Option 2X", DisableOption.DisableShowNumber);
menu.AddItem("Option 3X", DisableOption.DisableHideNumber);

menu.Display(player);
```

## Adding Submenus
You can add submenus to any menu. Here's how to link a submenu:
```csharp
menu.PrevMenu = AnySubMenu();

private static CenterHtmlMenu AnySubMenu()
{
    CenterHtmlMenu menu = new("Title", this);
    //...
    return menu;
}
```

## Post-Select Actions
You can set the behavior after selecting an option using PostSelectAction. The default is to close the menu after selection.
```csharp
menu.AddItem("Option After Reset", (p, o) =>
{
    o.PostSelectAction = PostSelectAction.Reset;
});
```

## Setting Menu Time
You can set the time for the menu. When the time is up, the menu is automatically closed.
```csharp
menu.Display(menu, 10);
// OR
ConsoleMenu menu = new("Console Menu", this)
{
    MenuTime = 20
};
```

## Panorama Vote Menu
The PanoramaVote menu allows you to create interactive vote menus using the Panorama UI. Here's an example:
```csharp
var menu = new PanoramaVote("#SFUI_vote_panorama_vote_default", "Hold on, Let me Cook", VoteResultCallback, VoteHandlerCallback, this)
{
    VoteCaller = player // null is the server.
};

menu.DisplayVoteToAll(20);
```
### And callbacks
```csharp
public bool VoteResultCallback(YesNoVoteInfo info)
{
    /*
    public int TotalVotes;
    public int YesVotes;
    public int NoVotes;
    public int TotalClients;
    public Dictionary<int, (int, int)> ClientInfo = [];
    */

    if (info.YesVotes > info.NoVotes)
    {
        Server.PrintToChatAll("Vote passed!");
        return true;
    }
    
    Server.PrintToChatAll("Vote failed!");
    return false;
}

public void VoteHandlerCallback(YesNoVoteAction action, int param1, CastVote param2)
{
    switch (action)
    {
        case YesNoVoteAction.VoteAction_Start:
            Server.PrintToChatAll("Vote started!");
            break;

        case YesNoVoteAction.VoteAction_Vote:
            var player = Utilities.GetPlayerFromSlot(param1);
            if (player == null) return;
            player.PrintToChat("You voted: " + (param2 == CastVote.VOTE_OPTION1 ? "Yes" : "No"));
            break;

        case YesNoVoteAction.VoteAction_End:
            switch ((YesNoVoteEndReason)param1)
            {
                case YesNoVoteEndReason.VoteEnd_Cancelled:
                    Server.PrintToChatAll("Vote Ended! Cancelled");
                    break;
                case YesNoVoteEndReason.VoteEnd_AllVotes:
                    Server.PrintToChatAll("Vote Ended! Thank you for participating.");
                    break;
                case YesNoVoteEndReason.VoteEnd_TimeUp:
                    Server.PrintToChatAll("Vote Ended! Time is up.");
                    break;
            }
            break;
    }
}
```

# References
## This project was prepared with the help of the following sources.
* `ChatMenu`,`ConsoleMenu`,`CenterHtmlMenu` => [CounterStrikeSharp](https://github.com/roflmuffin/CounterStrikeSharp) by [roflmuffin](https://github.com/roflmuffin)
* `WasdMenu` => [WasdMenuAPI](https://github.com/Interesting-exe/WASDMenuAPI) by [Interesting-exe](https://github.com/Interesting-exe)
* `ScreenMenu` => [CS2ScreenMenuAPI](https://github.com/T3Marius/CS2ScreenMenuAPI) by [T3Marius](https://github.com/T3Marius)
* `PanoramaVote` => [SLAYER_PanoramaVote](https://github.com/zakriamansoor47/SLAYER_PanoramaVote) by [zakriamansoor47](https://github.com/zakriamansoor47)

# Images
* Chat Menu

  ![image](https://github.com/user-attachments/assets/c14922aa-26cb-49d7-af50-c2e78f032c16)
* Console Menu

  ![image](https://github.com/user-attachments/assets/86d188a7-c7a3-4b8c-923e-c4fa2e81d9d1)
* CenterHtmlMenu

  ![image](https://github.com/user-attachments/assets/c0c0198a-01b1-46a1-8222-250b8ae1ec80)
* Wasd Menu

  ![image](https://github.com/user-attachments/assets/1fa862c9-dbc2-451a-9d32-fb2b6c4cc4df)
* Screen Menu

  ![image](https://github.com/user-attachments/assets/54bc171f-c5f1-4d2c-88e2-4951395c9dae)
