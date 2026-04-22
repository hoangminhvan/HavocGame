using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AIBattleManager : MonoBehaviour
{
    public static AIBattleManager Instance { get; private set; }

    private BaseUnit currentActiveUnit = null;
    
    private List<BaseUnit> exhaustedUnits = new List<BaseUnit>();

    private void Awake()
    {
        Instance = this;
    }

    public void OnTurnStarted(int playerTurn)
    {
        if (GameData.Instance != null && GameData.Instance.isPvEMode && playerTurn == 2)
        {
            currentActiveUnit = null;
            exhaustedUnits.Clear();
            StartCoroutine(AILogicRoutine());
        }
    }

    private IEnumerator AILogicRoutine()
    {
        yield return new WaitForSeconds(1.5f); 

        while (TurnHandler.Instance.currentPlayerTurn == 2 && TurnHandler.Instance.currentEnergy >= 3)
        {
            AIAction nextAction = DetermineNextAction();

            if (nextAction == null)
            {
                TurnHandler.Instance.EndTurn();
                break;
            }

            ExecuteAction(nextAction);

            yield return new WaitForSeconds(2.0f);
        }
    }

    private class AIAction
    {
        public BaseUnit Actor;
        public enum Type { Move, Attack, Skill }
        public Type ActionType;
        public Tile TargetTile;
        public BaseUnit TargetUnit;
    }

    private AIAction DetermineNextAction()
    {
        BaseUnit[] allUnits = FindObjectsByType<BaseUnit>(FindObjectsSortMode.None);
        List<BaseUnit> aiUnits = allUnits.Where(u => u.ownerPlayer == 2 && u.currentHP > 0 && u.stunTurns <= 0).ToList();
        List<BaseUnit> playerUnits = allUnits.Where(u => u.ownerPlayer == 1 && u.currentHP > 0 && u.stealthTurns <= 0).ToList();

        if (aiUnits.Count == 0 || playerUnits.Count == 0) return null;

        BaseUnit healer = aiUnits.FirstOrDefault(u => u is Healer);
        if (healer != null && !exhaustedUnits.Contains(healer))
        {
            BaseUnit dyingAlly = aiUnits.FirstOrDefault(u => u.currentHP <= u.maxHP * 0.4f);
            if (dyingAlly != null)
            {
                currentActiveUnit = healer; 
                int dist = Mathf.RoundToInt(Vector2Int.Distance(healer.currentTile.GridCoords, dyingAlly.currentTile.GridCoords));
                
                if (dist <= healer.attackRange && TurnHandler.Instance.currentEnergy >= BattleGameManager.SKILL_ENERGY && healer.currentMana >= healer.skillManaCost)
                {
                    return new AIAction { Actor = healer, ActionType = AIAction.Type.Skill, TargetTile = dyingAlly.currentTile, TargetUnit = dyingAlly };
                }
                else if (dist > healer.attackRange && TurnHandler.Instance.currentEnergy >= BattleGameManager.MOVE_ENERGY)
                {
                    Tile moveTile = FindTileClosestToTarget(healer, dyingAlly.currentTile);
                    if (moveTile != null) return new AIAction { Actor = healer, ActionType = AIAction.Type.Move, TargetTile = moveTile };
                }
            }
        }

        foreach (BaseUnit ai in aiUnits)
        {
            if (ai.currentHP <= ai.maxHP * 0.3f && TurnHandler.Instance.currentEnergy >= BattleGameManager.MOVE_ENERGY)
            {
                BaseUnit closestEnemy = GetClosestUnit(ai, playerUnits);
                if (closestEnemy != null && Vector2Int.Distance(ai.currentTile.GridCoords, closestEnemy.currentTile.GridCoords) <= 3)
                {
                    currentActiveUnit = ai;
                    Tile fleeTile = FindTileFurthestFromTarget(ai, closestEnemy.currentTile);
                    if (fleeTile != null && fleeTile != ai.currentTile)
                    {
                        return new AIAction { Actor = ai, ActionType = AIAction.Type.Move, TargetTile = fleeTile };
                    }
                }
            }
        }

        if (currentActiveUnit == null || exhaustedUnits.Contains(currentActiveUnit))
        {
            List<BaseUnit> availableAttackers = aiUnits.Where(u => !(u is Healer) && !exhaustedUnits.Contains(u)).ToList();
            if (availableAttackers.Count > 0)
            {
                currentActiveUnit = availableAttackers[Random.Range(0, availableAttackers.Count)];
            }
            else
            {
                currentActiveUnit = aiUnits.FirstOrDefault(u => !exhaustedUnits.Contains(u)); 
                if (currentActiveUnit == null) return null; 
            }
        }

        BaseUnit targetEnemy = GetClosestUnit(currentActiveUnit, playerUnits);
        if (targetEnemy == null) 
        {
            exhaustedUnits.Add(currentActiveUnit);
            return DetermineNextAction(); 
        }

        int distanceToEnemy = Mathf.RoundToInt(Vector2Int.Distance(currentActiveUnit.currentTile.GridCoords, targetEnemy.currentTile.GridCoords));

        if (distanceToEnemy <= currentActiveUnit.attackRange)
        {
            if (TurnHandler.Instance.currentEnergy >= BattleGameManager.SKILL_ENERGY && currentActiveUnit.currentMana >= currentActiveUnit.skillManaCost)
            {
                if (currentActiveUnit.isInstantSkill)
                {
                    return new AIAction { Actor = currentActiveUnit, ActionType = AIAction.Type.Skill, TargetTile = currentActiveUnit.currentTile, TargetUnit = currentActiveUnit };
                }
                else
                {
                    return new AIAction { Actor = currentActiveUnit, ActionType = AIAction.Type.Skill, TargetTile = targetEnemy.currentTile, TargetUnit = targetEnemy };
                }
            }

            if (TurnHandler.Instance.currentEnergy >= BattleGameManager.ATTACK_ENERGY)
            {
                return new AIAction { Actor = currentActiveUnit, ActionType = AIAction.Type.Attack, TargetUnit = targetEnemy };
            }

            exhaustedUnits.Add(currentActiveUnit);
            return DetermineNextAction(); 
        }

        if (TurnHandler.Instance.currentEnergy >= BattleGameManager.MOVE_ENERGY)
        {
            Tile moveTile = FindTileClosestToTarget(currentActiveUnit, targetEnemy.currentTile);
            if (moveTile != null && moveTile != currentActiveUnit.currentTile)
            {
                return new AIAction { Actor = currentActiveUnit, ActionType = AIAction.Type.Move, TargetTile = moveTile };
            }
        }

        exhaustedUnits.Add(currentActiveUnit);
        return DetermineNextAction(); 
    }

    private void ExecuteAction(AIAction action)
    {
        if (action.TargetUnit != null && action.TargetUnit != action.Actor)
        {
            float dirX = action.TargetUnit.transform.position.x - action.Actor.transform.position.x;
            action.Actor.transform.localRotation = Quaternion.Euler(0, dirX < 0 ? 180 : 0, 0);
        }

        if (action.ActionType == AIAction.Type.Attack)
        {
            ActionHandler.Instance.ProcessAttack(action.Actor, action.TargetUnit);
        }
        else if (action.ActionType == AIAction.Type.Skill)
        {
            ActionHandler.Instance.ProcessSkill(action.Actor, action.TargetTile, action.TargetUnit);
        }
        else if (action.ActionType == AIAction.Type.Move)
        {
            ActionHandler.Instance.ProcessMove(action.Actor, action.Actor.currentTile, action.TargetTile);
        }
    }

    private BaseUnit GetClosestUnit(BaseUnit fromUnit, List<BaseUnit> targets)
    {
        BaseUnit closest = null;
        float minDist = float.MaxValue;

        foreach (var t in targets)
        {
            float dist = Vector3.Distance(fromUnit.transform.position, t.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = t;
            }
        }
        return closest;
    }

    private Tile FindTileClosestToTarget(BaseUnit unit, Tile targetTile)
    {
        List<Vector2Int> reachable = HexGridUtils.GetMovementRange(unit.currentTile.GridCoords, unit.moveRange, BattleGameManager.Instance.allGridTiles);
        
        Tile bestTile = null;
        float closestDist = Vector3.Distance(unit.transform.position, targetTile.transform.position); 

        foreach (Vector2Int coord in reachable)
        {
            if (BattleGameManager.Instance.allGridTiles.TryGetValue(coord, out Tile t) && !t.IsOccupied)
            {
                float dist = Vector3.Distance(t.transform.position, targetTile.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    bestTile = t;
                }
            }
        }
        return bestTile;
    }

    private Tile FindTileFurthestFromTarget(BaseUnit unit, Tile targetTile)
    {
        List<Vector2Int> reachable = HexGridUtils.GetMovementRange(unit.currentTile.GridCoords, unit.moveRange, BattleGameManager.Instance.allGridTiles);
        
        Tile bestTile = null;
        float furthestDist = 0f;

        foreach (Vector2Int coord in reachable)
        {
            if (BattleGameManager.Instance.allGridTiles.TryGetValue(coord, out Tile t) && !t.IsOccupied)
            {
                float dist = Vector3.Distance(t.transform.position, targetTile.transform.position);
                if (dist > furthestDist)
                {
                    furthestDist = dist;
                    bestTile = t;
                }
            }
        }
        return bestTile;
    }
}