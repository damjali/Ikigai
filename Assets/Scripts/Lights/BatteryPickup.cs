using UnityEngine;

public class BatteryPickup : MonoBehaviour
{
    [Header("Medicine Properties")]
    public float batteryIncrease = 30f;

    // This function is automatically called when another 2D collider enters this object's trigger area.
    // Remember: This Medicine object MUST have its Collider2D "Is Trigger" checked.
    // The Player object MUST NOT have "Is Trigger" checked, and MUST have a Rigidbody2D.
    private void OnTriggerEnter2D(Collider2D other)

    {
        print("Something entered the battery");
        // 1. Check if the object that touched the medicine has the specific tag "Player1"
        if (other.CompareTag("Player1"))
        {
            print("player 1 entered battery");
            // 2. Find Player 2 in the scene using its tag
            GameObject player2 = GameObject.FindGameObjectWithTag("Player2");

            // 3. Ensure Player 2 actually exists in the scene before proceeding
            if (player2 != null)
            {
                // 4. Look for the Flashlight script component attached to Player 2
                Flashlight player2Flashlight = player2.GetComponent<Flashlight>();

                // 5. Ensure the Flashlight script was successfully found
                if (player2Flashlight != null)
                {
                    // Add battery to Player 2 by calling the public method we created earlier
                    player2Flashlight.AddBattery(batteryIncrease);
                    AudioManager.instance.PlayMedicineSound();
                    // Destroy this medicine object so it disappears from the scene after collection
                    Destroy(gameObject);
                }
                else
                {
                    // Error handling: Player 2 was found, but the Flashlight script is missing
                    Debug.LogError("Medicine Error: Found Player2, but could not find the 'Flashlight' script on it!");
                }
            }
            else
            {
                // Error handling: The game cannot find any object tagged as 'Player2'
                Debug.LogError("Medicine Error: Cannot find any object with the tag 'Player2' in the scene!");
            }
        }
    }
}