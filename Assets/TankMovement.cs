using UnityEngine;

public class TankMovement : MonoBehaviour
{
    [Header("Настройки")]
    public float moveSpeed = 50f;
    public float turnSpeed = 50f;
    public float rotateOnSpotSpeed = 50f;

    [Header("Туман войны")]
    public FogGrid fogGrid; // Сюда перетащи FogManager
    public float visionRadius = 200f; // Радиус обзора танка

    [Header("Педаль газа")]
    public RectTransform pedal_r;      // перетащи сюда pedal_r из иерархии
    public Vector2 pressedScale = new Vector2(0.8f, 0.8f);  // ~20% меньше
    public Vector2 pressedOffset = new Vector2(20f, 20f);   // сдвиг вниз, подгонишь

    private RectTransform tankRect;
    private GearShifter gearShifter;
    private SteeringWheelController steeringWheel;

    private Vector2 originalScale;
    private Vector2 originalAnchoredPos;

    void Start()
    {
        tankRect = GetComponent<RectTransform>();
        gearShifter = FindObjectOfType<GearShifter>();
        steeringWheel = FindObjectOfType<SteeringWheelController>();

        if (pedal_r != null)
        {
            originalScale = pedal_r.localScale;
            originalAnchoredPos = pedal_r.anchoredPosition;
        }
    }

    void Update()
    {
        if (gearShifter == null) return;

        // Ограничение по границам карты
        Vector2 pos = tankRect.anchoredPosition;
        pos.x = Mathf.Clamp(pos.x, -500f, 500f);
        pos.y = Mathf.Clamp(pos.y, -500f, 500f);
        tankRect.anchoredPosition = pos;

        // 1. РАССЕИВАЕМ ТУМАН (каждый кадр)
        if (fogGrid != null)
        {
            fogGrid.RevealFog(transform.position, visionRadius);
        }

        // 2. ПЕДАЛЬ: сжимаем и сдвигаем, пока зажат газ
        bool gas = Input.GetMouseButton(1);

        if (pedal_r != null)
        {
            if (gas)
            {
                pedal_r.localScale = originalScale * pressedScale;
                pedal_r.anchoredPosition = originalAnchoredPos + pressedOffset;
            }
            else
            {
                pedal_r.localScale = originalScale;
                pedal_r.anchoredPosition = originalAnchoredPos;
            }
        }

        // 3. ГАЗ (Правая кнопка мыши)
        if (gas)
        {
            // Вперед
            if (gearShifter.currentGear == 1)
            {
                tankRect.anchoredPosition += (Vector2)tankRect.up * moveSpeed * Time.deltaTime;

                if (steeringWheel != null)
                {
                    float steerInput = steeringWheel.GetSteeringInput();
                    tankRect.Rotate(0, 0, steerInput * turnSpeed * Time.deltaTime);
                }
            }
            // Назад
            else if (gearShifter.currentGear == -1)
            {
                tankRect.anchoredPosition -= (Vector2)tankRect.up * moveSpeed * Time.deltaTime;

                if (steeringWheel != null)
                {
                    float steerInput = steeringWheel.GetSteeringInput();
                    tankRect.Rotate(0, 0, -steerInput * turnSpeed * Time.deltaTime);
                }
            }
            // Нейтраль - поворот на месте
            else if (gearShifter.currentGear == 0)
            {
                float steerInput = 0f;
                if (steeringWheel != null) steerInput = steeringWheel.GetSteeringInput();

                if (Mathf.Abs(steerInput) > 0.1f)
                {
                    tankRect.Rotate(0, 0, steerInput * rotateOnSpotSpeed * Time.deltaTime);
                }
                else
                {
                    tankRect.Rotate(0, 0, -rotateOnSpotSpeed * Time.deltaTime);
                }
            }
        }
    }
}