using UnityEngine;

public class LevelEnd : MonoBehaviour
{
    public string nextSceneName = "Level2";
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            GameManager.Instance.LoadScene(nextSceneName);
    }
}