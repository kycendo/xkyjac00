using Mapbox.Utils;
using UnityEngine;

namespace Mission
{
    public class Zone
    {
        public Vector2d Location { get; set; }
        public double Height { get; set; }
        public double Width { get; set; }
        public ZoneType Type { get; set; }
        public GameObject ZoneGameObject { get; set; }

        public Zone(Transform parent, ZoneType type, Vector2d location, double height, double width, Color color)
        {
            Location = location;
            Height = height;
            Width = width;
            Type = type;

            ZoneGameObject = Object.Instantiate(MissionManager.Instance.ZonePrefab);
            ZoneGameObject.transform.parent = parent;
            color.a = 0.2f;

            var position = GPSManager.Instance.Map.GeoToWorldPosition(Location, false);
            position.y = 20;
            ZoneGameObject.transform.position = position;

            var renderer = ZoneGameObject.GetComponent<Renderer>();
            renderer.material.color = color;
        }
    }

    public enum ZoneType
    {
        FlyZone,
        NoFlyZone
    }
}

