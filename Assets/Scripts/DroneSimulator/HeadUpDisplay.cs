﻿/*
 * Head Up Display - manages visualizasion of HUD information
 * 
 * author: Marek Václavík
 * login: xvacla26
 * 
 */

using Mapbox.Unity.Map;
using Mapbox.Utils;
using UnityEngine;
using UnityEngine.UI;




public class HeadUpDisplay : MonoBehaviour
{
    const float NavigationArrowOffset = 400.0f;
    const float AltitudeIndicatorOffset = -174.0f;
    const float AltitudeValueToTransformMultiplier = 50.0f;
    const float AltitudeTextOffset = -88.0f;


    Transform MainCamera;
    public Transform drone;
    public Transform altitudeIndicator;
    public Transform batteryIndicator;
    public Transform mirrorDrone;
    RectTransform HUD;
    public RectTransform navigationArrow;
    public LineRenderer lineRenderer;
    public RectTransform canvas;

    public Text distanceText;
    public Text altitudeText;
    public Text batteryText;


    float distanceToUser;
    float altitude;
    Renderer altitutudeRenderer;
    RectTransform altitudeTextTransform;

    private DroneManager droneManager;
    private bool controlledDroneFound = false;
    private Transform NavigationLineAnchor;
    private float nextUpdate;
    // Start is called before the first frame update
    public void CustomStart()
    {
        MainCamera = Camera.main.gameObject.transform;
        HUD = transform.gameObject.GetComponent<RectTransform>();
        altitutudeRenderer = altitudeIndicator.gameObject.GetComponent<Renderer>();
        altitudeTextTransform = altitudeText.gameObject.GetComponent<RectTransform>();
        NavigationLineAnchor = mirrorDrone.transform.Find("NavigationLineAnchor");

        droneManager = DroneManager.Instance;

        nextUpdate = GPSManager.droneUpdateInterval;

        // TODO select drone from menu or sthg like that
        ToggleHeadUpDisplayElements(false);
        CheckIfDroneConnected();
    }

    /// <summary>
    /// Counts the bounding box corners of the given RectTransform that are visible from the given Camera in screen space.
    /// </summary>
    /// <returns>The amount of bounding box corners that are visible from the Camera.</returns>
    /// <param name="rectTransform">Rect transform.</param>
    /// <param name="camera">Camera.</param>
    private int CountCornersVisibleFrom(RectTransform rectTransform, Camera camera)
    {
        Rect screenBounds = new Rect(0f, 0f, Screen.width, Screen.height); // Screen space bounds (assumes camera renders across the entire screen)
        Vector3[] objectCorners = new Vector3[4];
        rectTransform.GetWorldCorners(objectCorners);

        int visibleCorners = 0;
        Vector3 tempScreenSpaceCorner; // Cached
        for (var i = 0; i < objectCorners.Length; i++) // For each corner in rectTransform
        {
            tempScreenSpaceCorner = camera.WorldToScreenPoint(objectCorners[i]); // Transform world space position of corner to screen space
            if (screenBounds.Contains(tempScreenSpaceCorner)) // If the corner is inside the screen
            {
                visibleCorners++;
            }
        }
        return visibleCorners;
    }

    /// <summary>
    /// Determines if this RectTransform is fully visible from the specified camera.
    /// Works by checking if each bounding box corner of this RectTransform is inside the cameras screen space view frustrum.
    /// </summary>
    /// <returns><c>true</c> if is fully visible from the specified camera; otherwise, <c>false</c>.</returns>
    /// <param name="rectTransform">Rect transform.</param>
    /// <param name="camera">Camera.</param>
    public bool IsFullyVisibleFrom(RectTransform rectTransform, Camera camera)
    {
        return CountCornersVisibleFrom(rectTransform, camera) == 4; // True if all 4 corners are visible
    }

    /// <summary>
    /// Determines if this RectTransform is at least partially visible from the specified camera.
    /// Works by checking if any bounding box corner of this RectTransform is inside the cameras screen space view frustrum.
    /// </summary>
    /// <returns><c>true</c> if is at least partially visible from the specified camera; otherwise, <c>false</c>.</returns>
    /// <param name="rectTransform">Rect transform.</param>
    /// <param name="camera">Camera.</param>
    public bool IsVisibleFrom(RectTransform rectTransform, Camera camera)
    {
        return CountCornersVisibleFrom(rectTransform, camera) > 0; // True if any corners are visible
    }

    /// <summary>
    /// Via Raz() getting altitude of drone againt spatial mesh surface under drone.
    /// </summary>
    public float GetDroneRelativeAltitude(DroneFlightData flightData)
    {
        var latLong = new Vector2d((float)flightData.Latitude, (float)flightData.Longitude);
        return (float)flightData.Altitude - droneManager.Map.GeoToWorldPosition(latLong).y;
    }

    /// <summary>
    /// Getting position of drone in screen and setting TARGET position.
    /// </summary>
    public void SetTarget()
    {
        canvas.position = droneManager.ControlledDroneGameObject.transform.position;
        transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
        this.transform.localScale = new Vector3(distanceToUser, distanceToUser, distanceToUser);
        //Vector3 viewPos = Camera.main.WorldToScreenPoint(drone.transform.position);
        //viewPos.x = (viewPos.x - (canvas.rect.width / 2)) * 2 + (canvas.rect.width / 2);
        //viewPos.y = (viewPos.y - (canvas.rect.height / 2)) * 2 + (canvas.rect.height / 2);
        //HUD.anchoredPosition = new Vector3(viewPos.x, viewPos.y, 0.0f);
        //canvas.GetComponent<Canvas>().planeDistance = distanceToUser;
    }

