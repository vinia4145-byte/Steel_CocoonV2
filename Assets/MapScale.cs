using UnityEngine;
using UnityEngine.EventSystems;

public class ScaleObject : MonoBehaviour, IPointerClickHandler
{
    [Header("Настройки масштаба")]
    public float zoomMultiplier = 1.4f;

    [Header("Настройки сдвига при увеличении")]
    public Vector2 zoomOffset = new Vector2(50f, -50f);

    private Vector3 originalScale;
    private Vector2 originalPosition;
    private bool isZoomed = false;
    private RectTransform rectTransform;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        originalScale = transform.localScale;
        originalPosition = rectTransform.anchoredPosition;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // ПРОВЕРКА: если нажата НЕ левая кнопка (например, правая) - выходим
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        if (!isZoomed)
        {
            transform.localScale = originalScale * zoomMultiplier;
            rectTransform.anchoredPosition = originalPosition + zoomOffset;
            isZoomed = true;
        }
        else
        {
            transform.localScale = originalScale;
            rectTransform.anchoredPosition = originalPosition;
            isZoomed = false;
        }
    }
}