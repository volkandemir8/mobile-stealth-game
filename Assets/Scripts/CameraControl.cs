using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    private Vector3 offset; //Kameray� Player'dan belirli bir mesafede tutmak i�in.
    [SerializeField] private Transform target; //Player
    [SerializeField] private float smoothTime;
    private Vector3 currentVelocity = Vector3.zero;


    private void Awake() //Oyun ba�lad���nda b�t�n fonksiyonlardan �nce �al��an fonksiyondur.Sadece bir kez �al���r.
    {
        offset = transform.position - target.position;
    }

    private void LateUpdate() //Genel olarak kamera kontrolleri gibi i�lemlerde tercih edilir.
    {
        Vector3 targetPosition = target.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothTime);
    }

}