    private bool CheckIfDroneConnected()
    {
        var previouslyFound = controlledDroneFound;
        controlledDroneFound = droneManager.SetControlledDrone(UserProfileManager.Instance.DroneName);

        if (controlledDroneFound && !previouslyFound)
        {
            ToggleHeadUpDisplayElements(true);
        }
        else if (!controlledDroneFound && previouslyFound)
        {
            ToggleHeadUpDisplayElements(false);
        }

        return controlledDroneFound;
    }

    private void ToggleHeadUpDisplayElements(bool active)
    {
        for (var i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(active);
        }
        navigationArrow.gameObject.SetActive(active);
        lineRenderer.gameObject.SetActive(active);
        lineRenderer.positionCount = 3;
        this.gameObject.GetComponent<Image>().enabled = active;
    }

    // Update is called once per frame
    void Update()
    {
        // don't do anything if no drone was found
        if (!controlledDroneFound && !CheckIfDroneConnected())
        {
            return;
        }

        // Distance to user has to be calculated first because it is used in SetTarget() as well
        distanceToUser = Vector3.Distance(MainCamera.position, drone.position);

        SetTarget();

        distanceText.text = Mathf.Round(distanceToUser * 100.0f) * 0.01f + "m"; //Rounding

        altitude = droneManager.ControlledDrone.RelativeAltitude;
        altitudeText.text = Mathf.Round(altitude * 100.0f) * 0.01f + "m";//Rounding

        //size of altitude bar based on altitude value
        altitudeIndicator.localScale = new Vector3(altitudeIndicator.localScale.x, altitude * AltitudeValueToTransformMultiplier, altitudeIndicator.localScale.z);
        float altititudeIndicatorPositionY = AltitudeIndicatorOffset + (altitude * AltitudeValueToTransformMultiplier) / 2;
        altitudeIndicator.localPosition = new Vector3(altitudeIndicator.localPosition.x, altititudeIndicatorPositionY, altitudeIndicator.localPosition.z);

        //set altitudeText transform next to a bar
        altitudeTextTransform.localPosition = new Vector3(altitudeTextTransform.localPosition.x, AltitudeTextOffset + (altitude * AltitudeValueToTransformMultiplier) / 2, altitudeTextTransform.localPosition.z);

        //changing color of bar based on altitude

        if (altitude < 0.5f)
        {
            altitutudeRenderer.material.SetColor("_Color", Color.red);
        }
        else if (altitude < 1.0f)
        {
            altitutudeRenderer.material.SetColor("_Color", Color.yellow);
        }
        else
        {
            altitutudeRenderer.material.SetColor("_Color", Color.green);
        }

        //TODO get battery capacity
        float batt = (600 - Time.time) / 6; //600 seconds in %
        batteryText.text = Mathf.Round(batt * 10.0f) * 0.1f + "%"; //Rounding

        //baterry incicator bars
        if (batt < 15.0f)
        {
            batteryIndicator.GetChild(0).gameObject.SetActive(false);
        }
        else if (batt < 35.0f)
        {
            batteryIndicator.GetChild(1).gameObject.SetActive(false);
        }
        else if (batt < 55.0f)
        {
            batteryIndicator.GetChild(2).gameObject.SetActive(false);
        }
        else if (batt < 75.0f)
        {
            batteryIndicator.GetChild(3).gameObject.SetActive(false);
        }
        else if (batt < 95.0f)
        {
            batteryIndicator.GetChild(4).gameObject.SetActive(false);
        }


        //visibility of TARGET, showing navigation arrow
        bool isVisible = IsVisibleFrom(HUD, Camera.main);
        //navigationArrow.gameObject.SetActive(true);
        //Vector3 targetDirection = (HUD.anchoredPosition - new Vector2((canvas.rect.width / 2), (canvas.rect.height / 2))).normalized;
        //float angleTargetDirection = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg;
        //navigationArrow.localEulerAngles = new Vector3(0, 0, angleTargetDirection);
        //navigationArrow.anchoredPosition = new Vector3((canvas.rect.width / 2), (canvas.rect.height / 2), 0.0f) + (targetDirection * NavigationArrowOffset);
        var distanceTextY = distanceText.transform.position.y;
        var cameraPosition = MainCamera.position;           
        var dronePosition = drone.position;
        var modifiedAnchor = dronePosition;
        if(Physics.Raycast(MainCamera.position, Vector3.down, out var hitInfo))
        {
            dronePosition.y = hitInfo.point.y;
            cameraPosition.y = hitInfo.point.y;
        }
        else
        {
            dronePosition.y = 0;
            cameraPosition.y = 0f;
        }

        // Anchor position needs to be modified. When drone is inclinated, anchor X,Z positions are slightly changed
        modifiedAnchor.y = NavigationLineAnchor.position.y;

        Vector3[] linePoints;

        if (distanceToUser < 2 || distanceTextY < 0.5)
        {
            lineRenderer.positionCount = 2;
            linePoints = new Vector3[] { cameraPosition, modifiedAnchor };
        }
        else
        {
            lineRenderer.positionCount = 3;
            linePoints = new Vector3[] { cameraPosition, dronePosition, modifiedAnchor };
        }
        lineRenderer.SetPositions(linePoints);

        // based on distace to user, gameObject mirror drone is active or non-active
        if (distanceToUser < 1.0f)
        {
            mirrorDrone.gameObject.SetActive(false);
        }
        else
        {
            mirrorDrone.gameObject.SetActive(true);
        }


    }
}


