using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public int coins = 0;
    public int enemiesDefeated = 0;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else Destroy(gameObject);   // a second one in the next scene self-destructs
    }

    void Start() => coins = PlayerPrefs.GetInt("Coins", 0);

    public void AddCoins(int amount)
    {
        coins += amount;
        PlayerPrefs.SetInt("Coins", coins); PlayerPrefs.Save();
    }

    public bool SpendCoins(int amount)
    {
        if (coins < amount) return false;
        coins -= amount;
        PlayerPrefs.SetInt("Coins", coins); PlayerPrefs.Save();
        return true;
    }

    public void EnemyDefeated() => enemiesDefeated++;
    public void RestartLevel() { Time.timeScale = 1f; SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); }
    public void LoadScene(string name) { Time.timeScale = 1f; SceneManager.LoadScene(name); }
}