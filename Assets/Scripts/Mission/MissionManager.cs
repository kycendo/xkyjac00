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
    private IEnumerable<Zone> Zones = new List<Zone>();
    public Waypoint CurrentTarget;

    public void Start()
    {
        
    }

    /// <summary>
    /// Check if drone reached active waypoint. If yes, set the next active waypoint
    /// </summary>
    public void Update()
    {
        var drone = DroneManager.Instance.ControlledDroneGameObject;

        if (CurrentTarget == null || drone == null) return;
        var dronePosition = drone.transform.position;
        var waypointPosition = CurrentTarget.WaypointGameObject.transform.position;
        if (Vector3.Distance(dronePosition, waypointPosition) <= CurrentTarget.WaypointGameObject.transform.lossyScale.x / 2)
        {
            CurrentTarget.State = WaypointState.Reached;
            CurrentTarget = Waypoints.FirstOrDefault(x => x.State == WaypointState.NotActive);
            if (CurrentTarget != null)
            {
                CurrentTarget.State = WaypointState.Active;
            }
        }
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

        var zone = new Vector2d[2] { new Vector2d(49.22716285112991, 16.597185306750877), new Vector2d(49.22724780597617, 16.597090758882445) };

        foreach (var waypoint in waypoints)
        {
            waypointObjects.Add(CreateWaypoint(waypoint, 2f));
        }

        Waypoints = waypointObjects;

        Zones = Zones.Append(CreateZone(zone));
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

    public Zone CreateZone(Vector2d[] points)
    {
        return new Zone(MissionGameObject, ZoneType.NoFlyZone, points, Color.red);
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

        CurrentTarget = Waypoints.First();
        CurrentTarget.State = WaypointState.Active;
    }
}

public enum WaypointsOrder
{
    FromClosest,
    OrderInList
}

