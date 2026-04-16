using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BattleUIManager : MonoBehaviour, IUnitObserver
{
    public static BattleUIManager Instance { get; private set; }

    [Header("Main Panel")]
    public GameObject unitInfoPanel;

    [Header("Environment")]
    public Transform backgroundImage;
    private Vector3 _bgStartPos;

    [Header("Visuals")]
    public Transform avatarContainer;
    public Transform skillIconContainer;
    public GameObject[] p2RedSquares;

    [Header("Health & Mana")]
    public Slider hpSlider;
    public TextMeshProUGUI hpText;
    public Slider manaSlider;
    public TextMeshProUGUI manaText;

    [Header("Detailed Stats")]
    public TextMeshProUGUI unitNameText;
    public TextMeshProUGUI unitIdText;
    public TextMeshProUGUI dmgText;
    public TextMeshProUGUI defText;
    public TextMeshProUGUI manaRegenText;
    public TextMeshProUGUI speedText;
    public TextMeshProUGUI critText;
    public TextMeshProUGUI skillManaCostText;

    [Header("Status Info")]
    public TextMeshProUGUI statusText;

    [Header("Action UI")]
    public GameObject actionPanel;

    private BaseUnit currentSelectedUnit;

    [Header("Win Condition UI")]
    public GameObject winPanel;
    public TextMeshProUGUI winnerText;

    [Header("Warning UI")]
    public TextMeshProUGUI warningText;
    private Coroutine _warningCoroutine;
    private Vector2 _initialWarningPos;

    [Header("Turn Notification UI")]
    public GameObject turnNotifyPanel;
    public TextMeshProUGUI turnNotifyText;

    [Header("Audio System")]
    public AudioSource bgmSource;
    public AudioSource uiAudioSource;
    public AudioClip winSFX;

    [Header("Fake Loading UI")]
    public GameObject fakeLoadingPanel;
    public float loadingDuration = 1.5f;

    private void Awake()
    {
        Instance = this;
        HidePanel();
        if (warningText != null)
        {
            _initialWarningPos = warningText.rectTransform.anchoredPosition;
            warningText.gameObject.SetActive(false);
        }

        if (backgroundImage != null)
        {
            _bgStartPos = backgroundImage.localPosition;
        }
    }

    /// Subscribes to Observer and starts background music
    private void Start()
    {
        if (UnitObserverManager.Instance != null)
        {
            UnitObserverManager.Instance.AddObserver(this);
        }

        if (bgmSource != null && !bgmSource.isPlaying)
        {
            bgmSource.loop = true;
            bgmSource.Play();
        }
        if (fakeLoadingPanel != null)
        {
            StartCoroutine(FakeLoadingRoutine());
        }
    }

    /// Unsubscribes from Observer when destroyed
    private void OnDestroy()
    {
        if (UnitObserverManager.Instance != null)
        {
            UnitObserverManager.Instance.RemoveObserver(this);
        }
    }

    /// Handles the visual loading screen delay
    private IEnumerator FakeLoadingRoutine()
    {
        fakeLoadingPanel.SetActive(true);
        yield return new WaitForSeconds(loadingDuration);
        fakeLoadingPanel.SetActive(false);
    }

    /// Triggers the turn start notification
    public void ShowTurnNotify(int playerNum)
    {
        if (turnNotifyPanel == null) return;
        StartCoroutine(TurnNotifyRoutine(playerNum));
    }

    /// Animates the turn change text and scale
    private IEnumerator TurnNotifyRoutine(int playerNum)
    {
        turnNotifyPanel.SetActive(true);
        turnNotifyText.text = $"PLAYER {playerNum} TURN";
        turnNotifyText.color = (playerNum == 1) ? Color.white : Color.red;

        RectTransform rect = turnNotifyPanel.GetComponent<RectTransform>();
        CanvasGroup group = turnNotifyPanel.GetComponent<CanvasGroup>();

        float elapsed = 0;
        float duration = 0.5f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float s = Mathf.Lerp(0, 1.1f, t);
            rect.localScale = new Vector3(s, s, 1);
            if (group != null) group.alpha = t;
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            if (group != null) group.alpha = 1 - t;
            yield return null;
        }

        turnNotifyPanel.SetActive(false);
    }

    /// Animates the background image position
    private void Update()
    {
        if (backgroundImage != null)
        {
            backgroundImage.localPosition = new Vector3(_bgStartPos.x, _bgStartPos.y + Mathf.Sin(Time.time * 1.5f) * 15f, _bgStartPos.z);
        }
    }

    /// Displays the end-game victory panel
    public void ShowWinPanel(int winningPlayer)
    {
        if (bgmSource != null) bgmSource.Stop();
        if (uiAudioSource != null && winSFX != null) uiAudioSource.PlayOneShot(winSFX);
        if (winPanel != null) winPanel.SetActive(true);

        if (winnerText != null)
        {
            winnerText.text = $"PLAYER {winningPlayer} WINS!";
            winnerText.color = (winningPlayer == 1) ? Color.white : Color.red;
        }

        if (actionPanel != null) actionPanel.SetActive(false);
        HidePanel();
    }

    /// Resets data and returns to the preparation scene
    public void OnBackToPrepareButtonClicked()
    {
        GameData.Instance.ClearData();
        if (CommandManager.Instance != null) CommandManager.Instance.ClearHistory();
        SceneManager.LoadScene("PrepareState");
    }

    /// Updates and shows the detailed info panel for a unit
    public void ShowUnitInfo(BaseUnit unit)
    {
        unitInfoPanel.SetActive(true);
        currentSelectedUnit = unit;

        OnHealthUpdated(unit, unit.currentHP, unit.maxHP);
        OnManaUpdated(unit, unit.currentMana, unit.maxMana);

        if (avatarContainer != null)
            foreach (Transform child in avatarContainer) child.gameObject.SetActive(child.name == unit.unitID);

        if (skillIconContainer != null)
            foreach (Transform child in skillIconContainer) child.gameObject.SetActive(child.name == unit.unitID);

        bool isP2 = (unit.ownerPlayer == 2);
        if (p2RedSquares != null)
            foreach (GameObject redSquare in p2RedSquares) if (redSquare != null) redSquare.SetActive(isP2);

        if (unitNameText != null) unitNameText.text = unit.unitName;
        if (unitIdText != null) unitIdText.text = unit.unitID;
        RefreshStatsUI(unit);

        bool isMyTurn = (unit.ownerPlayer == TurnHandler.Instance.currentPlayerTurn && unit.stunTurns <= 0);
        if (actionPanel != null) actionPanel.SetActive(isMyTurn);
    }

    /// Updates health bar UI via Observer pattern
    public void OnHealthUpdated(BaseUnit unit, int currentHP, int maxHP)
    {
        if (currentSelectedUnit == null || unit != currentSelectedUnit) return;

        if (hpSlider != null) { hpSlider.maxValue = maxHP; hpSlider.value = maxHP - currentHP; }
        if (hpText != null) hpText.text = $"{currentHP} / {maxHP}";
        RefreshStatsUI(unit);
    }

    /// Updates mana bar UI via Observer pattern
    public void OnManaUpdated(BaseUnit unit, int currentMana, int maxMana)
    {
        if (currentSelectedUnit == null || unit != currentSelectedUnit) return;

        if (manaSlider != null) { manaSlider.maxValue = maxMana; manaSlider.value = maxMana - currentMana; }
        if (manaText != null) manaText.text = $"{currentMana} / {maxMana}";
    }

    /// Refreshes text displays for all unit attributes and status effects
    public void RefreshStatsUI(BaseUnit unit)
    {
        if (dmgText != null) dmgText.text = unit.DisplayDamage.ToString();
        if (defText != null) defText.text = unit.defense.ToString();
        if (manaRegenText != null) manaRegenText.text = unit.manaRegen.ToString();
        if (speedText != null) speedText.text = unit.moveRange.ToString();
        if (critText != null) critText.text = $"{Mathf.RoundToInt(unit.DisplayCrit * 100)}%";
        if (skillManaCostText != null) skillManaCostText.text = unit.skillManaCost.ToString();

        if (statusText != null)
        {
            string currentStatus = "";

            if (unit.hasShield) currentStatus += "Reduces incoming damage by 30% (1 turn).\n";
            if (unit.stunTurns > 0) currentStatus += $"Stunned for {unit.stunTurns} turn(s).\n";
            if (unit.curseTurns > 0) currentStatus += $"Cursed for {unit.curseTurns} turn(s) (Losing 15% Max HP/turn).\n";

            if (unit is Berserker berserker)
            {
                if (berserker.buffTurns > 0)
                {
                    int uiTurns = Mathf.CeilToInt(berserker.buffTurns / 2f);
                    currentStatus += $"Blood Sacrifice active: {uiTurns} turn(s) (+20% DMG, +50% Crit).\n";
                }

                float missingHpPercent = 1f - ((float)berserker.currentHP / berserker.maxHP);
                int passiveBonus = Mathf.RoundToInt(missingHpPercent * 0.8f * 100);
                if (passiveBonus > 0) currentStatus += $"Frenzy Passive: +{passiveBonus}% Base DMG.\n";
            }

            statusText.text = currentStatus.Trim();
        }
    }

    /// Closes the unit information panel
    public void HidePanel()
    {
        unitInfoPanel.SetActive(false);
        currentSelectedUnit = null;
    }

    /// Switches unit state to movement mode
    public void OnMoveButtonClicked()
    {
        if (currentSelectedUnit != null)
        {
            if (TurnHandler.Instance.currentEnergy < BattleGameManager.MOVE_ENERGY)
            {
                ShowWarning("Not enough Energy!");
                return;
            }
            BattleGameManager.Instance.activeUnit = currentSelectedUnit;
            currentSelectedUnit.ChangeState(new UnitWaitingMoveState());
        }
    }

    /// Switches unit state to attack mode
    public void OnAttackButtonClicked()
    {
        if (currentSelectedUnit != null)
        {
            if (TurnHandler.Instance.currentEnergy < BattleGameManager.ATTACK_ENERGY)
            {
                ShowWarning("Not enough Energy!");
                return;
            }
            BattleGameManager.Instance.activeUnit = currentSelectedUnit;
            currentSelectedUnit.ChangeState(new UnitWaitingAttackState());
        }
    }

    /// Switches unit state to skill mode or executes instant skill
    public void OnSkillButtonClicked()
    {
        if (currentSelectedUnit != null)
        {
            if (currentSelectedUnit is Berserker berserker && berserker.buffTurns > 0)
            {
                return;
            }
            if (TurnHandler.Instance.currentEnergy < BattleGameManager.SKILL_ENERGY)
            {
                ShowWarning("Not enough Energy!");
                return;
            }

            if (currentSelectedUnit.currentMana < currentSelectedUnit.skillManaCost)
            {
                ShowWarning("Not enough Mana!");
                return;
            }

            if (currentSelectedUnit.isInstantSkill)
            {
                ActionHandler.Instance.ProcessSkill(currentSelectedUnit, currentSelectedUnit.currentTile, currentSelectedUnit);

                RefreshStatsUI(currentSelectedUnit);
                currentSelectedUnit.ChangeState(new UnitIdleState());
                return;
            }

            BattleGameManager.Instance.activeUnit = currentSelectedUnit;
            currentSelectedUnit.ChangeState(new UnitWaitingSkillState());
        }
    }

    /// Sets up and triggers the UI warning message
    public void ShowWarning(string message)
    {
        if (warningText == null) return;
        if (_warningCoroutine != null) StopCoroutine(_warningCoroutine);
        _warningCoroutine = StartCoroutine(WarningRoutine(message));
    }

    /// Animates warning text movement and fading
    private System.Collections.IEnumerator WarningRoutine(string message)
    {
        warningText.text = message;
        warningText.gameObject.SetActive(true);
        warningText.rectTransform.anchoredPosition = _initialWarningPos;

        Color c = Color.red;
        c.a = 1f;
        warningText.color = c;

        float timer = 1.5f;
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            warningText.rectTransform.anchoredPosition += new Vector2(0, 50f * Time.deltaTime);
            c.a = timer / 1.5f;
            warningText.color = c;
            yield return null;
        }
        warningText.gameObject.SetActive(false);
    }
}