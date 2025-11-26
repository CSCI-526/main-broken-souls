using UnityEngine;
using UnityEngine.SceneManagement;

public class ShopTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered shop! Loading shop scene...");
            SceneManager.LoadScene("ShopScene"); // <-- Change to your actual shop scene name
        }
    }
}
