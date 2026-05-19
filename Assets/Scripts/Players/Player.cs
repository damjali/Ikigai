using UnityEngine;

namespace Players
{
    public class Player : MonoBehaviour
    {
        protected bool win = false;
        [Header("Movement Settings")]
        public float moveSpeed = 5f;

        [Header("Abilities")]
        // Changed to public fields so they appear in the Inspector for easy debugging
        public bool haveUp = true;
        public bool haveDown = true;
        public bool haveLeft = true;
        public bool haveRight = true;

        protected Rigidbody2D rb;
        protected Animator anim; 
        protected Vector2 movement;

        private float originX;
        private float originY;
        private bool originSaved = false;

        protected virtual void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            anim = GetComponent<Animator>();
        }

        public virtual void Update()
        {
            
            if (!originSaved)
            {
                originX = transform.position.x;
                originY = transform.position.y;
                originSaved = true;
            }

            // Reset movement vector every frame
            movement = Vector2.zero;
        }

        protected virtual void FixedUpdate()
        {
            // Move the Rigidbody
            if (movement.sqrMagnitude > 0)
            {
                rb.MovePosition(rb.position + movement.normalized * moveSpeed * Time.fixedDeltaTime);
            }
        }

        protected void UpdateAnimations()
        {
            if (anim != null)
            {
                bool isMoving = movement.sqrMagnitude > 0;
                anim.SetBool("IsMoving", isMoving);

                // IMPORTANT: Only update MoveX and MoveY if we are actually moving
                // This allows the Blend Tree to stay in the last faced direction when idle
                if (isMoving)
                {
                    anim.SetFloat("MoveX", movement.x);
                    anim.SetFloat("MoveY", movement.y);
                }
            }
        }

        public void reset()
        {
            rb.position = new Vector2(originX, originY);
            
        }
        
        public void won()
        {
            win = true;
            GetComponent<SpriteRenderer>().color = Color.clear;
            GetComponent<Collider2D>().enabled = false;
        }
    }
}