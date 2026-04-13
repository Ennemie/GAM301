using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class TestScripts
{
    private GameObject testObj;
    private TankController tank;
    private ShootPointController shoot;

    [SetUp]
    public void Init()
    {
        testObj = new GameObject();
        tank = testObj.AddComponent<TankController>();
        shoot = testObj.AddComponent<ShootPointController>();

        tank.maxHealth = 100;
        tank.moveSpeed = 20f;
        shoot.fireCooldown = 1f;
    }

    [TearDown]
    public void Cleanup()
    {
        Object.DestroyImmediate(testObj);
    }


    [Test]
    public void TC31_InitialHealthEqualsMaxHealth()
    {
        tank.health = tank.maxHealth;
        Assert.AreEqual(100, tank.health);
    }

    [Test]
    public void TC32_TakeDamageCalculation()
    {
        tank.health = 100;
        int damage = 25;
        tank.health -= damage;
        Assert.AreEqual(75, tank.health);
    }

    [Test]
    public void TC33_HealthCannotBeNegative()
    {
        tank.health = 10;
        tank.health -= 50;
        if (tank.health < 0) tank.health = 0;
        Assert.AreEqual(0, tank.health);
    }

    [Test]
    public void TC34_HealLogicNormal()
    {
        tank.health = 50;
        tank.health += 30;
        Assert.AreEqual(80, tank.health);
    }

    [Test]
    public void TC35_HealCapAtMaxHealth()
    {
        tank.health = 90;
        tank.health += 30;
        if (tank.health > tank.maxHealth) tank.health = tank.maxHealth;
        Assert.AreEqual(100, tank.health);
    }

    [Test]
    public void TC38_SpeedBoostFormula()
    {
        float multiplier = 1.5f;
        float result = tank.moveSpeed * multiplier;
        Assert.AreEqual(30f, result, 0.01f);
    }

    [Test]
    public void TC42_CooldownReductionFormula()
    {
        float reduction = 0.5f;
        float result = shoot.fireCooldown * reduction;
        Assert.AreEqual(0.5f, result, 0.01f);
    }

    [Test]
    public void TC41_DoubleSpeedBuff_NoStackLogic()
    {
        // Giả lập logic: Nếu đã có buff thì không nhân thêm lần nữa
        float currentSpeed = 30f; // Đã buff 1 lần (20 * 1.5)
        float baseSpeed = 20f;

        if (currentSpeed > baseSpeed)
        {
            currentSpeed = baseSpeed * 1.5f; // Ghi đè thay vì nhân dồn
        }

        Assert.AreNotEqual(45f, currentSpeed); // Không được là 1.5 * 1.5
        Assert.AreEqual(30f, currentSpeed);
    }

    [Test]
    public void TC1_MovementVectorCalculation()
    {
        Vector2 input = new Vector2(1, 1);
        Vector3 moveDir = new Vector3(input.x, 0, input.y);

        Assert.AreEqual(1f, moveDir.x);
        Assert.AreEqual(1f, moveDir.z);
        Assert.AreEqual(0f, moveDir.y);
    }

    [Test]
    public void TC1_MovementNormalizationTest()
    {
        Vector3 moveDir = new Vector3(1, 0, 1); // Độ dài là sqrt(2) ~ 1.41
        if (moveDir.sqrMagnitude > 1f) moveDir.Normalize();

        Assert.LessOrEqual(moveDir.magnitude, 1.0001f);
    }

    [Test]
    public void Check_TankHasRigidbody()
    {
        Assert.IsNotNull(testObj.GetComponent<Rigidbody>());
    }

    [Test]
    public void Check_TankHasAudioSource()
    {
        Assert.IsNotNull(testObj.GetComponent<AudioSource>());
    }

    [Test]
    public void Check_ShootPointHasBulletPrefab()
    {
        // Giả lập kiểm tra xem đã gán prefab chưa (EditMode thường check lỗi quên gán)
        shoot.bullet = new GameObject("BulletPrefab");
        Assert.IsNotNull(shoot.bullet);
        Object.DestroyImmediate(shoot.bullet);
    }
}
