using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BattleGameManager : MonoBehaviour
{
    // Singleton instance to access BattleGameManager globally
    public static BattleGameManager Instance { get; private set; }

    public TextMeshProUGUI turnText;
    public TextMeshProUGUI energyText;
    public GameObject p1Indicator;
    public GameObject p2Indicator;

    // Energy cost constants for different actions
    public const int MOVE_ENERGY = 3;
    public const int ATTACK_ENERGY = 3;
    public const int SKILL_ENERGY = 4;
    public const int ITEM_ENERGY = 4;

    [Header("Camera Settings")]
    public Transform cameraTransform;
    public float originalCamX { get; private set; } 

    // Store all tiles in the grid by coordinates
    public Dictionary<Vector2Int, Tile> allGridTiles = new Dictionary<Vector2Int, Tile>();

    public List<Tile> highlightedTiles = new List<Tile>();
    // Currently selected unit
    public BaseUnit activeUnit;

    public ConsumableItem selectedItem;

    private void Awake()
    {
        // Initialize singleton instance
        Instance = this;

        if (cameraTransform != null)
        {
            originalCamX = cameraTransform.position.x;
        }
    }

    private void Start()
    {
        InitializeBattlefield();
        GameData.Instance.InitializeBattle();

        if (DataStorageContext.Repository.HasSavedMatch())
        {
            MatchSaveData savedData = DataStorageContext.Repository.LoadMatch();
            if (savedData != null)
            {
                TurnHandler.Instance.currentTurn = savedData.currentTurn;
                TurnHandler.Instance.currentPlayerTurn = savedData.currentPlayerTurn;
                TurnHandler.Instance.currentEnergy = savedData.currentEnergy;
                TurnHandler.Instance.UpdateTurnUI();

                if (savedData.isPvE && AIBattleManager.Instance != null)
                {
                    AIBattleManager.Instance.gameObject.SetActive(true);
                    if (savedData.currentPlayerTurn == 2)
                        AIBattleManager.Instance.OnTurnStarted(2);
                }
                return; 
            }
        }

        TurnHandler.Instance.StartNewTurn(1, 1);
    }

    public void ClearHighlights()
    {
        // Disable all highlighted tiles and clear the list
        foreach (Tile t in highlightedTiles) if (t != null) t.ClearAllHighlights();
        highlightedTiles.Clear();
    }

    public void DeselectActiveUnit()
    {
        // Deselect current unit and reset its state
        if (activeUnit != null)
        {
            activeUnit.SetSelected(false);

            // Return unit to idle state if needed
            if (!(activeUnit.currentState is UnitIdleState))
                activeUnit.ChangeState(new UnitIdleState());

            ClearHighlights();
            activeUnit = null;
        }

        // Hide UI panel after deselection
        BattleUIManager.Instance.HidePanel();
    }

    private void InitializeBattlefield()
    {
        // Find all tiles in the scene and store them
        Tile[] tiles = FindObjectsByType<Tile>(FindObjectsSortMode.None);
        allGridTiles.Clear();

        foreach (Tile t in tiles)
        {
            // Hide boundary visuals in battle phase
            if (t._boundaryVisual != null) t._boundaryVisual.SetActive(false);

            allGridTiles[t.GridCoords] = t;
        }

        // Spawn units from saved game data
        if (GameData.Instance != null)
        {
            SpawnUnitsFromData(GameData.Instance.p1Units);
            SpawnUnitsFromData(GameData.Instance.p2Units);
        }
    }

    private void SpawnUnitsFromData(List<PlacedUnitInfo> unitsData)
    {
        // Spawn units on tiles based on saved data
        foreach (PlacedUnitInfo info in unitsData)
        {
            if (allGridTiles.TryGetValue(info.coords, out Tile targetTile))
            {
                GameObject placedUnit = targetTile.ActivateHiddenUnit(info.unitID);

                if (placedUnit != null)
                {
                    BaseUnit unitScript = placedUnit.GetComponent<BaseUnit>();

                    if (unitScript != null)
                    {
                        // Assign unit ownership and tile reference
                        unitScript.ownerPlayer = info.playerOwner;
                        unitScript.currentTile = targetTile;
                        targetTile.OccupiedUnit = placedUnit;

                        if (info.currentHP > 0)
                        {
                            unitScript.currentHP = info.currentHP;
                            unitScript.currentMana = info.currentMana;

                            unitScript.NotifyHealthChanged();
                            unitScript.NotifyManaChanged();
                        }
                        // Apply team color/appearance
                        unitScript.SetupTeamAppearance();
                    }

                    // Rotate player 2 units to face opposite direction
                    if (info.playerOwner == 2)
                        placedUnit.transform.localRotation = Quaternion.Euler(0, 180, 0);
                }
            }
        }
    }

    public void PrepareItemUsage(ConsumableItem item)
    {
        // Prepare to use an item on a valid target
        ClearHighlights();
        selectedItem = item;

        foreach (var tile in allGridTiles.Values)
        {
            if (tile.IsOccupied)
            {
                BaseUnit unitOnTile = tile.OccupiedUnit.GetComponent<BaseUnit>();

                // Highlight only ally units of current player
                if (unitOnTile != null && unitOnTile.ownerPlayer == TurnHandler.Instance.currentPlayerTurn)
                {
                    tile.highlightMove.SetActive(true);
                    highlightedTiles.Add(tile);
                }
            }
        }

        // Show instruction to player
        BattleUIManager.Instance.ShowWarning("Select an ally to restore " + item.type.ToString());
    }
}