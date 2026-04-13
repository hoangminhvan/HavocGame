using UnityEngine;

public class Healer : BaseUnit
{
    protected override void InitStats()
    {
        unitID = "Healer";
        unitName = "Sophia";
        maxHP = 70;
        maxMana = 70;
        damage = 15;
        defense = 10;

        moveRange = 2;
        attackRange = 4;

        manaRegen = 10;
        skillManaCost = 30;

        skillDescription = "Divine Light: Heals an ally for 20% of their Max HP and grants a 30% DMG reduction shield for 1 turn.";
    }

    public override void UseSkill(Tile targetTile, BaseUnit targetUnit = null)
    {
        if (targetUnit != null && targetUnit.ownerPlayer == this.ownerPlayer && targetUnit.currentHP > 0)
        {
            UseMana(skillManaCost);

            PlaySound(skillSFX);

            StartCoroutine(PlayTemporaryAnimation(animSkill, 1f));

            int healAmount = Mathf.RoundToInt(targetUnit.maxHP * 0.2f);
            targetUnit.Heal(healAmount);

            targetUnit.hasShield = true;
            if (targetUnit.shieldVisual != null)
            {
                targetUnit.shieldVisual.SetActive(true);
            }

            float dirX = targetUnit.transform.position.x - transform.position.x;
            if (Mathf.Abs(dirX) > 0.1f)
            {
                transform.localRotation = Quaternion.Euler(0, dirX < 0 ? 180 : 0, 0);
            }
        }
    }
}