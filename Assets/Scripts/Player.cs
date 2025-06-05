using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour
{
    //static kullan sabit objelerde
    //SerializeField private de�i�kenlerin Inspector �zerinde g�r�nmesi i�in.
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private FixedJoystick joystick;
    [SerializeField] private Transform childTransform;//Player yerine onun child'� olan model d�nd�r�lecek.
    [SerializeField] private float speed;

    // FixedJoyStick'ten ald���m�z inputlar bu de�i�kenlere atan�r.
    private float horizontal;
    private float vertical;

    private bool disabled;

    private void Start()
    {
        Guard.onGuardSpottedPlayer += Disable;
    }

    //S�rekli olarak �al��an bir fonksiyondur.�al��ma h�z� oyunun �al��t��� cihaza g�re de�i�ir.
    private void Update() //Girdileri bu fonksiyondan al�caz.
    {
        GetMovement();

        if (disabled)
        {
            horizontal = 0;
            vertical = 0;
        }
    }
    //S�rekli olarak �al��an ve sabit bir �al��ma h�z�na sahip olan fonksiyondur.
    private void FixedUpdate() //Hareket i�lemini bu fonksiyonda yap�caz.
    {
        SetMovement();
        SetRotation();
    }

    private void SetRotation()
    {
        if (horizontal != 0 || vertical != 0)
        {
            childTransform.rotation = Quaternion.LookRotation(rb.velocity);
            animator.SetTrigger("Run");
            animator.ResetTrigger("Idle");
        }
        else
        {
            animator.SetTrigger("Idle");
            animator.ResetTrigger("Run");
        }
    }

    private void SetMovement()
    {
        //Velocity: h�z vekt�r�
        //Vector3: vekt�rel bir b�y�kl��� belirtmek i�in kulland���m�z de�i�ken tipidir.
        //fixedDeltaTime, bir �nceki kare ile �u anki kare aras�ndaki zaman� elde etmemizi sa�lar.
        rb.velocity = new Vector3(horizontal, rb.velocity.y, vertical) * speed * Time.fixedDeltaTime;
    }

    private void GetMovement()
    {
        horizontal = joystick.Horizontal;
        vertical = joystick.Vertical;
    }


    void Disable()
    {
        disabled = true;
    }

    private void OnDestroy() //obje yok edildi�inde (sahne de�i�ti�inde vb.) disable'� s�f�rlamak i�in
    {
        Guard.onGuardSpottedPlayer -= Disable;
    }
}
