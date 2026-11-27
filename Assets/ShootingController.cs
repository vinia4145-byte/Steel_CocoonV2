using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class ShootingController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Ссылки на механизмы")]
    public BreechDoorController breech;

    [Header("Визуал и Звук")]
    public GameObject shellCasing;
    public AudioSource audioSource;
    public AudioClip shotSound;

    [Header("Карта и Воронка")]
    public GameObject target;
    public GameObject craterPrefab;
    public ValveController leftValve;
    public ValveController rightValve;

    [Header("Настройки выстрела")]
    public float maxPullDistance = 100f;
    public float requiredPullDistance = 50f;
    public float ejectionDelay = 0.5f;
    public float shellFlightTime = 1.5f; // Время полета снаряда до появления воронки

    private bool isPulling = false;
    private Vector2 startPos;
    private Vector2 startMousePos;
    private RectTransform ropeRect;
    private float currentPull;

    void Start()
    {
        ropeRect = GetComponent<RectTransform>();
        startPos = ropeRect.anchoredPosition;

        if (shellCasing != null) shellCasing.SetActive(false);
        if (craterPrefab != null) craterPrefab.SetActive(false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPulling = true;
        startMousePos = eventData.position;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isPulling) return;
        isPulling = false;

        if (currentPull >= requiredPullDistance)
        {
            Fire();
        }
        else
        {
            Debug.Log("Слишком слабо дернул!");
        }

        ropeRect.anchoredPosition = startPos;
        currentPull = 0;
    }

    void Update()
    {
        if (isPulling)
        {
            float deltaY = Input.mousePosition.y - startMousePos.y;

            if (deltaY < 0)
            {
                currentPull = Mathf.Min(Mathf.Abs(deltaY), maxPullDistance);
                ropeRect.anchoredPosition = startPos - new Vector2(0, currentPull);
            }
        }
    }

    void Fire()
    {
        if (breech == null || !breech.isLoaded)
        {
            Debug.Log("Осечка! Нет снаряда.");
            return;
        }

        Debug.Log("ВЫСТРЕЛ!");

        if (audioSource != null && shotSound != null)
            audioSource.PlayOneShot(shotSound);

        StartCoroutine(FireSequence());
    }

    IEnumerator FireSequence()
    {
        // 1. Пауза перед выбросом гильзы (звук выстрела)
        yield return new WaitForSeconds(ejectionDelay);

        // 2. Открываем задвижку и выбрасываем гильзу
        breech.OpenForEjection();
        if (shellCasing != null)
        {
            shellCasing.SetActive(true);
            StartCoroutine(EjectCasingAnimation());
        }

        // 3. Ждем, пока снаряд летит до цели
        yield return new WaitForSeconds(shellFlightTime);

        // 4. Проверяем попадание и ставим воронку
        CheckHit();

        // 5. Закрываем задвижку (можно сделать это чуть позже или раньше, по желанию)
        yield return new WaitForSeconds(0.5f);
        breech.CloseAfterEjection();
    }

    void CheckHit()
    {
        if (leftValve == null || rightValve == null || target == null) return;

        bool isAimCorrect = leftValve.IsCorrect() && rightValve.IsCorrect();

        if (craterPrefab != null)
        {
            craterPrefab.SetActive(true);
            RectTransform craterRect = craterPrefab.GetComponent<RectTransform>();
            RectTransform targetRect = target.GetComponent<RectTransform>();

            if (isAimCorrect)
            {
                craterRect.position = targetRect.position;
                Debug.Log("ПОПАДАНИЕ! Мишень уничтожена.");
                target.SetActive(false);
            }
            else
            {
                float randomX = Random.Range(-100f, 100f);
                float randomY = Random.Range(-100f, 100f);
                craterRect.position = targetRect.position + new Vector3(randomX, randomY, 0);
                Debug.Log("ПРОМАХ! Воронка появилась в стороне.");
            }
        }
    }

    IEnumerator EjectCasingAnimation()
    {
        RectTransform casingRect = shellCasing.GetComponent<RectTransform>();
        casingRect.anchoredPosition = new Vector2(255f, 132f);

        Vector2 startPos = casingRect.anchoredPosition;
        Vector2 endPos = new Vector2(startPos.x + 80f, -250f);

        float elapsed = 0f;
        float duration = 0.3f;

        while (elapsed < duration)
        {
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            casingRect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);
        shellCasing.SetActive(false);
    }
}