using UnityEngine;

public class PowerDownSpeed : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public CharacterPowerUp carMovement; // Reference to the car movement script

    void OnTriggerEnter(Collider other)
    {
        // Check if the object colliding is the car (make sure the car has the "Car" tag)
        if (other.CompareTag("Car"))
        {
            CharacterPowerUp carMovement = other.GetComponent<CharacterPowerUp>();
            if (carMovement != null)
            {
                carMovement.ActivateSpeedDown();
                // Disable the power-up object and respawn after 5 seconds
                gameObject.SetActive(false);
                Invoke("Respawn", 5f);
            }
            else
            {
                Debug.LogError("The object with tag 'Car' does not have a CharacterPowerUp component.");
            }
        }
    }

    void Respawn()
    {
        gameObject.SetActive(true);
    }
}
