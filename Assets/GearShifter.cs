using UnityEngine;
using UnityEngine.EventSystems;

public class GearShifter : MonoBehaviour, IDragHandler, IEndDragHandler
{
    public Transform gearForward;
    public Transform gearBack;
    public Transform gearNeutral;

    public int currentGear = 0;
    private RectTransform leverRect;

    void Start()
    {
        leverRect = GetComponent<RectTransform>();
        CheckGearPosition(); // <--- Проверяем позицию при старте!
    }

    public void OnDrag(PointerEventData eventData)
    {
        leverRect.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        CheckGearPosition();
    }

    void CheckGearPosition()
    {
        float distForward = Vector2.Distance(leverRect.position, gearForward.position);
        float distBack = Vector2.Distance(leverRect.position, gearBack.position);
        float distNeutral = Vector2.Distance(leverRect.position, gearNeutral.position);

        float minDist = Mathf.Min(distForward, Mathf.Min(distBack, distNeutral));

        if (minDist == distForward)
        {
            leverRect.position = gearForward.position;
            currentGear = 1;
        }
        else if (minDist == distBack)
        {
            leverRect.position = gearBack.position;
            currentGear = -1;
        }
        else
        {
            leverRect.position = gearNeutral.position;
            currentGear = 0;
        }
    }
}