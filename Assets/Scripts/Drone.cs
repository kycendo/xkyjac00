using Mapbox.Unity.Map;
using Mapbox.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drone {

    public GameObject DroneGameObject {
        get; set;
    }

    public DroneFlightData FlightData {
        get; set;
    }

    public bool IsControlled {
        get; set;
    }

    public float RelativeAltitude {
        get
        {
            var latlong = new Vector2d(FlightData.Latitude, FlightData.Longitude);
            var groundAltitude = DroneManager.Instance.Map.QueryElevationInUnityUnitsAt(latlong);
            return (float)FlightData.Altitude - groundAltitude;
        }
    }

    public Drone(GameObject droneGameObject, DroneFlightData flightData, bool isControlled=false) {
        DroneGameObject = droneGameObject;
        FlightData = flightData;
        IsControlled = isControlled;
    }

    public void UpdateDroneFlightData(DroneFlightData flightData) {
        FlightData = flightData;
        if (double.IsNaN(FlightData.Latitude) || double.IsNaN(FlightData.Longitude)) {
            return;
        }

        Vector2d mapboxPosition = new Vector2d(FlightData.Latitude, FlightData.Longitude);
        Vector3 position3d = DroneManager.Instance.Map.GeoToWorldPosition(mapboxPosition, false);

        if (FlightData.DroneId == "DJI-Mavic2" || FlightData.DroneId == "DJI-MAVIC_PRO") {
            //float groundAltitude = DroneManager.Instance.Map.QueryElevationInUnityUnitsAt(DroneManager.Instance.Map.WorldToGeoPosition(position3d));
            //position3d.y = groundAltitude + (float) FlightData.Height;
            position3d.y = (float)FlightData.Altitude;
        } else {
            // ground altitude has to be calculated from camera location
            float groundAltitude = DroneManager.Instance.Map.QueryElevationInUnityUnitsAt(DroneManager.Instance.Map.WorldToGeoPosition(GPSManager.Instance.Camera.position));
            position3d.y = (float) FlightData.Altitude - groundAltitude;
        }
        if (!DroneManager.RunningInUnityEditor)
        {
            position3d.y -= UserProfileManager.Instance.Height;
        }

        if (CheckThreshold(DroneGameObject.transform.position, position3d))
        {
            DroneGameObject.transform.position = position3d;
        }
           
        DroneGameObject.transform.eulerAngles = new Vector3((float) FlightData.Pitch, (float) FlightData.Yaw, (float) FlightData.Roll);
    }

    public void UpdateDronePosition(double latitude, double longitude) {
        FlightData.Latitude = latitude;
        FlightData.Longitude = longitude;
    }

    private bool CheckThreshold(Vector3 point1, Vector3 point2)
    {
        return Vector3.Distance(point1, point2) > (float)(UserProfileManager.Instance.DroneThreshold);
    }
}
