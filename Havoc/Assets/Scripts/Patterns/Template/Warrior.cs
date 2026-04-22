using UnityEngine;

// PATTERN: Concrete Implementation (Template Method)
// Lop Warrior thuc thi mot don danh manh co kha nang gay choang va tu tao giap
public class Warrior : BaseUnit
{
    protected override void InitStats()
    {
        unitID = "Warrior";
        unitName = "Arthur";

        maxHP = 120; 
        maxMana = 40;
        damage = 22;
        defense = 20; 

        moveRange = 2;
        attackRange = 1;

        manaRegen = 10;
        skillManaCost = 20;

        isOffensiveSkill = true;

        skillDescription = "Power Strike: Deal 125% damage with a 50% chance to stun the target for 1 turn. Gain a shield that reduces incoming damage by 30% for 1 turn";
    }

    public override void UseSkill(Tile targetTile, BaseUnit targetUnit = null)
    {
        if (targetUnit == null || targetUnit.ownerPlayer == this.ownerPlayer) return;
        if (currentMana < skillManaCost) return;

        UseMana(skillManaCost);
        StartCoroutine(PlayTemporaryAnimation(animSkill, 0.5f));

        // Gay sat thuong ky nang
        int skillDamage = Mathf.RoundToInt(damage * 1.25f);
        targetUnit.TakeDamage(skillDamage);

        // Xac suat gay choang (Stun)
        if (targetUnit.currentHP > 0 && Random.value <= 0.5f)
        {
            targetUnit.ApplyStun(2);
        }

        // Tu kich hoat giap (Shield)
        this.hasShield = true;
        if (this.shieldVisual != null) this.shieldVisual.SetActive(true);

    }


}