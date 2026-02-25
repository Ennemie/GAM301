using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CanvaController : MonoBehaviour
{
    public Slider healthSlider;
    public Slider fuelSlider;
    public Image gameOverPanel;
    public Image speedUpIcon;
    public Image fireUpIcon;
    public GameObject pausePanel;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        healthSlider.gameObject.SetActive(true);
        fuelSlider.gameObject.SetActive(true);
        gameOverPanel.gameObject.SetActive(false);
        speedUpIcon.gameObject.SetActive(false);
        fireUpIcon.gameObject.SetActive(false);
        pausePanel.gameObject.SetActive(false);
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        pausePanel.gameObject.SetActive(true);
    }
    public void ResumeGame()
    {
        Time.timeScale = 1f;
        pausePanel.gameObject.SetActive(false);
    }
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("GamePlay");
    }
    public void ShowGameOverPanel()
    {
        Time.timeScale = 0f;
        gameOverPanel.gameObject.SetActive(true);
        healthSlider.gameObject.SetActive(false);
        fuelSlider.gameObject.SetActive(false);
        pausePanel.gameObject.SetActive(false);
    }
    public void SwitchSpeedUpIcon()
    {
        if (speedUpIcon.gameObject.activeInHierarchy)
            speedUpIcon.gameObject.SetActive(false);
        else
            speedUpIcon.gameObject.SetActive(true);
    }
    public void SwitchFireUpIcon()
    {
        if (fireUpIcon.gameObject.activeInHierarchy)
            fireUpIcon.gameObject.SetActive(false);
        else
            fireUpIcon.gameObject.SetActive(true);
    }
}
