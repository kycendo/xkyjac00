using Mapbox.Utils;
using UnityEngine;

namespace Mission
{
    public class Zone
    {
        public Vector2d[] Location { get; set; }
        public ZoneType Type { get; set; }
        public GameObject ZoneGameObject { get; set; }

        public Zone(Transform parent, ZoneType type, Vector2d[] location, Color color)
        {
            Location = location;
            Type = type;

            var map = GPSManager.Instance.Map;
            var pA = map.GeoToWorldPosition(location[0], false);
            var pB = map.GeoToWorldPosition(location[1], false);
            ZoneGameObject = CreateGameObject(MissionManager.Instance.ZonePrefab, new Vector2(pA.x, pA.z), new Vector2(pB.x, pB.z));
            ZoneGameObject.transform.parent = parent;
            
            color.a = 0.2f;
            var renderer = ZoneGameObject.GetComponent<Renderer>();
            renderer.material.color = color;
        }

        private GameObject CreateGameObject(GameObject prefab, Vector2 pA, Vector2 pB)
        {
            var height = 10f;
            var zone = Object.Instantiate(prefab);
            var scale = Vector2.Distance(pA, pB);
            var center = pA + (pB - pA) / 2;
            var rotationY = Vector2.Angle(Vector2.up, new Vector2(pB.x - pA.x, pB.y - pA.y)) - 90;

            zone.transform.position = new Vector3(center.x, height/2 - UserProfileManager.Instance.Height, center.y);
            zone.transform.localScale = new Vector3(0.5f, height, scale);
            zone.transform.Rotate(Vector3.up, rotationY);

            return zone;
        }
    }

    public enum ZoneType
    {
        FlyZone,
        NoFlyZone
    }
}

