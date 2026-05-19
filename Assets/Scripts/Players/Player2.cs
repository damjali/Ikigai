using UnityEngine;

namespace Players
{
    public class Player2 : Player
    {
        public bool scared { get; set; } = false;

        public void Start()
        {
            haveDown = false;
            haveLeft = false;
            haveRight = false;
            haveUp = false;
        }
        
        public override void Update()
        {
            base.Update();
            
            if (scared || win)
            {
                return;
            }
            
            if (Input.GetKey(KeyCode.LeftArrow) && haveLeft)
                movement.x = -1;
            if (Input.GetKey(KeyCode.RightArrow) && haveRight)
                movement.x = 1;
            if (Input.GetKey(KeyCode.UpArrow) && haveUp)
                movement.y = 1;
            if (Input.GetKey(KeyCode.DownArrow) && haveDown)
                movement.y = -1;

            UpdateAnimations();
        }

        public void scare()
        {
            // Only play the sound if they weren't ALREADY scared
            if (!scared) 
            {
                scared = true;
                rb.linearVelocity = Vector2.zero; // Stop movement immediately
                
                // Trigger the audio!
                if(AudioManager.instance != null) {
                    AudioManager.instance.PlayScaredSound();
                }
            }
        }
    }
}