using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public string firstLevel = "Level1";

    public void Play()
    {
        Time.timeScale = 1f;   // make sure time is normal
        SceneManager.LoadScene(firstLevel);
    }

    public void SetVolume(float v)
    {
        AudioListener.volume = v;
        PlayerPrefs.SetFloat("MasterVolume", v);
        PlayerPrefs.Save();
    }

    public void Quit()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}