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
        //Kendi etrafýnda sürekli dönmesi için
        transform.localRotation = Quaternion.Euler(-25f, Time.time * 100f, 0);
    }

    //Dokununca yok olmasý için
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            OnCollected?.Invoke();
            Destroy(gameObject);
        }
    }
}
