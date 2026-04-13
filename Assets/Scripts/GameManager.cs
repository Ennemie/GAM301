using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameConfig gameConfig;

    [Header("Music - Sound")]
    public AudioSource backGroundMusic;
    public AudioSource helicopterSound;
    public AudioSource shootSound;
    public AudioSource tankSound;

    void Start()
    {
        Application.targetFrameRate = 120;

        backGroundMusic.volume = gameConfig.musicVolume;
        if(gameConfig.musicVolume >= 0.5f)
        {
            helicopterSound.volume = gameConfig.musicVolume * 1.5f;
        }
        else
        {
            helicopterSound.volume = 0.5f;
        }

        shootSound.volume = gameConfig.soundVolume;
        tankSound.volume = gameConfig.soundVolume;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
