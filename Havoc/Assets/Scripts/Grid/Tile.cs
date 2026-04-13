using UnityEngine;

public enum TileZone
{
    Player1,
    Player2,
    Boundary
}
public enum ElementalType { None, Heal, Damage, Stealth }
public class Tile : MonoBehaviour
{
    public Vector2Int GridCoords;
    public GameObject OccupiedUnit;
    public bool IsOccupied => OccupiedUnit != null;

    [Header("Drafting Phase")]
    public TileZone zone;
    public GameObject _highlightObj;
    public GameObject _boundaryVisual;

    [Header("Battle Phase Highlights")]
    public GameObject highlightMove;
    public GameObject highlightAttack;
    public GameObject highlightSupport;

    [Header("Effects")]
    public GameObject explosionEffect;
    public GameObject stunEffect;
    public GameObject curseEffect;
    public GameObject buffEffect;

    [Header("Elemental Visuals")]
    public GameObject healVisual;
    public GameObject damageVisual;
    public GameObject stealthVisual;

    public ElementalType currentElementalType = ElementalType.None;
    public int elementalDuration = 0;
    private void Start()
    {
        if (_highlightObj != null) _highlightObj.SetActive(false);
        if (_boundaryVisual != null) _boundaryVisual.SetActive(zone == TileZone.Boundary);
        ClearAllHighlights();
    }

    private void OnMouseEnter()
    {
        if (_highlightObj != null) _highlightObj.SetActive(true);
    }

    private void OnMouseExit()
    {
        if (_highlightObj != null) _highlightObj.SetActive(false);
    }

    private void OnMouseDown()
    {
        if (DraftingGameManager.Instance != null)
        {
            if (!string.IsNullOrEmpty(DraftingGameManager.Instance.selectedUnitID))
            {
                DraftingGameManager.Instance.TryPlaceUnit(
                    this,
                    DraftingGameManager.Instance.selectedUnitID,
                    DraftingGameManager.Instance.selectedUnitCost
                );
            }
        }
    }

    public void SetElementalEffect(ElementalType type, int duration)
    {
        // Apply an elemental effect to the tile and show its visual
        ClearElementalEffect();
        currentElementalType = type;
        elementalDuration = duration;

        if (type == ElementalType.Heal && healVisual != null) healVisual.SetActive(true);
        else if (type == ElementalType.Damage && damageVisual != null) damageVisual.SetActive(true);
        else if (type == ElementalType.Stealth && stealthVisual != null) stealthVisual.SetActive(true);
    }

    public void ClearElementalEffect()
    {
        currentElementalType = ElementalType.None;
        elementalDuration = 0;
        if (healVisual != null) healVisual.SetActive(false);
        if (damageVisual != null) damageVisual.SetActive(false);
        if (stealthVisual != null) stealthVisual.SetActive(false);
    }
    public void PlayExplosion()
    {
        if (explosionEffect != null)
        {
            explosionEffect.SetActive(true);
            ParticleSystem ps = explosionEffect.GetComponent<ParticleSystem>();
            if (ps != null) ps.Play(); 
        }
    }
    public void SetStunEffect(bool isActive)
    {
        if (stunEffect != null) stunEffect.SetActive(isActive);
    }
    public GameObject ActivateHiddenUnit(string unitID)
    {
        Transform[] allChildren = GetComponentsInChildren<Transform>(true);
        foreach (Transform child in allChildren)
        {
            if (child == transform) continue;

            if (child.name.Contains(unitID))
            {
                child.gameObject.SetActive(true);
                OccupiedUnit = child.gameObject;
                return child.gameObject;
            }
        }
        return null;
    }
    public void SetCurseEffect(bool isActive)
    {
        if (curseEffect != null) curseEffect.SetActive(isActive);
    }
    public void ClearUnit()
    {
        if (OccupiedUnit != null)
        {
            OccupiedUnit.SetActive(false);
            OccupiedUnit = null;
        }
    }
    public void SetBuffEffect(bool isActive)
    {
        if (buffEffect != null) buffEffect.SetActive(isActive);
    }

    public void ClearAllHighlights()
    {
        if (highlightMove != null) highlightMove.SetActive(false);
        if (highlightAttack != null) highlightAttack.SetActive(false);
        if (highlightSupport != null) highlightSupport.SetActive(false);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        UnityEditor.Handles.Label(transform.position, $"{GridCoords.x},{GridCoords.y}");
    }
#endif
}