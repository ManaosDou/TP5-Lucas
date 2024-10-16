using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum KeyType
    {
        Cocina,
        Auditorio,
        Dormitorio,
        Quirofano,
        Clase,
        Oficina,
        Recepcion,
        Deposito,
        Salida
    }

public class KeyController : MonoBehaviour
{
    public KeyType keyType;

    private void Start()
    {
        gameObject.tag = "Llave";
    }

}

