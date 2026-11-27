using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class DragAndDropShell : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;

    [Header("Настройки")]
    public BreechDoorController breechController; // Ссылка на задвижку
    public float floorY = 200f; // Высота пола для анимации падения

    [HideInInspector] public bool canBeLoaded = false; // Включит задвижка, когда откроется
    private bool isDropping = false;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isDropping) return;
        canvasGroup.blocksRaycasts = false;
        rectTransform.rotation = Quaternion.identity;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isDropping) return;
        rectTransform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isDropping) return;
        canvasGroup.blocksRaycasts = true;

        // --- ОТЛАДКА ---
        Debug.Log("Снаряд отпущен. Задвижка открыта (canBeLoaded)? " + canBeLoaded);
        // ----------------

        if (breechController != null && canBeLoaded)
        {
            Debug.Log("УСЛОВИЕ СРАБОТАЛО! Заряжаем снаряд.");
            gameObject.SetActive(false);
            breechController.LoadShell();
            return;
        }

        Debug.Log("УСЛОВИЕ НЕ СРАБОТАЛО. Снаряд падает.");
        StartCoroutine(DropAnimation());
    }

    IEnumerator DropAnimation()
    {
        isDropping = true;
        canvasGroup.blocksRaycasts = false;

        float shellHalfHeight = rectTransform.rect.height / 2f;
        float lyingY = floorY - shellHalfHeight;

        Vector3 startPos = rectTransform.position;
        Vector3 lyingPos = new Vector3(startPos.x, lyingY, startPos.z);

        float elapsed = 0f;
        float duration = 0.2f;

        while (elapsed < duration)
        {
            rectTransform.position = Vector3.Lerp(startPos, lyingPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        rectTransform.position = lyingPos;

        Vector3 jumpPos = lyingPos + new Vector3(0, 15f, 0);
        elapsed = 0f;
        duration = 0.1f;
        while (elapsed < duration)
        {
            rectTransform.position = Vector3.Lerp(lyingPos, jumpPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        elapsed = 0f;
        duration = 0.1f;
        while (elapsed < duration)
        {
            rectTransform.position = Vector3.Lerp(jumpPos, lyingPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        rectTransform.position = lyingPos;

        elapsed = 0f;
        duration = 0.2f;
        Quaternion startRot = rectTransform.rotation;
        Quaternion endRot = Quaternion.Euler(0, 0, -90f);

        while (elapsed < duration)
        {
            rectTransform.rotation = Quaternion.Slerp(startRot, endRot, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        rectTransform.rotation = endRot;

        canvasGroup.blocksRaycasts = true;
        isDropping = false;
    }
}