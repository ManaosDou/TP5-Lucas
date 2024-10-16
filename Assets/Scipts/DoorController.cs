using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    public bool requiresKey = false;
    public KeyType requiredKeyType;
    public bool isLeftHinged = true;
    public float openAngle = 90f;
    public float openSpeed = 2f;

    private bool isOpen = false;
    private bool isMoving = false;
    private Quaternion initialRotation;
    private Quaternion targetRotation;

    private void Start()
    {
        gameObject.tag = "Puerta";
        initialRotation = transform.rotation;
        UpdateTargetRotation();
    }

    private void UpdateTargetRotation()
    {
        float rotationAngle = isLeftHinged ? openAngle : -openAngle;
        targetRotation = initialRotation * Quaternion.Euler(0, rotationAngle, 0);
    }

    public void ToggleDoor()
    {
        if (!isMoving)
        {
            StartCoroutine(AnimateDoor(!isOpen));
        }
    }

    private IEnumerator AnimateDoor(bool open)
    {
        isMoving = true;
        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = open ? targetRotation : initialRotation;
        float elapsedTime = 0f;

        while (elapsedTime < 1f)
        {
            transform.rotation = Quaternion.Slerp(startRotation, endRotation, elapsedTime);
            elapsedTime += Time.deltaTime * openSpeed;
            yield return null;
        }

        transform.rotation = endRotation;
        isOpen = open;
        isMoving = false;
        
        Debug.Log(isOpen ? "Door opened!" : "Door closed!");
    }
}