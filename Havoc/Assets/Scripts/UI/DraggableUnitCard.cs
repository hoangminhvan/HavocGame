using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableUnitCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    public string unitID;
    public int unitCost;
    public GameObject unitPrefab;

    private GameObject worldGhostObj;

    public void OnPointerClick(PointerEventData eventData)
    {
        // Truyền thêm Prefab để GameManager tạo bóng mờ
        DraftingGameManager.Instance.SelectUnitFromShop(unitID, unitCost, unitPrefab);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Hủy bóng mờ của Click (nếu có) để không bị trùng khi đang kéo
        DraftingGameManager.Instance.CancelSelection();

        worldGhostObj = new GameObject("WorldDragGhost");
        SpriteRenderer ghostSr = worldGhostObj.AddComponent<SpriteRenderer>();

        if (unitPrefab != null)
        {
            SpriteRenderer prefabSr = unitPrefab.GetComponentInChildren<SpriteRenderer>();
            if (prefabSr != null)
            {
                ghostSr.sprite = prefabSr.sprite;
                worldGhostObj.transform.localScale = unitPrefab.transform.localScale;
            }
        }

        ghostSr.color = new Color(1f, 1f, 1f, 0.7f);
        ghostSr.sortingOrder = 100;
        UpdateGhostPosition();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (worldGhostObj != null) UpdateGhostPosition();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (worldGhostObj != null) Destroy(worldGhostObj);
        TryPlaceUnit();
    }

    private void UpdateGhostPosition()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;
        if (worldGhostObj != null) worldGhostObj.transform.position = mouseWorldPos;
    }

    private void TryPlaceUnit()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mousePos2D = new Vector2(mouseWorldPos.x, mouseWorldPos.y);

        RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);
        if (hit.collider != null)
        {
            Tile tile = hit.collider.GetComponent<Tile>();
            if (tile != null)
            {
                DraftingGameManager.Instance.TryPlaceUnit(tile, unitID, unitCost);
            }
        }
    }
}