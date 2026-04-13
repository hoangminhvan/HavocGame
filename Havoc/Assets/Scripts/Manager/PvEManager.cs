using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class PvEManager : MonoBehaviour
{
    public void StartPvEMode()
    {
        if (GameData.Instance == null)
        {
            Debug.LogError("Loi: Khong tim thay GameData trong Scene hien tai!");
            return;
        }

        GameData.Instance.ClearData();

        AIFormationExporter exporter = new AIFormationExporter();
        exporter.SetStrategy(new JsonFormationExportStrategy());

        List<PlacedUnitInfo> aiFormation = exporter.ExecuteExport();

        if (aiFormation != null && aiFormation.Count > 0)
        {
            GameData.Instance.p2Units = aiFormation;
            GameData.Instance.p2AliveUnits = aiFormation.Count;
        }

        GameData.Instance.isPvEMode = true;

        SceneManager.LoadScene("PrepareState");
    }
}