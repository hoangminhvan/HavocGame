using UnityEngine;
using UnityEngine.EventSystems;
public enum ItemType { Health, Mana }

public class ConsumableItem : MonoBehaviour, IPointerClickHandler
{
    public ItemType type;
    public int value = 50;
    public int energyCost = 4;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (TurnHandler.Instance.currentEnergy < energyCost)
        {
            BattleUIManager.Instance.ShowWarning("Not enough Energy!");
            return;
        }

        BattleGameManager.Instance.PrepareItemUsage(this);
    }
}