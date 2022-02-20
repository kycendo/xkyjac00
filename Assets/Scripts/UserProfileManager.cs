using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UserProfileManager : Singleton<UserProfileManager>
{
    public float Height;
    public string DroneName;
    public string Username;
    public decimal DroneThreshold;
    public TextMeshPro TresholdSetting;

    private readonly decimal Increasement = 0.1m;
    // Start is called before the first frame update
    void Start()
    {
        TresholdSetting.text = "0";
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void IncreaseTreshold()
    {
        DroneThreshold += Increasement;
        SetThresholdText();
    }

    public void DecreaseTreshold()
    {
        if (DroneThreshold >= Increasement)
        {
            DroneThreshold -= Increasement;
            SetThresholdText();
        }    
    }

    public void ToggleDroneModel()
    {
        var droneModel = DroneManager.Instance.ControlledDroneGameObject;
        droneModel.SetActive(!droneModel.activeSelf);
    }

    private void SetThresholdText()
    {
        TresholdSetting.text = string.Format("{0:0.0}", DroneThreshold);
    }
}
