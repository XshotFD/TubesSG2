using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    public int value = 1;
    public float bobSpeed = 2f, bobHeight = 0.2f;
    private Vector3 startPos;

    void Start() => startPos = transform.position;
    void Update()
    {
        float y = startPos.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, y, transform.position.z);
        transform.Rotate(0, 0, 60 * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance?.AddCoins(value);
            Destroy(gameObject);
        }
    }
}