using System.Collections.Generic;
using UnityEngine;

public class Assassin : BaseUnit
{
    protected override void InitStats()
    {
        unitID = "Assassin";
        unitName = "Shadow";

        maxHP = 65;
        maxMana = 50;
        damage = 30;
        defense = 5;

        moveRange = 4;
        attackRange = 1;

        manaRegen = 15;
        skillManaCost = 30;

        criticalChance = 0.2f;

        isOffensiveSkill = true;
        isInstantSkill = false;

        skillDescription = "Shadow Step: Teleport behind the target and become untargetable for 1 turn.\nPassive: Attacks from behind always critically strike.";
    }

    public override void BasicAttack(BaseUnit targetUnit)
    {
        if (stealthTurns > 0) ClearStealth();

        StartCoroutine(PlayTemporaryAnimation(animAttack, 0.5f));

        bool isCrit = false;
        float finalDamage = damage;

        if (IsBehindTarget(targetUnit))
        {
            isCrit = true;
            finalDamage *= 2f;
        }
        else
        {
            if (Random.value <= criticalChance)
            {
                isCrit = true;
                finalDamage *= 2f;
            }
        }

        int actualDamage = Mathf.Max(1, Mathf.RoundToInt(finalDamage) - targetUnit.defense);
        if (isCrit) ShowTextPopup("CRIT!", Color.yellow, true);
        targetUnit.TakeDamage(actualDamage);
    }

    private bool IsBehindTarget(BaseUnit target)
    {
        bool isTargetFacingLeft = target.transform.localRotation.eulerAngles.y >= 90f;
        if (isTargetFacingLeft) return this.transform.position.x > target.transform.position.x;
        else return this.transform.position.x < target.transform.position.x;
    }

    public override void UseSkill(Tile targetTile, BaseUnit targetUnit = null)
    {
        if (targetUnit == null || targetUnit.ownerPlayer == this.ownerPlayer) return;
        if (currentMana < skillManaCost) return;

        UseMana(skillManaCost);
        StartCoroutine(PlayTemporaryAnimation(animSkill, 0.5f));

        Tile jumpTile = FindTileBehind(targetUnit);

        if (jumpTile != null)
        {
            ClearTileEffects();
            if (currentTile != null) currentTile.OccupiedUnit = null;

            transform.position = jumpTile.transform.position;
            transform.SetParent(jumpTile.transform);

            currentTile = jumpTile;
            jumpTile.OccupiedUnit = this.gameObject;

            Vector3 dir = targetUnit.transform.position - transform.position;
            if (dir.x != 0) transform.localRotation = Quaternion.Euler(0, dir.x < 0 ? 180 : 0, 0);

            RefreshTileEffects();
        }

        ApplyStealth(1);
    }

    private Tile FindTileBehind(BaseUnit target)
    {
        Tile bestTile = null;
        bool isTargetFacingLeft = target.transform.localRotation.eulerAngles.y >= 90f;
        List<Vector2Int> neighbors = HexGridUtils.GetTilesInRange(target.currentTile.GridCoords, 1, BattleGameManager.Instance.allGridTiles);

        foreach (Vector2Int n in neighbors)
        {
            if (BattleGameManager.Instance.allGridTiles.TryGetValue(n, out Tile t) && !t.IsOccupied)
            {
                bool isBehind = isTargetFacingLeft ? (t.transform.position.x > target.transform.position.x) : (t.transform.position.x < target.transform.position.x);
                if (isBehind) { bestTile = t; break; }
            }
        }
        return bestTile;
    }
}