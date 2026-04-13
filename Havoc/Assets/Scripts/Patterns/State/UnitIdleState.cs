using UnityEngine;

// PATTERN: Concrete State
// Trang thai cho: Nhan vat dung yen va khong thuc hien hanh dong nao
public class UnitIdleState : IUnitState
{
    public void Enter(BaseUnit unit)
    {
        // Chuyen doi animation sang trang thai nghi (Idle)
        Debug.Log("[ANIMATION] Kich hoat animation dung yen (Idle) cho " + unit.unitName);
    }

    public void Execute(BaseUnit unit) { }

    public void Exit(BaseUnit unit) { }

    // Trong trang thai Idle, click vao o gach thuong khong lam gi ca
    public void OnTileClicked(BaseUnit unit, Tile clickedTile) { }

    public void OnRightClick(BaseUnit unit) { }
}