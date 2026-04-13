using UnityEngine;

public class InputHandler : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            if (BattleGameManager.Instance.selectedItem != null)
            {
                BattleGameManager.Instance.selectedItem = null;
                BattleGameManager.Instance.ClearHighlights();
                BattleUIManager.Instance.ShowWarning("Item Cancelled");
            }
            else if (BattleGameManager.Instance.activeUnit != null)
            {
                BattleGameManager.Instance.activeUnit.currentState.OnRightClick(BattleGameManager.Instance.activeUnit);
            }
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) return;

            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);

            if (hit.collider != null)
            {
                Tile clickedTile = hit.collider.GetComponent<Tile>();
                if (clickedTile != null)
                {
                    if (clickedTile.IsOccupied)
                    {
                        BaseUnit target = clickedTile.OccupiedUnit.GetComponent<BaseUnit>();
                        if (target != null && target.ownerPlayer != TurnHandler.Instance.currentPlayerTurn && target.stealthTurns > 0)
                        {
                            if (BattleGameManager.Instance.activeUnit != null && !(BattleGameManager.Instance.activeUnit.currentState is UnitIdleState))
                            {
                                BattleUIManager.Instance.ShowWarning("Target is Stealthed!");
                                return;
                            }
                            BattleGameManager.Instance.DeselectActiveUnit();
                            return;
                        }
                    }

                    if (BattleGameManager.Instance.selectedItem != null)
                    {
                        bool success = false;
                        if (clickedTile.IsOccupied && BattleGameManager.Instance.highlightedTiles.Contains(clickedTile))
                        {
                            BaseUnit targetUnit = clickedTile.OccupiedUnit.GetComponent<BaseUnit>();
                            ActionHandler.Instance.ProcessItem(targetUnit, BattleGameManager.Instance.selectedItem);
                            success = true;
                        }
                        BattleGameManager.Instance.ClearHighlights();
                        BattleGameManager.Instance.selectedItem = null;
                        if (!success) BattleUIManager.Instance.ShowWarning("Cancelled");
                        return;
                    }

                    if (BattleGameManager.Instance.activeUnit != null && !(BattleGameManager.Instance.activeUnit.currentState is UnitIdleState))
                    {
                        BattleGameManager.Instance.activeUnit.currentState.OnTileClicked(BattleGameManager.Instance.activeUnit, clickedTile);
                    }
                    else
                    {
                        if (clickedTile.IsOccupied)
                        {
                            if (BattleGameManager.Instance.activeUnit != null) BattleGameManager.Instance.activeUnit.SetSelected(false);
                            BattleGameManager.Instance.activeUnit = clickedTile.OccupiedUnit.GetComponent<BaseUnit>();
                            BattleGameManager.Instance.activeUnit.SetSelected(true);
                            BattleUIManager.Instance.ShowUnitInfo(BattleGameManager.Instance.activeUnit);
                        }
                        else
                        {
                            BattleGameManager.Instance.DeselectActiveUnit();
                        }
                    }
                }
            }
            else
            {
                BattleGameManager.Instance.DeselectActiveUnit();
            }
        }
    }
}