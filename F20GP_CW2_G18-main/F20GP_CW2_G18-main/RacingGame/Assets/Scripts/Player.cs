using UnityEngine;

public class Player : MonoBehaviour
{
    // This script acts as a marker to identify the PlayerCar GameObject
    // You can add more functionality here later if needed
    private void Start()
    {
        Debug.Log("PlayerCar component initialized on " + gameObject.name);
    }
}