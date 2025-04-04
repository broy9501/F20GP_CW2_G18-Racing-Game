using UnityEngine;
using TMPro;

public class RaceUIController : MonoBehaviour
{
    // Reference to your CheckpointManager
    public CheckpointManager checkpointManager;

    // TextMeshProUGUI components (assign these in the Inspector)
    public TextMeshProUGUI checkpointUIText;
    public TextMeshProUGUI lapUIText;

    void Start()
    {
        // If no CheckpointManager reference is provided, try to find one in the scene.
        if (checkpointManager == null)
        {
            checkpointManager = FindObjectOfType<CheckpointManager>();
            if (checkpointManager == null)
            {
                Debug.LogError("CheckpointManager instance not found in the scene. Make sure the script is attached to a GameObject.");
            }
        }
    }

    void Update()
    {
        if (checkpointManager != null)
        {
            checkpointUIText.text = "Checkpoints: " + checkpointManager.CheckpointsCollectedCount + "/" + checkpointManager.TotalCheckpoints;
            lapUIText.text = "Lap: " + checkpointManager.CurrentLap;
        }
    }
}
