using System.Collections.Generic;
using UnityEngine;


/// OBSERVERS PATTERN (Subject): Manages all observers and notifies them when a unit's stats change.
public class UnitObserverManager : MonoBehaviour
{
    public static UnitObserverManager Instance { get; private set; }

    private readonly List<IUnitObserver> observers = new List<IUnitObserver>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// Registers a new observer if it has not already been added.
    public void AddObserver(IUnitObserver observer)
    {
        if (!observers.Contains(observer))
        {
            observers.Add(observer);
        }
    }

    /// Removes an existing observer from the list.
    public void RemoveObserver(IUnitObserver observer)
    {
        if (observers.Contains(observer))
        {
            observers.Remove(observer);
        }
    }

    /// Notifies all observers when a unit's health changes.
    public void NotifyHealthChanged(BaseUnit unit)
    {
        foreach (var observer in observers)
        {
            observer.OnHealthUpdated(unit, unit.currentHP, unit.maxHP);
        }
    }

    /// Notifies all observers when a unit's mana changes.
    public void NotifyManaChanged(BaseUnit unit)
    {
        foreach (var observer in observers)
        {
            observer.OnManaUpdated(unit, unit.currentMana, unit.maxMana);
        }
    }
}