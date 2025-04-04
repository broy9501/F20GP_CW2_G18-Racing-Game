using UnityEngine;
using System.Collections.Generic;

public class PowerupSpawner : MonoBehaviour
{
    public Transform[] spawnPointsArray;
    public GameObject[] powerupsArray;
    public Vector3 powerupSize = new Vector3(1f, 1f, 1f);
    private List<Transform> spawnPointsList = new List<Transform>();

    void Start()
    {
        //this sets an array of all the spawn points by looping through the children of SpawnPoints and then randomly assigns a powerup to spawn in the spawn point locations
        SetSpawnPoints();
        SpawnPowerups();
    }
    void SpawnPowerups()
    {
        if (spawnPointsArray.Length == 0 || powerupsArray.Length == 0)
        {
            Debug.LogWarning("No spawn points or powerups assigned in PowerupSpawner script");
            return;
        }

        for (int i = 0; i < spawnPointsArray.Length; i++)
        {
            Vector3 spawnPosition = spawnPointsArray[i].position;
            GameObject powerup = powerupsArray[Random.Range(0, powerupsArray.Length)];

            GameObject spawnPowerup = Instantiate(powerup, spawnPosition, Quaternion.identity);
            spawnPowerup.transform.localScale = powerupSize;
        }
    }

    void SetSpawnPoints()
    {
        Transform[] spawnPointsChildren = GetComponentsInChildren<Transform>();
        if (spawnPointsChildren.Length == 1) return;

        spawnPointsArray = new Transform[spawnPointsChildren.Length - 1];

        int index = 0;
        foreach (Transform child in spawnPointsChildren)
        {
            if (child != transform)
            {
                spawnPointsArray[index] = child;
                index++;
            }
        }
    }
}
