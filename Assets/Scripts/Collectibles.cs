using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Collectibles : MonoBehaviour
{
    public static event Action OnCollected;
    public static int total; 

    private void Awake()
    {
        total++;
    }

    void Update()
    {
        //Kendi etraf�nda s�rekli d�nmesi i�in
        transform.localRotation = Quaternion.Euler(-25f, Time.time * 100f, 0);
    }

    //Dokununca yok olmas� i�in
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            OnCollected?.Invoke();
            Destroy(gameObject);
        }
    }
}
