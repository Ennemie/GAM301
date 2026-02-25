using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyMechaController : MonoBehaviour
{
    private Animator animator;
    public ShootPointController shootPointController;
    private GameObject player;
    private NavMeshAgent agent;
    private int enemyHealth = 100;
    private bool canShoot = true;
    void Start()
    {
        animator = GetComponent<Animator>();
        animator.SetBool("isIdle", true);
        player = GameObject.FindWithTag("Player");
        agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Vector3.Distance(transform.position, player.transform.position) > 20f)
        {
            animator.SetBool("isIdle", false);
            animator.SetBool("isShooting", false);
            animator.SetBool("isWalking", true);
            agent.SetDestination(player.transform.position);
        }
        else
        {
            if (canShoot)
            {
                StartCoroutine(Shoot());
            }
        }
    }
    private IEnumerator Shoot()
    {
        agent.ResetPath();
        LookAtPlayer();
        animator.SetBool("isIdle", false);
        animator.SetBool("isShooting", true);
        animator.SetBool("isWalking", false);
        shootPointController.Shoot();
        canShoot = false;
        yield return new WaitForSeconds(3f);
        canShoot = true;
        animator.SetBool("isShooting", false);
        animator.SetBool("isWalking", true);
    }
    void LookAtPlayer()
    {
        Vector3 dir = player.transform.position - transform.position;
        dir.y = 0;

        if (dir != Vector3.zero)
        {
            Quaternion rot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * 5f);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bullet"))
        {
            enemyHealth -= 25;
            if (enemyHealth <= 0)
            {
                animator.SetBool("isDead", true);
                StartCoroutine(DestroyMechaAfterDeath(2f));
            }
        }
    }
    private IEnumerator DestroyMechaAfterDeath(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }
}
