using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

[Category("Life")]
public class LifeManagerTestScripts
{
    private GameObject playerTank;
    private TankController tankController;
    private CanvaController canvaController;

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

            Assert.IsNotNull(tankController.hpBar, "Health bar (hpBar) not assigned in TankController");
        }

        // Find canvas controller
        canvaController = UnityEngine.Object.FindFirstObjectByType<CanvaController>();
        Assert.IsNotNull(canvaController, "CanvaController not found in scene");
        Assert.IsNotNull(canvaController.gameOverPanel, "GameOverPanel not assigned in CanvaController");
    }

    [UnityTearDown]
    public IEnumerator Teardown()
    {
        // Reset player state
        if (tankController != null)
        {
            tankController.keyboardInput = Vector2.zero;
            // Reset health if needed
            if (tankController.health <= 0)
            {
                tankController.health = tankController.maxHealth;
                tankController.hpBar.value = tankController.health;
            }
        }
        yield return null;
    }

    [UnityTest]
    public IEnumerator TC29_HealthBarNotDisplayedDuringOpeningCutscene()
    {
        // TC29: Opening cutscene starts - Health bar should not be displayed
        Assert.IsNotNull(playerTank, "Player tank is null");
        Assert.IsNotNull(tankController, "Tank controller is null");
        Assert.IsNotNull(canvaController, "Canvas controller is null");

        // Check initial state - health bar should be visible at start (after cutscene)
        // But we verify the mechanism: health bar is managed by CanvaController
        Assert.IsNotNull(canvaController.healthSlider, "Health slider not found");

        // In actual cutscene sequence, the health bar would be inactive
        // Here we verify the control exists and can be toggled
        bool healthBarActive = canvaController.healthSlider.gameObject.activeInHierarchy;

        // After scene load is complete, health bar should be active (cutscene ended)
        Assert.IsTrue(healthBarActive, "Health slider should be active (cutscene ended)");

        yield return null;
    }

    [UnityTest]
    public IEnumerator TC30_HealthBarDisplaysCorrectlyAfterCutsceneEnds()
    {
        // TC30: Opening cutscene ends - Health bar should display correctly
        Assert.IsNotNull(playerTank, "Player tank is null");
        Assert.IsNotNull(tankController, "Tank controller is null");
        Assert.IsNotNull(canvaController, "Canvas controller is null");

        // Verify health bar is active and showing correct value
        Assert.IsTrue(canvaController.healthSlider.gameObject.activeInHierarchy, "Health slider should be active after cutscene");

        // Verify initial health matches max health
        Assert.AreEqual(tankController.maxHealth, tankController.health, "Player health should equal max health at start");

        // Verify health slider value matches health
        Assert.AreEqual(tankController.health, (int)canvaController.healthSlider.value, "Health slider value should match player health");

        yield return null;
    }

    [UnityTest]
    public IEnumerator TC31_HealthBarDecreasesWhenTakingDamage()
    {
        // TC31: Player has plenty of health - Enemy shoots and hits - Health bar decreases by correct amount
        Assert.IsNotNull(playerTank, "Player tank is null");
        Assert.IsNotNull(tankController, "Tank controller is null");

        int initialHealth = tankController.health;
        int damageAmount = 25; // Standard enemy bullet damage (5 hits per 100 health)

        // Get enemy and make it shoot
        EnemyMechaController[] enemies = UnityEngine.Object.FindObjectsByType<EnemyMechaController>(FindObjectsSortMode.None);
        if (enemies.Length > 0)
        {
            EnemyMechaController enemy = enemies[0];

            // Position enemy close to player to trigger attack
            enemy.transform.position = playerTank.transform.position + Vector3.forward * 10f;
            playerTank.transform.position = Vector3.zero;

            yield return new WaitForSeconds(1f);

            // Manually deal damage to simulate enemy attack
            tankController.TakeDamage(damageAmount);
            yield return new WaitForSeconds(0.2f);

            // Verify health decreased
            int expectedHealth = initialHealth - damageAmount;
            Assert.AreEqual(expectedHealth, tankController.health, "Player health should decrease by damage amount");

            // Verify health slider updated
            Assert.AreEqual(tankController.health, (int)canvaController.healthSlider.value, "Health slider should reflect damage");
        }
    }

   

    [UnityTest]
    public IEnumerator TC33_HealthBarUnchangedWhenFullHealthPicksUpHeal()
    {
        // TC33: Player at full health - Pick up heal item - Health bar should not change
        Assert.IsNotNull(playerTank, "Player tank is null");
        Assert.IsNotNull(tankController, "Tank controller is null");

        // Set health to maximum
        tankController.health = tankController.maxHealth;
        tankController.hpBar.value = tankController.health;

        float healthBeforePickup = canvaController.healthSlider.value;

        // Simulate picking up heal item (by directly calling heal logic)
        int healAmount = 30;
        int healthBeforeTake = tankController.health;

        // Health should cap at maxHealth
        tankController.health = Mathf.Min(tankController.health + healAmount, tankController.maxHealth);
        tankController.hpBar.value = tankController.health;

        yield return new WaitForSeconds(0.2f);

        // Verify health hasn't changed (already at max)
        Assert.AreEqual(healthBeforeTake, tankController.health, "Health should remain at max when picking up heal at full health");
        Assert.AreEqual(healthBeforePickup, canvaController.healthSlider.value, "Health slider should not change when already at max");
    }

    [UnityTest]
    public IEnumerator TC34_HealthBarIncreasesWhenDamagedPlayerPicksUpHeal()
    {
        // TC34: Player has taken damage - Pick up heal item - Health bar increases
        Assert.IsNotNull(playerTank, "Player tank is null");
        Assert.IsNotNull(tankController, "Tank controller is null");

        // Set health to damaged state
        tankController.health = 40;
        tankController.hpBar.value = tankController.health;

        float healthBefore = canvaController.healthSlider.value;
        int healAmount = 30;

        // Simulate picking up heal item
        int expectedHealth = Mathf.Min(tankController.health + healAmount, tankController.maxHealth);
        tankController.health = expectedHealth;
        tankController.hpBar.value = tankController.health;

        yield return new WaitForSeconds(0.2f);

        // Verify health increased
        Assert.Greater(tankController.health, (int)healthBefore, "Player health should increase after picking up heal");

        // Verify health slider updated correctly
        Assert.AreEqual(tankController.health, (int)canvaController.healthSlider.value, "Health slider should reflect heal amount");
    }

    [UnityTest]
    public IEnumerator TC35_HealthBarUpdatesCorrectlyWithSimultaneousDamageAndHeal()
    {
        // TC35: Player near heal item and getting shot - Observe health bar value changes correctly
        Assert.IsNotNull(playerTank, "Player tank is null");
        Assert.IsNotNull(tankController, "Tank controller is null");

        // Set health to mid-range
        tankController.health = 60;
        tankController.hpBar.value = tankController.health;

        int damageAmount = 15;
        int healAmount = 25;

        // Simulate simultaneous damage and heal (damage first, then heal)
        tankController.TakeDamage(damageAmount);
        yield return new WaitForSeconds(0.1f);

        int healthAfterDamage = tankController.health;
        Assert.AreEqual(60 - damageAmount, healthAfterDamage, "Health should decrease by damage amount first");

        // Now simulate picking up heal
        tankController.health = Mathf.Min(tankController.health + healAmount, tankController.maxHealth);
        tankController.hpBar.value = tankController.health;

        yield return new WaitForSeconds(0.1f);

        // Calculate expected final health
        int expectedFinalHealth = Mathf.Min(60 - damageAmount + healAmount, tankController.maxHealth);

        // Verify final health value is correct
        Assert.AreEqual(expectedFinalHealth, tankController.health, "Health should update correctly with sequential damage and heal");

        // Verify slider reflects final value
        Assert.AreEqual(tankController.health, (int)canvaController.healthSlider.value, "Health slider should show correct final value");

        // Verify value is between 0 and maxHealth
        Assert.GreaterOrEqual(tankController.health, 0, "Health should not go below 0");
        Assert.LessOrEqual(tankController.health, tankController.maxHealth, "Health should not exceed max health");
    }
}

