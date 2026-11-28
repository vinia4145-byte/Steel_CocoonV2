using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class RulerController : MonoBehaviour
{
    [Header("Ссылки")]
    public RectTransform mapRoot;       // зумящаяся карта
    public RectTransform tankRect;      // танк на карте
    public RectTransform knobA;         // левый конец (нулевая отметка)
    public RectTransform knobB;         // правый конец
    public RectTransform rulerBody;     // сама линейка (Image)
    public TextMeshProUGUI azimuthText; // надпись с азимутом (показывается при касании)

    [Header("Ссылка на калькулятор")]
    public BallisticCalculator calc;

    [Header("Калибровка")]
    public float mapScale = 10f; // метров на единиц карты

    [Header("Привязка")]
    public float snapToTankRadius = 60f;
    public float snapToTargetRadius = 60f;

    [Header("Риски на линейке")]
    public float tickStepMeters = 200f; // шаг риска в метрах
    public float tickLength = 18f;      // длина риски
    public float labelOffset = 18f;     // отступ подписи от линейки
    public float tickFontSize = 16f;
    public Color tickColor = Color.yellow;
    public Color labelColor = Color.white;

    private bool isTouchingTarget = false;
    private float startupTimer = 1f;
    private RectTransform tickRoot;
    private float lastLen = -1f;

    void Start()
    {
        if (azimuthText != null) azimuthText.gameObject.SetActive(false);

        // Контейнер рисок — ребёнок карты, каждый кадр прижимается к ручке A
        tickRoot = new GameObject("Ticks").AddComponent<RectTransform>();
        tickRoot.SetParent(mapRoot, false);
        tickRoot.localScale = Vector3.one;
    }

    void Update()
    {
        RulerKnob knobAComp = knobA.GetComponent<RulerKnob>();
        RulerKnob knobBComp = knobB.GetComponent<RulerKnob>();

        // Первая секунда: насильно ставим обе ручки на танк
        if (startupTimer > 0f && knobAComp != null && !knobAComp.isDragging
                                  && (knobBComp == null || !knobBComp.isDragging))
        {
            startupTimer -= Time.deltaTime;
            Vector2 tankLocal = mapRoot.InverseTransformPoint(tankRect.position);
            knobA.localPosition = tankLocal;
            knobB.localPosition = tankLocal;
            SnapKnobToTank();
            UpdateRuler();
            return;
        }

        // Автоснап к танку — каждый кадр, пока ручку не держат мышкой
        if (knobAComp == null || !knobAComp.isDragging)
            SnapKnobToTank();

        // Автоснап правого конца к активной мишени
        bool targetVisible = calc != null && calc.hasTarget && calc.target != null;
        if (targetVisible && (knobBComp == null || !knobBComp.isDragging))
            SnapKnobToTarget();

        UpdateRuler();
    }

    public void OnKnobDrag(RulerKnob knob, PointerEventData eventData)
    {
        RectTransform handle = knob.isTankEnd ? knobA : knobB;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                mapRoot, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
        {
            handle.localPosition = localPoint;
            UpdateRuler();
        }
    }

    void SnapKnobToTank()
    {
        Vector2 tankLocal = mapRoot.InverseTransformPoint(tankRect.position);
        Image knobImg = knobA.GetComponent<Image>();

        if (Vector2.Distance(knobA.localPosition, tankLocal) <= snapToTankRadius)
        {
            knobA.localPosition = tankLocal;
            if (knobImg != null) knobImg.color = Color.green;
        }
        else
        {
            if (knobImg != null) knobImg.color = Color.red;
        }
    }

    void SnapKnobToTarget()
    {
        Vector2 targetLocal = mapRoot.InverseTransformPoint(calc.target.position);
        if (Vector2.Distance(knobB.localPosition, targetLocal) <= snapToTargetRadius)
            knobB.localPosition = targetLocal;
    }

    void UpdateRuler()
    {
        Vector2 a = knobA.localPosition;
        Vector2 b = knobB.localPosition;
        Vector2 dir = b - a;
        float len = dir.magnitude;

        rulerBody.localPosition = a;
        rulerBody.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
        Vector2 size = rulerBody.sizeDelta;
        size.x = Mathf.Max(len, 1f);
        rulerBody.sizeDelta = size;

        // Риски жёстко привязаны к ручке A
        if (tickRoot != null)
        {
            tickRoot.localPosition = a;
            tickRoot.localRotation = rulerBody.localRotation;
        }

        // Перестроить риски, если длина заметно изменилась
        if (Mathf.Abs(len - lastLen) > 0.5f)
        {
            lastLen = len;
            RebuildTicks(len);
        }

        // Касание мишени
        bool targetVisible = calc != null && calc.hasTarget && calc.target != null;
        isTouchingTarget = false;

        if (targetVisible)
        {
            Vector2 targetLocal = mapRoot.InverseTransformPoint(calc.target.position);
            if (Vector2.Distance(b, targetLocal) <= 0.01f)
                isTouchingTarget = true;
        }

        if (azimuthText != null)
        {
            if (isTouchingTarget)
            {
                float azimuth = Vector2.SignedAngle(tankRect.up, dir);
                azimuthText.gameObject.SetActive(true);
                azimuthText.text = $"Азимут {Mathf.Abs(azimuth):F1}°";
            }
            else
            {
                azimuthText.gameObject.SetActive(false);
            }
        }
    }

    void RebuildTicks(float len)
    {
        // Сколько единиц карты в одном шаге (200 м)
        float unitsPerStep = tickStepMeters * 2f / mapScale;
        int count = Mathf.FloorToInt(len / unitsPerStep);

        // Чистим старые риски
        for (int i = tickRoot.childCount - 1; i >= 0; i--)
            Destroy(tickRoot.GetChild(i).gameObject);

        // Рисуем новые
        for (int i = 1; i <= count; i++)
        {
            float x = i * unitsPerStep;

            GameObject tickGo = new GameObject("Tick" + i);
            RectTransform tickRect = tickGo.AddComponent<RectTransform>();
            tickRect.SetParent(tickRoot, false);
            tickRect.anchorMin = new Vector2(0f, 0.5f);
            tickRect.anchorMax = new Vector2(0f, 0.5f);
            tickRect.pivot = new Vector2(0.5f, 0.5f);
            tickRect.localPosition = new Vector3(x, 0f, 0f);
            tickRect.sizeDelta = new Vector2(3f, tickLength);

            Image tickImg = tickGo.AddComponent<Image>();
            tickImg.color = tickColor;
            tickImg.raycastTarget = false;

            GameObject labelGo = new GameObject("Label" + i);
            RectTransform labelRect = labelGo.AddComponent<RectTransform>();
            labelRect.SetParent(tickRoot, false);
            labelRect.anchorMin = new Vector2(0f, 0.5f);
            labelRect.anchorMax = new Vector2(0f, 0.5f);
            labelRect.pivot = new Vector2(0.5f, 0.5f);
            labelRect.localPosition = new Vector3(x, -labelOffset, 0f);
            labelRect.sizeDelta = new Vector2(80f, 20f);

            TextMeshProUGUI label = labelGo.AddComponent<TextMeshProUGUI>();
            label.text = (i * tickStepMeters).ToString();
            label.fontSize = tickFontSize;
            label.color = labelColor;
            label.alignment = TextAlignmentOptions.Center;
            label.raycastTarget = false;
        }
    }

    // Схлопнуть линейку обратно на танк — вызывать после выстрела
    public void ResetRuler()
    {
        Vector2 tankLocal = mapRoot.InverseTransformPoint(tankRect.position);
        knobA.localPosition = tankLocal;
        knobB.localPosition = tankLocal;
        lastLen = -1f;
        SnapKnobToTank();
        UpdateRuler();
    }
}
