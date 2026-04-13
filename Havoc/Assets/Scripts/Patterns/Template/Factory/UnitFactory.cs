using UnityEngine;
using System.Collections.Generic;

/// FACTORY PATTERN: This class acts as a "Kitchen" that creates Units (Warrior, Mage, ...) 
public class UnitFactory : MonoBehaviour
{
    public static UnitFactory Instance { get; private set; }

    [System.Serializable]
    public class UnitPrefabMapping
    {
        public string unitID; 
        public BaseUnit prefab; 
    }

    /// This list shows up in the Unity Inspector so we can drag and drop our units.
    public List<UnitPrefabMapping> unitDatabase;
    private Dictionary<string, BaseUnit> prefabDictionary;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); 
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // Keep the Factory alive when changing levels.

        /// Fill the Dictionary with data from our list for faster searching later.
        prefabDictionary = new Dictionary<string, BaseUnit>();
        foreach (var mapping in unitDatabase)
        {
            if (!prefabDictionary.ContainsKey(mapping.unitID))
            {
                prefabDictionary.Add(mapping.unitID, mapping.prefab);
            }
        }
    }

    /// This is the main "Cook" method. Give it a name and a location, and it creates a unit.
    public BaseUnit CreateUnit(string unitID, Tile targetTile, int playerOwner)
    {
        GameObject unitObject = targetTile.ActivateHiddenUnit(unitID);
        BaseUnit unitScript = null;

        if (unitObject == null)
        {
            if (prefabDictionary != null && prefabDictionary.TryGetValue(unitID, out BaseUnit prefab))
            {
                unitScript = Instantiate(prefab, targetTile.transform.position, Quaternion.identity);
                unitObject = unitScript.gameObject;

                Vector3 originalScale = prefab.transform.localScale;
                unitObject.transform.SetParent(targetTile.transform);
                unitObject.transform.localScale = originalScale;
                unitObject.transform.localPosition = Vector3.zero;

                targetTile.OccupiedUnit = unitObject;
            }
        }
        else
        {
            unitScript = unitObject.GetComponent<BaseUnit>();
        }

        if (unitScript != null)
        {
            /// Assign who owns this unit (Player 1 or Player 2).
            unitScript.ownerPlayer = playerOwner;
            unitScript.currentTile = targetTile;

            unitScript.SetupTeamAppearance();

            /// Rotate Player 2's units by 180 degrees so they face Player 1.
            unitObject.transform.localRotation = Quaternion.Euler(0, playerOwner == 2 ? 180 : 0, 0);
        }

        return unitScript;
    }
}