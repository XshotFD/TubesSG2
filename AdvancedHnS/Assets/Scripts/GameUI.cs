using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUI : MonoBehaviour
{
    [Header("Health")]
    public Image healthFill;
    public TMP_Text healthText;
    public Gradient healthGradient;

    [Header("Coins")]
    public TMP_Text coinText;

    [Header("Boss (optional)")]
    public GameObject bossHealthBarRoot;
    public Image bossHealthFill;
    public TMP_Text bossNameText;

    [Header("Notifications")]
    public TMP_Text notificationText;
    public float notificationDuration = 2f;

    private HealthSystem playerHealth;
    private float notificationTimer;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerHealth = player.GetComponent<HealthSystem>();
            if (playerHealth != null)
            {
                healthFill.fillAmount = 1f;
                playerHealth.onDamage.AddListener(RefreshHealth);
                playerHealth.onHeal.AddListener(RefreshHealth);
                RefreshHealth();
            }
        }
        if (bossHealthBarRoot != null) bossHealthBarRoot.SetActive(false);
        if (notificationText != null) notificationText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (notificationTimer > 0f)
        {
            notificationTimer -= Time.deltaTime;
            if (notificationTimer <= 0f && notificationText != null)
                notificationText.gameObject.SetActive(false);
        }

        if (playerHealth != null && healthFill != null)
        {
            float target = playerHealth.GetHealthPercent();
            healthFill.fillAmount = Mathf.Lerp(healthFill.fillAmount, target, Time.deltaTime * 8f);
            healthFill.color = healthGradient.Evaluate(healthFill.fillAmount);
        }

        // 🔧 FIX: coins read from GameManager (Doc 4 used an undefined CurrencySystem)
        if (coinText != null && GameManager.Instance != null)
            coinText.text = GameManager.Instance.coins.ToString();
    }

    void RefreshHealth()
    {
        if (playerHealth == null) return;
        if (healthText != null)
            healthText.text = $"{Mathf.RoundToInt(playerHealth.currentHealth)} / {playerHealth.maxHealth}";
    }

    public void ShowBossBar(string name, HealthSystem boss)
    {
        if (bossHealthBarRoot == null) return;
        bossHealthBarRoot.SetActive(true);
        if (bossNameText != null) bossNameText.text = name;
        boss.onDamage.AddListener(() => { if (bossHealthFill != null) bossHealthFill.fillAmount = boss.GetHealthPercent(); });
        boss.onDeath.AddListener(() => bossHealthBarRoot.SetActive(false));
    }

    public void ShowNotification(string msg)
    {
        if (notificationText == null) return;
        notificationText.text = msg;
        notificationText.gameObject.SetActive(true);
        notificationTimer = notificationDuration;
    }
}