using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class DeathScreen : MonoBehaviour
{
    public AudioClip deathSound;        // drag the death sound here
    public AudioSource deathAudioSource;

    [Header("Overlay (YOU DIED fade)")]
    public GameObject deathOverlay;          // the black panel with "YOU DIED"
    public CanvasGroup overlayCanvasGroup;   // its CanvasGroup
    public float fadeInDuration = 1.5f;      // time to fade IN the text
    public float holdDuration = 1.5f;        // how long the text stays fully visible
    public float fadeOutDuration = 1.5f;     // time to fade OUT the text

    [Header("Death Panel (stats + buttons)")]
    public GameObject deathPanel;
    public TMP_Text enemiesText;
    public TMP_Text coinsText;
    public TMP_Text timeText;

    private float levelStartTime;

    void Start()
    {
        levelStartTime = Time.time;
        if (deathOverlay != null) deathOverlay.SetActive(false);
        if (deathPanel != null) deathPanel.SetActive(false);
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            player.GetComponent<HealthSystem>()?.onDeath.AddListener(OnDeath);
    }

    void OnDeath()
    {
        // Play death sound immediately on the always‑active audio source
        if (deathAudioSource != null && deathSound != null)
            deathAudioSource.PlayOneShot(deathSound);

        Invoke(nameof(StartSequence), 0.5f);
    }

    void StartSequence()
    {
        Time.timeScale = 0f;           // freeze game immediately
        StartCoroutine(PlaySequence());
    }

    IEnumerator PlaySequence()
    {
        // --- FADE IN ---
        deathOverlay.SetActive(true);
        overlayCanvasGroup.alpha = 0f;
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            overlayCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInDuration);
            yield return null;
        }
        overlayCanvasGroup.alpha = 1f;

        // --- HOLD ---
        yield return new WaitForSecondsRealtime(holdDuration);

        // --- FADE OUT ---
        elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            overlayCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeOutDuration);
            yield return null;
        }
        overlayCanvasGroup.alpha = 0f;
        deathOverlay.SetActive(false);

        // --- SHOW DEATH PANEL ---
        deathPanel.SetActive(true);
        // Populate stats
        int enemies = GameManager.Instance != null ? GameManager.Instance.enemiesDefeated : 0;
        int coins = GameManager.Instance != null ? GameManager.Instance.coins : 0;
        float elapsedTime = Time.time - levelStartTime;
        int mins = Mathf.FloorToInt(elapsedTime / 60f);
        int secs = Mathf.FloorToInt(elapsedTime % 60f);
        if (enemiesText != null) enemiesText.text = $"Enemies Defeated: {enemies}";
        if (coinsText != null) coinsText.text = $"Coins: {coins}";
        if (timeText != null) timeText.text = $"Time: {mins}:{secs:00}";
    }

    public void Retry() {
        Debug.Log("MainMenu button clicked");
        Time.timeScale = 1f; 
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); 
    }
    public void MainMenu() { Time.timeScale = 1f; SceneManager.LoadScene("MainMenu"); }
    public void QuitGame()
    {
        Debug.Log("Quit button clicked");
        Time.timeScale = 1f; Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}