using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DraftingGameManager : MonoBehaviour
{
    // Singleton instance for global access
    public static DraftingGameManager Instance { get; private set; }

    // Current player's turn (1 or 2)
    public int currentPlayerTurn = 1;

    // UI text for displaying gold of each player
    public TextMeshProUGUI p1GoldText;
    public TextMeshProUGUI p2GoldText;

    // Gold values for each player
    public int p1Gold = 20;
    public int p2Gold = 20;

    // Currently selected unit from shop
    public string selectedUnitID = "";
    public int selectedUnitCost = 0;

    private GameObject clickGhostObj;

    private void Awake()
    {
        // Initialize singleton instance
        Instance = this;
    }

    private void Start()
    {
        // Spawn existing AI units and update UI
        SpawnExistingUnits();
        UpdateGoldUI();
    }

    private void SpawnExistingUnits()
    {
        // Load and place pre-existing AI units onto the board
        if (GameData.Instance == null) return;

        Tile[] allTiles = FindObjectsByType<Tile>(FindObjectsSortMode.None);
        Dictionary<Vector2Int, Tile> tileDict = new Dictionary<Vector2Int, Tile>();

        foreach (Tile t in allTiles)
            tileDict[t.GridCoords] = t;

        // Spawn Player 2 (AI) units
        foreach (PlacedUnitInfo info in GameData.Instance.p2Units)
        {
            if (tileDict.TryGetValue(info.coords, out Tile t))
            {
                UnitFactory.Instance.CreateUnit(info.unitID, t, info.playerOwner);
            }
        }
    }

    private void Update()
    {
        if (clickGhostObj != null)
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0f;
            clickGhostObj.transform.position = mouseWorldPos;

            if (Input.GetMouseButtonDown(1))
            {
                CancelSelection();
            }
        }
    }

    public void UpdateGoldUI()
    {
        // Update gold display for the current player only
        if (p1GoldText != null)
        {
            p1GoldText.text = $"{p1Gold}";
            p1GoldText.gameObject.SetActive(currentPlayerTurn == 1);
        }

        if (p2GoldText != null)
        {
            p2GoldText.text = $"{p2Gold}";
            p2GoldText.gameObject.SetActive(currentPlayerTurn == 2);
        }
    }

    public void SelectUnitFromShop(string unitID, int cost, GameObject prefab)
    {
        CancelSelection();

        selectedUnitID = unitID;
        selectedUnitCost = cost;

        clickGhostObj = new GameObject("ClickGhost");
        SpriteRenderer ghostSr = clickGhostObj.AddComponent<SpriteRenderer>();

        if (prefab != null)
        {
            SpriteRenderer prefabSr = prefab.GetComponentInChildren<SpriteRenderer>();
            if (prefabSr != null)
            {
                ghostSr.sprite = prefabSr.sprite;
                clickGhostObj.transform.localScale = prefab.transform.localScale;
            }
        }

        ghostSr.color = new Color(1f, 1f, 1f, 0.7f);
        ghostSr.sortingOrder = 100;
    }

    public void CancelSelection()
    {
        // Clear selection and destroy ghost preview
        selectedUnitID = "";
        selectedUnitCost = 0;

        if (clickGhostObj != null)
            Destroy(clickGhostObj);
    }

    public bool TryPlaceUnit(Tile targetTile, string unitID, int cost)
    {
        // Validate tile placement conditions
        if (targetTile.IsOccupied) return false;
        if (targetTile.zone == TileZone.Boundary) return false;
        if (currentPlayerTurn == 1 && targetTile.zone != TileZone.Player1) return false;
        if (currentPlayerTurn == 2 && targetTile.zone != TileZone.Player2) return false;

        // Check if player has enough gold
        int currentGold = (currentPlayerTurn == 1) ? p1Gold : p2Gold;
        if (currentGold < cost) return false;

        // Create unit on tile
        BaseUnit createdUnit = UnitFactory.Instance.CreateUnit(unitID, targetTile, currentPlayerTurn);
        if (createdUnit == null) return false;

        // Deduct gold
        if (currentPlayerTurn == 1) p1Gold -= cost;
        else p2Gold -= cost;

        UpdateGoldUI();

        // Save unit data for later use
        PlacedUnitInfo info = new PlacedUnitInfo
        {
            playerOwner = currentPlayerTurn,
            unitID = unitID,
            coords = targetTile.GridCoords,
            currentHP = -1,
            currentMana = -1
        };

        if (currentPlayerTurn == 1)
            GameData.Instance.p1Units.Add(info);
        else
            GameData.Instance.p2Units.Add(info);

        if (selectedUnitID == unitID)
        {
            CancelSelection();
        }

        return true;
    }

    public void OnReloadButtonClicked()
    {
        // Reset current player's board and gold
        Tile[] allTiles = FindObjectsByType<Tile>(FindObjectsSortMode.None);

        if (currentPlayerTurn == 1)
        {
            foreach (Tile t in allTiles)
                if (t.zone == TileZone.Player1 && t.IsOccupied)
                    t.ClearUnit();

            p1Gold = 30;
            GameData.Instance.p1Units.Clear();
        }
        else
        {
            foreach (Tile t in allTiles)
                if (t.zone == TileZone.Player2 && t.IsOccupied)
                    t.ClearUnit();

            p2Gold = 30;
            GameData.Instance.p2Units.Clear();
        }

        CancelSelection();
        UpdateGoldUI();
    }

    public void UpdateMapVisibility(int activePlayer)
    {
        // Control visibility of tiles and units based on player perspective
        Tile[] allTiles = FindObjectsByType<Tile>(FindObjectsSortMode.None);
        bool isPvE = GameData.Instance != null && GameData.Instance.isPvEMode;

        foreach (Tile t in allTiles)
        {
            SpriteRenderer sr = t.GetComponentInChildren<SpriteRenderer>();
            if (sr == null) sr = t.GetComponent<SpriteRenderer>();

            bool isVisible = false;

            // Determine visibility rules
            if (t.zone == TileZone.Boundary) isVisible = true;
            else if (activePlayer == 1 && t.zone == TileZone.Player1) isVisible = true;
            else if (activePlayer == 2 && t.zone == TileZone.Player2) isVisible = true;

            if (isPvE && t.zone == TileZone.Player2) isVisible = true;

            if (sr != null)
            {
                Color c = sr.color;
                c.a = isVisible ? 1f : 0.2f;
                sr.color = c;
            }

            if (t.IsOccupied && t.OccupiedUnit != null)
            {
                t.OccupiedUnit.SetActive(isVisible);
            }
        }
    }

    public void ResetMapForBattle()
    {
        // Restore full visibility for battle phase
        Tile[] allTiles = FindObjectsByType<Tile>(FindObjectsSortMode.None);

        foreach (Tile t in allTiles)
        {
            SpriteRenderer sr = t.GetComponentInChildren<SpriteRenderer>();
            if (sr == null) sr = t.GetComponent<SpriteRenderer>();

            if (sr != null)
            {
                Color c = sr.color;
                c.a = 1f;

                if (t.zone == TileZone.Boundary)
                    c = Color.white;

                sr.color = c;
            }

            if (t.IsOccupied && t.OccupiedUnit != null)
            {
                t.OccupiedUnit.SetActive(true);
            }
        }
    }
}