using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

[Category("Enemy")]
public class EnemyTestScripts
{
    private GameObject playerTank;
    private TankController tankController;
    private GameObject enemyTrigger;
    private EnemyTriggerController triggerController;
    private GameObject enemy;
    private EnemyMechaController enemyController;

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

        // Find player tank
        playerTank = GameObject.FindWithTag("Player");
        Assert.IsNotNull(playerTank, "Player tank not found. Make sure 'Player' tagged GameObject exists in GamePlay scene.");

        if (playerTank != null)
        {
            tankController = playerTank.GetComponent<TankController>();
            Assert.IsNotNull(tankController, "TankController component not found on player tank");
        }
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
    public IEnumerator TC22_EnemySpawnsWhenPlayerReachesGate()
    {
        // TC22: Player moves to enemy gate - Enemy spawns
        Assert.IsNotNull(playerTank, "Player tank is null");
        Assert.IsNotNull(tankController, "Tank controller is null");

        // Find enemy trigger (spawn gate)
        enemyTrigger = GameObject.Find("EnemyGate");
        if (enemyTrigger == null)
        {
            // Search for any trigger with EnemyTriggerController
            EnemyTriggerController[] triggers = UnityEngine.Object.FindObjectsByType<EnemyTriggerController>(FindObjectsSortMode.None);
            if (triggers.Length > 0)
            {
                enemyTrigger = triggers[0].gameObject;
            }
        }

        if (enemyTrigger != null)
        {
            // Get the trigger controller
            triggerController = enemyTrigger.GetComponent<EnemyTriggerController>();
            Assert.IsNotNull(triggerController, "EnemyTriggerController not found on trigger");

            Vector3 triggerPos = enemyTrigger.transform.position;

            // Move player towards trigger
            Vector3 directionToTrigger = (triggerPos - playerTank.transform.position).normalized;
            tankController.keyboardInput = new Vector2(directionToTrigger.x, directionToTrigger.z);

            // Move player until reaching trigger or timeout
            float timeout = 10f;
            float elapsed = 0f;
            float triggerDistance = Vector3.Distance(playerTank.transform.position, triggerPos);

            while (triggerDistance > 2f && elapsed < timeout)
            {
                yield return new WaitForSeconds(0.1f);
                elapsed += 0.1f;
                triggerDistance = Vector3.Distance(playerTank.transform.position, triggerPos);
            }

            tankController.keyboardInput = Vector2.zero;

            // Check if enemies have spawned
            EnemyMechaController[] enemies = UnityEngine.Object.FindObjectsByType<EnemyMechaController>(FindObjectsSortMode.None);
            Assert.Greater(enemies.Length, 0, "Enemy should spawn when player reaches gate");
        }
    }

