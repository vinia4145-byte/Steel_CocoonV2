using UnityEngine;
using TMPro;



public class BallisticCalculator : MonoBehaviour
{
    public Transform tank;
    public Transform target;
    public float mapScale = 10f;
    public TextMeshProUGUI debugText;

    [Header("Настройки сложности")]
    public float distanceTolerance = 50f;

    [HideInInspector] public float targetAzimuth;
    [HideInInspector] public float targetDistance;

    void Update()
    {
        if (tank == null || target == null) return;

        // Показываем текст только когда мишень активна
        if (target.gameObject.activeSelf)
        {
            Vector2 directionToTarget = target.position - tank.position;
            Vector2 barrelDirection = tank.up;

            targetAzimuth = Vector2.SignedAngle(barrelDirection, directionToTarget);
            float distanceInPixels = Vector2.Distance(tank.position, target.position);
            targetDistance = distanceInPixels * mapScale;

            if (debugText != null)
            {
                debugText.text = $"Цель: Азимут {Mathf.Abs(targetAzimuth):F1}° | Дальность {targetDistance:F0}м";
            }
        }
        else
        {
            // Прячем текст, если мишени ещё нет
            if (debugText != null)
                debugText.text = "";
        }
    }
}