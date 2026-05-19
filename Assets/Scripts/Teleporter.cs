
using UnityEngine;

public class Teleporter : MonoBehaviour
{
    public Transform p2;
    public Camera cam2;
    public Camera cam1;
    public Transform target;
    public LevelManager levelManager;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player2"))
        {
            
            Vector2 targetPosition = new Vector2(target.position.x, target.position.y); // Example target position
            CameraFollow cameraFollow1 = cam1.GetComponent<CameraFollow>();
            CameraFollow cameraFollow2 = cam2.GetComponent<CameraFollow>();
            cameraFollow2.maxX = cameraFollow1.maxX;
            cameraFollow2.maxY = cameraFollow1.maxY;
            cameraFollow2.minX = cameraFollow1.minX;
            cameraFollow2.minY = cameraFollow1.minY;
            cam2.GetComponent<Transform>().position = targetPosition;
            p2.position = targetPosition;
            levelManager.spawnExit();
        }
    }
}
