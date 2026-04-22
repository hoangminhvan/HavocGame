using UnityEngine;

// PATTERN: Concrete Repository
// Thuc thi viec luu tru du lieu duoi dang chuoi JSON vao PlayerPrefs
public class JsonMatchRepository : IMatchRepository
{
    private string saveKey = "MatchSaveDataKey";
    private string hasSaveKey = "HasSave";

    public void SaveMatch(MatchSaveData matchData)
    {
        // Chuyen doi doi tuong C# sang chuoi JSON
        string jsonString = JsonUtility.ToJson(matchData, true);
        PlayerPrefs.SetString(saveKey, jsonString);
        PlayerPrefs.SetInt(hasSaveKey, 1);
        PlayerPrefs.Save();
    }

    public MatchSaveData LoadMatch()
    {
        if (HasSavedMatch())
        {
            string jsonString = PlayerPrefs.GetString(saveKey);
            // Chuyen doi nguoc tu chuoi JSON sang doi tuong C#
            return JsonUtility.FromJson<MatchSaveData>(jsonString);
        }
        return null;
    }

    public bool HasSavedMatch()
    {
        return PlayerPrefs.GetInt(hasSaveKey, 0) == 1;
    }

    public void ClearSavedMatch()
    {
        PlayerPrefs.DeleteKey(saveKey);
        PlayerPrefs.SetInt(hasSaveKey, 0);
        PlayerPrefs.Save();
    }
}