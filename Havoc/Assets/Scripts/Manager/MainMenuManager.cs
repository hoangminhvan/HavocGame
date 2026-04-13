using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Audio;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject mainMenuPanel;
    public GameObject tutorialPanel;
    public GameObject optionsPanel;

    [Header("Buttons")]
    public Button continueButton;

    [Header("Audio Settings")]
    public AudioMixer mainMixer;
    public Slider musicSlider;
    public Slider sfxSlider;

    private void Start()
    {
        // Check if a save exists and enable/disable Continue button
        int hasSave = PlayerPrefs.GetInt("HasSave", 0);
        if (continueButton != null)
        {
            continueButton.interactable = (hasSave == 1);
        }

        // Initialize audio sliders and bind volume controls
        float currentMusicVol;
        mainMixer.GetFloat("MusicVol", out currentMusicVol);

        musicSlider.value = 1f;
        sfxSlider.value = 1f;

        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
    }

    public void OnContinueClicked()
    {
        // Load the last saved scene 
        string sceneToLoad = PlayerPrefs.GetString("SavedScene", "PrepareState");
        SceneManager.LoadScene(sceneToLoad);
    }

    public void OnStartGameClicked()
    {
        // Clear saved match data and start a new game
        DataStorageContext.Repository.ClearSavedMatch();

        if (GameData.Instance != null)
            GameData.Instance.ClearData();

        SceneManager.LoadScene("PrepareState");
    }

    public void OnTutorialClicked()
    {
        // Open tutorial panel
        mainMenuPanel.SetActive(false);
        tutorialPanel.SetActive(true);
    }

    public void OnOptionsClicked()
    {
        // Open options panel
        mainMenuPanel.SetActive(false);
        optionsPanel.SetActive(true);
    }

    public void OnQuitClicked()
    {
        // Quit the application
        Debug.Log("Quit Game");
        Application.Quit();
    }

    public void OnClosePopupClicked()
    {
        // Close all popups and return to main menu
        tutorialPanel.SetActive(false);
        optionsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    public void SetMusicVolume(float value)
    {
        // Convert slider value to decibel and set music volume
        float dbValue = (value <= 0.0001f) ? -80f : Mathf.Log10(value) * 20;
        mainMixer.SetFloat("MusicVol", dbValue);
    }

    public void SetSFXVolume(float value)
    {
        // Convert slider value to decibel and set SFX volume
        float dbValue = (value <= 0.0001f) ? -80f : Mathf.Log10(value) * 20;
        mainMixer.SetFloat("SFXVol", dbValue);
    }
}