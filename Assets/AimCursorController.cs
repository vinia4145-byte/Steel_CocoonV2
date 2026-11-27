using UnityEngine;
using TMPro;

public class AimCursorController : MonoBehaviour
{
    public TextMeshProUGUI infoText;

    void Update()
    {
        // Если зажата ЛКМ
        if (Input.GetMouseButton(0))
        {
            if (infoText != null)
            {
                infoText.gameObject.SetActive(true);
                infoText.text = "Азимут: " + Mathf.Abs(FindObjectOfType<BallisticCalculator>().targetAzimuth).ToString("F1") + "°";
            }
        }
        else
        {
            if (infoText != null) infoText.gameObject.SetActive(false);
        }
    }
}