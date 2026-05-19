using System.Collections;
using Players;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class BlackoutScare : MonoBehaviour
{ // Reference to your Flashlight script to check battery
    private Flashlight playerSpotlight;    // The new spotlight on the player
    private Player2 p2;
    public LevelManager levelManager { get; set; }

    [Header("Core Scripts & Lights")]
    public Light2D globalLight;            // The map's main light

    public bool triggered { get; set;}// To make sure this only happens ONCE

    void Awake()
    {   
        playerSpotlight = GetComponent<Flashlight>();
        globalLight.intensity = 0f;
        p2 = GetComponentInParent<Player2>();
    }

    void Update()
    {
        if (triggered)
        {
            ExecuteScareSequence();
        }
    }

    private void ExecuteScareSequence()
    {
        p2.scare();
        levelManager.blinkMonster();
    }

    public void StopScareSequence()
    {
        p2.scared = false;
        levelManager.stopBlink();
    }
}