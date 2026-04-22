using System.Collections.Generic;

[System.Serializable]
public class MatchSaveData
{
    // Cac trang thai trong turn
    public int currentTurn;
    public int currentPlayerTurn;
    public int currentEnergy;
    public bool isPvE;
    // Dan tran cua 2 player
    public List<PlacedUnitInfo> p1Units = new List<PlacedUnitInfo>();
    public List<PlacedUnitInfo> p2Units = new List<PlacedUnitInfo>();
}