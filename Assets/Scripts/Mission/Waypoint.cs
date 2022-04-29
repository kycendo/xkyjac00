using Mapbox.Utils;
using UnityEngine;

namespace Mission
{
    public class Waypoint
    {
        public Vector2d Location { get; set; }
        public WaypointState State 
        { 
            get
            {
                return _state;
            }

            set
            {
                _state = value;
                ChangeColor(value);
                ToggleText(value == WaypointState.Active);
            }
        }
        public GameObject WaypointGameObject { get; set; }
        private WaypointState _state { get; set; }

        public Waypoint(Transform parent, WaypointState state, Vector2d location, float altitude)
        {
            var map = GPSManager.Instance.Map;
            var position = map.GeoToWorldPosition(location, false);

            Location = location;

            WaypointGameObject = Object.Instantiate(MissionManager.Instance.WaypointPrefab);
            WaypointGameObject.transform.parent = parent;

            var textForHololens = WaypointGameObject.transform.Find("TextForHololens");
            textForHololens.GetComponent<WaypointManager>().cameraToFace = Camera.main.transform;

            position.y = altitude - UserProfileManager.Instance.Height;
            WaypointGameObject.transform.position = position;

            var renderer = WaypointGameObject.GetComponent<Renderer>();
            renderer.material.color = MissionManager.Instance.NonActiveWaypoint;

            // Waypoint state must be set after game object is created because of custom setter
            State = state;
        }

        private void ChangeColor(WaypointState state)
        {
            var renderer = WaypointGameObject?.GetComponent<Renderer>();
            if (renderer == null) return;

            var missionManager = MissionManager.Instance;
            Color color;
            switch (state)
            {
                case WaypointState.Active:
                    color = missionManager.ActiveWaypoint;
                    break;
                case WaypointState.Reached:
                    color = missionManager.ReachedWaypoint;
                    break;
                default:
                    color = missionManager.NonActiveWaypoint;
                    break;
            }

            renderer.material.color = color;
        }

        private void ToggleText(bool value)
        {
            var textForHololens = WaypointGameObject.transform.Find("TextForHololens");
            textForHololens.gameObject.SetActive(value);
        }
    }
    public enum WaypointState
    {
        Active,
        Reached,
        NotActive
    }
}

