using UnityEngine;
using UnityEngine.UI;

public class FogGrid : MonoBehaviour
{
    public GameObject fogTilePrefab;
    public Transform mapParent;
    public int gridWidth = 10;
    public int gridHeight = 10;
    public float tileSize = 100f;
    public float offset = 25f; // Отступ от края карты

    public float revealRadius = 150f; // радиус обзора В ЕДИНИЦАХ КАРТЫ

    private GameObject[,] fogTiles;

    void Start()
    {
        fogTiles = new GameObject[gridWidth, gridHeight];

        RectTransform mapRect = mapParent.GetComponent<RectTransform>();
        float startX = mapRect.rect.xMin + offset;
        float startY = mapRect.rect.yMin + offset;

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                GameObject newTile = Instantiate(fogTilePrefab, mapParent);
                RectTransform rect = newTile.GetComponent<RectTransform>();

                rect.anchoredPosition = new Vector2(startX + (x * tileSize), startY + (y * tileSize));
                fogTiles[x, y] = newTile;
            }
        }
    }

    // Метод для рассеивания тумана
    public void RevealFog(Vector2 tankPosition, float radius)
    {
        // Переводим обе точки в координаты карты — зум больше не влияет
        Vector2 tankLocal = mapParent.InverseTransformPoint(tankPosition);

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (fogTiles[x, y] != null && fogTiles[x, y].activeSelf)
                {
                    Vector2 tileLocal = mapParent.InverseTransformPoint(fogTiles[x, y].transform.position);
                    float distance = Vector2.Distance(tileLocal, tankLocal);

                    if (distance < radius)
                    {
                        fogTiles[x, y].SetActive(false);
                    }
                }
            }
        }
    }
}
