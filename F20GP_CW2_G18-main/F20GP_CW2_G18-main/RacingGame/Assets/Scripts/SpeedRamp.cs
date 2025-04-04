using UnityEngine;

public class SpeedRamp : MonoBehaviour
{
    public CharacterPowerUp carMovement; // Reference to the car's movement script
    private bool isBoosted = false; // Flag to track if the boost has been triggered

    // This method will handle when the specific car enters the ramp
    void OnTriggerEnter(Collider other)
    {
        // Check if the other object entering the trigger is the car with the "Car" tag
        if (other.CompareTag("Car") && !isBoosted)
        {
            // Ensure we only activate the boost for the car that entered the trigger
            CharacterPowerUp otherCarMovement = other.GetComponent<CharacterPowerUp>();

            if (otherCarMovement != null)
            {
                otherCarMovement.ActivateRampBoost(); // Activate speed boost for the car
                isBoosted = true; // Set the flag to prevent multiple activations
            }
        }
    }

    // Reset the flag when the car exits the ramp area to allow the boost again
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Car"))
        {
            isBoosted = false; // Reset the flag to allow the next boost activation
        }
    }
}
