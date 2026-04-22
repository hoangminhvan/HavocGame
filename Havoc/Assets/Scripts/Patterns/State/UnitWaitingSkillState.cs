using UnityEngine;
using System.Collections.Generic;

// PATTERN: Concrete State
// Trang thai cho nguoi choi chon muc tieu de tung ky nang (Skill)
public class UnitWaitingSkillState : IUnitState
{
    public void Enter(BaseUnit unit)
    {
        BattleGameManager.Instance.ClearHighlights();

        // Skill co the co tam xa khac voi danh thuong
        List<Vector2Int> skillCoords = HexGridUtils.GetTilesInRange(
            unit.currentTile.GridCoords,
            unit.attackRange,
            BattleGameManager.Instance.allGridTiles
        );

        foreach (Vector2Int coords in skillCoords)
        {
            if (BattleGameManager.Instance.allGridTiles.TryGetValue(coords, out Tile t))
            {
                // Phan loai mau highlight: Do cho skill tan cong, Xanh duong cho skill ho tro
                if (unit.isOffensiveSkill) t.highlightAttack.SetActive(true);
                else t.highlightSupport.SetActive(true);

                BattleGameManager.Instance.highlightedTiles.Add(t);
            }
        }
    }

    public void OnTileClicked(BaseUnit unit, Tile clickedTile)
    {
        if (BattleGameManager.Instance.highlightedTiles.Contains(clickedTile))
        {
            BaseUnit targetUnit = clickedTile.IsOccupied ? clickedTile.OccupiedUnit.GetComponent<BaseUnit>() : null;

            if (!clickedTile.IsOccupied)
            {
                BattleUIManager.Instance.ShowWarning("Please select a Unit!");
                return; 
            }
            if (targetUnit != null && targetUnit.stealthTurns > 0)
            {
                BattleUIManager.Instance.ShowWarning("Cannot target Stealthed unit!");
                return;
            }

            // Thuc hien logic ky nang thong qua ActionHandler
            ActionHandler.Instance.ProcessSkill(unit, clickedTile, targetUnit);

            // Dung xong quay ve Idle
            unit.ChangeState(new UnitIdleState());
            return;
        }
        unit.ChangeState(new UnitIdleState());
    }

    public void Exit(BaseUnit unit) => BattleGameManager.Instance.ClearHighlights();
    public void Execute(BaseUnit unit) { }
    public void OnRightClick(BaseUnit unit) => unit.ChangeState(new UnitIdleState());
}