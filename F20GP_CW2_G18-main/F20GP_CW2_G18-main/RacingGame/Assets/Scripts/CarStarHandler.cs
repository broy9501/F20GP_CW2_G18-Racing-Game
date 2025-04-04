using UnityEngine;

public class CarStarHandler : MonoBehaviour
{
    [Header("Star Block Settings")]
    public GameObject starBlockPrefab;    // Assign the Star Block prefab here
    public float starBlockDuration = 3f;    // How long the Star Block will remain
    public float spawnDistance = 2f;        // How far behind the car to spawn the block
    public float spawnYOffset = 2f;       // Vertical offset to prevent the block from intersecting the floor

    private bool hasStar = false;           // Tracks whether the car currently has the star

    [Header("Star Sound Effect")]
    public AudioClip starSoundEffect;
    private CharacterPowerUp characterPowerUpScript;

    void Start()
    {
        characterPowerUpScript = GetComponent<CharacterPowerUp>();
    }
    
    void Update()
    {
        // If the car has a star and the player presses Left Shift, deploy the star block
        if (hasStar && Input.GetKeyDown(KeyCode.LeftShift))
        {
            DeployStarBlock();
        }
    }

    public void CollectStar()
    {
        hasStar = true;
        Debug.Log("Car has collected the Star Power-Up!");
    }

    private void DeployStarBlock()
    {
        if (starBlockPrefab == null)
        {
            Debug.LogError("StarBlock prefab is not assigned in the Inspector!");
            return;
        }

        if (characterPowerUpScript != null && starSoundEffect != null)
        {
            characterPowerUpScript.PlayPowerupEffect(starSoundEffect); // Play the sound effect from the CharacterPowerUp script
        }

        // Calculate spawn position behind the car with an added vertical offset
        Vector3 spawnPos = transform.position - transform.forward * spawnDistance;
        spawnPos.y += spawnYOffset;
        Quaternion spawnRot = transform.rotation;

        // Instantiate the Star Block prefab
        GameObject starBlock = Instantiate(starBlockPrefab, spawnPos, spawnRot);
        Destroy(starBlock, starBlockDuration);

        // Car no longer holds the star once deployed
        hasStar = false;
        Debug.Log("Star Block deployed behind the car!");
    }
}
