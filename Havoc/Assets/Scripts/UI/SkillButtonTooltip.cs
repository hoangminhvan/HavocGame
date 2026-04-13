using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class SkillButtonTooltip : MonoBehaviour, IPointerClickHandler
{
    public GameObject tooltipPanel;
    public TextMeshProUGUI tooltipText;

    private BaseUnit currentDisplayedUnit;

    private void Update()
    {
        if (tooltipPanel != null && tooltipPanel.activeSelf)
        {
            BaseUnit activeUnit = BattleGameManager.Instance.activeUnit;

            if (activeUnit == null || activeUnit != currentDisplayedUnit)
            {
                HideTooltip();
                return;
            }

            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
            {
                if (!EventSystem.current.IsPointerOverGameObject())
                {
                    HideTooltip();
                }
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            BaseUnit activeUnit = BattleGameManager.Instance.activeUnit;
            if (activeUnit != null)
            {
                bool isActive = !tooltipPanel.activeSelf;
                tooltipPanel.SetActive(isActive);

                if (isActive && tooltipText != null)
                {
                    tooltipText.text = activeUnit.skillDescription;
                    currentDisplayedUnit = activeUnit;
                }
            }
        }
        else if (eventData.button == PointerEventData.InputButton.Left)
        {
            HideTooltip();
            BattleUIManager.Instance.OnSkillButtonClicked();
        }
    }

    private void HideTooltip()
    {
        if (tooltipPanel != null) tooltipPanel.SetActive(false);
    }

    private void OnDisable()
    {
        HideTooltip();
    }
}