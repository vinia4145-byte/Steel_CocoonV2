using UnityEngine;

public class BallisticCalculator : MonoBehaviour
{
    public Transform tank;
    public Transform target;
    // mapScale теперь можно вообще не трогать — мы просто режем итоговую цифру
    public float mapScale = 10f;

    [Header("Состояние цели")]
    public bool hasTarget = false;

    [Header("Настройки сложности")]
    public float distanceTolerance = 50f;

    [HideInInspector] public float targetAzimuth;
    [HideInInspector] public float targetDistance;

    private Transform mapRoot;

    void Start()
    {
        // Если танк лежит внутри зумящейся карты — это сработает автоматически
        mapRoot = tank.parent;
    }

    void Update()
    {
        if (tank == null || target == null) return;

        if (target.gameObject.activeSelf)
        {
            hasTarget = true;

            Vector2 directionToTarget = target.position - tank.position;
            Vector2 barrelDirection = tank.up;

            targetAzimuth = Vector2.SignedAngle(barrelDirection, directionToTarget);

            float distanceInMapUnits;
            if (mapRoot != null)
            {
                // Считаем дистанцию ВНУТРИ карты — зум больше не влияет
                Vector2 tankLocal = mapRoot.InverseTransformPoint(tank.position);
                Vector2 targetLocal = mapRoot.InverseTransformPoint(target.position);
                distanceInMapUnits = Vector2.Distance(tankLocal, targetLocal);
            }
            else
            {
                distanceInMapUnits = Vector2.Distance(tank.position, target.position);
            }

            // Сначала переводим в метры по твоему масштабу...
            float rawDistance = distanceInMapUnits * mapScale;

            // ...а потом просто делаем в 2 раза меньше, как ты просила
            targetDistance = rawDistance / 2f;
        }
        else
        {
            hasTarget = false;
        }
    }
}
