using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

[Category("Item")]
public class ItemManagerTestScripts
{
    private GameObject playerTank;
    private TankController tankController;
    private ShootPointController shootController;
    private CanvaController canvaController;
    private PowerUp powerUpPrefab;

    [UnitySetUp]
    public IEnumerator Setup()
    {
        // Clean up unwanted objects from previous scenes
        GameObject openingScene = GameObject.Find("OpeningScene");
        if (openingScene != null) openingScene.SetActive(false);

        GameObject helicopter = GameObject.Find("Helicopter");
        if (helicopter != null) helicopter.SetActive(false);

        // Load GamePlay scene
        AsyncOperation load = SceneManager.LoadSceneAsync("GamePlay");
        yield return new WaitUntil(() => load.isDone);

        // Wait for scripts to initialize
        yield return new WaitForSeconds(1f);

        // Find player tank and controllers
        playerTank = GameObject.FindWithTag("Player");
        Assert.IsNotNull(playerTank, "Player tank not found. Make sure 'Player' tagged GameObject exists in GamePlay scene.");

        if (playerTank != null)
        {
            tankController = playerTank.GetComponent<TankController>();
            Assert.IsNotNull(tankController, "TankController component not found on player tank");

            shootController = playerTank.GetComponentInChildren<ShootPointController>();
            Assert.IsNotNull(shootController, "ShootPointController component not found on player tank");
        }

        // Find canvas controller
        canvaController = UnityEngine.Object.FindFirstObjectByType<CanvaController>();
        Assert.IsNotNull(canvaController, "CanvaController not found in scene");
    }

    [UnityTearDown]
    public IEnumerator Teardown()
    {
        if (tankController != null)
        {
            tankController.keyboardInput = Vector2.zero;
        }
        yield return null;
    }

    [UnityTest]
    public IEnumerator TC36_HealBuffIncreasesHealthWhenDamaged()
    {
        // TC36: Player has taken damage - Pick up heal buff item - Health increases by correct value
        Assert.IsNotNull(playerTank, "Player tank is null");
        Assert.IsNotNull(tankController, "Tank controller is null");

        // Set health to damaged state
        tankController.health = 40;
        tankController.hpBar.value = tankController.health;

        int healthBefore = tankController.health;
        int expectedHealAmount = 30;

        // Create and trigger heal buff
        PowerUp healBuff = CreatePowerUp(PowerUp.PowerUpType.Heal, playerTank.transform.position);
        yield return new WaitForSeconds(0.1f);

        // Verify health increased
        int expectedHealth = Mathf.Min(healthBefore + expectedHealAmount, tankController.maxHealth);
        Assert.AreEqual(expectedHealth, tankController.health, "Health should increase by heal amount");

        // Verify item disappeared
        Assert.IsFalse(healBuff.gameObject.activeInHierarchy, "Heal buff item should disappear after pickup");
    }

    [UnityTest]
    public IEnumerator TC37_HealBuffDoesNotActivateWhenHealthFull()
    {
        // TC37: Player at full health - Pick up heal buff - Health unchanged, item should not disappear
        Assert.IsNotNull(playerTank, "Player tank is null");
        Assert.IsNotNull(tankController, "Tank controller is null");

        // Set health to maximum
        tankController.health = tankController.maxHealth;
        tankController.hpBar.value = tankController.health;

        float healthBefore = tankController.health;

        // Create heal buff
        PowerUp healBuff = CreatePowerUp(PowerUp.PowerUpType.Heal, playerTank.transform.position);
        yield return new WaitForSeconds(0.2f);

        // Verify health unchanged
        Assert.AreEqual((int)healthBefore, tankController.health, "Health should not change when at full health");

        // Verify item is still active (not consumed)
        Assert.IsTrue(healBuff.gameObject.activeInHierarchy, "Heal buff item should remain active when player at full health");
    }

