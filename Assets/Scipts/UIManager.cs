using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI ammoText;
    public TextMeshProUGUI batteryText;
    public TextMeshProUGUI interactText;

    public void UpdateAmmoText(int currentAmmo)
    {
        if (ammoText != null)
        {
            ammoText.text = $"Ammo: {currentAmmo}";
        }
    }

    public void UpdateBatteryText(float currentBattery)
    {
        if (batteryText != null)
        {
            batteryText.text = $"Battery: {currentBattery:F1}%";
        }
    }

    public void ShowInteractText(string message)
    {
        if (interactText != null)
        {
            interactText.text = message;
            interactText.gameObject.SetActive(true);
        }
    }

    public void HideInteractText()
    {
        if (interactText != null)
        {
            interactText.gameObject.SetActive(false);
        }
    }
}


