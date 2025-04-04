using System.Collections.Generic;
using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    // List of all checkpoints in the track.
    private List<Transform> allCheckpoints = new List<Transform>();
    // Checkpoints the player has hit in the current lap.
    private List<Transform> checkpointsCollected = new List<Transform>();

    // The last checkpoint hit.
    private Transform lastCheckpoint = null;

    // Floor collision for respawn.
    private bool isOnFloor = false;
    private float floorTimer = 0f;
    public float timeToRespawn = 0.3f; // Delay before respawning on floor contact.

    // Timer to prevent immediate re-triggering of checkpoints.
    public float checkpointBufferTime = 1f;
    private float checkpointBufferTimer = 0f;

    // Lap tracking.
    private int lapCounter = 1;
    private const int maxLaps = 3;
    public List<Transform> lap1 = new List<Transform>();
    public List<Transform> lap2 = new List<Transform>();
    public List<Transform> lap3 = new List<Transform>();

    // Controls the brief pause between laps.
    private bool lapResetting = false;
    public float lapResetBufferTime = 1f;
    private float lapResetTimer = 0f;

    // UI elements for when the race is finished.
    public GameObject winScreenUI;
    public GameObject winText;

    void Start()
    {
        // Find all checkpoint objects tagged "checkpoints" in the scene.
        GameObject[] checkpointObjects = GameObject.FindGameObjectsWithTag("checkpoints");
        foreach (GameObject checkpoint in checkpointObjects)
        {
            allCheckpoints.Add(checkpoint.transform);
        }

        Debug.Log("Total checkpoints in track: " + allCheckpoints.Count);
    }

    void Update()
    {
        // Decrease checkpoint buffer timer.
        if (checkpointBufferTimer > 0f)
            checkpointBufferTimer -= Time.deltaTime;

        // If on floor and buffer has expired, start respawn timer.
        if (isOnFloor && checkpointBufferTimer <= 0f)
        {
            floorTimer += Time.deltaTime;
            if (floorTimer >= timeToRespawn)
            {
                RespawnToLastCheckpoint();
                floorTimer = 0f;
                isOnFloor = false;
            }
        }

        // Handle lap reset buffer.
        if (lapResetting)
        {
            lapResetTimer -= Time.deltaTime;
            if (lapResetTimer <= 0f)
                lapResetting = false;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // When hitting a checkpoint.
        if (other.CompareTag("checkpoints"))
        {
            // Ignore if we're in a lap reset period.
            if (lapResetting) return;

            Transform checkpoint = other.transform;

            // Only add if this checkpoint wasn't already hit.
            if (!checkpointsCollected.Contains(checkpoint))
            {
                checkpointsCollected.Add(checkpoint);
                lastCheckpoint = checkpoint;

                Debug.Log("Collected checkpoint: " + checkpoint.name);
                Debug.Log("Checkpoints collected in lap " + lapCounter + ": " + checkpointsCollected.Count + "/" + allCheckpoints.Count);
            }

            // Reset floor respawn and add a buffer delay.
            isOnFloor = false;
            floorTimer = 0f;
            checkpointBufferTimer = checkpointBufferTime;
        }

        // When hitting the finish line, check if all checkpoints are collected.
        if (other.CompareTag("FinishLine"))
        {
            if (checkpointsCollected.Count == allCheckpoints.Count)
                CompleteLap();
            else
                Debug.Log("Cannot complete lap. Missing checkpoints.");
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Start the respawn timer when touching the floor
        if (collision.gameObject.CompareTag("Floor") && checkpointBufferTimer <= 0f)
        {
            isOnFloor = true;
            floorTimer = 0f;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        // Reset floor detection when leaving the floor
        if (collision.gameObject.CompareTag("Floor"))
        {
            isOnFloor = false;
            floorTimer = 0f;
        }
    }

    // Respawn player to the last checkpoint
    void RespawnToLastCheckpoint()
    {
        if (lastCheckpoint != null)
        {
            transform.position = lastCheckpoint.position;
            GetComponent<move>().ResetMovement();
            Debug.Log("Respawned at checkpoint: " + lastCheckpoint.name);
        }
        else
        {
            Debug.Log("No checkpoint reached yet. Cannot respawn.");
        }
    }

    // Called when a lap is completed
    void CompleteLap()
    {
        // Save the current lap's checkpoints
        switch (lapCounter)
        {
            case 1:
                lap1 = new List<Transform>(checkpointsCollected);
                break;
            case 2:
                lap2 = new List<Transform>(checkpointsCollected);
                break;
            case 3:
                lap3 = new List<Transform>(checkpointsCollected);
                break;
        }

        // If more laps remain, prepare for the next lap
        if (lapCounter < maxLaps)
        {
            lapCounter++;
            checkpointsCollected.Clear();
            lapResetting = true;
            lapResetTimer = lapResetBufferTime;
            Debug.Log("Starting lap: " + lapCounter);
        }
        else
        {
            Debug.Log("Laps finished! Race completed.");
            WinGameScreen();
        }
    }

    // Public properties for external access
    public int CheckpointsCollectedCount { get { return checkpointsCollected.Count; } }
    public int TotalCheckpoints { get { return allCheckpoints.Count; } }
    public int CurrentLap { get { return lapCounter; } }

    // Display the win screen
    void WinGameScreen()
    {
        winText.SetActive(true);
        Time.timeScale = 0; // Pause the game
    }
}
