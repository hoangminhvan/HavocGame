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
        // Start a new game and clear old save data
        ResumeGame();

        PlayerPrefs.SetInt("HasSave", 0);
        PlayerPrefs.Save();

        if (GameData.Instance != null)
            GameData.Instance.ClearData();

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
        // Save current match state if in battle scene
        if (SceneManager.GetActiveScene().name == "BattleScene" && BattleGameManager.Instance != null)
        {
            MatchSaveData currentData = new MatchSaveData();

            currentData.currentTurn = TurnHandler.Instance.currentTurn;
            currentData.currentPlayerTurn = TurnHandler.Instance.currentPlayerTurn;
            currentData.currentEnergy = TurnHandler.Instance.currentEnergy;

            BaseUnit[] allUnits = FindObjectsByType<BaseUnit>(FindObjectsSortMode.None);

            foreach (BaseUnit unit in allUnits)
            {
                if (unit.currentHP > 0 && unit.currentTile != null && unit.gameObject.activeInHierarchy)
                {
                    // Save unit state data
                    PlacedUnitInfo unitInfo = new PlacedUnitInfo();
                    unitInfo.unitID = unit.unitID;
                    unitInfo.playerOwner = unit.ownerPlayer;
                    unitInfo.coords = unit.currentTile.GridCoords;
                    unitInfo.currentHP = unit.currentHP;
                    unitInfo.currentMana = unit.currentMana;

                    if (unit.ownerPlayer == 1) currentData.p1Units.Add(unitInfo);
                    else if (unit.ownerPlayer == 2) currentData.p2Units.Add(unitInfo);
                }
            }

            // Save match data using repository
            DataStorageContext.Repository.SaveMatch(currentData);
        }

        // Return to main menu
        ResumeGame();
        SceneManager.LoadScene("MainMenu");
    }
}