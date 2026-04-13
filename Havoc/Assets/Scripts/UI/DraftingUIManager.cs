using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DraftingUIManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject textP1;
    public GameObject textP2;
    public TextMeshProUGUI textStatus;
    public TextMeshProUGUI btnConfirmText;

    [Header("Environment & Audio")]
    public Transform backgroundImage;
    public AudioSource bgmSource;

    private Vector3 bgStartPos;
    private Vector3 p1StartPos;
    private Vector3 p2StartPos;

    private void Start()
    {
        if (backgroundImage != null) bgStartPos = backgroundImage.localPosition;
        if (textP1 != null) p1StartPos = textP1.transform.localPosition;
        if (textP2 != null) p2StartPos = textP2.transform.localPosition;

        if (bgmSource != null && !bgmSource.isPlaying) bgmSource.Play();

        SetupPlayer1Turn();
    }

    private void Update()
    {
        if (textStatus != null)
        {
            textStatus.transform.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(Time.time * 3f) * 1f);
        }

        float shakeOffset = Mathf.Sin(Time.time * 5f) * 3f;

        if (DraftingGameManager.Instance.currentPlayerTurn == 1 && textP1 != null)
        {
            textP1.transform.localPosition = p1StartPos + new Vector3(shakeOffset, 0, 0);
            if (textP2 != null) textP2.transform.localPosition = p2StartPos;
        }
        else if (DraftingGameManager.Instance.currentPlayerTurn == 2 && textP2 != null)
        {
            textP2.transform.localPosition = p2StartPos + new Vector3(shakeOffset, 0, 0);
            if (textP1 != null) textP1.transform.localPosition = p1StartPos;
        }

        if (backgroundImage != null)
        {
            backgroundImage.localPosition = new Vector3(bgStartPos.x, bgStartPos.y + Mathf.Sin(Time.time * 1.5f) * 15f, bgStartPos.z);
        }
    }

    private void SetupPlayer1Turn()
    {
        textP1.SetActive(true);
        textP2.SetActive(false);
        textStatus.text = "Player 1 Drafting...";

        // Đổi chữ nút Confirm tùy theo Mode
        if (GameData.Instance != null && GameData.Instance.isPvEMode)
            btnConfirmText.text = "Start Game (Vs AI)";
        else
            btnConfirmText.text = "End P1 Turn";

        DraftingGameManager.Instance.UpdateMapVisibility(1);
    }

    private void SetupPlayer2Turn()
    {
        textP1.SetActive(false);
        textP2.SetActive(true);
        textStatus.text = "Player 2 Drafting...";
        btnConfirmText.text = "Start Game!";

        DraftingGameManager.Instance.currentPlayerTurn = 2;
        DraftingGameManager.Instance.UpdateGoldUI();
        DraftingGameManager.Instance.UpdateMapVisibility(2);
    }

    public void OnConfirmButtonClicked()
    {
        if (DraftingGameManager.Instance.currentPlayerTurn == 1)
        {
            if (GameData.Instance != null && GameData.Instance.isPvEMode)
            {
                DraftingGameManager.Instance.ResetMapForBattle();
                SceneManager.LoadScene("BattleScene");
            }
            else 
            {
                SetupPlayer2Turn();
            }
        }
        else if (DraftingGameManager.Instance.currentPlayerTurn == 2)
        {
            DraftingGameManager.Instance.ResetMapForBattle();
            SceneManager.LoadScene("BattleScene");
        }
    }
}