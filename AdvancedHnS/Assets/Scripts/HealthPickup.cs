using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    public float healAmount = 25f;
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<HealthSystem>()?.Heal(healAmount);
            Destroy(gameObject);
        }
    }
}