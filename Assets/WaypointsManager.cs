using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointsManager : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

public class Waypoint
{
    public Vector2 Location { get; set; }
    public WaypointState State { get; set; }
}

public enum WaypointState
{
    Active,
    Reached,
    NotActive
}
