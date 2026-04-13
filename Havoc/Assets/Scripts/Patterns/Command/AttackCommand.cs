using UnityEngine;

/// Concrete Command: Handles basic attacks and state restoration.
public class AttackCommand : ICommand
{
    private readonly BaseUnit attacker;
    private readonly BaseUnit target;

    private readonly int previousTargetHP;
    private bool didTargetDie;

    public AttackCommand(BaseUnit attacker, BaseUnit target)
    {
        this.attacker = attacker;
        this.target = target;

        previousTargetHP = target.currentHP;
        didTargetDie = false;
    }

    public void Execute()
    {
        attacker.BasicAttack(target);

        if (target.currentHP <= 0)
        {
            didTargetDie = true;
        }

        TurnHandler.Instance.SpendEnergy(BattleGameManager.ATTACK_ENERGY);
        TurnHandler.Instance.UpdateTurnUI();
    }

    public void Undo()
    {
        // Revive target if killed by this attack
        if (didTargetDie)
        {
            target.gameObject.SetActive(true);

            if (target.currentTile != null)
            {
                target.currentTile.OccupiedUnit = target.gameObject;
            }

            target.ChangeAnimation(target.animIdle);
            GameData.Instance.UnitRevived(target.ownerPlayer);
        }

        // Restore HP
        target.currentHP = previousTargetHP;
        target.NotifyHealthChanged();

        // Refund energy and reset selection
        TurnHandler.Instance.currentEnergy += BattleGameManager.ATTACK_ENERGY;
        TurnHandler.Instance.UpdateTurnUI();
        BattleGameManager.Instance.DeselectActiveUnit();
    }
}