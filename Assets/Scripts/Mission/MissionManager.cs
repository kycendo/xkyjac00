using Mapbox.Utils;
using Mission;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MissionManager : Singleton<MissionManager>
{
    public GameObject WaypointPrefab;
    public GameObject ZonePrefab;
    public Transform MissionGameObject;
    public Color ActiveWaypoint;
    public Color ReachedWaypoint;
    public Color NonActiveWaypoint;

    private IEnumerable<Waypoint> Waypoints = new List<Waypoint>();

    public void Start()
    {
        
    }

    public void GenerateWaypoints()
    {
        var waypointObjects = new List<Waypoint>();

        var waypoints = new List<Vector2d>
        {
            new Vector2d(49.227335036894324, 16.597139818462026),
            new Vector2d(49.22743663200937, 16.597099585331513),
            new Vector2d(49.22746816148536, 16.597248447914414)
        };

        foreach (var waypoint in waypoints)
        {
            waypointObjects.Add(CreateWaypoint(waypoint, 2f));
        }

        Waypoints = waypointObjects;
    }

    /// <summary>
    /// Create new Waypoint and render it in the scene. Newly created waypoint has
    /// NonActive state and red color and is created under the MissionGameObject.
    /// </summary>
    /// <param name="location">Latitude and longitude of waypoint</param>
    /// <param name="altitude">Relative altitude above the ground</param>
    public Waypoint CreateWaypoint(Vector2d location, float altitude)
    {
        return new Waypoint(MissionGameObject, WaypointState.NotActive, location, altitude);
    }

    public void StartMission(WaypointsOrder order=WaypointsOrder.OrderInList)
    {
        if (!Waypoints.Any())
        {
            return;
        }

        if  (order == WaypointsOrder.FromClosest)
        {
            Waypoints = Waypoints.OrderBy(w => Vector3.Distance(w.WaypointGameObject.transform.position, Camera.main.transform.position));
        }

        Waypoints.First().State = WaypointState.Active;
    }
}

public enum WaypointsOrder
{
    FromClosest,
    OrderInList
}

