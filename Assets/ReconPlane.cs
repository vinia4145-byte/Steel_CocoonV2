using UnityEngine;

public class ReconPlane : MonoBehaviour
{
    [Header("Настройки полета")]
    public float flySpeed = 200f;
    public float startX = -600f;
    public float endX = 600f;
    public float spawnY = 0f;
    public float revealRadius = 150f;
    

    [Header("Ссылки")]
    public GameObject target; // ВОТ ОНА, НАША МИШЕНЬ

    private RectTransform planeRect;
    private FogGrid fogGrid;

    void Start()
    {
        planeRect = GetComponent<RectTransform>();
        fogGrid = FindObjectOfType<FogGrid>();
        target.SetActive(false); // Прячем мишень в начале
    }

    public void LaunchPlane(float customY)
    {
        spawnY = customY;
        planeRect.localRotation = Quaternion.Euler(0, 0, 0f);
        planeRect.anchoredPosition = new Vector2(startX, spawnY);
        gameObject.SetActive(true);
    }

    void Update()
    {
        planeRect.anchoredPosition += new Vector2(flySpeed * Time.deltaTime, 0);

        if (fogGrid != null)
        {
            fogGrid.RevealFog(planeRect.position, revealRadius);
        }

        // Показываем мишень на середине пути
        if (planeRect.anchoredPosition.x > 0 && target != null)
        {
            target.SetActive(true);
           
        }

        if (planeRect.anchoredPosition.x > endX)
        {
            gameObject.SetActive(false);

        }
    }
}