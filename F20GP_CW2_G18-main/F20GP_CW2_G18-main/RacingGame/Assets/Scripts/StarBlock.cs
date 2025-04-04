using UnityEngine;

public class StarBlock : MonoBehaviour
{
    public float blockDuration = 3f; // How long the block stays in the scene

    private void Start()
    {
        // Destroy the block after the specified duration
        Destroy(gameObject, blockDuration);
    }

    // Optionally, add collision detection with other cars to stop their movement
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Car"))
        {
            // Handle collision with cars (e.g., stop or slow them down)
            Debug.Log("Star Block hit by a car!");
        }
    }
}
