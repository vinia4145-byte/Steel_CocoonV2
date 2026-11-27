using UnityEngine;
using UnityEngine.EventSystems;

public class SteeringWheelController : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Настройки")]
    public float maxRotation = 90f; // Максимальный угол (от -90 до 90)
    public float returnSpeed = 5f; // Скорость возврата в центр

    private float currentRotation = 0f;
    private float lastMouseAngle = 0f;
    private bool isDragging = false;
    private RectTransform rectTransform;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        // Плавный возврат в центр, если руль отпущен
        if (!isDragging)
        {
            currentRotation = Mathf.Lerp(currentRotation, 0f, Time.deltaTime * returnSpeed);
            rectTransform.localRotation = Quaternion.Euler(0, 0, currentRotation);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;

        // Запоминаем угол мыши при захвате (как в вентилях)
        Vector2 centerScreenPos = RectTransformUtility.WorldToScreenPoint(eventData.pressEventCamera, transform.position);
        Vector2 mouseDir = eventData.position - centerScreenPos;
        lastMouseAngle = Mathf.Atan2(mouseDir.y, mouseDir.x) * Mathf.Rad2Deg;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Считаем угол мыши сейчас
        Vector2 centerScreenPos = RectTransformUtility.WorldToScreenPoint(eventData.pressEventCamera, transform.position);
        Vector2 mouseDir = eventData.position - centerScreenPos;
        float currentMouseAngle = Mathf.Atan2(mouseDir.y, mouseDir.x) * Mathf.Rad2Deg;

        // Разница углов (дельта)
        float angleDelta = Mathf.DeltaAngle(lastMouseAngle, currentMouseAngle);
        lastMouseAngle = currentMouseAngle;

        // Крутим руль
        currentRotation += angleDelta;
        currentRotation = Mathf.Clamp(currentRotation, -maxRotation, maxRotation);
        rectTransform.localRotation = Quaternion.Euler(0, 0, currentRotation);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
    }

    public float GetSteeringInput()
    {
        return currentRotation / maxRotation;
    }
}