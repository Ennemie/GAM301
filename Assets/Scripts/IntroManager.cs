using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class IntroManager : MonoBehaviour
{
    public GameObject openingHub;
    public GameObject settingsHub;
    public Slider musicBar;
    public Slider soundBar;
    public AudioSource backGroundMusic;
    public AudioSource soundFx;
    public GameConfig gameConfig;
    void Start()
    {
        musicBar.value = 1;
        soundBar.value = 1;
        SetMusicVolume();
        SetSoundVolume();
        OpenOpeningHub();
    }

    public void OpenOpeningHub()
    {
        openingHub.SetActive(true);
        settingsHub.SetActive(false);
    }
    public void OpenSettingsHub()
    {
        openingHub.SetActive(false);
        settingsHub.SetActive(true);
    }
    public void Startgame()
    {
        SceneManager.LoadScene("Gameplay");
    }
    public void SetMusicVolume()
    {
        gameConfig.musicVolume = musicBar.value;
        backGroundMusic.volume = gameConfig.musicVolume;
    }
    public void SetSoundVolume()
    {
        gameConfig.soundVolume = soundBar.value;
        soundFx.volume = gameConfig.soundVolume;
    }
    public void ButtonSound()
    {
        soundFx.Play();
    }
}
