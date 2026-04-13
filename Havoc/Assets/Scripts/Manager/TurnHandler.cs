using System.Collections.Generic;
using UnityEngine;

public class TurnHandler : MonoBehaviour
{
    public static TurnHandler Instance { get; private set; }

    public int currentTurn = 1;
    public int currentPlayerTurn = 1;
    public int currentEnergy;
    public int maxEnergyPerTurn = 10;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void StartNewTurn(int playerTurn, int turnNumber)
    {
        if (CommandManager.Instance != null) CommandManager.Instance.ClearHistory();
        if (playerTurn == 1) ProcessElementalTiles();

        BattleGameManager.Instance.selectedItem = null;
        currentPlayerTurn = playerTurn;
        currentTurn = turnNumber;
        currentEnergy = maxEnergyPerTurn;

        HandleTurnEffects();
        BattleGameManager.Instance.DeselectActiveUnit();
        BattleUIManager.Instance.ShowTurnNotify(playerTurn);

        if (BattleGameManager.Instance.cameraTransform != null)
        {
            float targetX = (playerTurn == 1) ? BattleGameManager.Instance.originalCamX - 10f : BattleGameManager.Instance.originalCamX + 10f;
            BattleGameManager.Instance.cameraTransform.position = new Vector3(targetX, BattleGameManager.Instance.cameraTransform.position.y, BattleGameManager.Instance.cameraTransform.position.z);
        }

        UpdateUnitsOnTurnStart(playerTurn);
        UpdateTurnUI();

        if (AIBattleManager.Instance != null)
        {
            AIBattleManager.Instance.OnTurnStarted(playerTurn);
        }
    }

    public void SpendEnergy(int amount)
    {
        currentEnergy -= amount;
        UpdateTurnUI();
        if (currentEnergy < 3) Invoke(nameof(EndTurn), 1.0f);
    }

    public void EndTurn()
    {
        if (currentPlayerTurn == 1) StartNewTurn(2, currentTurn);
        else StartNewTurn(1, currentTurn + 1);
    }

    public void UpdateTurnUI()
    {
        if (BattleGameManager.Instance.turnText != null) BattleGameManager.Instance.turnText.text = $"Turn {currentTurn}";
        if (BattleGameManager.Instance.energyText != null) BattleGameManager.Instance.energyText.text = $"{currentEnergy}";
        if (BattleGameManager.Instance.p1Indicator != null) BattleGameManager.Instance.p1Indicator.SetActive(currentPlayerTurn == 1);
        if (BattleGameManager.Instance.p2Indicator != null) BattleGameManager.Instance.p2Indicator.SetActive(currentPlayerTurn == 2);
    }

    private void HandleTurnEffects()
    {
        Dictionary<BaseUnit, int> pendingVampireHeals = new Dictionary<BaseUnit, int>();
        Dictionary<BaseUnit, BaseUnit> unitsToSpreadCurse = new Dictionary<BaseUnit, BaseUnit>();

        foreach (var tile in BattleGameManager.Instance.allGridTiles.Values)
        {
            if (tile.IsOccupied)
            {
                BaseUnit u = tile.OccupiedUnit.GetComponent<BaseUnit>();
                if (u == null) continue;

                if (u.ownerPlayer == currentPlayerTurn)
                {
                    if (tile.currentElementalType == ElementalType.Damage) u.TakeDamage(10);
                    u.RemoveShield();
                    u.RegenMana();
                    if (u.stunTurns > 0)
                    {
                        u.stunTurns--;
                        if (u.stunTurns <= 0) u.ClearStun();
                    }
                }

                if (u.curseTurns > 0 && u.curseCasterPlayer == currentPlayerTurn)
                {
                    if (currentTurn > u.curseAppliedTurn)
                    {
                        int curseDamage = Mathf.Max(1, Mathf.RoundToInt(u.maxHP * 0.15f));
                        u.TakeDamage(curseDamage);

                        if (u.curseCaster != null && u.curseCaster.currentHP > 0)
                        {
                            int healAmount = Mathf.Max(1, Mathf.RoundToInt(curseDamage * 0.7f));
                            if (pendingVampireHeals.ContainsKey(u.curseCaster)) pendingVampireHeals[u.curseCaster] += healAmount;
                            else pendingVampireHeals[u.curseCaster] = healAmount;
                        }

                        List<Vector2Int> neighbors = HexGridUtils.GetTilesInRange(u.currentTile.GridCoords, 2, BattleGameManager.Instance.allGridTiles);
                        foreach (Vector2Int n in neighbors)
                        {
                            if (BattleGameManager.Instance.allGridTiles.TryGetValue(n, out Tile nt) && nt.IsOccupied)
                            {
                                BaseUnit nu = nt.OccupiedUnit.GetComponent<BaseUnit>();
                                if (nu != null && nu != u && nu.ownerPlayer == u.ownerPlayer && nu.curseTurns <= 0)
                                    if (!unitsToSpreadCurse.ContainsKey(nu)) unitsToSpreadCurse.Add(nu, u.curseCaster);
                            }
                        }
                        u.curseTurns--;
                        if (u.curseTurns <= 0 || u.currentHP <= 0) u.ClearCurse();
                    }
                }
            }
        }
        foreach (var kvp in pendingVampireHeals) kvp.Key.Heal(kvp.Value);
        foreach (var kvp in unitsToSpreadCurse) if (kvp.Key.currentHP > 0) kvp.Key.ApplyCurse(3, kvp.Value);
    }

    private void UpdateUnitsOnTurnStart(int playerTurn)
    {
        BaseUnit[] allUnits = FindObjectsByType<BaseUnit>(FindObjectsSortMode.None);
        foreach (BaseUnit u in allUnits)
        {
            if (u.ownerPlayer == playerTurn && u.currentHP > 0)
            {
                u.RegenMana();
                u.RemoveShield();
                u.FaceClosestEnemy();
                if (u.stunTurns > 0) u.stunTurns--;
                if (u.stealthTurns > 0)
                {
                    u.stealthTurns--;
                    if (u.stealthTurns <= 0) u.ClearStealth();
                }
            }
        }
    }

    private void ProcessElementalTiles()
    {
        List<Tile> eligibleTiles = new List<Tile>();
        foreach (var tile in BattleGameManager.Instance.allGridTiles.Values)
        {
            if (tile.currentElementalType != ElementalType.None)
            {
                tile.elementalDuration--;
                if (tile.elementalDuration <= 0) tile.ClearElementalEffect();
            }
            if (!tile.IsOccupied && tile.currentElementalType == ElementalType.None) eligibleTiles.Add(tile);
        }
        int toAttempt = Mathf.Min(2, eligibleTiles.Count);
        for (int i = 0; i < toAttempt; i++)
        {
            int idx = Random.Range(0, eligibleTiles.Count);
            Tile st = eligibleTiles[idx];
            eligibleTiles.RemoveAt(idx);
            float roll = Random.value;
            if (roll <= 0.10f) st.SetElementalEffect(ElementalType.Heal, 1);
            else if (roll <= 0.20f) st.SetElementalEffect(ElementalType.Stealth, 1);
            else if (roll <= 0.40f) st.SetElementalEffect(ElementalType.Damage, 2);
        }
    }
}