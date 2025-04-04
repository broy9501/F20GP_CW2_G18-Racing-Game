using UnityEngine;

public class PowerUpSpeed : MonoBehaviour
{
    public CharacterPowerUp carMovement; // Reference to the car movement script

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Car"))
        {
            CharacterPowerUp carMovement = other.GetComponent<CharacterPowerUp>();
            if (carMovement != null)
            {
                carMovement.ActivateSpeedBoost(); // Call the speed boost method
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
