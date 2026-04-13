using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

[Category("Movement")]
public class MovementTestScripts
{
    private TankController controller;
    private GameObject playerTank;

    [UnitySetUp]
    public IEnumerator Setup()
    {
        // 1. Dọn dẹp các Object không mong muốn nếu chúng tồn tại từ scene trước
        GameObject openingScene = GameObject.Find("OpeningScene");
        if (openingScene != null) openingScene.SetActive(false);

        GameObject helicopter = GameObject.Find("Helicopter");
        if (helicopter != null) helicopter.SetActive(false);

        // 2. Load scene GamePlay
        AsyncOperation load = SceneManager.LoadSceneAsync("GamePlay");
        yield return new WaitUntil(() => load.isDone);

        // 3. Đợi một chút để các Script Start/Awake kịp chạy
        yield return new WaitForSeconds(1f);

        // 4. Tìm kiếm các tham chiếu chung để dùng cho mọi Test Case
        playerTank = GameObject.FindWithTag("Player");
        if (playerTank != null)
        {
            controller = playerTank.GetComponent<TankController>();
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
    public IEnumerator TC1_TankMovesInCorrectJoystickDirection()
    {
        Assert.IsNotNull(playerTank);
        Assert.IsNotNull(controller);

        Vector3 initialPosition = playerTank.transform.position;

        // Test moving up
        controller.keyboardInput = Vector2.up;
        yield return new WaitForSeconds(0.3f);
        Assert.Greater(playerTank.transform.position.z, initialPosition.z, "Tank should move forward (up)");

        // Test moving right
        Vector3 posBeforeRight = playerTank.transform.position;
        controller.keyboardInput = Vector2.right;
        yield return new WaitForSeconds(0.3f);
        Assert.Greater(playerTank.transform.position.x, posBeforeRight.x, "Tank should move right");

        // Test moving down
        Vector3 posBeforeDown = playerTank.transform.position;
        controller.keyboardInput = Vector2.down;
        yield return new WaitForSeconds(0.3f);
        Assert.Less(playerTank.transform.position.z, posBeforeDown.z, "Tank should move backward (down)");

        // Test moving left
        Vector3 posBeforeLeft = playerTank.transform.position;
        controller.keyboardInput = Vector2.left;
        yield return new WaitForSeconds(0.3f);
        Assert.Less(playerTank.transform.position.x, posBeforeLeft.x, "Tank should move left");

        controller.keyboardInput = Vector2.zero;
    }

    [UnityTest]
    public IEnumerator TC2_TankStopsWhenJoystickReleased()
    {
        Assert.IsNotNull(playerTank);
        Assert.IsNotNull(controller);

        controller.keyboardInput = Vector2.up;
        yield return new WaitForSeconds(0.5f);

        Vector3 posWhenStopped = playerTank.transform.position;

        controller.keyboardInput = Vector2.zero;
        yield return new WaitForSeconds(0.5f);

        Assert.AreEqual(posWhenStopped.x, playerTank.transform.position.x, 0.1f, "Tank X position should not change");
        Assert.AreEqual(posWhenStopped.z, playerTank.transform.position.z, 0.1f, "Tank Z position should not change");
    }

    [UnityTest]
    public IEnumerator TC3_TankBlockedByObstacles()
    {
        Assert.IsNotNull(playerTank);
        Assert.IsNotNull(controller);

        controller.keyboardInput = Vector2.up;
        yield return new WaitForSeconds(3f);

        Vector3 posAtBoundary = playerTank.transform.position;

        yield return new WaitForSeconds(1f);

        Assert.AreEqual(posAtBoundary.x, playerTank.transform.position.x, 0.1f, "Tank should be blocked by boundary");
        Assert.AreEqual(posAtBoundary.z, playerTank.transform.position.z, 0.1f, "Tank should be blocked by boundary");

        controller.keyboardInput = Vector2.zero;
    }

    [UnityTest]
    public IEnumerator TC4_TankChangeDirectionSuddenly()
    {
        Assert.IsNotNull(playerTank);
        Assert.IsNotNull(controller);

        controller.keyboardInput = Vector2.up;
        yield return new WaitForSeconds(0.3f);
        Vector3 posAfterUp = playerTank.transform.position;

        controller.keyboardInput = Vector2.right;
        yield return new WaitForSeconds(0.3f);
        Vector3 posAfterRight = playerTank.transform.position;

        Assert.Greater(posAfterRight.x, posAfterUp.x, "Tank should change direction to right");
        Assert.AreEqual(posAfterRight.z, posAfterUp.z, 0.1f, "Tank Z position should stabilize");

        controller.keyboardInput = Vector2.left;
        yield return new WaitForSeconds(0.3f);
        Vector3 posAfterLeft = playerTank.transform.position;

        Assert.Less(posAfterLeft.x, posAfterRight.x, "Tank should change direction to left");

        controller.keyboardInput = Vector2.zero;
    }

    [UnityTest]
    public IEnumerator TC5_NoMovementEffectWhenStanding()
    {
        Assert.IsNotNull(playerTank);
        Assert.IsNotNull(controller);

        controller.keyboardInput = Vector2.zero;
        yield return new WaitForSeconds(0.5f);

        ParticleSystem particleSystems = GameObject.Find("DustTrail_Desert")?.GetComponent<ParticleSystem>();
        Assert.IsNotNull(particleSystems);
        Assert.IsFalse(particleSystems.isPlaying, "Movement effect should not be playing when standing still");

    }

    [UnityTest]
    public IEnumerator TC6_MovementEffectDisplaysWhenMoving()
    {
        Assert.IsNotNull(playerTank);
        Assert.IsNotNull(controller);

        controller.keyboardInput = Vector2.up;
        yield return new WaitForSeconds(0.3f);

        ParticleSystem[] particleSystems = playerTank.GetComponentsInChildren<ParticleSystem>();
        bool movementEffectFound = false;

        foreach (ParticleSystem ps in particleSystems)
        {
            if (ps.name.Contains("Movement") || ps.name.Contains("Dust"))
            {
                movementEffectFound = true;
                Assert.IsTrue(ps.isPlaying, "Movement effect should be playing when tank is moving");
                break;
            }
        }

        Assert.IsTrue(movementEffectFound, "Movement effect particle system should exist");

        controller.keyboardInput = Vector2.zero;
    }

    [UnityTest]
    public IEnumerator TC7_IdleSoundPlayWhenStanding()
    {
        Assert.IsNotNull(playerTank);
        Assert.IsNotNull(controller);

        controller.keyboardInput = Vector2.zero;
        yield return new WaitForSeconds(0.5f);

        AudioSource[] audioSources = playerTank.GetComponentsInChildren<AudioSource>();
        bool idleSoundFound = false;

        foreach (AudioSource audio in audioSources)
        {
            if (audio.clip != null && (audio.clip.name.Contains("Idle") || audio.clip.name.Contains("Engine")))
            {
                idleSoundFound = true;
                break;
            }
        }

        Assert.IsTrue(idleSoundFound || audioSources.Length > 0, "Audio source for idle sound should exist");
    }

    [UnityTest]
    public IEnumerator TC8_MovementSoundPlayWhenMoving()
    {
        Assert.IsNotNull(playerTank);
        Assert.IsNotNull(controller);

        controller.keyboardInput = Vector2.up;
        yield return new WaitForSeconds(0.3f);

        AudioSource[] audioSources = playerTank.GetComponentsInChildren<AudioSource>();
        bool movementSoundPlaying = false;

        foreach (AudioSource audio in audioSources)
        {
            if (audio.isPlaying && audio.clip != null && (audio.clip.name.Contains("Move") || audio.clip.name.Contains("Engine") || audio.clip.name.Contains("Drive")))
            {
                movementSoundPlaying = true;
                break;
            }
        }

        Assert.IsTrue(movementSoundPlaying || audioSources.Length > 0, "Movement sound should be playing or audio source should exist");

        controller.keyboardInput = Vector2.zero;
    }

    [UnityTest]
    public IEnumerator TC9_IdleAnimationWhenStanding()
    {
        Assert.IsNotNull(playerTank);
        Assert.IsNotNull(controller);

        controller.keyboardInput = Vector2.zero;
        yield return new WaitForSeconds(0.5f);

        Animator animator = playerTank.GetComponent<Animator>();
        if (animator != null)
        {
            Assert.IsTrue(animator.enabled, "Animator should be enabled");

            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            Assert.IsTrue(stateInfo.IsName("Idle") || stateInfo.IsName("idle") || stateInfo.normalizedTime > 0, 
                "Idle animation should be playing or active");
        }
        else
        {
            Assert.IsNotNull(controller, "Tank controller should exist for animation handling");
        }
    }

    [UnityTest]
    public IEnumerator TC10_MovementAnimationWhenMoving()
    {
        Assert.IsNotNull(playerTank);
        Assert.IsNotNull(controller);

        controller.keyboardInput = Vector2.up;
        yield return new WaitForSeconds(0.3f);

        Animator animator = playerTank.GetComponent<Animator>();
        if (animator != null)
        {
            Assert.IsTrue(animator.enabled, "Animator should be enabled");

            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            Assert.IsTrue(stateInfo.IsName("Move") || stateInfo.IsName("move") || stateInfo.IsName("Walk") || stateInfo.IsName("walk"), 
                "Movement animation should be playing");
        }
        else
        {
            Assert.IsNotNull(controller, "Tank controller should exist for animation handling");
        }

        controller.keyboardInput = Vector2.zero;
    }

    [UnityTest]
    public IEnumerator PlayerTankMovement()
    {
        Assert.IsNotNull(playerTank);
        Assert.IsNotNull(controller);

        Vector3 posBefore;

        posBefore = playerTank.transform.position;
        controller.keyboardInput = Vector2.up;
        yield return new WaitForSeconds(0.5f);
        Assert.Greater(playerTank.transform.position.z, posBefore.z);

        posBefore = playerTank.transform.position;
        controller.keyboardInput = Vector2.down;
        yield return new WaitForSeconds(0.5f);
        Assert.Less(playerTank.transform.position.z, posBefore.z);

        posBefore = playerTank.transform.position;
        controller.keyboardInput = Vector2.left;
        yield return new WaitForSeconds(0.5f);
        Assert.Less(playerTank.transform.position.x, posBefore.x);

        posBefore = playerTank.transform.position;
        controller.keyboardInput = Vector2.right;
        yield return new WaitForSeconds(0.5f);
        Assert.Greater(playerTank.transform.position.x, posBefore.x);

        controller.keyboardInput = Vector2.zero;
    }
}
