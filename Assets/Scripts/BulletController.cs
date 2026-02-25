using System.Collections;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    public float bulletSpeed;
    public ParticleSystem explodeEffect;
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        rb.linearVelocity = transform.forward * bulletSpeed;
        StartCoroutine(DestroyBulletAfterTime(2f));
    }

    IEnumerator DestroyBulletAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        gameObject.SetActive(false);
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player") || other.CompareTag("PowerUp")) return;
        else
        {
            Instantiate(explodeEffect, transform.position, Quaternion.identity);
            gameObject.SetActive(false);
        }
    }
}
