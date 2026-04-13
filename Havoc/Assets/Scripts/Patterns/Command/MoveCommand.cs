using UnityEngine;

// PATTERN: Concrete Command
// Xu ly hanh dong di chuyen cua Unit va ho tro hoan tac vi tri
public class MoveCommand : ICommand
{
    private BaseUnit unit;
    private Tile startTile;
    private Tile endTile;

    public MoveCommand(BaseUnit unit, Tile startTile, Tile endTile)
    {
        this.unit = unit;
        this.startTile = startTile;
        this.endTile = endTile;
    }

    public void Execute()
    {
        // 1. Thuc hien di chuyen unit toi o dich
        unit.Move(endTile);

        // 2. Tru nang luong (Energy) cua nguoi choi
        TurnHandler.Instance.SpendEnergy(BattleGameManager.MOVE_ENERGY);
        TurnHandler.Instance.UpdateTurnUI();
    }

    public void Undo()
    {
        // 1. Dua unit tro lai o bat dau
        unit.Move(startTile);

        // 2. Hoan lai nang luong da chi tra
        TurnHandler.Instance.currentEnergy += BattleGameManager.MOVE_ENERGY;
        TurnHandler.Instance.UpdateTurnUI();

        // Bo chon unit sau khi hoan tac de tranh loi trang thai
        BattleGameManager.Instance.DeselectActiveUnit();
    }
}