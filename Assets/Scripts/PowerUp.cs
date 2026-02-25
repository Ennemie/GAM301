using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

public class PowerUp : MonoBehaviour
{
    public enum PowerUpType
    {
        SpeedBoost,
        ShootingBoost,
        Heal
    }
    public PowerUpType powerUpType;

    private float rotateSpeed = 50f;
    private TankController tankPlayer;
    private ShootPointController tankShootPoint;

    public float speedBoostDuration;
    public float shootingBoostDuration;

    private float moveSpeedOG;
    private int healthOG;
    private float fireCooldownOG;

    private CanvaController canvaController;
    private void Awake()
    {
        tankPlayer = GameObject.FindWithTag("Player").GetComponent<TankController>();
        tankShootPoint = GameObject.FindWithTag("TankShootPoint").GetComponent<ShootPointController>();
        canvaController = GameObject.FindWithTag("Canva").GetComponent<CanvaController>();

        moveSpeedOG = tankPlayer.moveSpeed;
        healthOG = tankPlayer.health;
        fireCooldownOG = tankShootPoint.fireCooldown;
    }
    void Update()
    {
        transform.Rotate(0f, rotateSpeed * Time.deltaTime, 0f, Space.World);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        switch (powerUpType)
        {
            case PowerUpType.SpeedBoost:
                tankPlayer.moveSpeed = moveSpeedOG * 1.5f;
                canvaController.SwitchSpeedUpIcon();

                CancelInvoke(nameof(ResetSpeed));
                Invoke(nameof(ResetSpeed), speedBoostDuration);
                break;
            case PowerUpType.ShootingBoost:
                tankShootPoint.fireCooldown = fireCooldownOG * 0.5f;
                canvaController.SwitchFireUpIcon();

                CancelInvoke(nameof(ResetFireCooldown));
                Invoke(nameof(ResetFireCooldown), shootingBoostDuration);
                break;
            case PowerUpType.Heal:
                if (tankPlayer.health >= tankPlayer.maxHealth) return;
                tankPlayer.health += 30;
                tankPlayer.hpBar.value = tankPlayer.health;
                if (tankPlayer.health > tankPlayer.maxHealth)
                {
                    tankPlayer.health = tankPlayer.maxHealth;
                    tankPlayer.hpBar.value = tankPlayer.maxHealth;
                }
                break;
        }
        gameObject.SetActive(false);
    }
    private void ResetSpeed()
    {
        tankPlayer.moveSpeed = moveSpeedOG;
        canvaController.SwitchSpeedUpIcon();
        Destroy(gameObject);
    }
    private void ResetFireCooldown()
    {
        tankShootPoint.fireCooldown = fireCooldownOG;
        canvaController.SwitchFireUpIcon();
        Destroy(gameObject);
    }
}
