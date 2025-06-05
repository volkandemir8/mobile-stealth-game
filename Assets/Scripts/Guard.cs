using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

public class Guard : MonoBehaviour
{
    public static event System.Action onGuardSpottedPlayer; //Guard, Player'� g�rd���nde �a��rd��� static event

    public Animator animator;
    public float speed; //h�z
    public float waitTime; //belirtilen noktada bekleme s�resi
    public float turnSpeed; //sonraki noktaya kafas�n� �evirme h�z�
    public float timeToSpotPlayer; // ka� saniye i�inde oyuncuyu fark edecek
    public float viewDistance; //g�r�� mesafesi (spotlight menzilindan daha k�sa) 
    public Light spotlight;
    public LayerMask viewMask; //player ile guard aras�na "Obstacle" layeri gelirse g�remeyecek

    float viewAngle; // guard g�r�� a��s�
    float playerVisibleTimer; // Player'� g�rd��� zaman ba�layan saya�

    public Transform pathHolder; //hareket edilecek noktalar
    Transform player;
    Color originalSpotlightColor;

    public UnityEngine.UI.Slider slider;
    bool seen;
    Vector3 lastSeen; //Son g�r�len konum
    Vector3[] waypoints;
    public Animator playerAnimation;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform; //player transformuna eri�mek i�in
        viewAngle = spotlight.spotAngle; //spotlight ile ayn� geni�li�e sahip
        originalSpotlightColor = spotlight.color;

