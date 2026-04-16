/// OBSERVER PATTERN (Interface)
public interface IUnitObserver
{
    /// This method is called whenever a Unit takes damage or gets healed.
    void OnHealthUpdated(BaseUnit unit, int currentHP, int maxHP);

    /// This method is called whenever a Unit uses mana or regens mana.
    void OnManaUpdated(BaseUnit unit, int currentMana, int maxMana);
}