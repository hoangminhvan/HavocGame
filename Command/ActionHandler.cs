using UnityEngine;

/// Class to handle all actions (Commands) in the battle
public class ActionHandler : MonoBehaviour
{
    public static ActionHandler Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void ProcessMove(BaseUnit unit, Tile startTile, Tile endTile)
    {
        if (TurnHandler.Instance.currentEnergy < BattleGameManager.MOVE_ENERGY)
        {
            BattleUIManager.Instance.ShowWarning("Not enough Energy!");
            return;
        }

        ICommand moveCmd = new MoveCommand(unit, startTile, endTile);
        CommandManager.Instance.ExecuteCommand(moveCmd);
    }


    public void ProcessAttack(BaseUnit attacker, BaseUnit target)
    {
        if (TurnHandler.Instance.currentEnergy < BattleGameManager.ATTACK_ENERGY)
        {
            BattleUIManager.Instance.ShowWarning("Not enough Energy!");
            return;
        }

        ICommand attackCmd = new AttackCommand(attacker, target);
        CommandManager.Instance.ExecuteCommand(attackCmd);
    }

    public void ProcessSkill(BaseUnit caster, Tile targetTile, BaseUnit targetUnit)
    {
        if (TurnHandler.Instance.currentEnergy < BattleGameManager.SKILL_ENERGY)
        {
            BattleUIManager.Instance.ShowWarning("Not enough Energy!");
            return;
        }

        if (caster.currentMana < caster.skillManaCost)
        {
            BattleUIManager.Instance.ShowWarning("Not enough Mana!");
            return;
        }

        ICommand skillCmd = new SkillCommand(caster, targetTile, targetUnit);
        CommandManager.Instance.ExecuteCommand(skillCmd);
    }

    public void ProcessItem(BaseUnit target, ConsumableItem item)
    {
        if (TurnHandler.Instance.currentEnergy < BattleGameManager.ITEM_ENERGY)
        {
            BattleUIManager.Instance.ShowWarning("Not enough Energy!");
            return;
        }

        ICommand itemCmd = new ItemCommand(target, item.type, item.value);
        CommandManager.Instance.ExecuteCommand(itemCmd);
    }
}