        // waypointlerin konumlar�n� i�eren bir dizi olu�turmak i�in
        waypoints = new Vector3[pathHolder.childCount];
        for (int i = 0; i < waypoints.Length; i++)
        {
            waypoints[i] = pathHolder.GetChild(i).position;
            //waypointler y ekseninde guard'la ayn� y�ksekli�e sahip olsun diye (yoksa guard yerin alt�na batar)
            waypoints[i] = new Vector3(waypoints[i].x, transform.position.y, waypoints[i].z);
        }
        StartCoroutine(FollowPath());
    }

    private void Update()
    {
        slider.value = playerVisibleTimer;
        //playerVisibleTimer'�n min ev max de�erini belirtme
        playerVisibleTimer = Mathf.Clamp(playerVisibleTimer, 0, timeToSpotPlayer);
        //playerVisibleTimer 0 oldu�unda renk orijinal halde, timeToSpotPlayer'a e�it oldu�unda k�rm�z�
        spotlight.color = Color.Lerp(originalSpotlightColor, Color.red, playerVisibleTimer / timeToSpotPlayer);


        //Player'� g�rd��� zaman ileri saymaya ba�layacak, g�r�� alan�ndan ��k�nca ise geriye 
        if (CanSeePlayer())
        {
            slider.gameObject.SetActive(true);
            playerVisibleTimer += Time.deltaTime;

            StartCoroutine(ChasePlayer());

            if (playerVisibleTimer >= timeToSpotPlayer)
            {
                if (onGuardSpottedPlayer != null)
                {
                    if (Vector3.Distance(transform.position, player.position) <= viewDistance / 5)
                    {
                        animator.SetTrigger("attackGuard");
                        playerAnimation.SetTrigger("lost");
                    }
                    onGuardSpottedPlayer();
                }
            }
        }
        else
        {
            slider.gameObject.SetActive(false);
            playerVisibleTimer -= Time.deltaTime;

            if(seen)
            {
                StartCoroutine(SearchPlayer());
            }
        }
    }

    IEnumerator ChasePlayer()
    {
        seen = true;
        transform.LookAt(player.position);
        animator.SetTrigger("idleGuard");
        animator.ResetTrigger("walkGuard");
        if (Vector3.Distance(transform.position, player.position) > viewDistance / 5)//�ok yak�nda de�ilse y�r�yor
        {
            animator.SetTrigger("walkGuard");
            animator.ResetTrigger("idleGuard");
            transform.position = Vector3.MoveTowards(transform.position, player.position, speed * Time.deltaTime);
        }

        yield return null;
    }

    IEnumerator SearchPlayer() // G�r�� a��s�ndan ��k�ld�ktan sonra player'�n son konumua gitmek i�in
    {
        transform.LookAt(lastSeen);
        animator.SetTrigger("walkGuard");
        animator.ResetTrigger("idleGuard");
        transform.position = Vector3.MoveTowards(transform.position, lastSeen, speed * Time.deltaTime);

        if (transform.position == lastSeen)
        {
            animator.SetTrigger("idleGuard");
            animator.ResetTrigger("walkGuard");
            seen = false;
            yield return new WaitForSeconds(waitTime);
            yield return StartCoroutine(LookForPlayer());
        }
    }

    IEnumerator LookForPlayer()
    {
        //kafasini saga sola dondurt
        yield return StartCoroutine(FollowPath());
    }


    //g�rmesi i�in 3 fakt�r var: viewDistance i�inde olacak, viewAngle i�inde olacak, Arada engel olmayacak
    bool CanSeePlayer()
    {
        if(Vector3.Distance(transform.position, player.position) < viewDistance)
        {
            //Guard'�n bakt��� y�n ile player'a bakt��� y�n aras�ndaki a�� viewAngle'dan d���k m�
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            float angleBetweenGuardAndPlayer = Vector3.Angle(transform.forward, directionToPlayer);
            if(angleBetweenGuardAndPlayer < viewAngle / 2f)
            {
                // Guard ile Player aras�nda viewMask:Obstacle varm�
                if(!Physics.Linecast(transform.position, player.position, viewMask))
                {
                    //Son g�rd��� konumu kaydet
                    lastSeen = new Vector3(player.position.x, player.position.y, player.position.z);
                    StopAllCoroutines();
                    return true;
                }
            }
        }
        return false;
    }


    IEnumerator FollowPath() //waypointler aras� gezmesi i�in
    {
        //transform.position = waypoints[0]; // ilk �nce 0.indexe git
        //int targetWaypointIndex = 1; // sonraki waypoint indexi
        //Vector3 targetWaypoint = waypoints[targetWaypointIndex];
        //transform.LookAt(targetWaypoint);

        int targetWaypointIndex = 0;
        Vector3 targetWaypoint = waypoints[targetWaypointIndex];
        yield return StartCoroutine(TurnToFace(targetWaypoint));
        
        while (true)
        {
            animator.SetTrigger("walkGuard");
            animator.ResetTrigger("idleGuard");
            transform.position = Vector3.MoveTowards(transform.position, targetWaypoint, speed * Time.deltaTime);
            
            if (transform.position == targetWaypoint)
            {
                animator.SetTrigger("idleGuard");
                animator.ResetTrigger("walkGuard");
                targetWaypointIndex = (targetWaypointIndex + 1 ) % waypoints.Length; // sonuncu indexe gelince 0'a sar
                targetWaypoint = waypoints[targetWaypointIndex];
                yield return new WaitForSeconds(waitTime); // konuma ula��nca belirtilen s�re kadar bekle
                yield return StartCoroutine(TurnToFace(targetWaypoint));//d�nd��� esnada durmas� i�in
            }
            yield return null;
        }
    }


    IEnumerator TurnToFace(Vector3 lookTarget) //bir sonraki konuma bakmak i�in kafa �evirme
    {
        //normalized: Vector3 de�i�kenini al�p, x,y,z bile�en de�erlerini vekt�r�n boyutu 1 birim olacak
        //�ekilde hesaplar. Vector�n sadece b�y�kl���n� de�i�tirir, y�n�nde bir de�i�iklik olmaz.
        Vector3 directionToLookTarget = (lookTarget - transform.position).normalized;
        float targetAngle= 90 - Mathf.Atan2(directionToLookTarget.z, directionToLookTarget.x) * Mathf.Rad2Deg;

        while(Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.y, targetAngle)) > 0.05f)
        {
            float angle = Mathf.MoveTowardsAngle(transform.eulerAngles.y, targetAngle, turnSpeed * Time.deltaTime);
            transform.eulerAngles = Vector3.up * angle;
            yield return null;
        }
    }


    private void OnDrawGizmos() // g�r�nmeyen nesneleri g�rmek i�in--waypointler ve aralar�ndaki �izgiler
    {
        Vector3 startPosition = pathHolder.GetChild(0).position;
        Vector3 previousPosition = startPosition;

        foreach (Transform waypoint in pathHolder)
        {
            Gizmos.DrawSphere(waypoint.position, .3f);
            Gizmos.DrawLine(previousPosition, waypoint.position);
            previousPosition = waypoint.position;
        }
        Gizmos.DrawLine(previousPosition,startPosition);

        //guard'�n ger�ek g�r�� a��s� kadar uzunlukta bir �izgi
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * viewDistance);
    }

}
