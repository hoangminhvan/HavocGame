using UnityEngine;
/// Concrete Command: Handles consumable item usage.
public class ItemCommand : ICommand
{
    private BaseUnit target;
    private ItemType type;
    private int amount;

    public ItemCommand(BaseUnit target, ItemType type, int amount)
    {
        this.target = target;
        this.type = type;
        this.amount = amount;
    }

    public void Execute()
    {
        if (type == ItemType.Health) target.Heal(amount);
        else target.RestoreMana(amount);

        TurnHandler.Instance.SpendEnergy(BattleGameManager.ITEM_ENERGY);  
        TurnHandler.Instance.UpdateTurnUI();
    }

    public void Undo()
    {
        if (type == ItemType.Health)
        {
            target.currentHP = Mathf.Clamp(target.currentHP - amount, 1, target.maxHP);
            target.ShowTextPopup("-" + amount, Color.red);
        }
        else
        {
            target.currentMana = Mathf.Clamp(target.currentMana - amount, 0, target.maxMana);
            target.ShowTextPopup("-" + amount + " Mana", Color.blue);
        }

        target.NotifyHealthChanged();
        target.NotifyManaChanged();
        TurnHandler.Instance.currentEnergy += BattleGameManager.ITEM_ENERGY;
        TurnHandler.Instance.UpdateTurnUI();
    }
}