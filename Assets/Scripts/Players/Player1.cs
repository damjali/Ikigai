using UnityEngine;

namespace Players
{
    public class Player1 : Player
    {
        // Start is used for initialization
        void Start()
        {
            // Ensure abilities are active
            haveDown = true;
            haveLeft = true;
            haveRight = true;
            haveUp = true;
        }

        public override void Update()
        {
            // 1. Reset movement vector in base class
            base.Update();
            if (win)
            {
                return;
            }

            // 2. Capture Keyboard Input (WASD)
            if (Input.GetKey(KeyCode.A) && haveLeft)
                movement.x = -1;
            else if (Input.GetKey(KeyCode.D) && haveRight)
                movement.x = 1;

            if (Input.GetKey(KeyCode.W) && haveUp)
                movement.y = 1;
            else if (Input.GetKey(KeyCode.S) && haveDown)
                movement.y = -1;

            // 3. Update the animator parameters
            UpdateAnimations();
        }
    }
}