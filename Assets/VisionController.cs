using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class VisionController : MonoBehaviour
{
    public RectTransform tankTransform;
    public Image fogImage;

    private Vector3 lastTankPos;
    private Coroutine fogRoutine;
    private Color originalFogColor; // Запоминаем изначальный цвет тумана

    void Start()
    {
        if (tankTransform != null) lastTankPos = tankTransform.position;

        // Запоминаем цвет тумана, который ты настроила в инспекторе
        if (fogImage != null) originalFogColor = fogImage.color;
    }

    void Update()
    {
        if (tankTransform != null)
        {
            transform.position = tankTransform.position;

            if (Vector3.Distance(tankTransform.position, lastTankPos) > 0.1f)
            {
                // Танк едет — делаем туман прозрачным (Alpha = 0)
                if (fogImage != null)
                    fogImage.color = new Color(originalFogColor.r, originalFogColor.g, originalFogColor.b, 0f);

                lastTankPos = tankTransform.position;

                if (fogRoutine != null) StopCoroutine(fogRoutine);
                fogRoutine = null;
            }
            else
            {
                // Танк стоит — запускаем возврат цвета
                if (fogImage != null && fogImage.color.a < originalFogColor.a && fogRoutine == null)
                {
                    fogRoutine = StartCoroutine(ReturnFogAfterDelay(2f));
                }
            }
        }
    }

    IEnumerator ReturnFogAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        // Возвращаем ИМЕННО ТОТ цвет и прозрачность, которые были изначально
        if (fogImage != null) fogImage.color = originalFogColor;
        fogRoutine = null;
    }
}