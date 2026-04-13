using UnityEngine;
using System.Collections.Generic;

// PATTERN: Concrete State
// Trang thai cho nguoi choi chon o gach de di chuyen toi
public class UnitWaitingMoveState : IUnitState
{
    public void Enter(BaseUnit unit)
    {
        BattleGameManager.Instance.ClearHighlights();

        // Thuat toan tim cac o trong tam di chuyen cua nhan vat
        List<Vector2Int> reachableCoords = HexGridUtils.GetMovementRange(
            unit.currentTile.GridCoords,
            unit.moveRange,
            BattleGameManager.Instance.allGridTiles
        );

        // Hien thi mau xanh (highlight) cho cac o co the di den
        foreach (Vector2Int coords in reachableCoords)
        {
            if (BattleGameManager.Instance.allGridTiles.TryGetValue(coords, out Tile t))
            {
                if (!t.IsOccupied) // Chi highlight o dang trong
                {
                    t.highlightMove.SetActive(true);
                    BattleGameManager.Instance.highlightedTiles.Add(t);
                }
            }
        }
    }

    public void OnTileClicked(BaseUnit unit, Tile clickedTile)
    {
        // Neu click vao o hop le (da highlight) thi tien hanh di chuyen
        if (BattleGameManager.Instance.highlightedTiles.Contains(clickedTile) && !clickedTile.IsOccupied)
        {
            // Goi ActionHandler de xu ly logic di chuyen va tru nang luong
            ActionHandler.Instance.ProcessMove(unit, unit.currentTile, clickedTile);
            BattleGameManager.Instance.DeselectActiveUnit();
        }
        else
        {
            BattleUIManager.Instance.ShowWarning("Invalid Move!");
            BattleGameManager.Instance.DeselectActiveUnit();
        }

        // Sau khi thuc hien xong hoac huy, tu dong quay ve Idle thong qua Deselect
    }

    public void Exit(BaseUnit unit)
    {
        // Don dep cac o highlight sau khi thoat trang thai
        BattleGameManager.Instance.ClearHighlights();
    }

    public void Execute(BaseUnit unit) { }
    public void OnRightClick(BaseUnit unit) => unit.ChangeState(new UnitIdleState());
}