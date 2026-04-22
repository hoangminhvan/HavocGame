using UnityEngine;

public class Berserker : BaseUnit
{
    public int buffTurns = 0;

    protected override void InitStats()
    {
        unitID = "Berserker";
        unitName = "Olaf";

        maxHP = 120;
        maxMana = 40;
        damage = 16;
        defense = 8;

        moveRange = 2;
        attackRange = 1;

        manaRegen = 10;
        skillManaCost = 30;

        criticalChance = 0.1f;

        isOffensiveSkill = false;
        isInstantSkill = true;

        skillDescription = "Blood Sacrifice: Consume 25% max HP to gain +25% damage and +40% crit chance for 2 turns.\nPassive: Gain +0.6% damage for each 1% missing HP.";
    }

    public override int DisplayDamage
    {
        get
        {
            float finalDamage = damage;

            float missingHpPercent = 1f - ((float)currentHP / maxHP);
            float passiveBonus = missingHpPercent * 0.6f;

            finalDamage += damage * passiveBonus;

            if (buffTurns > 0) finalDamage *= 1.25f;

            return Mathf.RoundToInt(finalDamage);
        }
    }

    public override int GetFinalDamage(out bool isCrit)
    {
        isCrit = false;

        float finalCrit = criticalChance;
        float finalDamage = DisplayDamage;

        if (buffTurns > 0) finalCrit += 0.4f;

        if (Random.value <= finalCrit)
        {
            isCrit = true;
            finalDamage *= 2f;
        }

        return Mathf.RoundToInt(finalDamage);
    }

    public void ApplyBuff(int turns)
    {
        buffTurns = turns;

        if (animator != null) animator.speed = 1.5f;

        ShowTextPopup("Blood Rage!", Color.red, true);

        if (currentTile != null) currentTile.SetBuffEffect(true);
        NotifyHealthChanged();
    }

    public override void ClearTileEffects()
    {
        base.ClearTileEffects();
        if (currentTile != null) currentTile.SetBuffEffect(false);
    }

    public override void RefreshTileEffects()
    {
        base.RefreshTileEffects();
        if (currentTile != null) currentTile.SetBuffEffect(buffTurns > 0);
    }

    public override float DisplayCrit
    {
        get
        {
            float finalCrit = criticalChance;
            if (buffTurns > 0) finalCrit += 0.4f;
            return finalCrit;
        }
    }

    public void ClearBuff()
    {
        buffTurns = 0;

        if (animator != null) animator.speed = 1f;

        if (currentTile != null) currentTile.SetBuffEffect(false);
        NotifyHealthChanged();
    }

    public override void UseSkill(Tile targetTile, BaseUnit targetUnit = null)
    {
        if (buffTurns > 0) return;

        if (currentMana < skillManaCost) return;

        UseMana(skillManaCost);
        StartCoroutine(PlayTemporaryAnimation(animSkill, 1f));

        int sacrificeAmount = Mathf.RoundToInt(maxHP * 0.25f);
        if (currentHP <= sacrificeAmount) sacrificeAmount = currentHP - 1;

        if (sacrificeAmount > 0) TakeDamage(sacrificeAmount);

        ApplyBuff(4);
    }
}