    [UnityTest]
    public IEnumerator TC38_SpeedBuffIncreasesMoveSpeed()
    {
        // TC38: Player at normal speed - Pick up speed buff - Speed increases
        Assert.IsNotNull(playerTank, "Player tank is null");
        Assert.IsNotNull(tankController, "Tank controller is null");

        float originalSpeed = tankController.moveSpeed;

        // Create speed buff
        PowerUp speedBuff = CreatePowerUp(PowerUp.PowerUpType.SpeedBoost, playerTank.transform.position);
        yield return new WaitForSeconds(0.2f);

        // Verify speed increased to 1.5x
        float expectedSpeed = originalSpeed * 1.5f;
        Assert.AreEqual(expectedSpeed, tankController.moveSpeed, 0.01f, "Speed should increase to 1.5x when buff is picked up");

        // Verify speed icon is active
        Assert.IsTrue(canvaController.speedUpIcon.gameObject.activeInHierarchy, "Speed up icon should be active");

        // Verify item disappeared
        Assert.IsFalse(speedBuff.gameObject.activeInHierarchy, "Speed buff item should disappear after pickup");
    }

    [UnityTest]
    public IEnumerator TC39_MultipleSpeedBuffsDontStackDuration()
    {
        // TC39: Pick up multiple speed buffs - Speed increases but buff time doesn't stack
        Assert.IsNotNull(playerTank, "Player tank is null");
        Assert.IsNotNull(tankController, "Tank controller is null");

        float originalSpeed = tankController.moveSpeed;

        // Pick up first speed buff
        PowerUp speedBuff1 = CreatePowerUp(PowerUp.PowerUpType.SpeedBoost, playerTank.transform.position);
        yield return new WaitForSeconds(0.2f);

        float boostedSpeed = tankController.moveSpeed;
        Assert.AreEqual(originalSpeed * 1.5f, boostedSpeed, 0.01f, "First buff should increase speed");

        // Pick up second speed buff - should reset timer, not increase further
        PowerUp speedBuff2 = CreatePowerUp(PowerUp.PowerUpType.SpeedBoost, playerTank.transform.position + Vector3.forward);
        yield return new WaitForSeconds(0.2f);

        // Speed should still be 1.5x (not 2.25x from stacking)
        Assert.AreEqual(originalSpeed * 1.5f, tankController.moveSpeed, 0.01f, "Speed should remain at 1.5x, not stack");
    }

    [UnityTest]
    public IEnumerator TC40_SpeedBuffDurationIsTemporary()
    {
        // TC40: Pick up speed buff - Speed increases temporarily then returns to normal
        Assert.IsNotNull(playerTank, "Player tank is null");
        Assert.IsNotNull(tankController, "Tank controller is null");

        float originalSpeed = tankController.moveSpeed;
        float speedBoostDuration = 2f; // Assuming 2 second duration

        // Create speed buff with controlled duration
        PowerUp speedBuff = CreatePowerUpWithDuration(PowerUp.PowerUpType.SpeedBoost, playerTank.transform.position, speedBoostDuration);
        yield return new WaitForSeconds(0.2f);

        // Verify speed is boosted
        Assert.AreEqual(originalSpeed * 1.5f, tankController.moveSpeed, 0.01f, "Speed should be boosted");

        // Wait for buff to expire
        yield return new WaitForSeconds(speedBoostDuration + 0.5f);

        // Verify speed returned to original
        Assert.AreEqual(originalSpeed, tankController.moveSpeed, 0.01f, "Speed should return to normal after buff expires");

        // Verify icon is deactivated
        Assert.IsFalse(canvaController.speedUpIcon.gameObject.activeInHierarchy, "Speed up icon should be deactivated");
    }

