using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

public class SimpleHoverInfo : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Info Content")]
    [TextArea(2, 5)]
    public string infoText = "Description";

    [Header("Popup Settings")]
    public GameObject infoPrefab;
    public Vector3 offset = new Vector3(0f, 50f, 0f);
    public float hoverDelay = 1f; 

    private GameObject currentPopup;
    private Coroutine hoverCoroutine; 

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverCoroutine != null) StopCoroutine(hoverCoroutine);
        hoverCoroutine = StartCoroutine(ShowPopupRoutine());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (hoverCoroutine != null)
        {
            StopCoroutine(hoverCoroutine);
            hoverCoroutine = null;
        }
        DestroyPopup();
    }

    private void OnDisable()
    {
        if (hoverCoroutine != null)
        {
            StopCoroutine(hoverCoroutine);
            hoverCoroutine = null;
        }
        DestroyPopup();
    }

    private IEnumerator ShowPopupRoutine()
    {
        yield return new WaitForSeconds(hoverDelay);

        if (infoPrefab != null && currentPopup == null)
        {
            currentPopup = Instantiate(infoPrefab, transform.position + offset, Quaternion.identity);

            Canvas parentCanvas = GetComponentInParent<Canvas>();
            if (parentCanvas != null)
            {
                currentPopup.transform.SetParent(parentCanvas.transform, false);
                currentPopup.transform.position = transform.position + offset;
            }

            TextMeshProUGUI tmpUI = currentPopup.GetComponentInChildren<TextMeshProUGUI>();
            if (tmpUI != null)
            {
                tmpUI.text = infoText;
            }
            else
            {
                TextMeshPro tmpWorld = currentPopup.GetComponentInChildren<TextMeshPro>();
                if (tmpWorld != null) tmpWorld.text = infoText;
            }
        }
    }

    private void DestroyPopup()
    {
        if (currentPopup != null)
        {
            Destroy(currentPopup);
            currentPopup = null;
        }
    }
}