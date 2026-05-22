using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    public GameObject pausePanel;
    public GameObject settingsWindow;
    public GameObject pauseWindow;
    public Slider volumeSlider;

    private bool paused;

    void Start()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        if (settingsWindow != null) settingsWindow.SetActive(false);
        if (volumeSlider != null)
        {
            volumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1f);
            AudioListener.volume = volumeSlider.value;
        }
    }

    // 🔧 FIX: New Input System callback instead of Input.GetKeyDown(KeyCode.Escape)
    public void OnPause(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        if (settingsWindow != null && settingsWindow.activeSelf) CloseSettings();
        else SetPaused(!paused);
    }

    public void SetPaused(bool value)
    {
        paused = value;
        if (pausePanel != null) pausePanel.SetActive(paused);
        Time.timeScale = paused ? 0f : 1f;
    }

    public void Resume() => SetPaused(false);
    public void OpenSettings() { if (pauseWindow) pauseWindow.SetActive(false); if (settingsWindow) settingsWindow.SetActive(true); }
    public void CloseSettings() { if (settingsWindow) settingsWindow.SetActive(false); if (pauseWindow) pauseWindow.SetActive(true); }
    public void SetVolume(float v) { AudioListener.volume = v; PlayerPrefs.SetFloat("MasterVolume", v); PlayerPrefs.Save(); }
    public void LoadMainMenu() { Time.timeScale = 1f; SceneManager.LoadScene("MainMenu"); }
    public void Quit()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}