using System;
using Cinemachine;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class TPDisparadorController : MonoBehaviour
{
    [Header("UI")]
    public UIManager uiManager;

    [Header("Cinemachine")]
    public CinemachineVirtualCamera aimCamera;
    public int prioridadApuntando = 5;

    public LayerMask aimColliderMask = new LayerMask();

    public Animator animator;
    public TPInput input;
    public GameObject crosshair;

    public Transform aimTarget;

    public float rotationSpeed = 10f; 
    public float maxAimDistance = 100f;

    [Header("Disparo")]
    public GameObject balaPrefab;
    public Transform balaSpawnPosition;
    public int maxAmmo = 10;
    public float fireRate = 0.5f;
    private int currentAmmo;
    private float nextFireTime;

    [Header("Linterna")]
    public GameObject laserObject;
    public GameObject fLight;
    public float maxBatteryLife = 100f;
    private float currentBatteryLife;
    public float batteryDrainRate = 10f; 
    private float targetLinternaWeight = 0f;
    private bool linternaActiva = false;
    public float fadeOutThreshold = 0.1f; // 10% de la batería
    public float minLightIntensity = 0.1f;
    public float maxLightIntensity = 1f;
    private float initialLightIntensity;

    public Rig aimRig;
    public Rig aimRig2;

    private int aimLayerIndex;
    private int linternaLayerIndex;
    private bool armaEnUso = false;
    private bool apuntando = false;
    
    [Header("Llaves")]
    public bool llaveCocina = false;
    public bool llaveAuditorio = false;
    public bool llaveDormitorio = false;
    public bool llaveQuirofano = false;
    public bool llaveClase = false;
    public bool llaveOficina= false;
    public bool llaveRecepcion = false;
    public bool llaveDeposito = false;
    public bool llaveSalida = false;

    

    private void Start()
    {
        aimLayerIndex = animator.GetLayerIndex("AimPistol");
        linternaLayerIndex = animator.GetLayerIndex("AimFlashlight");
        currentAmmo = maxAmmo;
        currentBatteryLife = maxBatteryLife;
        if (laserObject != null) laserObject.SetActive(false);
        if (fLight != null) fLight.SetActive(false);
        if (fLight != null)
        {
            initialLightIntensity = fLight.GetComponent<Light>().intensity;
        }

        UpdateUI();
    }

    private void Update()
    {
        apuntando = input.aim || (input.shoot && !armaEnUso);

        if (input.aim)
        {
            Aiming();
            armaEnUso = true;
        }
        else
        {
            NoAiming();
            armaEnUso = false;
        }

        if (input.shoot && !armaEnUso)
        {
            UsarLinterna();
        }
        else
        {
            ApagarLinterna();
        }

        ActualizarPesoLinterna();

        if (apuntando)
        {
            ActualizarAimTarget();
        }

        if (armaEnUso && input.shoot)
        {
            TryShoot();
        }
        UpdateFlashlightFadeOut();
    }

    private void Aiming()
    {
        aimCamera.Priority = prioridadApuntando;
        crosshair.SetActive(true);
        aimTarget.gameObject.SetActive(true);

        var weight = Mathf.Lerp(aimRig.weight, 1f, Time.deltaTime * 10f);
        aimRig.weight = weight;
        animator.SetLayerWeight(aimLayerIndex, weight);
    }

    private void NoAiming()
    {
        if (!linternaActiva)
        {
            aimCamera.Priority = 0;
            crosshair.SetActive(false);
            aimTarget.gameObject.SetActive(false);
        }

        var weight = Mathf.Lerp(aimRig.weight, 0f, Time.deltaTime * 10f);
        aimRig.weight = weight;
        animator.SetLayerWeight(aimLayerIndex, weight);
    }

    private void ActualizarAimTarget()
    {
        var screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Ray ray = Camera.main.ScreenPointToRay(screenCenter);

        Vector3 targetPoint;
        if (Physics.Raycast(ray, out RaycastHit hit, maxAimDistance, aimColliderMask))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.GetPoint(maxAimDistance);
        }

        aimTarget.position = targetPoint;

        // Calcular la dirección de apuntado solo en el plano horizontal
        Vector3 horizontalTargetPosition = new Vector3(targetPoint.x, transform.position.y, targetPoint.z);
        Vector3 aimDirection = (horizontalTargetPosition - transform.position).normalized;

        // Rotar suavemente hacia la dirección de apuntado
        Quaternion targetRotation = Quaternion.LookRotation(aimDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }


    private void UsarLinterna()
    {
        if (currentBatteryLife > 0)
        {
            if (laserObject != null && !linternaActiva)
            {
                laserObject.SetActive(true);
                fLight.SetActive(true);
                linternaActiva = true;
            }

            aimCamera.Priority = prioridadApuntando;
            crosshair.SetActive(true);
            aimTarget.gameObject.SetActive(true);

            float batteryDrain = batteryDrainRate * Time.deltaTime;
            currentBatteryLife = Mathf.Max(0, currentBatteryLife - batteryDrain);
            UpdateUI();

            targetLinternaWeight = 1f;

            if (currentBatteryLife <= 0)
            {
                ApagarLinterna();
            }
        }
        else
        {
            ApagarLinterna();
        }
    }


    private void ApagarLinterna()
    {
        if (laserObject != null && linternaActiva)
        {
            laserObject.SetActive(false);
            fLight.SetActive(false);
            linternaActiva = false;
        }
        
        if (!armaEnUso)
        {
            aimCamera.Priority = 0;
            crosshair.SetActive(false);
            aimTarget.gameObject.SetActive(false);
        }
        
        targetLinternaWeight = 0f;
    }

    private void ActualizarPesoLinterna()
    {
        float currentLinternaWeight = animator.GetLayerWeight(linternaLayerIndex);
        float newLinternaWeight = Mathf.Lerp(currentLinternaWeight, targetLinternaWeight, Time.deltaTime * 10f);
        animator.SetLayerWeight(linternaLayerIndex, newLinternaWeight);

        // Actualizar el peso del AimRig2
        aimRig2.weight = Mathf.Lerp(aimRig2.weight, targetLinternaWeight, Time.deltaTime * 10f);
    }

    private void TryShoot()
    {
        if (Time.time >= nextFireTime && currentAmmo > 0)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
            currentAmmo--;
            UpdateUI();
            
        }
        else if (currentAmmo <= 0)
        {
            Debug.Log("Out of ammo!");
        }
}

    private void Shoot()
    {
        var mouseWorldPosition = aimTarget.position;
        var balaDirection = (mouseWorldPosition - balaSpawnPosition.position).normalized;

        Instantiate(
            balaPrefab,
            balaSpawnPosition.position,
            Quaternion.LookRotation(balaDirection, Vector3.up)
        );
    }

    public void OnInteract()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 2f);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Balas"))
            {
                CollectAmmo(hitCollider.gameObject);
            }
            else if (hitCollider.CompareTag("Pila"))
            {
                CollectBattery(hitCollider.gameObject);
            }
            else if (hitCollider.CompareTag("Llave"))
            {
                CollectKey(hitCollider.gameObject);
            }
            else if (hitCollider.CompareTag("Puerta"))
            {
                TryOpenDoor(hitCollider.gameObject);
            }
            uiManager.HideInteractText();
        }
    }

    private void CollectAmmo(GameObject ammoCrate)
    {
        currentAmmo = maxAmmo;
        UpdateUI();
        Destroy(ammoCrate);
    }

    private void CollectBattery(GameObject battery)
    {
        currentBatteryLife = maxBatteryLife;
        UpdateUI();
        Destroy(battery);
    }

    private void CollectKey(GameObject key)
    {
        KeyController keyController = key.GetComponent<KeyController>();
        if (keyController != null)
        {
            switch (keyController.keyType)
            {
                case KeyType.Cocina:
                    llaveCocina = true;
                    break;
                case KeyType.Auditorio:
                    llaveAuditorio = true;
                    break;
                case KeyType.Dormitorio:
                    llaveDormitorio = true;
                    break;
                case KeyType.Quirofano:
                    llaveQuirofano = true;
                    break;
                case KeyType.Clase:
                    llaveClase = true;
                    break;
                case KeyType.Oficina:
                    llaveOficina = true;
                    break;
                case KeyType.Recepcion:
                    llaveRecepcion = true;
                    break;
                case KeyType.Deposito:
                    llaveDeposito = true;
                    break;
                case KeyType.Salida:
                    llaveSalida = true;
                    break;
            }
            Debug.Log($"Collected key: {keyController.keyType}");
            Destroy(key);
        }
    }

    private void TryOpenDoor(GameObject door)
    {
        DoorController doorController = door.GetComponent<DoorController>();
        if (doorController != null)
        {
            if (doorController.requiresKey)
            {
                if (HasRequiredKey(doorController.requiredKeyType))
                {
                    doorController.ToggleDoor();
                }
                else
                {
                    Debug.Log("No tienes la llave necesaria para abrir esta puerta.");
                }
            }
            else
            {
                doorController.ToggleDoor();
            }
        }
    }

    private bool HasRequiredKey(KeyType requiredKeyType)
    {
        switch (requiredKeyType)
        {
            case KeyType.Cocina:
                return llaveCocina;
            case KeyType.Auditorio:
                return llaveAuditorio;
            case KeyType.Dormitorio:
                return llaveDormitorio;
            case KeyType.Quirofano:
                return llaveQuirofano;
            case KeyType.Clase:
                return llaveClase;
            case KeyType.Oficina:
                return llaveOficina;
            case KeyType.Recepcion:
                return llaveRecepcion;
            case KeyType.Deposito:
                return llaveDeposito;
            case KeyType.Salida:
                return llaveSalida;
            default:
                return false;
        }
    }
    private void UpdateFlashlightFadeOut()
    {
        if (fLight != null && linternaActiva)
        {
            float batteryPercentage = currentBatteryLife / maxBatteryLife;
            if (batteryPercentage <= fadeOutThreshold)
            {
                float t = Mathf.InverseLerp(0, fadeOutThreshold, batteryPercentage);
                float targetIntensity = Mathf.Lerp(minLightIntensity, maxLightIntensity, t);
                fLight.GetComponent<Light>().intensity = targetIntensity * initialLightIntensity;
            }
            else
            {
                fLight.GetComponent<Light>().intensity = initialLightIntensity;
            }
        }
    }
    private void UpdateUI()
    {
        if (uiManager != null)
        {
            uiManager.UpdateAmmoText(currentAmmo);
            uiManager.UpdateBatteryText(currentBatteryLife);
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (uiManager != null)
        {
            if (other.CompareTag("Balas") || other.CompareTag("Pila") || other.CompareTag("Llave"))
            {
                uiManager.ShowInteractText("Pulsa [E] para recoger");
            }
            else if (other.CompareTag("Puerta"))
            {
                uiManager.ShowInteractText("Pulsa [E] para abrir/cerrar");
            }
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (uiManager != null)
        {
            uiManager.HideInteractText();
        }
    }

}
