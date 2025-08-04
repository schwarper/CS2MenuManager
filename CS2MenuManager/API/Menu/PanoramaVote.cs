﻿using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.UserMessages;
using CounterStrikeSharp.API.Modules.Utils;
using CS2MenuManager.API.Class;
using CS2MenuManager.API.Enum;
using static CS2MenuManager.API.Class.BaseVoteInstance;
using static CS2MenuManager.API.Class.VoteManager;

namespace CS2MenuManager.API.Menu;

/// <summary>
/// Represents a panorama vote menu.
/// </summary>
public class PanoramaVote(string title, string details, YesNoVoteResult resultCallback, YesNoVoteHandler? handler, BasePlugin plugin)
    : BaseVote(title, details, resultCallback, handler, plugin)
{
    /// <summary>
    /// Displays the vote to all players.
    /// </summary>
    /// <param name="time">The duration of the vote in seconds.</param>
    public override void DisplayVoteToAll(int time)
    {
        VoteTime = time;
        List<CCSPlayerController> players = [.. Utilities.GetPlayers().Where(player => !player.IsBot)];
        OpenVoteMenu(players, this, (p, m) => new PanoramaVoteInstance(p, m));
    }
}

/// <summary>
/// Represents an instance of a Panorama vote.
/// </summary>
public class PanoramaVoteInstance : BaseVoteInstance
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PanoramaVoteInstance"/> class.
    /// </summary>
    /// <param name="players">The list of players participating in the vote.</param>
    /// <param name="menu">The menu associated with the vote.</param>
    public PanoramaVoteInstance(List<CCSPlayerController> players, PanoramaVote menu) : base(players, menu)
    {
        if (IsVoteActive || players.Count == 0)
            return;

        VoteMenu.Plugin.RegisterEventHandler<EventVoteCast>(OnVoteCast);
    }

    /// <summary>
    /// Closes the menu.
    /// </summary>
    public override void Close()
    {
        base.Close();
        VoteMenu.Plugin.DeregisterEventHandler<EventVoteCast>(OnVoteCast);
    }

    /// <summary>
    /// Displays the vote menu.
    /// </summary>
    public override void Display()
    {
        ActiveVoteInstance = this;
        ActiveVoteEnded = false;

        ResetVoteController();
        VoteCount++;

        InitVoters();

        VoteController.PotentialVotes = VoterCount;
        VoteController.ActiveIssueIndex = 2;

        UpdateVoteCounts();
        SendVoteStartUM(CurrentVotefilter);

        VoteMenu.Handler?.Invoke(YesNoVoteAction.VoteAction_Start, 0, 0);

        Timer = VoteMenu.Plugin.AddTimer(VoteMenu.VoteTime, () => EndVote(YesNoVoteEndReason.VoteEnd_TimeUp));
    }

    private HookResult OnVoteCast(EventVoteCast @event, GameEventInfo info)
    {
        if (@event.Userid is not { } player)
            return HookResult.Continue;

        VoteMenu.Handler?.Invoke(YesNoVoteAction.VoteAction_Vote, player.Slot, (CastVote)@event.VoteOption);
        UpdateVoteCounts();
        CheckForEarlyVoteClose();
        return HookResult.Continue;
    }

    private void ResetVoteController()
    {
        for (int i = 0; i < VoteController.VotesCast.Length; i++)
            VoteController.VotesCast[i] = (int)CastVote.VOTE_UNCAST;

        for (int i = 0; i < VoteController.VoteOptionCount.Length; i++)
            VoteController.VoteOptionCount[i] = 0;
    }

    private void InitVoters()
    {
        VoterCount = CurrentVotefilter.Count;
        for (int i = 0, j = 0; i < VoterCount; i++)
        {
            if (CurrentVotefilter[i].Slot != -1)
                Voters[j++] = CurrentVotefilter[i].Slot;
        }
    }

    private void UpdateVoteCounts()
    {
        new EventVoteChanged(true)
        {
            VoteOption1 = VoteController.VoteOptionCount[0],
            VoteOption2 = VoteController.VoteOptionCount[1],
            VoteOption3 = VoteController.VoteOptionCount[2],
            VoteOption4 = VoteController.VoteOptionCount[3],
            VoteOption5 = VoteController.VoteOptionCount[4],
            Potentialvotes = VoterCount
        }.FireEvent(false);
    }

    internal void Revote(CCSPlayerController? player)
    {
        if (player == null || !CurrentVotefilter.Contains(player))
            return;

        int vote = VoteController.VotesCast[player.Slot];

        if (vote != (int)CastVote.VOTE_UNCAST)
        {
            VoteController.VoteOptionCount[vote]--;
            VoteController.VotesCast[player.Slot] = (int)CastVote.VOTE_UNCAST;
            UpdateVoteCounts();
        }

        SendVoteStartUM(new RecipientFilter(player));
    }

    internal void EndVote(YesNoVoteEndReason reason)
    {
        if (ActiveVoteEnded)
            return;

        VoteMenu.Handler?.Invoke(YesNoVoteAction.VoteAction_End, (int)reason, 0);

        if (reason == YesNoVoteEndReason.VoteEnd_Cancelled)
        {
            SendVoteFailed(reason);
            VoteController.ActiveIssueIndex = -1;
            ActiveVoteEnded = true;
            Close();
            return;
        }

        YesNoVoteInfo info = new()
        {
            TotalClients = VoterCount,
            YesVotes = VoteController.VoteOptionCount[(int)CastVote.VOTE_OPTION1],
            NoVotes = VoteController.VoteOptionCount[(int)CastVote.VOTE_OPTION2],
            TotalVotes = VoteController.VoteOptionCount[(int)CastVote.VOTE_OPTION1] + VoteController.VoteOptionCount[(int)CastVote.VOTE_OPTION2]
        };

        for (int i = 0; i < CurrentVotefilter.Count; i++)
            info.ClientInfo[i] = i < VoterCount ? (Voters[i], VoteController.VotesCast[Voters[i]]) : (-1, -1);

        if (VoteMenu.Result(info))
            SendVotePassed("#SFUI_vote_passed_panorama_vote", "Vote Passed!");
        else
            SendVoteFailed(reason);

        ActiveVoteEnded = true;
        Close();
    }

    private void CheckForEarlyVoteClose()
    {
        int votes = VoteController.VoteOptionCount[(int)CastVote.VOTE_OPTION1] + VoteController.VoteOptionCount[(int)CastVote.VOTE_OPTION2];
        if (votes >= VoterCount)
            Server.NextFrame(() => EndVote(YesNoVoteEndReason.VoteEnd_AllVotes));
    }

    private void SendVoteStartUM(RecipientFilter recipientFilter)
    {
        UserMessage um = UserMessage.FromPartialName("VoteStart");

        um.SetInt("team", -1);
        um.SetInt("player_slot", VoteMenu.VoteCaller?.Slot ?? 99);
        um.SetInt("vote_type", -1);
        um.SetString("disp_str", VoteMenu.Title);
        um.SetString("details_str", VoteMenu.Details);
        um.SetBool("is_yes_no_vote", true);

        um.Send(recipientFilter);
    }

    private void SendVoteFailed(YesNoVoteEndReason reason)
    {
        UserMessage um = UserMessage.FromId(348);

        um.SetInt("team", -1);
        um.SetInt("reason", (int)reason);

        um.Send(CurrentVotefilter);
    }

    private void SendVotePassed(string disp_str = "#SFUI_Vote_None", string details_str = "")
    {
        UserMessage um = UserMessage.FromId(347);

        um.SetInt("team", -1);
        um.SetInt("vote_type", 2);
        um.SetString("disp_str", disp_str);
        um.SetString("details_str", details_str);

        um.Send(CurrentVotefilter);
    }
}