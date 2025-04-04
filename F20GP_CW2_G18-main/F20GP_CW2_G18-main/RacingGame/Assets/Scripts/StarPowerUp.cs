using UnityEngine;

public class StarPowerUp : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Check if the car collides with the Star Power-Up
        if (other.CompareTag("Car"))
        {
            // Get the CarStarHandler component on the car
            CarStarHandler carHandler = other.GetComponent<CarStarHandler>();
            if (carHandler != null)
            {
                // Let the car know it has collected the star
                carHandler.CollectStar();
            }
            gameObject.SetActive(false);

            Invoke("Respawn", 5f);
        }
    }

    void Respawn()
    {
        gameObject.SetActive(true);
    }
}
