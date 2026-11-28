using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using TMPro;

public class ShootingController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Ссылки на механизмы")]
    public BreechDoorController breech;

    [Header("Предупреждение о дальности")]
    public TextMeshProUGUI rangeWarningText; // красная надпись на экране
    public float maxRange = 2000f;           // максимальная дальность выстрела

    private BallisticCalculator calc;

    [Header("Визуал и Звук")]
    public GameObject shellCasing;
    public RectTransform ejectPoint;
    public AudioSource audioSource;
    public AudioClip shotSound;

    [Header("Звуки результата")]
    public AudioClip hitSound;   // «Точное попадание»
    public AudioClip missSound;  // «Промах, корректируй наведение»

    [Header("Карта и Воронка")]
    public GameObject target;
    public GameObject craterPrefab;
    public ValveController leftValve;
    public ValveController rightValve;

    [Header("Выброс гильзы")]
    public float ejectDropHeight = 400f;   // насколько вниз падает гильза
    public float ejectSideOffset = 80f;    // сдвиг вбок при вылете

    [Header("Ссылка на линейку")]
    public RulerController ruler;

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

        calc = FindObjectOfType<BallisticCalculator>();
        if (rangeWarningText != null) rangeWarningText.gameObject.SetActive(false);
        ruler = FindObjectOfType<RulerController>(); // самообслуживание: найдём линейку сами
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
        // ПРЕДУПРЕЖДЕНИЕ: цель дальше максимальной дальности
        if (rangeWarningText != null)
        {
            bool tooFar = calc != null && calc.hasTarget && calc.targetDistance > maxRange;
            rangeWarningText.gameObject.SetActive(tooFar);
        }
        // выход по esc
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    void Fire()
    {
        if (breech == null || !breech.isLoaded)
        {
            Debug.Log("Осечка! Нет снаряда.");
            return;
        }

        // Проверка дальности: снаряд летит максимум на maxRange
        if (calc != null && calc.targetDistance > maxRange)
        {
            Debug.Log("Допустимая дальность превышена!");
            return; // выстрел не происходит
        }

        Debug.Log("ВЫСТРЕЛ!");

        if (audioSource != null && shotSound != null)
            audioSource.PlayOneShot(shotSound);

        StartCoroutine(FireSequence());
        if (ruler != null) ruler.ResetRuler();
    }


    IEnumerator FireSequence()
    {
        // 1. Пауза перед выбросом гильзы (звук выстрела)
        yield return new WaitForSeconds(ejectionDelay);

        // 2. Открываем задвижку и ЖДЁМ, пока она откроется до конца
        breech.OpenForEjection();

        while (!breech.IsFullyOpen())
        {
            yield return null; // ждём следующий кадр, пока затвор доезжает
        }

        // 3. Затвор открыт — показываем гильзу и запускаем анимацию выброса
        if (shellCasing != null)
        {
            shellCasing.SetActive(true);
            StartCoroutine(EjectCasingAnimation());
        }

        // 4. Ждем, пока снаряд летит до цели
        yield return new WaitForSeconds(shellFlightTime);

        // 5. Проверяем попадание и ставим воронку
        CheckHit();

        // 6. Закрываем задвижку
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

                if (hitSound != null && audioSource != null)
                    audioSource.PlayOneShot(hitSound);
            }
            else
            {
                float randomX = Random.Range(-100f, 100f);
                float randomY = Random.Range(-100f, 100f);
                craterRect.position = targetRect.position + new Vector3(randomX, randomY, 0);
                Debug.Log("ПРОМАХ! Воронка появилась в стороне.");

                if (missSound != null && audioSource != null)
                    audioSource.PlayOneShot(missSound);
            }
        }
    }

    IEnumerator EjectCasingAnimation()
    {
        RectTransform casingRect = shellCasing.GetComponent<RectTransform>();

        // Стартовая точка — позиция казенника, переведённая в координаты родителя гильзы
        RectTransform parent = casingRect.parent as RectTransform;
        Vector2 startPos;

        if (ejectPoint != null && parent != null)
        {
            // мировая позиция казенника -> локальная позиция внутри родителя гильзы
            startPos = parent.InverseTransformPoint(ejectPoint.position);
        }
        else
        {
            // запасной вариант, если точку не назначили
            startPos = new Vector2(255f, 132f);
        }

        casingRect.anchoredPosition = startPos;

        Vector2 endPos = new Vector2(startPos.x + ejectSideOffset, startPos.y - ejectDropHeight);

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