    [UnityTest]
    public IEnumerator TC41_SpeedBuffTimerResetsOnNewPickup()
    {
        // TC41: Pick up speed buff - During buff - Pick up another speed buff - Timer resets not stacks
        Assert.IsNotNull(playerTank, "Player tank is null");
        Assert.IsNotNull(tankController, "Tank controller is null");

        float originalSpeed = tankController.moveSpeed;
        float speedBoostDuration = 1.5f;

        // Pick up first buff
        PowerUp speedBuff1 = CreatePowerUpWithDuration(PowerUp.PowerUpType.SpeedBoost, playerTank.transform.position, speedBoostDuration);
        yield return new WaitForSeconds(0.2f);

        float boostedSpeed = tankController.moveSpeed;
        Assert.AreEqual(originalSpeed * 1.5f, boostedSpeed, 0.01f, "Speed should be boosted");

        // Wait 0.8 seconds (half duration passed)
        yield return new WaitForSeconds(0.8f);

        // Pick up second buff - this resets the timer
        PowerUp speedBuff2 = CreatePowerUpWithDuration(PowerUp.PowerUpType.SpeedBoost, playerTank.transform.position + Vector3.forward, speedBoostDuration);
        yield return new WaitForSeconds(0.2f);

        // Wait another 1.2 seconds - if stacked would still be boosted, if reset will be back to normal
        yield return new WaitForSeconds(1.2f);

        // Speed should be back to normal if timer was reset (1.4 seconds total > 1.5 second duration from second pickup)
        Assert.AreEqual(originalSpeed, tankController.moveSpeed, 0.01f, "Speed should return to normal (buff timer was reset, not stacked)");
    }

    [UnityTest]
    public IEnumerator TC42_FireCooldownBuffDecreasesCooldownTime()
    {
        // TC42: Pick up fire rate buff - Weapon cooldown decreases (shoots faster)
        Assert.IsNotNull(playerTank, "Player tank is null");
        Assert.IsNotNull(shootController, "Shoot controller is null");

        float originalCooldown = shootController.fireCooldown;

        // Create fire cooldown buff
        PowerUp cooldownBuff = CreatePowerUp(PowerUp.PowerUpType.ShootingBoost, playerTank.transform.position);
        yield return new WaitForSeconds(0.2f);

        // Verify cooldown decreased to 0.5x
        float expectedCooldown = originalCooldown * 0.5f;
        Assert.AreEqual(expectedCooldown, shootController.fireCooldown, 0.01f, "Cooldown should decrease to 0.5x when buff is picked up");

        // Verify fire up icon is active
        Assert.IsTrue(canvaController.fireUpIcon.gameObject.activeInHierarchy, "Fire up icon should be active");

        // Verify item disappeared
        Assert.IsFalse(cooldownBuff.gameObject.activeInHierarchy, "Cooldown buff item should disappear after pickup");
    }

    [UnityTest]
    public IEnumerator TC43_FireCooldownBuffReturnsToNormalAfterDuration()
    {
        // TC43: Pick up fire rate buff - Cooldown decreases - After duration expires - Cooldown returns to normal
        Assert.IsNotNull(playerTank, "Player tank is null");
        Assert.IsNotNull(shootController, "Shoot controller is null");

        float originalCooldown = shootController.fireCooldown;
        float shootBoostDuration = 2f;

        // Create fire cooldown buff
        PowerUp cooldownBuff = CreatePowerUpWithDuration(PowerUp.PowerUpType.ShootingBoost, playerTank.transform.position, shootBoostDuration);
        yield return new WaitForSeconds(0.2f);

        // Verify cooldown decreased
        Assert.AreEqual(originalCooldown * 0.5f, shootController.fireCooldown, 0.01f, "Cooldown should be decreased");

        // Wait for buff to expire
        yield return new WaitForSeconds(shootBoostDuration + 0.5f);

        // Verify cooldown returned to original
        Assert.AreEqual(originalCooldown, shootController.fireCooldown, 0.01f, "Cooldown should return to normal after buff expires");

        // Verify icon is deactivated
        Assert.IsFalse(canvaController.fireUpIcon.gameObject.activeInHierarchy, "Fire up icon should be deactivated");
    }

