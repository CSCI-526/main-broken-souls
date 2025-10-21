using UnityEngine;

public class Collectible : MonoBehaviour
{
    [Header("Collectible Settings")]
    public int scoreValue = 50;
    public GameObject collectEffect;   // optional VFX prefab

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // Spawn optional pickup VFX
        if (collectEffect != null)
            Instantiate(collectEffect, transform.position, Quaternion.identity);

        // Add score
        ScoreManager.Instance.AddScore(scoreValue);

        // Show floating "+XX" popup at the coin position
        FloatingTextSpawner.I?.Show($"+{scoreValue}", transform.position);

        Debug.Log("Collected! +" + scoreValue);

        // Remove coin
        Destroy(gameObject);
    }
}
