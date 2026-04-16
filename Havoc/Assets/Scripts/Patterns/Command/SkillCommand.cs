using System.Collections.Generic;
using UnityEngine;

// PATTERN: Concrete Command
// Xu ly cac ky nang phuc tap va su dung "Snapshot" de khoi phuc trang thai chinh xac khi Undo
public class SkillCommand : ICommand
{
    private readonly BaseUnit caster;
    private readonly Tile targetTile;
    private readonly BaseUnit targetUnit;

    // Lop noi bo de luu tru "anh chup" trang thai cua tung Unit
    private class UnitSnapshot
    {
        public BaseUnit Unit { get; set; }
        public int Hp { get; set; }
        public int Mana { get; set; }
        public int StunTurns { get; set; }
        public int CurseTurns { get; set; }
        public int BuffTurns { get; set; }
        public bool HasShield { get; set; }
        public bool IsActive { get; set; }
        public Tile Tile { get; set; }
    }

    private readonly List<UnitSnapshot> snapshots = new List<UnitSnapshot>();

    public SkillCommand(BaseUnit caster, Tile targetTile, BaseUnit targetUnit)
    {
        this.caster = caster;
        this.targetTile = targetTile;
        this.targetUnit = targetUnit;

        // Chup lai trang thai cua tat ca Unit tren ban co truoc khi thi trien ky nang
        BaseUnit[] allUnits = Object.FindObjectsByType<BaseUnit>(FindObjectsSortMode.None);
        foreach (var u in allUnits)
        {
            int bTurns = (u is Berserker b) ? b.buffTurns : 0;
            snapshots.Add(new UnitSnapshot
            {
                Unit = u,
                Hp = u.currentHP,
                Mana = u.currentMana,
                StunTurns = u.stunTurns,
                CurseTurns = u.curseTurns,
                BuffTurns = bTurns,
                HasShield = u.hasShield,
                IsActive = u.gameObject.activeInHierarchy,
                Tile = u.currentTile
            });
        }
    }

    public void Execute()
    {
        caster.UseSkill(targetTile, targetUnit);
        TurnHandler.Instance.SpendEnergy(BattleGameManager.SKILL_ENERGY);
        TurnHandler.Instance.UpdateTurnUI();
    }

    public void Undo()
    {
        // Duyet qua danh sach snapshots de khoi phuc tung Unit ve trang thai truoc khi trung chieu
        foreach (var snap in snapshots)
        {
            BaseUnit u = snap.Unit;

            // Neu Unit da bi tieu diet, hoi sinh lai Unit do
            if (snap.IsActive && !u.gameObject.activeInHierarchy)
            {
                u.gameObject.SetActive(true);
                if (snap.Tile != null) snap.Tile.OccupiedUnit = u.gameObject;
                u.ChangeAnimation(u.animIdle);
                GameData.Instance.UnitRevived(u.ownerPlayer);
            }

            // Cap nhat lai cac chi so HP, Mana, Hieu ung xau tu Snapshot
            u.currentHP = snap.Hp;
            u.currentMana = snap.Mana;
            u.stunTurns = snap.StunTurns;
            u.curseTurns = snap.CurseTurns;
            if (u is Berserker b) b.buffTurns = snap.BuffTurns;

            u.hasShield = snap.HasShield;
            if (!u.hasShield) u.RemoveShield();

            // Bao tin cho UI cap nhat lai thanh mau/mana
            u.RefreshTileEffects();
            u.NotifyHealthChanged();
            u.NotifyManaChanged();
        }

        TurnHandler.Instance.currentEnergy += BattleGameManager.SKILL_ENERGY;
        TurnHandler.Instance.UpdateTurnUI();
        BattleGameManager.Instance.DeselectActiveUnit();
    }
}