using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class ValveController : MonoBehaviour, IDragHandler, IBeginDragHandler
{
    [Header("Настройки")]
    public float valueSpeed = 5f;
    public bool isAzimuthValve = true;

    [Header("Табло и Отладка")]
    public TextMeshProUGUI[] digits = new TextMeshProUGUI[4];
    public TextMeshProUGUI debugResultText;

    private float currentAngle = 0f;
    private float currentValueFloat = 0f;
    private int currentValue = 0;

    private float lastMouseAngle = 0f;
    private bool isDragging = false;

    private BallisticCalculator calc;

    void Start()
    {
        calc = FindObjectOfType<BallisticCalculator>();
    }

    void Update()
    {
        if (debugResultText != null)
        {
            bool isOk = IsCorrect();
            debugResultText.text = isOk ? "<color=green>OK</color>" : "<color=red>Не совпало</color>";
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        Vector2 centerScreenPos = RectTransformUtility.WorldToScreenPoint(eventData.pressEventCamera, transform.position);
        Vector2 mouseDir = eventData.position - centerScreenPos;
        lastMouseAngle = Mathf.Atan2(mouseDir.y, mouseDir.x) * Mathf.Rad2Deg;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        // 1. КРУТИМ ВЕНТИЛЬ (как в том красивом коде)
        Vector2 centerScreenPos = RectTransformUtility.WorldToScreenPoint(eventData.pressEventCamera, transform.position);
        Vector2 mouseDir = eventData.position - centerScreenPos;
        float currentMouseAngle = Mathf.Atan2(mouseDir.y, mouseDir.x) * Mathf.Rad2Deg;
        float angleDelta = Mathf.DeltaAngle(lastMouseAngle, currentMouseAngle);
        lastMouseAngle = currentMouseAngle;

        currentAngle += angleDelta;
        transform.localRotation = Quaternion.Euler(0, 0, currentAngle);

        // 2. МЕНЯЕМ ЦИФРЫ
        currentValueFloat += angleDelta * valueSpeed;
        currentValueFloat = Mathf.Clamp(currentValueFloat, 0f, 9999f);

        currentValue = Mathf.RoundToInt(currentValueFloat);
        UpdateDisplay(currentValue);
    }

    void UpdateDisplay(int value)
    {
        string valueString = value.ToString("D4");
        for (int i = 0; i < digits.Length; i++)
        {
            if (digits[i] != null) digits[i].text = valueString[i].ToString();
        }
    }

    public bool IsCorrect()
    {
        if (calc == null) return false;

        // Пока мишени нет — совпадение невозможно в принципе
        if (!calc.hasTarget) return false;

        // Берем абсолютное значение азимута, чтобы на табло не было минусов
        if (isAzimuthValve)
        {
            int targetAzi = Mathf.RoundToInt(Mathf.Abs(calc.targetAzimuth)); // Модуль числа!
            return Mathf.Abs(currentValue - targetAzi) <= 1; // Погрешность 1 градус
        }
        else
        {
            return Mathf.Abs(currentValue - calc.targetDistance) <= calc.distanceTolerance;
        }
    }
}
