using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlacedUnitInfo
{
    public int playerOwner;
    public string unitID;
    public Vector2Int coords;
    public int currentHP = -1;
    public int currentMana = -1;
}

public class GameData : MonoBehaviour
{
    public static GameData Instance { get; private set; }

    public List<PlacedUnitInfo> p1Units = new List<PlacedUnitInfo>();
    public List<PlacedUnitInfo> p2Units = new List<PlacedUnitInfo>();

    public int p1AliveUnits = 0;
    public int p2AliveUnits = 0;
    public bool isGameOver = false;
    public bool isPvEMode = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    public void InitializeBattle()
    {
        p1AliveUnits = p1Units.Count;
        p2AliveUnits = p2Units.Count;
        isGameOver = false;
    }

    public void UnitDied(int playerOwner)
    {
        if (playerOwner == 1) p1AliveUnits--;
        else if (playerOwner == 2) p2AliveUnits--;

        CheckWinCondition();
    }

    public void UnitRevived(int playerOwner)
    {
        if (playerOwner == 1) p1AliveUnits++;
        else if (playerOwner == 2) p2AliveUnits++;
    }

    private void CheckWinCondition()
    {
        if (isGameOver) return;

        if (p1AliveUnits <= 0) TriggerGameOver(2);
        else if (p2AliveUnits <= 0) TriggerGameOver(1);
    }

    private void TriggerGameOver(int winningPlayer)
    {
        isGameOver = true;
        Debug.Log($"Game Over! Player {winningPlayer} Wins!");

        if (BattleUIManager.Instance != null)
        {
            BattleUIManager.Instance.ShowWinPanel(winningPlayer);
        }
    }

    public void ClearData()
    {
        p1Units.Clear();
        p2Units.Clear();
        p1AliveUnits = 0;
        p2AliveUnits = 0;
        isGameOver = false;
        isPvEMode = false;
    }
}