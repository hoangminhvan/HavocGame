using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// TEMPLATE METHOD PATTERN 
/// This is the main class for all game units (Warrior, Mage, ...)

public abstract class BaseUnit : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string unitID;
    public string unitName;
    public string skillDescription;

    /// STATE PATTERN: This variable holds the current behavior of the unit.
    /// It allows the unit to change how it reacts to clicks depending on its current state.
    public IUnitState currentState;
    public Tile currentTile;
    public int ownerPlayer;

    [Header("Unit stats")]
    public int maxHP;
    public int currentHP;
    public int maxMana;
    public int currentMana;
    public int damage;
    public int defense;
    public int moveRange;
    public int attackRange;
    public int manaRegen;
    public float criticalChance;
    public int skillManaCost;
    public bool isOffensiveSkill = false;
    public bool isInstantSkill = false;

    public virtual int DisplayDamage => damage;
    public virtual float DisplayCrit => criticalChance;

    public bool hasShield = false;
    public Slider hpSlider;
    public Slider manaSlider;
    public GameObject selectionOutline;
    public GameObject p1OutlineObj;
    public GameObject p2OutlineObj;
    public GameObject shieldVisual;

    public float moveSpeed = 15f;
    private Vector3 initialLocalPos;

    // Animation System For Unit
    public Animator animator;
    public RuntimeAnimatorController animIdle;
    public RuntimeAnimatorController animWalk;
    public RuntimeAnimatorController animAttack;
    public RuntimeAnimatorController animHit;
    public RuntimeAnimatorController animDead;
    public RuntimeAnimatorController animSkill;
    public int stunTurns = 0;

    public int curseTurns = 0;
    public BaseUnit curseCaster;
    public int curseAppliedTurn = 0;
    public int curseCasterPlayer = 0;
    public int stealthTurns = 0;

    [Header("Popups")]
    public GameObject damagePopupPrefab;
    public Transform popupSpawnPoint;

    [Header("Audio System")]
    public AudioSource unitAudioSource;
    public AudioClip attackSFX;
    public AudioClip skillSFX;
    public AudioClip hitSFX;

    /// Setup basic data and initial state when the unit is first created
    protected virtual void Awake()
    {
        InitStats();
        currentHP = maxHP;
        currentMana = maxMana;
        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHP;
            hpSlider.value = 0; 
        }
        if (manaSlider != null)
        {
            manaSlider.maxValue = maxMana;
            manaSlider.value = 0;
        }
        initialLocalPos = transform.localPosition;
        SetSelected(false);

        NotifyHealthChanged();
        NotifyManaChanged();

        ChangeAnimation(animIdle);

        ChangeState(new UnitIdleState());
    }

    /// TEMPLATE METHOD: An abstract method that specific units must implement 
    protected abstract void InitStats();
    public abstract void UseSkill(Tile targetTile, BaseUnit targetUnit = null);

    protected void PlaySound(AudioClip clip)
    {
        if (unitAudioSource != null && clip != null)
        {
            unitAudioSource.PlayOneShot(clip);
        }
    }

    /// OBSERVER PATTERN: Notifies the UnitObserverManager that health has changed so the UI can update its health bars
    public void NotifyHealthChanged()
    {
        if (hpSlider != null) hpSlider.value = maxHP - currentHP;

        if (UnitObserverManager.Instance != null)
        {
            UnitObserverManager.Instance.NotifyHealthChanged(this);
        }
    }

    /// OBSERVER PATTERN: Notifies the system to update mana-related visuals.
    public void NotifyManaChanged()
    {
        if (manaSlider != null) manaSlider.value = maxMana - currentMana;

        if (UnitObserverManager.Instance != null)
        {
            UnitObserverManager.Instance.NotifyManaChanged(this);
        }
    }

    public void ShowTextPopup(string text, Color color, bool isStatus = false)
    {
        if (damagePopupPrefab != null)
        {
            Vector3 spawnPos = popupSpawnPoint != null ? popupSpawnPoint.position : transform.position + new Vector3(0, 0.5f, -1f);
            spawnPos.z = -1f;

            GameObject popup = Instantiate(damagePopupPrefab, spawnPos, Quaternion.identity);
            DamagePopup popupScript = popup.GetComponent<DamagePopup>();
            if (popupScript != null) popupScript.Setup(text, color, isStatus);
        }
    }


    /// Updates the team color outline based on the owner player.
    public void SetupTeamAppearance()
    {
        if (p1OutlineObj != null) p1OutlineObj.SetActive(ownerPlayer == 1);
        if (p2OutlineObj != null) p2OutlineObj.SetActive(ownerPlayer == 2);
    }

    /// STATE PATTERN: Logic to switch between different unit behaviors.
    public void ChangeState(IUnitState newState)
    {
        if (currentState != null) currentState.Exit(this);
        currentState = newState;
        currentState.Enter(this);
    }

    public void SetSelected(bool isSelected)
    {
        if (selectionOutline != null) selectionOutline.SetActive(isSelected);
    }

    /// Changes the current Animator controller to trigger different animations.
    public void ChangeAnimation(RuntimeAnimatorController newAnim)
    {
        if (animator != null && newAnim != null && animator.runtimeAnimatorController != newAnim)
            animator.runtimeAnimatorController = newAnim;
    }

    /// Plays an animation and automatically reverts to Idle after a delay.
    public IEnumerator PlayTemporaryAnimation(RuntimeAnimatorController tempAnim, float duration)
    {
        ChangeAnimation(tempAnim);
        yield return new WaitForSeconds(duration);
        if (currentHP > 0) ChangeAnimation(animIdle);
    }

    /// Applies a curse effect that causes damage over time.
    public void ApplyCurse(int turns, BaseUnit caster)
    {
        curseTurns = turns;
        curseCaster = caster;
        curseCasterPlayer = caster != null ? caster.ownerPlayer : 0;
        curseAppliedTurn = TurnHandler.Instance.currentTurn;
        ShowTextPopup("Cursed!", new Color(0.6f, 0f, 0f), true);

        if (currentTile != null) currentTile.SetCurseEffect(true);
        NotifyHealthChanged();
    }

    /// Removes the curse and cleans up tile visuals.
    public void ClearCurse()
    {
        curseTurns = 0;
        curseCaster = null;
        if (currentTile != null) currentTile.SetCurseEffect(false);
        NotifyHealthChanged();
    }

    /// Makes the unit transparent and untargetable for a set duration.
    public void ApplyStealth(int turns)
    {
        stealthTurns = turns;
        ShowTextPopup("Stealth!", Color.gray, true);
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null) { Color c = sr.color; c.a = 0.4f; sr.color = c; }
    }

    /// Removes stealth and restores full visibility
    public void ClearStealth()
    {
        stealthTurns = 0;
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        if (sr != null) { Color c = sr.color; c.a = 1f; sr.color = c; }
    }

    /// Initiates movement to a target tile
    public void Move(Tile targetTile)
    {
        StartCoroutine(SmoothMoveRoutine(targetTile));
    }

    /// Handles frame-by-frame movement and logic when landing on a new tile
    private IEnumerator SmoothMoveRoutine(Tile targetTile)
    {
        ChangeAnimation(animWalk);
        ClearTileEffects();
        if (currentTile != null) currentTile.OccupiedUnit = null;

        Vector3 targetWorldPos = targetTile.transform.TransformPoint(initialLocalPos);
        while (Vector3.Distance(transform.position, targetWorldPos) > 0.01f)
        {
            float direction = targetWorldPos.x - transform.position.x;
            if (Mathf.Abs(direction) > 0.01f)
            {
                transform.localRotation = Quaternion.Euler(0, direction < 0 ? 180 : 0, 0);
            }

            transform.position = Vector3.MoveTowards(transform.position, targetWorldPos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.SetParent(targetTile.transform);
        transform.localPosition = initialLocalPos;

        currentTile = targetTile;
        targetTile.OccupiedUnit = this.gameObject;

        if (targetTile.currentElementalType == ElementalType.Heal)
        {
            Heal(20);
            targetTile.ClearElementalEffect();
        }
        else if (targetTile.currentElementalType == ElementalType.Stealth)
        {
            ApplyStealth(1);
            targetTile.ClearElementalEffect();
        }
        else if (targetTile.currentElementalType == ElementalType.Damage)
        {
            TakeDamage(10);
        }
        FaceClosestEnemy();

        RefreshTileEffects();
        ChangeAnimation(animIdle);
        ChangeState(new UnitIdleState());
    }

    /// Rotates the unit to face the nearest enemy
    public void FaceClosestEnemy()
    {
        BaseUnit[] allUnits = FindObjectsByType<BaseUnit>(FindObjectsSortMode.None);
        BaseUnit closestEnemy = null;
        float minDist = Mathf.Infinity;

        foreach (BaseUnit u in allUnits)
        {
            if (u.ownerPlayer != this.ownerPlayer && u.currentHP > 0)
            {
                float dist = Vector3.Distance(transform.position, u.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    closestEnemy = u;
                }
            }
        }
        if (closestEnemy != null)
        {
            float dirX = closestEnemy.transform.position.x - transform.position.x;
            transform.localRotation = Quaternion.Euler(0, dirX < 0 ? 180 : 0, 0);
        }
    }

    public void OnPointerEnter(PointerEventData eventData) { }
    public void OnPointerExit(PointerEventData eventData) { }

    /// Reduces HP while considering active shields.
    public void TakeDamage(int amount)
    {
        if (currentHP <= 0) return;
        if (hasShield) amount = Mathf.RoundToInt(amount * 0.7f);
        currentHP = Mathf.Clamp(currentHP - amount, 0, maxHP);
        NotifyHealthChanged();
        ShowTextPopup("-" + amount, Color.red);
        PlaySound(hitSFX);

        if (currentHP <= 0) Die();
        else StartCoroutine(PlayTemporaryAnimation(animHit, 0.7f));
    }

    /// Restores health to the unit
    public void Heal(int amount)
    {
        currentHP = Mathf.Clamp(currentHP + amount, 0, maxHP);
        NotifyHealthChanged();
        ShowTextPopup("+" + amount, Color.green);
    }

    /// Deducts mana when skills are used
    public void UseMana(int amount)
    {
        currentMana = Mathf.Clamp(currentMana - amount, 0, maxMana);
        NotifyManaChanged();
        ShowTextPopup("-" + amount, Color.blue);
    }

    /// Passively restores mana
    public void RegenMana()
    {
        currentMana = Mathf.Clamp(currentMana + manaRegen, 0, maxMana);

        NotifyManaChanged();

    }

    /// Restores mana via items or effects.
    public void RestoreMana(int amount)
    {
        currentMana = Mathf.Clamp(currentMana + amount, 0, maxMana);
        NotifyManaChanged();
        ShowTextPopup("+" + amount + " Mana", Color.blue);
    }

    /// Disables the shield visual and effect
    public void RemoveShield()
    {
        hasShield = false;
        if (shieldVisual != null) shieldVisual.SetActive(false);
    }

    /// Calculates final damage and critical hit chance
    public virtual int GetFinalDamage(out bool isCrit)
    {
        isCrit = false;
        float finalDamage = damage;

        if (UnityEngine.Random.value <= criticalChance)
        {
            isCrit = true;
            finalDamage *= 2f;
        }

        return Mathf.RoundToInt(finalDamage);
    }

    /// Standard attack logic including armor calculation
    public virtual void BasicAttack(BaseUnit targetUnit)
    {
        PlaySound(attackSFX);
        StartCoroutine(PlayTemporaryAnimation(animAttack, 1f));

        bool isCrit;
        int rawDamage = GetFinalDamage(out isCrit);

        float damageMultiplier = 100f / (100f + targetUnit.defense);
        int finalDamage = Mathf.Max(1, Mathf.RoundToInt(rawDamage * damageMultiplier));

        if (isCrit)
        {
            ShowTextPopup("CRIT!", Color.yellow, true);
        }

        targetUnit.TakeDamage(finalDamage);
    }

    public virtual void ClearTileEffects()
    {
        if (currentTile != null)
        {
            currentTile.SetStunEffect(false);
            currentTile.SetCurseEffect(false);
        }
    }

    public virtual void RefreshTileEffects()
    {
        if (currentTile != null)
        {
            currentTile.SetStunEffect(stunTurns > 0);
            currentTile.SetCurseEffect(curseTurns > 0);
        }
    }

    /// Triggers the death animation
    protected virtual void Die()
    {
        ChangeAnimation(animDead);
        StartCoroutine(DieRoutine());
    }

    /// Final cleanup of the unit from the grid after death
    private IEnumerator DieRoutine()
    {
        yield return new WaitForSeconds(1f);
        ClearTileEffects();

        if (currentTile != null)
        {
            currentTile.OccupiedUnit = null;
        }

        if (BattleGameManager.Instance.activeUnit == this)
        {
            BattleGameManager.Instance.DeselectActiveUnit();
        }

        gameObject.SetActive(false);
        GameData.Instance.UnitDied(ownerPlayer);
    }

    /// Inflicts stun status, preventing actions
    public void ApplyStun(int turns)
    {
        stunTurns = turns;
        ShowTextPopup("Stunned!", Color.yellow, true);
        if (currentTile != null) currentTile.SetStunEffect(true);
    }

    /// Clears the stun status
    public void ClearStun()
    {
        stunTurns = 0;
        if (currentTile != null) currentTile.SetStunEffect(false);
    }


}