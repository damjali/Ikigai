using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;

    public float minX, maxX, minY, maxY;
    public float smoothSpeed = 5f;

    private float camHeight;
    private float camWidth;

    void Start()
    {
        Camera camera = GetComponent<Camera>();
        Vector2 initialPos = transform.position;
        GetComponent<Transform>().position = initialPos;
        camHeight = camera.orthographicSize;
        camWidth = camHeight * camera.aspect;
    }

    void LateUpdate()
    {
        float x = Mathf.Clamp(
            target.position.x,
            minX + camWidth,
            maxX - camWidth
        );

        float y = Mathf.Clamp(
            target.position.y,
            minY + camHeight,
            maxY - camHeight
        );

        Vector3 targetPos = new Vector3(x, y, -10f);

        // 🔥 Smooth movement
        transform.position = Vector3.Lerp(
            transform.position,
            targetPos,
            smoothSpeed * Time.deltaTime
        );
    }
}