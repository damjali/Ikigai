using UnityEngine;

public class BatteryPickup : MonoBehaviour
{
    [Header("Medicine Properties")]
    public float batteryIncrease = 30f;
    public float stunDuration = 2f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1. Check if the object that touched the medicine has the specific tag "Player1"
        if (other.CompareTag("Player1"))
        {
            print("player 1 collected medicine - Stunning enemies");

            // --- NEW LOGIC: Stun Enemy and Handle Medicine Count ---
            LevelManager levelManager = Object.FindFirstObjectByType<LevelManager>();
            if (levelManager != null)
            {
                levelManager.StunEnemies(stunDuration);
                // levelManager.MedicineCollected();
            }

            // 2. Find Player 2 in the scene to increase battery
            GameObject player2 = GameObject.FindGameObjectWithTag("Player2");

            if (player2 != null)
            {
                Flashlight player2Flashlight = player2.GetComponent<Flashlight>();
                if (player2Flashlight != null)
                {
                    player2Flashlight.AddBattery(batteryIncrease);
                    AudioManager.instance.PlayMedicineSound();
                    Destroy(gameObject);
                }
                else
                {
                    Debug.LogError("Medicine Error: Could not find Flashlight script on Player2!");
                }
            }
            else
            {
                Debug.LogError("Medicine Error: Cannot find Player2!");
                // Even if player2 is missing, still destroy and count the medicine
                Destroy(gameObject);
            }
        }
    }
}