    [UnityTest]
    public IEnumerator TC44_MultipleFireCooldownBuffsDontStack()
    {
        // TC44: Pick up multiple fire rate buffs - Cooldown decreases but duration doesn't stack
        Assert.IsNotNull(playerTank, "Player tank is null");
        Assert.IsNotNull(shootController, "Shoot controller is null");

        float originalCooldown = shootController.fireCooldown;
        float shootBoostDuration = 1.5f;

        // Pick up first cooldown buff
        PowerUp cooldownBuff1 = CreatePowerUpWithDuration(PowerUp.PowerUpType.ShootingBoost, playerTank.transform.position, shootBoostDuration);
        yield return new WaitForSeconds(0.2f);

        Assert.AreEqual(originalCooldown * 0.5f, shootController.fireCooldown, 0.01f, "First buff should decrease cooldown");

        // Wait 0.8 seconds
        yield return new WaitForSeconds(0.8f);

        // Pick up second cooldown buff - resets timer
        PowerUp cooldownBuff2 = CreatePowerUpWithDuration(PowerUp.PowerUpType.ShootingBoost, playerTank.transform.position + Vector3.forward, shootBoostDuration);
        yield return new WaitForSeconds(0.2f);

        // Verify cooldown is still 0.5x (not 0.25x from stacking)
        Assert.AreEqual(originalCooldown * 0.5f, shootController.fireCooldown, 0.01f, "Cooldown should remain at 0.5x, not stack");

        // Wait another 1.2 seconds - if timer was reset, should return to normal
        yield return new WaitForSeconds(1.2f);

        Assert.AreEqual(originalCooldown, shootController.fireCooldown, 0.01f, "Cooldown should return to normal (duration reset, not stacked)");
    }

    [UnityTest]
    public IEnumerator TC45_MultipleDifferentBuffsWorkTogether()
    {
        // TC45: Pick up speed buff - Pick up fire rate buff - Both buffs active simultaneously
        Assert.IsNotNull(playerTank, "Player tank is null");
        Assert.IsNotNull(tankController, "Tank controller is null");
        Assert.IsNotNull(shootController, "Shoot controller is null");

        float originalSpeed = tankController.moveSpeed;
        float originalCooldown = shootController.fireCooldown;

        // Pick up speed buff
        PowerUp speedBuff = CreatePowerUp(PowerUp.PowerUpType.SpeedBoost, playerTank.transform.position);
        yield return new WaitForSeconds(0.2f);

        // Verify speed boosted
        Assert.AreEqual(originalSpeed * 1.5f, tankController.moveSpeed, 0.01f, "Speed should be boosted");
        Assert.IsTrue(canvaController.speedUpIcon.gameObject.activeInHierarchy, "Speed icon should be active");

        // Pick up fire rate buff
        PowerUp cooldownBuff = CreatePowerUp(PowerUp.PowerUpType.ShootingBoost, playerTank.transform.position + Vector3.forward);
        yield return new WaitForSeconds(0.2f);

        // Verify both buffs are active
        Assert.AreEqual(originalSpeed * 1.5f, tankController.moveSpeed, 0.01f, "Speed should still be boosted");
        Assert.AreEqual(originalCooldown * 0.5f, shootController.fireCooldown, 0.01f, "Cooldown should be decreased");

        Assert.IsTrue(canvaController.speedUpIcon.gameObject.activeInHierarchy, "Speed icon should be active");
        Assert.IsTrue(canvaController.fireUpIcon.gameObject.activeInHierarchy, "Fire icon should be active");
    }

    // Helper method to create PowerUp
    private PowerUp CreatePowerUp(PowerUp.PowerUpType type, Vector3 position)
    {
        GameObject powerUpObj = new GameObject("TestPowerUp");
        powerUpObj.transform.position = position;

        // Add collider
        SphereCollider collider = powerUpObj.AddComponent<SphereCollider>();
        collider.isTrigger = true;

        // Add PowerUp component
        PowerUp powerUp = powerUpObj.AddComponent<PowerUp>();
        powerUp.powerUpType = type;
        powerUp.speedBoostDuration = 2f;
        powerUp.shootingBoostDuration = 2f;

        return powerUp;
    }

    // Helper method to create PowerUp with specific duration
    private PowerUp CreatePowerUpWithDuration(PowerUp.PowerUpType type, Vector3 position, float duration)
    {
        GameObject powerUpObj = new GameObject("TestPowerUp");
        powerUpObj.transform.position = position;

        // Add collider
        SphereCollider collider = powerUpObj.AddComponent<SphereCollider>();
        collider.isTrigger = true;

        // Add PowerUp component
        PowerUp powerUp = powerUpObj.AddComponent<PowerUp>();
        powerUp.powerUpType = type;
        powerUp.speedBoostDuration = duration;
        powerUp.shootingBoostDuration = duration;

        return powerUp;
    }
}