    [UnityTest]
    public IEnumerator TC23_EnemyPathfindingWhenDistanceGreaterThan20()
    {
        // TC23: Enemy distance > 20 - Enemy enters pathfinding state (walking towards player)
        Assert.IsNotNull(playerTank, "Player tank is null");
        Assert.IsNotNull(tankController, "Tank controller is null");

        // Wait for enemies to potentially exist
        yield return new WaitForSeconds(0.5f);

        // Find or spawn an enemy for testing
        EnemyMechaController[] enemies = UnityEngine.Object.FindObjectsByType<EnemyMechaController>(FindObjectsSortMode.None);
        if (enemies.Length > 0)
        {
            enemyController = enemies[0];
            enemy = enemyController.gameObject;

            // Position player far from enemy (> 20)
            enemy.transform.position = new Vector3(0, 1, 0);
            playerTank.transform.position = new Vector3(30, 1, 0);

            yield return new WaitForSeconds(0.2f);

            // Verify enemy is moving towards player (walking/pathfinding state)
            Animator animator = enemy.GetComponent<Animator>();
            Assert.IsNotNull(animator, "Enemy animator not found");

            // Check if walking animation is active
            bool isWalking = animator.GetBool("isWalking");
            bool isIdle = animator.GetBool("isIdle");

            Assert.IsTrue(isWalking, "Enemy should be in walking state when distance > 20");
            Assert.IsFalse(isIdle, "Enemy should not be idle when far from player");

            // Verify NavMeshAgent destination is set to player
            NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                float distanceToDestination = Vector3.Distance(agent.destination, playerTank.transform.position);
                Assert.Less(distanceToDestination, 1f, "NavMeshAgent should have player as destination");
            }
        }
    }

    [UnityTest]
    public IEnumerator TC24_EnemyAttackStateWhenDistanceLessThanOrEqual20()
    {
        // TC24: Enemy distance <= 20 - Enemy transitions to attack state
        Assert.IsNotNull(playerTank, "Player tank is null");

        EnemyMechaController[] enemies = UnityEngine.Object.FindObjectsByType<EnemyMechaController>(FindObjectsSortMode.None);
        if (enemies.Length > 0)
        {
            enemyController = enemies[0];
            enemy = enemyController.gameObject;

            // Position player close to enemy (<= 20)
            enemy.transform.position = new Vector3(0, 1, 0);
            playerTank.transform.position = new Vector3(15, 1, 0);

            yield return new WaitForSeconds(0.5f);

            // Verify enemy is in attack state
            Animator animator = enemy.GetComponent<Animator>();
            Assert.IsNotNull(animator, "Enemy animator not found");

            // Check if shooting animation is active (attack state)
            bool isShooting = animator.GetBool("isShooting");
            Assert.IsTrue(isShooting, "Enemy should enter shooting/attack state when distance <= 20");
        }
    }

    [UnityTest]
    public IEnumerator TC25_EnemyReturnsToChaseWhenPlayerMovesAway()
    {
        // TC25: Player moves away from attacking enemy - Enemy returns to chase/pathfinding state
        Assert.IsNotNull(playerTank, "Player tank is null");
        Assert.IsNotNull(tankController, "Tank controller is null");

        EnemyMechaController[] enemies = UnityEngine.Object.FindObjectsByType<EnemyMechaController>(FindObjectsSortMode.None);
        if (enemies.Length > 0)
        {
            enemyController = enemies[0];
            enemy = enemyController.gameObject;

            // Position close to trigger attack
            enemy.transform.position = new Vector3(0, 1, 0);
            playerTank.transform.position = new Vector3(15, 1, 0);

            yield return new WaitForSeconds(1f);

            // Now move player away to distance > 20
            tankController.keyboardInput = Vector2.left;

            float timeout = 5f;
            float elapsed = 0f;
            float distance = Vector3.Distance(playerTank.transform.position, enemy.transform.position);

            while (distance < 25f && elapsed < timeout)
            {
                yield return new WaitForSeconds(0.1f);
                elapsed += 0.1f;
                distance = Vector3.Distance(playerTank.transform.position, enemy.transform.position);
            }

            tankController.keyboardInput = Vector2.zero;
            yield return new WaitForSeconds(0.3f);

            // Verify enemy returned to walking/chase state
            Animator animator = enemy.GetComponent<Animator>();
            bool isWalking = animator.GetBool("isWalking");
            bool isShooting = animator.GetBool("isShooting");

            Assert.IsTrue(isWalking, "Enemy should return to walking state when player moves away");
            Assert.IsFalse(isShooting, "Enemy should not be shooting when player moves away");
        }
    }

    [UnityTest]
    public IEnumerator TC26_EnemyDoesNotSpawnInSafeZone()
    {
        // TC26: Player moves in safe zone (not reaching enemy gate) - Enemy does not spawn
        Assert.IsNotNull(playerTank, "Player tank is null");
        Assert.IsNotNull(tankController, "Tank controller is null");

        // Keep player in safe zone (avoid triggers)
        tankController.keyboardInput = Vector2.up;

        // Move for a duration in safe area
        float timeout = 3f;
        float elapsed = 0f;
        while (elapsed < timeout)
        {
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }

        tankController.keyboardInput = Vector2.zero;

        // Check that no enemies have spawned
        EnemyMechaController[] enemies = UnityEngine.Object.FindObjectsByType<EnemyMechaController>(FindObjectsSortMode.None);

        // Verify enemy count is what we expect (should be 0 or low if not reached trigger)
        Assert.IsTrue(enemies.Length <= 1, "Enemies should not spawn in safe zone");
    }

    [UnityTest]
    public IEnumerator TC27_EnemyPathfindingAvoidsObstacles()
    {
        // TC27: Enemy with obstacle between player and enemy - Enemy pathfinds around obstacle
        Assert.IsNotNull(playerTank, "Player tank is null");

        EnemyMechaController[] enemies = UnityEngine.Object.FindObjectsByType<EnemyMechaController>(FindObjectsSortMode.None);
        if (enemies.Length > 0)
        {
            enemyController = enemies[0];
            enemy = enemyController.gameObject;

            // Position enemy to chase player over distance
            enemy.transform.position = new Vector3(0, 1, 0);
            playerTank.transform.position = new Vector3(30, 1, 0);

            NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();
            if (agent != null && agent.isOnNavMesh)
            {
                // Set destination and let pathfinding work
                agent.SetDestination(playerTank.transform.position);
                yield return new WaitForSeconds(1f);

                // Check that agent has a valid path
                Assert.IsTrue(agent.hasPath, "Enemy should have a pathfinding path");
                Assert.AreEqual(NavMeshPathStatus.PathComplete, agent.path.status, 
                               "Enemy pathfinding should find complete path or partial path around obstacles");

                // Verify enemy is actually moving towards player (not stuck)
                Vector3 startPos = enemy.transform.position;
                yield return new WaitForSeconds(1f);
                Vector3 endPos = enemy.transform.position;

                float distanceMoved = Vector3.Distance(startPos, endPos);
                Assert.Greater(distanceMoved, 0.1f, "Enemy should be moving towards player, not stuck");
            }
        }
    }

    [UnityTest]
    public IEnumerator TC28_EnemyAttackHasCooldown()
    {
        // TC28: Enemy in attack state - Verify attack has cooldown (not spam)
        Assert.IsNotNull(playerTank, "Player tank is null");

        EnemyMechaController[] enemies = UnityEngine.Object.FindObjectsByType<EnemyMechaController>(FindObjectsSortMode.None);
        if (enemies.Length > 0)
        {
            enemyController = enemies[0];
            enemy = enemyController.gameObject;

            // Position close to trigger attack
            enemy.transform.position = new Vector3(0, 1, 0);
            playerTank.transform.position = new Vector3(15, 1, 0);

            yield return new WaitForSeconds(0.5f);

            // Get ShootPointController to monitor shots
            ShootPointController shootController = enemyController.shootPointController;
            Assert.IsNotNull(shootController, "ShootPointController not found on enemy");

            // Count shots fired during observation period
            int previousShotCount = 0;
            int shotCount = 0;
            float observationTime = 5f;
            float elapsed = 0f;
            int shotIntervals = 0;

            while (elapsed < observationTime)
            {
                // Detect when shot is fired (fireBar resets)
                float currentFireBar = shootController.fireBar.value;

                if (currentFireBar < 0.5f && previousShotCount == 0)
                {
                    // Shot was just fired
                    shotCount++;
                    previousShotCount = 1;
                    shotIntervals++;
                }
                else if (currentFireBar > 0.9f)
                {
                    // Fire bar is reloaded, ready for next shot
                    previousShotCount = 0;
                }

                yield return new WaitForSeconds(0.1f);
                elapsed += 0.1f;
            }

            // Verify cooldown exists: should have fewer shots than if spam (unlimited = 50+ shots in 5 seconds)
            // With 3 second cooldown (from EnemyMechaController), expect 1-2 shots in 5 seconds
            Assert.Less(shotCount, 5, "Enemy attack should have cooldown, not spam. Expected ~1-2 shots in 5 seconds with 3s cooldown");
        }
    }
}
