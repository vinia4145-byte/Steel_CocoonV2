using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class BreechDoorController : MonoBehaviour, IPointerClickHandler
{
    [Header("Настройки задвижки")]
    public float openAngle = 90f;
    public float animationSpeed = 5f;

    [Header("Ссылки")]
    public Transform breechHole;
    public DragAndDropShell shellScript;
    public GameObject loadedShellVisual;

    [Header("Звук заряжания")]
    public AudioSource audioSource;
    public AudioClip loadSound;

    private bool isOpen = false;
    public bool isLoaded = false;
    private Quaternion closedRotation;
    private Quaternion targetRotation;

    void Start()
    {
        closedRotation = transform.localRotation;
        targetRotation = closedRotation;

        if (loadedShellVisual != null) loadedShellVisual.SetActive(false);
        isLoaded = false;
        isOpen = false;
    }

    void Update()
    {
        transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, Time.deltaTime * animationSpeed);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isLoaded)
        {
            Debug.Log("Сначала выстрели! Задвижка заблокирована, пока внутри снаряд.");
            return;
        }

        if (!isOpen)
        {
            targetRotation = closedRotation * Quaternion.Euler(0, 0, openAngle);
            isOpen = true;

            if (shellScript != null)
            {
                shellScript.canBeLoaded = true;
                Debug.Log("Задвижка открыта. canBeLoaded включен.");
            }
        }
        else
        {
            targetRotation = closedRotation;
            isOpen = false;
            if (shellScript != null)
            {
                shellScript.canBeLoaded = false;
                Debug.Log("Задвижка закрыта вручную.");
            }
        }
    }

    public void LoadShell()
    {
        Debug.Log("Снаряд заряжен, задвижка закрывается.");
        isLoaded = true;
        isOpen = false;
        targetRotation = closedRotation;

        if (shellScript != null) shellScript.canBeLoaded = false;
        if (loadedShellVisual != null) loadedShellVisual.SetActive(true);

        if (audioSource != null && loadSound != null)
        {
            audioSource.PlayOneShot(loadSound);
        }
    }

    // Вызывается из ShootingController, когда происходит выстрел
    public void OpenForEjection()
    {
        Debug.Log("Открываем задвижку для выброса гильзы.");
        isLoaded = false;
        if (loadedShellVisual != null) loadedShellVisual.SetActive(false);

        targetRotation = closedRotation * Quaternion.Euler(0, 0, openAngle);
        isOpen = true;
    }

    // Вызывается из ShootingController, чтобы закрыть задвижку после вылета гильзы
    public void CloseAfterEjection()
    {
        Debug.Log("Закрываем задвижку после вылета гильзы.");
        targetRotation = closedRotation;
        isOpen = false;
    }
}