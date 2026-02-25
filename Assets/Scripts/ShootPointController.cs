using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections.Generic;

public class ShootPointController : MonoBehaviour
{
    public GameObject bullet;
    public Slider fireBar;
    public ParticleSystem shootEffect;
    public float fireCooldown = 1f;
    private bool canFire = true;

    private int bulletPoolSize = 10;
    private GameObject[] bullets;

    [HideInInspector]
    public AudioSource shootSound;

    void Start()
    {
        if (fireBar != null) fireBar.value = 1f;
        shootSound = GetComponent<AudioSource>();

        bullets = new GameObject[bulletPoolSize];
        for (int i = 0; i < bulletPoolSize; i++)
        {
            bullets[i] = Instantiate(bullet);
            bullets[i].SetActive(false);
        }
    }

    public void Shoot()
    {
        if (!canFire) return;
        for (int i = 0; i < bulletPoolSize; i++)
        {
            if (!bullets[i].activeInHierarchy)
            {
                bullets[i].transform.position = transform.position;
                bullets[i].transform.rotation = transform.rotation;
                bullets[i].SetActive(true);
                break;
            }
        }

        shootEffect.Play();
        shootSound.Play();
        StartCoroutine(FireCoolDown(fireCooldown));
    }

    private IEnumerator FireCoolDown(float cooldownTime)
    {
        canFire = false;
        if(fireBar!=null) fireBar.value = 0f;

        float timer = 0f;
        while (timer < cooldownTime)
        {
            timer += Time.deltaTime;
            fireBar.value = timer / cooldownTime;
            yield return null;
        }

        fireBar.value = 1f;
        canFire = true;
    }
}
