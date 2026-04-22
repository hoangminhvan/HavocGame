using System.Collections.Generic;
using UnityEngine;

// PATTERN: Concrete Implementation (Template Method)
// Lop Mage thuc thi ky nang no gay sat thuong lan cho cac o xung quanh
public class Mage : BaseUnit
{
    protected override void InitStats()
    {
        unitID = "Mage"; unitName = "Merlin";
        maxHP = 60; maxMana = 60;
        damage = 20; defense = 5;
        moveRange = 2; attackRange = 3; // Tam xa
        manaRegen = 10; skillManaCost = 40;

        isOffensiveSkill = true;
        skillDescription = "Explosion: Deal 200% damage to target and 100% damage to enemies within 1-tile radius.";
    }

    public override void UseSkill(Tile targetTile, BaseUnit targetUnit = null)
    {
        if (currentMana < skillManaCost) return;

        if (targetUnit != null && targetUnit.ownerPlayer != this.ownerPlayer)
        {
            UseMana(skillManaCost);
            StartCoroutine(PlayTemporaryAnimation(animSkill, 0.5f));

            int primaryDamage = damage * 2;
            int splashDamage = damage;

            // Lay danh sach cac o trong ban kinh anh huong (AoE)
            List<Vector2Int> aoeCoords = HexGridUtils.GetTilesInRange(targetTile.GridCoords, 2, BattleGameManager.Instance.allGridTiles);

            foreach (Vector2Int coords in aoeCoords)
            {
                if (BattleGameManager.Instance.allGridTiles.TryGetValue(coords, out Tile t))
                {
                    t.PlayExplosion(); // Hieu ung no tai o

                    if (t.IsOccupied)
                    {
                        BaseUnit hitUnit = t.OccupiedUnit.GetComponent<BaseUnit>();
                        // Gay sat thuong cho ke dich (khong gay sat thuong cho dong doi)
                        if (hitUnit != null && hitUnit.currentHP > 0 && hitUnit.ownerPlayer != this.ownerPlayer)
                        {
                            if (hitUnit == targetUnit) hitUnit.TakeDamage(primaryDamage);
                            else hitUnit.TakeDamage(splashDamage);
                        }
                    }
                }
            }
        }
    }
}