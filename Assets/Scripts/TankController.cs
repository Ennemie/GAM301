using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AudioSource))] // Đảm bảo luôn có AudioSource
public class TankController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed;
    private Rigidbody rb;
    [HideInInspector] public Vector2 keyboardInput;
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

        // Thiết lập mặc định cho AudioSource
        tankSound.playOnAwake = true;
        tankSound.loop = true;
        tankSound.spatialBlend = 0f; // Để 0 nếu muốn nghe rõ (2D), để 1 nếu muốn âm thanh 3D

        joystick = GameObject.FindWithTag("Joystick")?.GetComponent<FixedJoystick>();
        health = maxHealth;
        hpBar.value = health;
    }

    void Start()
    {
        // Phát âm thanh Idle ngay khi bắt đầu
        if (tankIdleSound != null)
        {
            tankSound.clip = tankIdleSound;
            tankSound.Play();
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        keyboardInput = context.ReadValue<Vector2>();
    }

    public void OnShoot(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        shootPoint.Shoot();
    }

    void FixedUpdate()
    {
        Vector2 finalInput = keyboardInput;

        if (joystick != null && joystick.Direction.sqrMagnitude > 0.01f)
        {
            finalInput = joystick.Direction;
        }

        Vector3 moveDir = new Vector3(finalInput.x, 0f, finalInput.y);
        bool isMoving = finalInput.sqrMagnitude > 0.01f;

        HandleMovementSound(isMoving);

        if (isMoving)
        {
            if (moveDir.sqrMagnitude > 1f) moveDir.Normalize();
            float angleY = Mathf.Atan2(finalInput.x, finalInput.y) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, angleY, 0f);
        }

        rb.linearVelocity = new Vector3(moveDir.x * moveSpeed, rb.linearVelocity.y, moveDir.z * moveSpeed);
    }

    private void HandleMovementSound(bool isMoving)
    {
        AudioClip clipToPlay = isMoving ? tankRunSound : tankIdleSound;

        // Chỉ đổi clip và Play lại nếu clip hiện tại khác với clip cần phát
        if (tankSound.clip != clipToPlay && clipToPlay != null)
        {
            tankSound.Stop();
            tankSound.clip = clipToPlay;
            tankSound.Play();
        }
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        hpBar.value = health;
        if (health <= 0) canvaController.ShowGameOverPanel();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("EnemyBullet")) TakeDamage(5);
    }
}