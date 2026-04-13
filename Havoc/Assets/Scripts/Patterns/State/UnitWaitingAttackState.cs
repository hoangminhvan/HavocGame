using UnityEngine;
using System.Collections.Generic;

// PATTERN: Concrete State
// Trang thai cho nguoi choi chon muc tieu de tan cong thuong
public class UnitWaitingAttackState : IUnitState
{
    public void Enter(BaseUnit unit)
    {
        BattleGameManager.Instance.ClearHighlights();

        // Tim cac o trong tam danh (Attack Range)
        List<Vector2Int> attackCoords = HexGridUtils.GetTilesInRange(
            unit.currentTile.GridCoords,
            unit.attackRange,
            BattleGameManager.Instance.allGridTiles
        );

        // Highlight mau do cho cac o trong tam danh
        foreach (Vector2Int coords in attackCoords)
        {
            if (BattleGameManager.Instance.allGridTiles.TryGetValue(coords, out Tile t))
            {
                t.highlightAttack.SetActive(true);
                BattleGameManager.Instance.highlightedTiles.Add(t);
            }
        }
    }

    public void OnTileClicked(BaseUnit unit, Tile clickedTile)
    {
        if (BattleGameManager.Instance.highlightedTiles.Contains(clickedTile))
        {
            if (clickedTile.IsOccupied)
            {
                BaseUnit target = clickedTile.OccupiedUnit.GetComponent<BaseUnit>();
                if (!clickedTile.IsOccupied)
                {
                    BattleUIManager.Instance.ShowWarning("Please select a Unit!");
                    return;
                }
                // Kiem tra Stealth (An than): Khong the tan cong ke dich dang an than
                if (target != null && target.stealthTurns > 0)
                {
                    BattleUIManager.Instance.ShowWarning("Cannot target Stealthed unit!");
                    return;
                }

                // Tan cong neu ke dich thuoc phe khac
                if (target != null && target.ownerPlayer != unit.ownerPlayer)
                {
                    // Quay mat ve phia ke dich
                    Vector3 dir = target.transform.position - unit.transform.position;
                    if (dir.x != 0) unit.transform.localRotation = Quaternion.Euler(0, dir.x < 0 ? 180 : 0, 0);

                    // Thuc hien tan cong
                    ActionHandler.Instance.ProcessAttack(unit, target);

                    // Cap nhat UI va ket thuc luot hanh dong cua unit nay
                    BattleUIManager.Instance.ShowUnitInfo(target);
                    unit.ChangeState(new UnitIdleState());
                    return;
                }
            }
        }
        unit.ChangeState(new UnitIdleState());
    }

    public void Exit(BaseUnit unit) => BattleGameManager.Instance.ClearHighlights();
    public void Execute(BaseUnit unit) { }
    public void OnRightClick(BaseUnit unit) => unit.ChangeState(new UnitIdleState());
}