using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
public class TankController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed;
    private Rigidbody rb;
    private Vector2 moveInput;
    private FixedJoystick joystick;

    [Header("Health")]
    public Slider hpBar;
    [HideInInspector] public int health;
    [HideInInspector] public int maxHealth = 100;


    [Header("Shooting")]
    public ShootPointController shootPoint;

    [Header("Sound")]
    public AudioClip tankRunSound;
    public AudioClip tankIdleSound;
    private AudioSource tankSound;

    public CanvaController canvaController;
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        tankSound = GetComponent<AudioSource>();
        joystick = GameObject.FindWithTag("Joystick").GetComponent<FixedJoystick>();
        health = maxHealth;
        hpBar.value = health;
    }
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
    public void OnShoot(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        shootPoint.Shoot();
    }
    //void FixedUpdate()
    //{
    //    Vector3 moveDir = new Vector3(moveInput.x, 0f, moveInput.y);

    //    bool isMoving = moveInput.sqrMagnitude > 0.01f;

    //    if (isMoving)
    //    {
    //        if (tankSound.clip != tankRunSound)
    //        {
    //            tankSound.clip = tankRunSound;
    //            tankSound.loop = true;
    //            tankSound.Play();
    //        }
    //    }
    //    else
    //    {
    //        if (tankSound.clip != tankIdleSound)
    //        {
    //            tankSound.clip = tankIdleSound;
    //            tankSound.loop = true;
    //            tankSound.Play();
    //        }
    //    }

    //    if (moveDir.sqrMagnitude > 1f)
    //        moveDir.Normalize();

    //    if (moveInput.sqrMagnitude > 0.001f)
    //    {
    //        float angleY = Mathf.Atan2(moveInput.x, moveInput.y) * Mathf.Rad2Deg;
    //        transform.rotation = Quaternion.Euler(0f, angleY, 0f);
    //    }

    //    rb.linearVelocity = new Vector3(moveDir.x * moveSpeed, rb.linearVelocity.y, moveDir.z * moveSpeed);
    //}
    void FixedUpdate()
    {
        // 1️⃣ Lấy input từ joystick
        Vector2 input = new Vector2(joystick.Horizontal, joystick.Vertical);

        // 2️⃣ Kiểm tra có đang di chuyển không
        bool isMoving = input.sqrMagnitude > 0.01f;

        // 3️⃣ Âm thanh chạy / đứng yên
        if (isMoving)
        {
            if (tankSound.clip != tankRunSound)
            {
                tankSound.clip = tankRunSound;
                tankSound.loop = true;
                tankSound.Play();
            }
        }
        else
        {
            if (tankSound.clip != tankIdleSound)
            {
                tankSound.clip = tankIdleSound;
                tankSound.loop = true;
                tankSound.Play();
            }
        }

        // 4️⃣ Vector di chuyển
        Vector3 moveDir = new Vector3(input.x, 0f, input.y);

        // 5️⃣ Giới hạn tốc độ khi joystick chéo
        if (moveDir.sqrMagnitude > 1f)
            moveDir.Normalize();

        // 6️⃣ Xoay tank theo hướng joystick
        if (isMoving)
        {
            float angleY = Mathf.Atan2(input.x, input.y) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, angleY, 0f);
        }

        // 7️⃣ Áp vận tốc cho Rigidbody
        rb.linearVelocity = new Vector3(
            moveDir.x * moveSpeed,
            rb.linearVelocity.y,
            moveDir.z * moveSpeed
        );
    }


    private void TakeDamage(int damage)
    {
        health -= damage;
        hpBar.value = health;
        if (health <= 0)
        {
            canvaController.ShowGameOverPanel();
        }
    }
    private void GetHeal(int heal)
    {
        health += heal;
        if (health > 100) health = 100;
        hpBar.value = health;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("EnemyBullet"))
        {
            TakeDamage(5);
        }
        //if (other.CompareTag("EnemyCanonBullet"))
        //{
        //    TakeDamage(20);
        //}
    }
}
