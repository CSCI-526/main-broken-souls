using UnityEngine;

public class GunPickup : MonoBehaviour
{
    [SerializeField] private float lifeSeconds = 15f;

    private void Start()
    {
        if (lifeSeconds > 0f) Destroy(gameObject, lifeSeconds);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        var gun = other.GetComponent<PlayerGun>();
        if (gun == null) return;

        // Do not allow pickup if player already has one
        if (gun.HasGun) return;

        gun.GiveGun();
        Destroy(gameObject);
    }
}
