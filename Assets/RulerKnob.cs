using UnityEngine;
using UnityEngine.EventSystems;

public class RulerKnob : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public RulerController ruler;
    public bool isTankEnd = false; // true у левого (нулевого) конца

    [HideInInspector] public bool isDragging = false;

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (ruler != null) ruler.OnKnobDrag(this, eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
    }
}
