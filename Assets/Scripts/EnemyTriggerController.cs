using UnityEngine;
using System.Collections.Generic;


public class EnemyTriggerController : MonoBehaviour
{
    public List<GameObject> enemies;

    private void Start()
    {
        foreach (var enemy in enemies)
        {
            enemy.SetActive(false);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            foreach (var enemy in enemies)
            {
                enemy.SetActive(true);
            }
            Destroy(gameObject);
        }
    }
}
