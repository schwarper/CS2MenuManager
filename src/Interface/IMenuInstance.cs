using CounterStrikeSharp.API.Core;

namespace CS2MenuManager;

public interface IMenuInstance
{
    CCSPlayerController Player { get; }
    int Page { get; }
    int CurrentOffset { get; }
    int NumPerPage { get; }
    Stack<int> PrevPageOffsets { get; }
    IMenu Menu { get; }
    int MenuTime { get; }
    IMenu? PrevMenu { get; }
    int PrevMenuTime { get; }

    void NextPage();
    void PrevPage();
    void Reset();
    void Close();
    void Display();
    void OnKeyPress(CCSPlayerController player, int key);
}