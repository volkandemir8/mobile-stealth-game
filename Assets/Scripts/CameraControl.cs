using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    private Vector3 offset; //Kamerayý Player'dan belirli bir mesafede tutmak için.
    [SerializeField] private Transform target; //Player
    [SerializeField] private float smoothTime;
    private Vector3 currentVelocity = Vector3.zero;


    private void Awake() //Oyun baþladýðýnda bütün fonksiyonlardan önce çalýþan fonksiyondur.Sadece bir kez çalýþýr.
    {
        offset = transform.position - target.position;
    }

    private void LateUpdate() //Genel olarak kamera kontrolleri gibi iþlemlerde tercih edilir.
    {
        Vector3 targetPosition = target.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothTime);
    }

}
