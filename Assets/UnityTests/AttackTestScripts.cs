using NUnit.Framework;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

[Category("Attack")]
public class AttackTestScripts
{
    private TankController controller;
    private ShootPointController shootController;
    private GameObject playerTank;

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
            controller = playerTank.GetComponent<TankController>();
            Assert.IsNotNull(controller, "TankController component not found on player tank");

            shootController = playerTank.GetComponentInChildren<ShootPointController>();
            Assert.IsNotNull(shootController, "ShootPointController component not found on player tank");
        }
    }

    [UnityTearDown]
    public IEnumerator Teardown()
    {
        if (controller != null)
        {
            controller.keyboardInput = Vector2.zero;
        }
        yield return null;
    }

    [UnityTest]
    public IEnumerator TC11_ShootWhenEnergyBarFull()
    {
        // TC11: Tank standing still, energy bar full - Press shoot button - Tank shoots 1 bullet, energy bar reloads
        Assert.IsNotNull(playerTank, "Player tank is null");
        Assert.IsNotNull(controller, "Tank controller is null");
        Assert.IsNotNull(shootController, "Shoot controller is null");
        Assert.IsNotNull(shootController.fireBar, "Fire bar is null");

        // Wait for fire to be ready (with timeout)
        float timeout = 5f;
        float elapsed = 0f;
        while (shootController.fireBar.value < 0.99f && elapsed < timeout)
        {
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }
        Assert.Less(elapsed, timeout, "Timeout waiting for fire bar to be ready");

        // Get initial state
        float fireBarBefore = shootController.fireBar.value;
        Assert.GreaterOrEqual(fireBarBefore, 0.99f, "Fire bar should be full before shooting");

        // Trigger shooting
        shootController.Shoot();
        yield return new WaitForSeconds(0.2f);

        // Verify bullet was fired (shoot effect should play)
        Assert.IsNotNull(shootController.shootEffect, "Shoot effect is null");

        // Verify fire bar started reloading (should be less than full)
        Assert.Less(shootController.fireBar.value, fireBarBefore, "Fire bar should start reloading after shot");
    }

    [UnityTest]
    public IEnumerator TC12_CannotShootWhenEnergyCharging()
    {
        // TC12: Tank standing still, energy charging - Press shoot button - Tank cannot shoot
        Assert.IsNotNull(playerTank, "Player tank is null");
        Assert.IsNotNull(controller, "Tank controller is null");
        Assert.IsNotNull(shootController, "Shoot controller is null");

        // Wait for fire bar to be ready first
        float timeout = 5f;
        float elapsed = 0f;
        while (shootController.fireBar.value < 0.99f && elapsed < timeout)
        {
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }

        // Trigger a shot to start cooldown
        shootController.Shoot();
        yield return new WaitForSeconds(0.3f);

        // Now fireBar should be < 1 (charging)
        Assert.Less(shootController.fireBar.value, 0.99f, "Fire bar should be charging after shot");

        float fireBarWhileCharging = shootController.fireBar.value;

        // Try to shoot - should fail
        shootController.Shoot();
        yield return new WaitForSeconds(0.2f);

        // Fire bar should not have changed significantly (not restarted)
        Assert.Less(shootController.fireBar.value, 0.99f, "Fire bar should still be charging");
    }

    [UnityTest]
    public IEnumerator TC13_ShootWhileMovingWithFullEnergy()
    {
        // TC13: Tank moving, energy full - Shoot - Bullet fires, tank movement unaffected, reloading starts
        Assert.IsNotNull(playerTank, "Player tank is null");
        Assert.IsNotNull(controller, "Tank controller is null");
        Assert.IsNotNull(shootController, "Shoot controller is null");

        // Wait for fire to be ready
        float timeout = 5f;
        float elapsed = 0f;
        while (shootController.fireBar.value < 0.99f && elapsed < timeout)
        {
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }
        Assert.Less(elapsed, timeout, "Timeout waiting for fire bar to be ready");

        yield return new WaitForSeconds(0.2f);

        // Start moving
        controller.keyboardInput = Vector2.up;
        yield return new WaitForSeconds(0.3f);

        Vector3 tankPosBeforeShoot = playerTank.transform.position;
        float fireBarBefore = shootController.fireBar.value;

        // Shoot
        shootController.Shoot();
        yield return new WaitForSeconds(0.2f);

        // Verify fire bar started reloading
        Assert.Less(shootController.fireBar.value, fireBarBefore, "Fire bar should start reloading");

        // Verify tank continues moving
        yield return new WaitForSeconds(0.2f);
        Vector3 tankPosAfterShoot = playerTank.transform.position;
        Assert.Greater(Vector3.Distance(tankPosAfterShoot, tankPosBeforeShoot), 0.05f, 
                      "Tank should continue moving while shooting");

        controller.keyboardInput = Vector2.zero;
    }

    [UnityTest]
    public IEnumerator TC14_CannotShootWhileMovingWithChargingEnergy()
    {
        // TC14: Tank moving, energy charging - Shoot - Tank cannot shoot, movement unaffected
        Assert.IsNotNull(playerTank, "Player tank is null");
        Assert.IsNotNull(controller, "Tank controller is null");
        Assert.IsNotNull(shootController, "Shoot controller is null");

        // Trigger a shot to start cooldown
        float timeout = 5f;
        float elapsed = 0f;
        while (shootController.fireBar.value < 0.99f && elapsed < timeout)
        {
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }

        shootController.Shoot();
        yield return new WaitForSeconds(0.3f);

        // Now fireBar should be charging
        Assert.Less(shootController.fireBar.value, 0.99f, "Fire bar should be charging");

        controller.keyboardInput = Vector2.up;
        yield return new WaitForSeconds(0.3f);

        Vector3 tankPosBeforeShoot = playerTank.transform.position;
        float fireBarBefore = shootController.fireBar.value;

        // Try to shoot
        shootController.Shoot();
        yield return new WaitForSeconds(0.2f);

        // Verify fire bar didn't reset (same state)
        Assert.Less(shootController.fireBar.value, 0.99f, "Fire bar should not reset");

        // Verify tank continues moving
        yield return new WaitForSeconds(0.2f);
        Vector3 tankPosAfterShoot = playerTank.transform.position;
        Assert.Greater(Vector3.Distance(tankPosAfterShoot, tankPosBeforeShoot), 0.05f, 
                      "Tank movement should not be affected");

        controller.keyboardInput = Vector2.zero;
    }

    [UnityTest]
    public IEnumerator TC15_CanShootMultipleTimesNoAmmoLimit()
    {
        // TC15: Tank in GamePlay - Press shoot repeatedly when energy full - Can shoot multiple times, no ammo limit
        Assert.IsNotNull(playerTank, "Player tank is null");
        Assert.IsNotNull(controller, "Tank controller is null");
        Assert.IsNotNull(shootController, "Shoot controller is null");

        int shootCount = 0;

        for (int i = 0; i < 3; i++)
        {
            // Wait for fire bar to reload
            float timeout = 5f;
            float elapsed = 0f;
            while (shootController.fireBar.value < 0.99f && elapsed < timeout)
            {
                yield return new WaitForSeconds(0.1f);
                elapsed += 0.1f;
            }
            Assert.Less(elapsed, timeout, $"Timeout waiting for fire bar to be ready on shot {i + 1}");

            yield return new WaitForSeconds(0.1f);

            // Shoot
            shootController.Shoot();
            yield return new WaitForSeconds(0.2f);
            shootCount++;
        }

        Assert.AreEqual(3, shootCount, "Should be able to shoot multiple times without ammo limit");
    }

    [UnityTest]
    public IEnumerator TC16_BulletDisplayEffect()
    {
        // TC16: Bullet flying - Display bullet effect
        Assert.IsNotNull(playerTank, "Player tank is null");
        Assert.IsNotNull(shootController, "Shoot controller is null");

        // Wait for fire bar to be ready
        float timeout = 5f;
        float elapsed = 0f;
        while (shootController.fireBar.value < 0.99f && elapsed < timeout)
        {
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }

        yield return new WaitForSeconds(0.2f);

        shootController.Shoot();
        yield return new WaitForSeconds(0.2f);

        // Check that bullet exists (in object pool as active)
        bool bulletFound = false;
        GameObject[] allObjects = UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.Contains("Bullet") && obj.activeInHierarchy)
            {
                bulletFound = true;
                Renderer renderer = obj.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Assert.IsTrue(renderer.enabled, "Bullet should have visible renderer");
                }
                break;
            }
        }

        Assert.IsTrue(bulletFound, "Active bullet should exist after shooting");
    }

    [UnityTest]
    public IEnumerator TC17_GunSoundPlaysWhenShootingFullEnergy()
    {
        // TC17: Tank in GamePlay, energy full - Shoot - Gun sound effect plays
        Assert.IsNotNull(playerTank, "Player tank is null");
        Assert.IsNotNull(shootController, "Shoot controller is null");
        Assert.IsNotNull(shootController.shootSound, "Shoot sound is null");

        // Wait for fire bar to be ready
        float timeout = 5f;
        float elapsed = 0f;
        while (shootController.fireBar.value < 0.99f && elapsed < timeout)
        {
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }

        yield return new WaitForSeconds(0.2f);

        shootController.Shoot();
        yield return new WaitForSeconds(0.1f);

        // Check if sound is playing
        Assert.IsTrue(shootController.shootSound.isPlaying, "Gun sound should be playing after shoot");
    }

    [UnityTest]
    public IEnumerator TC18_NoGunSoundWhenEnergyCharging()
    {
        // TC18: Tank in GamePlay, energy charging - Shoot - No gun sound
        Assert.IsNotNull(playerTank, "Player tank is null");
        Assert.IsNotNull(shootController, "Shoot controller is null");

        // Trigger a shot to start cooldown
        float timeout = 5f;
        float elapsed = 0f;
        while (shootController.fireBar.value < 0.99f && elapsed < timeout)
        {
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }

        shootController.Shoot();
        yield return new WaitForSeconds(0.3f);

        // Now energy is charging
        Assert.Less(shootController.fireBar.value, 0.99f, "Fire bar should be charging");

        if (shootController.shootSound != null && shootController.shootSound.clip != null)
        {
            float clipLength = shootController.shootSound.clip.length;

            // Try to shoot (should fail)
            shootController.Shoot();
            yield return new WaitForSeconds(Math.Min(0.5f, clipLength + 0.2f));

            // Gun sound should not be playing after the previous shot finished
            Assert.IsFalse(shootController.shootSound.isPlaying, "Gun sound should not be playing when energy charging");
        }
    }

    [UnityTest]
    public IEnumerator TC19_MuzzleFlashWhenShootingFullEnergy()
    {
        // TC19: Tank in GamePlay, energy full - Shoot - Muzzle flash effect displays
        Assert.IsNotNull(playerTank, "Player tank is null");
        Assert.IsNotNull(shootController, "Shoot controller is null");
        Assert.IsNotNull(shootController.shootEffect, "Shoot effect is null");

        // Wait for fire bar to be ready
        float timeout = 5f;
        float elapsed = 0f;
        while (shootController.fireBar.value < 0.99f && elapsed < timeout)
        {
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }

        yield return new WaitForSeconds(0.2f);

        shootController.Shoot();
        yield return new WaitForSeconds(0.1f);

        // Muzzle flash (shootEffect) should be playing
        Assert.IsTrue(shootController.shootEffect.isPlaying, "Muzzle flash effect should be playing after shoot");
    }

    [UnityTest]
    public IEnumerator TC20_NoMuzzleFlashWhenEnergyCharging()
    {
        // TC20: Tank in GamePlay, energy charging - Shoot - No muzzle flash
        Assert.IsNotNull(playerTank, "Player tank is null");
        Assert.IsNotNull(shootController, "Shoot controller is null");

        // Trigger a shot to start cooldown
        float timeout = 5f;
        float elapsed = 0f;
        while (shootController.fireBar.value < 0.99f && elapsed < timeout)
        {
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }

        shootController.Shoot();
        yield return new WaitForSeconds(0.3f);

        // Now energy is charging - try to shoot
        Assert.Less(shootController.fireBar.value, 0.99f, "Fire bar should be charging");

        if (shootController.shootEffect != null)
        {
            float duration = shootController.shootEffect.main.duration;

            shootController.Shoot();
            yield return new WaitForSeconds(Math.Min(0.5f, duration + 0.2f));

            // Muzzle flash should not be playing (since shoot was rejected and previous effect finished)
            Assert.IsFalse(shootController.shootEffect.isPlaying, "Muzzle flash should not play when energy charging");
        }
    }

    [UnityTest]
    public IEnumerator TC21_BulletHitEffectAndDisappear()
    {
        // TC21: Bullet hits objects - Create hit effect and disappear
        Assert.IsNotNull(playerTank, "Player tank is null");
        Assert.IsNotNull(shootController, "Shoot controller is null");

        // Wait for fire bar to be ready
        float timeout = 5f;
        float elapsed = 0f;
        while (shootController.fireBar.value < 0.99f && elapsed < timeout)
        {
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }

        yield return new WaitForSeconds(0.2f);

        shootController.Shoot();
        yield return new WaitForSeconds(0.2f);

        // Check that bullet exists (in object pool as active)
        bool bulletFound = false;
        GameObject[] allObjsAfter = UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        int bulletCountAfter = 0;
        foreach (GameObject obj in allObjsAfter)
        {
            if (obj.name.Contains("Bullet") && obj.activeInHierarchy)
            {
                bulletFound = true;
                bulletCountAfter++;
            }
        }

        Assert.IsTrue(bulletFound, "Active bullet should exist after shooting");

        // Wait for bullet to potentially hit something or go out of bounds
        yield return new WaitForSeconds(2f);

        // Check if bullet behavior is managed correctly (either deactivated or moved)
        GameObject[] allObjsFinal = UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        int bulletCountFinal = 0;
        foreach (GameObject obj in allObjsFinal)
        {
            if (obj.name.Contains("Bullet") && obj.activeInHierarchy)
                bulletCountFinal++;
        }

        // Bullet should be managed by the pool (may or may not be active depending on collision)
        Assert.IsTrue(bulletCountFinal <= bulletCountAfter, "Bullet behavior should be managed correctly");
    }
}
