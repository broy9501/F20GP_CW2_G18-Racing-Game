using UnityEngine;

public class InvertControls : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Car")) {
            CharacterPowerUp carMovement = other.GetComponent<CharacterPowerUp>();

            if (carMovement != null)
            {
                carMovement.ActivateInvertedControls();
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
