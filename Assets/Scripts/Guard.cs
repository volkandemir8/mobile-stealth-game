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
    public static event System.Action onGuardSpottedPlayer; //Guard, Player'ý gördüðünde çaðýrdýðý static event

    public Animator animator;
    public float speed; //hýz
    public float waitTime; //belirtilen noktada bekleme süresi
    public float turnSpeed; //sonraki noktaya kafasýný çevirme hýzý
    public float timeToSpotPlayer; // kaç saniye içinde oyuncuyu fark edecek
    public float viewDistance; //görüþ mesafesi (spotlight menzilindan daha kýsa) 
    public Light spotlight;
    public LayerMask viewMask; //player ile guard arasýna "Obstacle" layeri gelirse göremeyecek

    float viewAngle; // guard görüþ açýsý
    float playerVisibleTimer; // Player'ý gördüðü zaman baþlayan sayaç

    public Transform pathHolder; //hareket edilecek noktalar
    Transform player;
    Color originalSpotlightColor;

    public UnityEngine.UI.Slider slider;
    bool seen;
    Vector3 lastSeen; //Son görülen konum
    Vector3[] waypoints;
    public Animator playerAnimation;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform; //player transformuna eriþmek için
        viewAngle = spotlight.spotAngle; //spotlight ile ayný geniþliðe sahip
        originalSpotlightColor = spotlight.color;

        // waypointlerin konumlarýný içeren bir dizi oluþturmak için
        waypoints = new Vector3[pathHolder.childCount];
        for (int i = 0; i < waypoints.Length; i++)
        {
            waypoints[i] = pathHolder.GetChild(i).position;
            //waypointler y ekseninde guard'la ayný yüksekliðe sahip olsun diye (yoksa guard yerin altýna batar)
            waypoints[i] = new Vector3(waypoints[i].x, transform.position.y, waypoints[i].z);
        }
        StartCoroutine(FollowPath());
    }

    private void Update()
    {
        slider.value = playerVisibleTimer;
        //playerVisibleTimer'ýn min ev max deðerini belirtme
        playerVisibleTimer = Mathf.Clamp(playerVisibleTimer, 0, timeToSpotPlayer);
        //playerVisibleTimer 0 olduðunda renk orijinal halde, timeToSpotPlayer'a eþit olduðunda kýrmýzý
        spotlight.color = Color.Lerp(originalSpotlightColor, Color.red, playerVisibleTimer / timeToSpotPlayer);


        //Player'ý gördüðü zaman ileri saymaya baþlayacak, görüþ alanýndan çýkýnca ise geriye 
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
        if (Vector3.Distance(transform.position, player.position) > viewDistance / 5)//çok yakýnda deðilse yürüyor
        {
            animator.SetTrigger("walkGuard");
            animator.ResetTrigger("idleGuard");
            transform.position = Vector3.MoveTowards(transform.position, player.position, speed * Time.deltaTime);
        }

        yield return null;
    }

    IEnumerator SearchPlayer() // Görüþ açýsýndan çýkýldýktan sonra player'ýn son konumua gitmek için
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


    //görmesi için 3 faktör var: viewDistance içinde olacak, viewAngle içinde olacak, Arada engel olmayacak
    bool CanSeePlayer()
    {
        if(Vector3.Distance(transform.position, player.position) < viewDistance)
        {
            //Guard'ýn baktýðý yön ile player'a baktýðý yön arasýndaki açý viewAngle'dan düþük mü
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            float angleBetweenGuardAndPlayer = Vector3.Angle(transform.forward, directionToPlayer);
            if(angleBetweenGuardAndPlayer < viewAngle / 2f)
            {
                // Guard ile Player arasýnda viewMask:Obstacle varmý
                if(!Physics.Linecast(transform.position, player.position, viewMask))
                {
                    //Son gördüðü konumu kaydet
                    lastSeen = new Vector3(player.position.x, player.position.y, player.position.z);
                    StopAllCoroutines();
                    return true;
                }
            }
        }
        return false;
    }


    IEnumerator FollowPath() //waypointler arasý gezmesi için
    {
        //transform.position = waypoints[0]; // ilk önce 0.indexe git
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
                yield return new WaitForSeconds(waitTime); // konuma ulaþýnca belirtilen süre kadar bekle
                yield return StartCoroutine(TurnToFace(targetWaypoint));//döndüðü esnada durmasý için
            }
            yield return null;
        }
    }


    IEnumerator TurnToFace(Vector3 lookTarget) //bir sonraki konuma bakmak için kafa çevirme
    {
        //normalized: Vector3 deðiþkenini alýp, x,y,z bileþen deðerlerini vektörün boyutu 1 birim olacak
        //þekilde hesaplar. Vectorün sadece büyüklüðünü deðiþtirir, yönünde bir deðiþiklik olmaz.
        Vector3 directionToLookTarget = (lookTarget - transform.position).normalized;
        float targetAngle= 90 - Mathf.Atan2(directionToLookTarget.z, directionToLookTarget.x) * Mathf.Rad2Deg;

        while(Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.y, targetAngle)) > 0.05f)
        {
            float angle = Mathf.MoveTowardsAngle(transform.eulerAngles.y, targetAngle, turnSpeed * Time.deltaTime);
            transform.eulerAngles = Vector3.up * angle;
            yield return null;
        }
    }


    private void OnDrawGizmos() // görünmeyen nesneleri görmek için--waypointler ve aralarýndaki çizgiler
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

        //guard'ýn gerçek görüþ açýsý kadar uzunlukta bir çizgi
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * viewDistance);
    }

}
