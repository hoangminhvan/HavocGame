using UnityEngine;

public class Vampire : BaseUnit
{
    protected override void InitStats()
    {
        unitID = "Vampire";
        unitName = "Dracula";

        maxHP = 90;
        maxMana = 60;
        damage = 22;
        defense = 10;

        moveRange = 3;
        attackRange = 1;

        manaRegen = 15;
        skillManaCost = 30;

        isOffensiveSkill = true;

        skillDescription = "Blood Curse: Curse a target for 3 turns, dealing 15% of its max HP each turn. When the curse triggers, it spreads to nearby enemies within 2 tile and heals the caster for 50% of the damage dealt.";
    }

    public override void UseSkill(Tile targetTile, BaseUnit targetUnit = null)
    {
        if (targetUnit == null || targetUnit.ownerPlayer == this.ownerPlayer) return;
        if (currentMana < skillManaCost) return;

        UseMana(skillManaCost);
        StartCoroutine(PlayTemporaryAnimation(animSkill, 1f));

        targetUnit.ApplyCurse(3, this);
    }
}