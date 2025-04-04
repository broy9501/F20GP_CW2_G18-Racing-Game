using UnityEngine;
using System.Collections;

public class CharacterPowerUp : MonoBehaviour
{
    // Reference to the movement script
    public move characterMovement;

    [Header("Speed Boost Settings")]
    public float boostSpeed = 30f;      // Boosted speed value
    public float boostAcc = 8f;         // Boosted acceleration
    public float boostDuration = 3f;    // How long the boost lasts
    private bool isBoosted = false;
    private float originalSpeed;        // Stores normal speed
    private float originalAcc;          // Stores normal acceleration

    [Header("Ramp Boost Settings")]
    public float boostSpeedRamp = 35f;  // Ramp boost speed
    public float boostAccRamp = 12f;    // Ramp boost acceleration
    public float boostRampDuration = 0.5f; // Duration of the ramp boost

    [Header("Speed Down Settings")]
    public float speedDown = 5f;        // Slowed speed value
    public float speedDownAcc = 3f;       // Slowed acceleration
    private bool isSpeedDown = false;

    [Header("Shield Settings")]
    public float shieldDuration = 5f;   // How long the shield lasts
    public float boostedMass = 500f;    // Increased mass while shielded
    private bool isShielded = false;
    private float normalMass;           // Normal mass
    public GameObject shieldBubbleObject; // Shield visual prefab
    private GameObject shieldBubble;

    [Header("Inverted Controls Settings")]
    public float invertedControlsDuration = 3f;
    private bool isInvertedControls = false;

    private Rigidbody rb;

    [Header("Sound Effects")]
    public AudioClip speedBoostSound;
    public AudioClip invertControlsSound;
    public AudioClip powerDownSound;
    public AudioClip shieldSound;
    private AudioSource powerupAudioSource;

    void Start()
    {
        // Get movement and physics components
        if (characterMovement == null)
            characterMovement = GetComponent<move>();

        rb = GetComponent<Rigidbody>();
        normalMass = rb.mass;
        originalSpeed = characterMovement.speed;
        originalAcc = characterMovement.acceleration;
        powerupAudioSource = gameObject.AddComponent<AudioSource>();
    }

    // Temporarily increase speed and acceleration
    public void ActivateSpeedBoost()
    {
        if (!isBoosted)
        {
            isBoosted = true;
            characterMovement.speed = boostSpeed;
            characterMovement.acceleration = boostAcc;
            PlayPowerupEffect(speedBoostSound);
            Invoke("DeactivateSpeedBoost", boostDuration);
        }
    }

    // Short, quick boost ramp
    public void ActivateRampBoost()
    {
        if (!isBoosted)
        {
            isBoosted = true;
            characterMovement.speed = boostSpeedRamp;
            characterMovement.acceleration = boostAccRamp;
            PlayPowerupEffect(speedBoostSound);
            Invoke("DeactivateSpeedBoost", boostRampDuration);
        }
    }

    // Reduce speed and acceleration temporarily
    public void ActivateSpeedDown()
    {
        if (isShielded) return; // Don't slow down if shielded
        if (!isSpeedDown)
        {
            isSpeedDown = true;
            characterMovement.speed = speedDown;
            characterMovement.acceleration = speedDownAcc;
            PlayPowerupEffect(powerDownSound);
            Invoke("DeactivateSpeedBoost", boostDuration);
        }
    }

    // Slow down when hitting the floor
    public void ActivateSpeedDownFloor()
    {
        if (!isSpeedDown)
        {
            isSpeedDown = true;
            characterMovement.speed = speedDown;
            characterMovement.acceleration = speedDownAcc;
            PlayPowerupEffect(powerDownSound);
        }
    }

    // Reset speed and acceleration back to normal
    void DeactivateSpeedBoost()
    {
        characterMovement.speed = originalSpeed;
        characterMovement.acceleration = originalAcc;
        isBoosted = false;
        isSpeedDown = false;
    }

    // Activate a shield that increases mass and shows a visual bubble.
    public void ActivateShield()
    {
        if (!isShielded)
        {
            isShielded = true;
            rb.mass = boostedMass;
            PlayPowerupEffect(shieldSound);
            shieldBubble = Instantiate(shieldBubbleObject, transform.position, Quaternion.identity);
            shieldBubble.transform.parent = transform;
            shieldBubble.transform.localPosition = new Vector3(0f, 1f, 0f);
            Debug.Log("Shield activated. Mass: " + rb.mass);
            Invoke("DeactivateShield", shieldDuration);
        }
    }

    // Turn off the shield effect
    void DeactivateShield()
    {
        rb.mass = normalMass;
        isShielded = false;
        Destroy(shieldBubble);
        Debug.Log("Shield deactivated. Mass: " + rb.mass);
    }

    // Activate inverted controls for a short duration
    public void ActivateInvertedControls()
    {
        if (isShielded) return;
        if (!isInvertedControls)
        {
            PlayPowerupEffect(invertControlsSound);
            isInvertedControls = true;
            StartCoroutine(InvertControlsActivation());
        }
    }

    private IEnumerator InvertControlsActivation()
    {
        characterMovement.isInvertedControls = true;
        yield return new WaitForSeconds(invertedControlsDuration);
        characterMovement.isInvertedControls = false;
        isInvertedControls = false;
    }

    // Play a sound effect for powerups
    public void PlayPowerupEffect(AudioClip soundEffect)
    {
        if (powerupAudioSource != null && soundEffect != null)
            powerupAudioSource.PlayOneShot(soundEffect);
    }

    // When colliding with the floor, slow down and mark as off-track
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Floor"))
        {
            ActivateSpeedDownFloor();
            characterMovement.isOffTrack = true;
        }
    }

    // When leaving the floor, restore speed and clear off-track state
    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Floor"))
        {
            DeactivateSpeedBoost();
            characterMovement.isOffTrack = false;
        }
    }
}
