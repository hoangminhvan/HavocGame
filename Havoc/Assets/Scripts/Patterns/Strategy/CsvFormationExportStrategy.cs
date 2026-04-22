using System.Collections.Generic;
using UnityEngine;

// PATTERN: Concrete Strategy
// Trien khai cach lay du lieu tu file CSV bang cach phan tich tung dong van ban
public class CsvFormationExportStrategy : IAIFormationExportStrategy
{
    public List<PlacedUnitInfo> ExportRandomFormation()
    {
        // Tai tat ca cac file trong thu muc CSV
        TextAsset[] allFiles = Resources.LoadAll<TextAsset>("AIFormations/CSV");

        if (allFiles == null || allFiles.Length == 0)
        {
            return new List<PlacedUnitInfo>();
        }

        int randomIndex = Random.Range(0, allFiles.Length);
        TextAsset selectedFile = allFiles[randomIndex];

        List<PlacedUnitInfo> units = new List<PlacedUnitInfo>();
        // Cat van ban thanh cac dong rieng biet
        string[] lines = selectedFile.text.Split('\n');

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            // Tach du lieu moi cot dua tren dau phay.
            string[] columns = line.Split(',');

            if (columns.Length >= 6)
            {
                PlacedUnitInfo info = new PlacedUnitInfo();
                info.playerOwner = int.Parse(columns[0]);
                info.unitID = columns[1];
                info.coords = new Vector2Int(int.Parse(columns[2]), int.Parse(columns[3]));
                info.currentHP = int.Parse(columns[4]);
                info.currentMana = int.Parse(columns[5]);

                units.Add(info);
            }
        }

        return units;
    }
}