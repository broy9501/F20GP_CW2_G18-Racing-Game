using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointHolder : MonoBehaviour
{
    public List<Transform> AIWaypoints = new List<Transform>();

    void Awake()
    {
        foreach (Transform waypointTransform in GetComponentsInChildren<Transform>())
        {
            AIWaypoints.Add(waypointTransform);
        }

            AIWaypoints.RemoveAt(0);

        foreach (Transform waypoint in AIWaypoints)
        {
            MeshRenderer rendererEnabled = waypoint.GetComponent<MeshRenderer>();
            if (rendererEnabled)
            {
                rendererEnabled.enabled = false;
            }
        }
        
    }

    void Update() { }
}
