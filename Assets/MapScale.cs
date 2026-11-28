using UnityEngine;
using UnityEngine.EventSystems;

public class ScaleObject : MonoBehaviour, IPointerClickHandler
{
    [Header("Настройки масштаба")]
    public float zoomMultiplier = 2f; // было 1.4 — теперь зум заметно сильнее

    [Header("Настройки сдвига при увеличении")]
    // Сдвиг задаётся для БАЗОВОГО масштаба, а при зуме умножается на кратность
    public Vector2 zoomOffset = new Vector2(60f, -60f);

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
            // Сдвиг пропорционален зуму: чем сильнее приблизили, тем дальше уехали
            rectTransform.anchoredPosition = originalPosition + zoomOffset * zoomMultiplier;
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
