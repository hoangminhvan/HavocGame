using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    // Singleton instance for global access
    public static PauseManager Instance { get; private set; }

    [Header("UI Panels")]
    public GameObject pauseMenuPanel;
    public GameObject optionsPanel;

    // Track pause state
    private bool isPaused = false;

    private void Awake()
    {
        // Ensure only one instance exists across scenes
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this.gameObject); // Keep this object across scenes
    }

    private void Start()
    {
        // Disable all UI panels at start
        pauseMenuPanel.SetActive(false);
        optionsPanel.SetActive(false);
    }

    private void Update()
    {
        // Get current scene name
        string currentScene = SceneManager.GetActiveScene().name;

        // Prevent pause in Main Menu
        if (currentScene == "MainMenu") return;

        // Toggle pause with ESC key
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (optionsPanel.activeSelf)
            {
                CloseOptions(); // Go back to pause menu if options is open
            }
            else
            {
                if (isPaused) ResumeGame();
                else PauseGame();
            }
        }
    }

    private void PauseGame()
    {
        // Pause the game and show pause menu
        isPaused = true;
        pauseMenuPanel.SetActive(true);
        Time.timeScale = 0f; // Freeze game
    }

    public void ResumeGame()
    {
        // Resume gameplay and hide all pause UI
        isPaused = false;
        pauseMenuPanel.SetActive(false);
        optionsPanel.SetActive(false);
        Time.timeScale = 1f; // Resume game time
    }

    public void OnNewGameClicked()
    {
        bool wasPvE = false;
        if (GameData.Instance != null)
        {
            wasPvE = GameData.Instance.isPvEMode;
        }

        ResumeGame();

        PlayerPrefs.SetInt("HasSave", 0);
        PlayerPrefs.Save();

        if (DataStorageContext.Repository != null)
            DataStorageContext.Repository.ClearSavedMatch();

        if (GameData.Instance != null)
        {
            GameData.Instance.ClearData();

            GameData.Instance.isPvEMode = wasPvE;

            if (wasPvE)
            {
                AIFormationExporter exporter = new AIFormationExporter();
                exporter.SetStrategy(new JsonFormationExportStrategy());
                List<PlacedUnitInfo> aiFormation = exporter.ExecuteExport();

                if (aiFormation != null && aiFormation.Count > 0)
                {
                    GameData.Instance.p2Units = aiFormation;
                    GameData.Instance.p2AliveUnits = aiFormation.Count;
                }
                else
                {
                }
            }
        }

        SceneManager.LoadScene("PrepareState");
    }

    public void OpenOptions()
    {
        // Open options panel from pause menu
        pauseMenuPanel.SetActive(false);
        optionsPanel.SetActive(true);
    }

    public void CloseOptions()
    {
        // Return to pause menu from options panel
        optionsPanel.SetActive(false);
        pauseMenuPanel.SetActive(true);
    }

    public void ReturnToMenu()
    {
        if (SceneManager.GetActiveScene().name == "BattleScene" && BattleGameManager.Instance != null)
        {
            MatchSaveData currentData = new MatchSaveData();
            currentData.currentTurn = TurnHandler.Instance.currentTurn;
            currentData.currentPlayerTurn = TurnHandler.Instance.currentPlayerTurn;
            currentData.currentEnergy = TurnHandler.Instance.currentEnergy;

            if (GameData.Instance != null)
                currentData.isPvE = GameData.Instance.isPvEMode;

            BaseUnit[] allUnits = FindObjectsByType<BaseUnit>(FindObjectsSortMode.None);
            foreach (BaseUnit unit in allUnits)
            {
                if (unit.currentHP > 0 && unit.gameObject.activeInHierarchy)
                {
                    PlacedUnitInfo unitInfo = new PlacedUnitInfo();
                    unitInfo.unitID = unit.unitID;
                    unitInfo.playerOwner = unit.ownerPlayer;
                    unitInfo.coords = unit.currentTile.GridCoords;
                    unitInfo.currentHP = unit.currentHP;
                    unitInfo.currentMana = unit.currentMana;

                    if (unit.ownerPlayer == 1) currentData.p1Units.Add(unitInfo);
                    else currentData.p2Units.Add(unitInfo);
                }
            }
            DataStorageContext.Repository.SaveMatch(currentData);
            PlayerPrefs.SetInt("HasSave", 1); 
            PlayerPrefs.Save();
        }

        ResumeGame(); 
        SceneManager.LoadScene("MainMenu");
    }
}