using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonShakeHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public float shakeIntensity = 2f;
    private Vector3 originalPosition;
    private bool isHovering = false;
    private RectTransform rectTransform;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        originalPosition = rectTransform.anchoredPosition;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        rectTransform.anchoredPosition = originalPosition;
    }

    void Update()
    {
        if (isHovering)
        {
            Vector3 randomOffset = new Vector3(
                Random.Range(-shakeIntensity, shakeIntensity),
                Random.Range(-shakeIntensity, shakeIntensity),
                0f
            );
            rectTransform.anchoredPosition = originalPosition + randomOffset;
        }
    }
}