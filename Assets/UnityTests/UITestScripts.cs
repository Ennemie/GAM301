using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

[Category("UI")]
public class UITestScripts
{
    [UnitySetUp]
    public IEnumerator Setup()
    {
        // Load the Intro scene for UI tests
        AsyncOperation load = SceneManager.LoadSceneAsync("Intro");
        yield return new WaitUntil(() => load.isDone);
        yield return new WaitForSeconds(0.5f);
    }

    [UnityTest]
    public IEnumerator TC46_IntroSceneShowsStartUI()
    {
        // Test: Khởi động scene "Intro" - Hiển thị UI bắt đầu game
        Assert.AreEqual("Intro", SceneManager.GetActiveScene().name);

        // Find the main start UI panel
        GameObject startUI = GameObject.Find("StartUI");
        Assert.IsNotNull(startUI);
        Assert.IsTrue(startUI.activeSelf);

        yield return null;
    }

    [UnityTest]
    public IEnumerator TC47_PlayButtonNavigatesToGameplay()
    {
        // Test: Vào scene Intro, nhấn nút Play - Game chuyển sang scene Gameplay
        GameObject playButton = GameObject.Find("PlayButton");
        Assert.IsNotNull(playButton);

        Button btn = playButton.GetComponent<Button>();
        Assert.IsNotNull(btn);

        // Simulate button click
        btn.onClick.Invoke();
        yield return new WaitForSeconds(1f);

        Assert.AreEqual("GamePlay", SceneManager.GetActiveScene().name);
    }

    [UnityTest]
    public IEnumerator TC48_SettingsButtonShowsSettingsUI()
    {
        // Test: Vào scene Intro, nhấn nút Settings - Hiển thị UI setting game
        GameObject settingsButton = GameObject.Find("SettingsButton");
        Assert.IsNotNull(settingsButton);

        Button btn = settingsButton.GetComponent<Button>();
        Assert.IsNotNull(btn);

        GameObject startUI = GameObject.Find("StartUI");
        Assert.IsTrue(startUI.activeSelf);

        btn.onClick.Invoke();
        yield return new WaitForSeconds(0.5f);

        GameObject settingsUI = GameObject.Find("SettingsUI");
        Assert.IsNotNull(settingsUI);
        Assert.IsTrue(settingsUI.activeSelf);
        Assert.IsFalse(startUI.activeSelf);
    }

    [UnityTest]
    public IEnumerator TC49_MusicSliderAffectsMusicVolume()
    {
        // Test: Thay đổi giá trị slider Music - Âm lượng của âm thanh nền thay đổi
        GameObject settingsButton = GameObject.Find("SettingsButton");
        settingsButton.GetComponent<Button>().onClick.Invoke();
        yield return new WaitForSeconds(0.5f);

        Slider musicSlider = GameObject.Find("MusicSlider").GetComponent<Slider>();
        Assert.IsNotNull(musicSlider);

        AudioSource backgroundMusic = GameObject.FindGameObjectWithTag("BackgroundMusic").GetComponent<AudioSource>();
        Assert.IsNotNull(backgroundMusic);

        float initialVolume = backgroundMusic.volume;

        // Change slider value
        musicSlider.value = 0.5f;
        yield return new WaitForSeconds(0.2f);

        Assert.AreNotEqual(initialVolume, backgroundMusic.volume);
        Assert.AreEqual(0.5f, backgroundMusic.volume, 0.01f);
    }

    [UnityTest]
    public IEnumerator TC51_BackButtonReturnsToStartUI()
    {
        // Test: Nhấn nút Back trong UI setting game - Quay về UI bắt đầu game
        GameObject settingsButton = GameObject.Find("SettingsButton");
        settingsButton.GetComponent<Button>().onClick.Invoke();
        yield return new WaitForSeconds(0.5f);

        GameObject settingsUI = GameObject.Find("SettingsUI");
        Assert.IsTrue(settingsUI.activeSelf);

        GameObject backButton = GameObject.Find("BackButton");
        Assert.IsNotNull(backButton);

        backButton.GetComponent<Button>().onClick.Invoke();
        yield return new WaitForSeconds(0.5f);

        GameObject startUI = GameObject.Find("StartUI");
        Assert.IsTrue(startUI.activeSelf);
        Assert.IsFalse(settingsUI.activeSelf);
    }

    [UnityTest]
    public IEnumerator TC52_GameplaySceneShowsPlayUI()
    {
        // Test: Khởi động scene Gameplay - Hiển thị UI play game
        AsyncOperation load = SceneManager.LoadSceneAsync("GamePlay");
        yield return new WaitUntil(() => load.isDone);
        yield return new WaitForSeconds(0.5f);

        Assert.AreEqual("GamePlay", SceneManager.GetActiveScene().name);

        GameObject playUI = GameObject.Find("PlayUI");
        Assert.IsNotNull(playUI);
        Assert.IsTrue(playUI.activeSelf);
    }


    [UnityTest]
    public IEnumerator TC56_PlayerHealthSliderUpdatesOnDamage()
    {
        // Test: Khi nhân vật mất máu - Hp slider trong UI play game thay đổi giá trị
        AsyncOperation load = SceneManager.LoadSceneAsync("GamePlay");
        yield return new WaitUntil(() => load.isDone);
        yield return new WaitForSeconds(0.5f);

        GameObject playerTank = GameObject.FindWithTag("Player");
        Assert.IsNotNull(playerTank);

        TankController tankController = playerTank.GetComponent<TankController>();
        Assert.IsNotNull(tankController);

        Slider healthSlider = GameObject.Find("HealthSlider").GetComponent<Slider>();
        Assert.IsNotNull(healthSlider);

        float initialHealth = tankController.health;
        float initialSliderValue = healthSlider.value;

        // Simulate damage
        tankController.TakeDamage(10);
        yield return new WaitForSeconds(0.2f);

        Assert.Less(tankController.health, initialHealth);
        Assert.Less(healthSlider.value, initialSliderValue);
    }
}
