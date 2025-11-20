using UnityEngine;

public class CoinMover : MonoBehaviour
{
    public EndlessGround ground;
    public float despawnX = -20f;

    void Update()
    {
        // always match the ground scroll speed in real time
        float speed = ground.scrollSpeed;

        transform.position += Vector3.left * speed * Time.deltaTime;

        if (transform.position.x < despawnX)
            Destroy(gameObject);
    }
}
