using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FormationWrapper
{
    public List<PlacedUnitInfo> units;
}
// PATTERN: Concrete Strategy
// Trien khai cach lay du lieu tu file JSON 
public class JsonFormationExportStrategy : IAIFormationExportStrategy
{
    public List<PlacedUnitInfo> ExportRandomFormation()
    {
        TextAsset[] allFiles = Resources.LoadAll<TextAsset>("AIFormations/JSON");

        if (allFiles == null || allFiles.Length == 0)
        {
            return new List<PlacedUnitInfo>();
        }

        // Chon ngau nhien mot file trong kho du lieu
        int randomIndex = Random.Range(0, allFiles.Length);
        TextAsset selectedFile = allFiles[randomIndex];

        // Chuyen doi chuoi JSON tu file thanh doi tuong C# 
        FormationWrapper wrapper = JsonUtility.FromJson<FormationWrapper>(selectedFile.text);

        if (wrapper != null && wrapper.units != null)
        {
            return wrapper.units;
        }

        return new List<PlacedUnitInfo>();
    }
}