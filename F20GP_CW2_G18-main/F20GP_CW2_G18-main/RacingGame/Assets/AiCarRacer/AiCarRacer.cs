using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AICarRacer : MonoBehaviour
{
    [Header("Waypoint System Settings")]
    public WaypointHolder waypointHolder;
    public List<Transform> waypoints;
    public int CurrentWaypoint;
    public float rangeWaypoint = 5f;

    [Header("NavMesh Settings")]
    private NavMeshAgent navMeshAgent;

    [Header("Car Movement Settings")]
    public float maxSpeed = 30f;
    public float acceleration = 20f;
    public float deceleration = 5f;
    public float braking = 30f;
    public float steering = 3f;
    [Range(0f, 1f)] 
    private float turnInput;
    private Vector3 moveDirection;
    private bool isBraking = false;
    private float currentSpeed;
    private float targetSpeed;
    public float brakingSpeedThreshold;

    [Header("Speed Up and Down Settings")]
    public float speedBoostMultiplier = 1.5f;
    public float PowerDownMultiplier = 0.5f;
    public float SpeedUpDownDuration = 2f;
    private bool isBoosted = false;
    private bool isSlowed = false;

    [Header("Shield Powerup Settings")]
    public float shieldDuration = 10f;
    private bool isShielded = false;
    public GameObject shieldBubbleObject;
    private GameObject shieldBubble;

    [Header("Sound Effect Settings")]
    public AudioClip engineSound;
    public AudioClip speedBoostSound;
    public AudioClip starSound;
    public AudioClip powerDownSound;
    public AudioClip shieldSound;
    private AudioSource engineAudioSource;
    private AudioSource powerupAudioSource;
    private bool soundEffectPlayed = false;

    [Header("Star Powerup Settings")]
    public GameObject starBlockPrefab;
    public float starBlockLifetime = 3f;
    public Vector3 starSpawnOffset = new Vector3(0f, 5f, -3f);

    [Header ("RigidBody Settings")]
    private Rigidbody AIrb;
    private float originalCarMass;

    [Header("Laps Settings")]
    public int laps = 0;
    public int maxLaps = 1;
    public GameObject loseScreenUI;
    private bool passedBeforeFinishCheck = false;
    public GameObject loseText;

    public Transform[] wheels;
    public float wheelRadius = 0.4f; 
    public float steeringAngle = 30f; 
    public bool[] isSteeringWheel; 

    void Start()
    {
        waypointHolder = FindObjectOfType<WaypointHolder>();
        waypoints = waypointHolder.AIWaypoints;
        CurrentWaypoint = 0;

        AIrb = GetComponent<Rigidbody>();
        if (AIrb != null)
        {
            originalCarMass = AIrb.mass;
        }

        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.stoppingDistance = rangeWaypoint;
        navMeshAgent.speed = maxSpeed / 3.6f;
        navMeshAgent.acceleration = acceleration;
        navMeshAgent.angularSpeed = steering * 50f;
        currentSpeed = maxSpeed;
        targetSpeed = maxSpeed;
        brakingSpeedThreshold = maxSpeed * 0.9f / 3.6f;

        engineAudioSource = gameObject.AddComponent<AudioSource>();
        engineAudioSource.spatialBlend = 1f;
        engineAudioSource.maxDistance = 50f;
        engineAudioSource.minDistance = 10f;
        powerupAudioSource = gameObject.AddComponent<AudioSource>();
        powerupAudioSource.spatialBlend = 1f;
        powerupAudioSource.maxDistance = 50f;
        powerupAudioSource.minDistance = 10f;
        engineAudioSource.loop = true;
        engineAudioSource.clip = engineSound;
        engineAudioSource.Play();     
        NextWaypoint();
    }

    void Update()
    {
        if (!navMeshAgent.pathPending)
        {
            //if navmesh agent reaches waypoint change current waypoint and call NextWaypoint to set new destination
            if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
            {
                CurrentWaypoint++;
                if (CurrentWaypoint >= waypoints.Count)
                {
                    CurrentWaypoint = 0;
                }
                NextWaypoint();
            }
        }
        AdjustMovement();
        AdjustSpeed();
        UpdateWheels();
    }

    private void NextWaypoint()
    {
        //set navmesh agent destination to next waypoint in lists position
        navMeshAgent.SetDestination(waypoints[CurrentWaypoint].position);
    }

    private void AdjustMovement()
    {
        moveDirection = (waypoints[CurrentWaypoint].position - transform.position).normalized;
        float angleToWaypoint = Vector3.SignedAngle(transform.forward, moveDirection, Vector3.up);
        turnInput = Mathf.Clamp(angleToWaypoint / 45f, -1f, 1f);

        float turnStrength = turnInput * steering;
        float speedFactor = Mathf.Clamp01(navMeshAgent.speed / (maxSpeed / 3.6f));
        turnStrength *= Mathf.Lerp(1f, 0.5f, speedFactor);
        transform.Rotate(0, turnStrength * Time.deltaTime * 100f, 0);
    }

    
    private void AdjustSpeed()
    {
        //if braking adjust speed to be slower for easier corner turning
        if (isBraking)
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, braking * Time.deltaTime);
        }
        //else if not braking go maximum speed
        else
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, maxSpeed, acceleration * Time.deltaTime);
        }
        
        navMeshAgent.speed = currentSpeed / 3.6f;
        UpdateEngineSound();

    }
     private void UpdateWheels()
    {
        float wheelRPM = Mathf.Abs(navMeshAgent.speed * 3.6f) / (2f * Mathf.PI * wheelRadius) * 60f;
        float rotationSpeed = (wheelRPM / 60f) * 360f;
        float rotationDirection = Mathf.Sign(navMeshAgent.velocity.magnitude);

        for (int i = 0; i < wheels.Length; i++)
        {
            if (wheels[i] != null)
            {
                bool isBackWheel = isSteeringWheel != null && i < isSteeringWheel.Length && !isSteeringWheel[i];
                float wheelRotationModifier = isBackWheel ? -1f : 1f;
                wheels[i].Rotate(rotationSpeed * rotationDirection * wheelRotationModifier * Time.deltaTime, 0, 0, Space.Self);

                if (isSteeringWheel != null && i < isSteeringWheel.Length && isSteeringWheel[i])
                {
                    float steerAngle = navMeshAgent.desiredVelocity.x * steeringAngle;
                    wheels[i].localRotation = Quaternion.Euler(
                        wheels[i].localRotation.eulerAngles.x,
                        steerAngle,
                        wheels[i].localRotation.eulerAngles.z
                    );
                }
            }
        }
    }
    private void UpdateEngineSound()
    {
        //change engine pitch and volume depending on current speed of the car
        if (engineAudioSource != null)
        {
            engineAudioSource.pitch = Mathf.Lerp(1f, 2f, currentSpeed / maxSpeed);
            engineAudioSource.volume = Mathf.Lerp(0.5f, 1f, currentSpeed / maxSpeed);
        }
    }

    //When car enters a brake zone they reduce their speed to take turns easier
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("BrakingZone"))
        {
           if (navMeshAgent.speed >= brakingSpeedThreshold)
            {
                //Debug.Log(gameObject.name + " entered a brake zone");
                isBraking = true;
                targetSpeed = maxSpeed * 0.5f;
            }
        }
        if (other.CompareTag("SpeedRamp"))
        {
            Debug.Log(gameObject.name + " hit a speedboost");
            other.gameObject.SetActive(false);
            if (!soundEffectPlayed)
            {
                PlayPowerupEffect(speedBoostSound);  //Play speed boost sound
                soundEffectPlayed = true;
            }
            StartCoroutine(RespawnPowerup(other.gameObject));            
            StartCoroutine(ActivateSpeedBoost());
        }
        if (other.CompareTag("PowerDown"))
        {
            Debug.Log(gameObject.name + " hit a powerdown");
            other.gameObject.SetActive(false);
            if (!soundEffectPlayed)
            {
                PlayPowerupEffect(powerDownSound);  //Play slow down effect
                soundEffectPlayed = true;
            }
            StartCoroutine(RespawnPowerup(other.gameObject));
            StartCoroutine(ActivatePowerDown());
        }
        if (other.CompareTag("Shield"))
        {
            Debug.Log(gameObject.name + " has a shield");
            other.gameObject.SetActive(false);
            if (!soundEffectPlayed)
            {
                PlayPowerupEffect(shieldSound);  //Play speed boost sound
                soundEffectPlayed = true;
            }
            StartCoroutine(RespawnPowerup(other.gameObject));
            StartCoroutine(ActivateShield());
        }
        if (other.CompareTag("StarZone"))
        {
            other.gameObject.SetActive(false);
            Debug.Log(gameObject.name + " has entered a star zone");
            if (!soundEffectPlayed)
            {
                PlayPowerupEffect(starSound);  //Play speed boost sound
                soundEffectPlayed = true;
            }
            StartCoroutine(RespawnPowerup(other.gameObject));
            StartCoroutine(SpawnStarObstacle());
        }
        if (other.CompareTag("InvertZone"))
        {
            other.gameObject.SetActive(false);
            StartCoroutine(RespawnPowerup(other.gameObject));
        }
        if (other.CompareTag("StuckZone"))
        {
            Debug.Log(gameObject.name + " is stuck outside of the map. Teleporting back now.");
            TeleportToNextWaypoint();
        }
        if (other.CompareTag("FinishLine") && passedBeforeFinishCheck)
        {
            laps++;
            passedBeforeFinishCheck = false;
            Debug.Log(gameObject.name + " is on lap " + laps);
            if (laps >= 3)
            {
                Debug.Log(gameObject.name + " had finished the race and completed all laps. You lose.");
                LoseGameScreen();
            }  
        }
        if (other.CompareTag("BeforeFinish"))
        {
            passedBeforeFinishCheck = true;
        }

    }

    private void OnCollisionEnter(Collision collision)
    {
        //if an ai hits a star block they bounce of it.
        if (collision.gameObject.CompareTag("starBlock"))
        {
            Debug.Log(gameObject.name + " hit a placed star");            
            if (AIrb != null)
            {
                Vector3 directionOfBounce = (transform.position - collision.contacts[0].point).normalized;
                float forceOfBounce = 2f;
                AIrb.AddForce(directionOfBounce * forceOfBounce, ForceMode.Impulse);
            }
        }
    }

    //When car exits a brake zone they increase speed again
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("BrakingZone"))
        {
            //Debug.Log(gameObject.name + " has left a brake zone");
            isBraking = false;
            targetSpeed = maxSpeed;
        }
    }

    //When an ai car enters a speed boost zone they get a temporary speed boost for a couple of seconds
    private IEnumerator ActivateSpeedBoost()
    {
        if (!isBoosted)
        {
            isBoosted = true;
            
            float normalSpeed = maxSpeed;  //save old max speed to return to after speed up
            maxSpeed *= speedBoostMultiplier;  //allow the car to get a faster max speed
            yield return new WaitForSeconds(SpeedUpDownDuration); //wait for speed up duration
            soundEffectPlayed = false;
            
            maxSpeed = normalSpeed;  //change max speed back to normal
            isBoosted = false;
        }
    }

    //when an ai car enters a power down zone their speed is reduced for a couple of seconds
    private IEnumerator ActivatePowerDown()
    {
         if (!isShielded)
         {
            if (!isSlowed)
            {
                isSlowed = true;
                
                float normalSpeed = maxSpeed;  //save old max speed to return to after slow down
                maxSpeed *= PowerDownMultiplier;  //slow the car down by halfing max speed
                yield return new WaitForSeconds(SpeedUpDownDuration);  //wait for slowdown duration
                soundEffectPlayed = false;
                
                maxSpeed = normalSpeed;  //change max speed back to normal
                isSlowed = false;
            }
         }
        else
        {
            Debug.Log(gameObject.name + " hit a powerdown but has a shield!");
        }
    }

    //when ai car enters a shield power-up, they get shielded from slowdown effects
    private IEnumerator ActivateShield()
    {
        if (!isShielded)
        {
            isShielded = true;
            Debug.Log(gameObject.name + " shield activated");
            shieldBubble = Instantiate(shieldBubbleObject, transform.position, Quaternion.identity);
            shieldBubble.transform.parent = transform;
            shieldBubble.transform.localPosition = new Vector3(0f, 1f, 0f);
            AIrb.mass = 1000f;
            
            yield return new WaitForSeconds(shieldDuration);
            soundEffectPlayed = false;
            Destroy(shieldBubble);
            AIrb.mass = originalCarMass;
            isShielded = false;
            Debug.Log(gameObject.name + " shield ran out");
        }
    }

    private IEnumerator SpawnStarObstacle()
    {
        Vector3 starSpawnPos = transform.position + transform.TransformDirection(starSpawnOffset);
        GameObject starPrefab = Instantiate(starBlockPrefab, starSpawnPos, Quaternion.identity);

        Collider starCollider = starPrefab.GetComponent<Collider>();
        yield return new WaitForSeconds(starBlockLifetime);
        soundEffectPlayed = false;

        Destroy(starPrefab);
        Debug.Log("Star obstacle despawned!");
    }

    private void PlayPowerupEffect(AudioClip soundEffect)
    {
        if (powerupAudioSource != null && soundEffect != null)
        {
            powerupAudioSource.PlayOneShot(soundEffect);
        }
    }
    
    private IEnumerator RespawnPowerup(GameObject powerup)
    {
        yield return new WaitForSeconds(5f);
        powerup.SetActive(true);
        Debug.Log("powerup respawned!");
    }

    
    private void TeleportToNextWaypoint()
    {
        //used to teleport ai racers to their next waypoint if they get stuck on the edges of the map
        Vector3 nextWaypointPosition = waypoints[CurrentWaypoint].position;
        nextWaypointPosition.y += 1f;
        transform.position = nextWaypointPosition;
        navMeshAgent.SetDestination(nextWaypointPosition);
    }

    void LoseGameScreen()
    {
        Debug.Log("The AI racers won the race. You lose...");
        loseText.SetActive(true);
        Time.timeScale = 0;
    }

}
