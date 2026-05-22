using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class DeathScreen : MonoBehaviour
{
    public GameObject deathPanel;
    public TMP_Text enemiesText;
    public TMP_Text coinsText;
    public TMP_Text timeText;

    private float levelStartTime;

    void Start()
    {
        levelStartTime = Time.time;
        if (deathPanel != null) deathPanel.SetActive(false);
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            player.GetComponent<HealthSystem>()?.onDeath.AddListener(OnDeath);
    }

    void OnDeath() => Invoke(nameof(Show), 1.5f);

    void Show()
    {
        deathPanel.SetActive(true);
        Time.timeScale = 0f;
        int enemies = GameManager.Instance != null ? GameManager.Instance.enemiesDefeated : 0;
        int coins = GameManager.Instance != null ? GameManager.Instance.coins : 0; // 🔧 FIX: was CurrencySystem
        float elapsed = Time.time - levelStartTime;
        int mins = Mathf.FloorToInt(elapsed / 60f);
        int secs = Mathf.FloorToInt(elapsed % 60f);
        if (enemiesText != null) enemiesText.text = $"Enemies Defeated: {enemies}";
        if (coinsText != null) coinsText.text = $"Coins: {coins}";
        if (timeText != null) timeText.text = $"Time: {mins}:{secs:00}";
    }

    public void Retry() { Time.timeScale = 1f; SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); }
    public void MainMenu() { Time.timeScale = 1f; SceneManager.LoadScene("MainMenu"); }
}