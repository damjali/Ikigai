using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class Enemy : MonoBehaviour
{
    [Header("References")]
    public Tilemap wallTilemap;
    public Transform player;
    private Animator anim;
    private Rigidbody2D rb;

    [Header("Movement")]
    public float speed = 3f;
    public float pathUpdateRate = 0.3f;

    [Header("Behavior Settings")]
    public float lockOnDistance = 3f;
    public int targetSpread = 2;
    [Range(0f, 0.4f)] public float randomJitter = 0.25f;

    [Header("Intelligence Settings")]
    [Range(0f, 1f)]
    public float curiosity = 0.3f;

    [Header("Audio Settings (The Heartbeat)")]
    public float heartbeatDistance = 8f; // How close before the heartbeat starts
    public float maxHeartbeatRate = 1.2f; // Slow beat (when enemy is at the edge of the distance)
    public float minHeartbeatRate = 0.3f; // Fast, frantic beat (when enemy is right next to you)
    private float heartbeatTimer;

    [Header("Spawn Settings")]
    private float originX;
    private float originY;
    private bool originSaved = false;

    // Pathfinding & Movement State
    private Vector3 currentTargetWithJitter;
    private Vector2 currentMovement;
    private bool[,] grid;
    private Vector2Int gridOffset;
    private List<Vector2Int> currentPath = new List<Vector2Int>();
    private int pathIndex;
    private float timer;
    private float individualSpeed;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void Start()
    {
        if (wallTilemap != null) CreateGridFromTilemap();

        individualSpeed = speed + Random.Range(-0.5f, 0.5f);

        // Save origin position if not already set
        if (!originSaved)
        {
            Vector2Int startGrid = WorldToGrid(transform.position);
            originX = startGrid.x;
            originY = startGrid.y;
            originSaved = true;
        }
    }

    void Update()
    {
        if (player == null || grid == null) return;

        // --- THE TENSION BUILDER (HEARTBEAT LOGIC) ---
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        if (distanceToPlayer <= heartbeatDistance)
        {
            heartbeatTimer -= Time.deltaTime;
            if (heartbeatTimer <= 0)
            {
                // Play the sound!
                AudioManager.instance.PlayHeartbeat();
                
                // Calculate how close the monster is (0 = touching you, 1 = far away)
                float distanceRatio = distanceToPlayer / heartbeatDistance;
                
                // Make the timer shorter (faster heartbeat) as the monster gets closer!
                heartbeatTimer = Mathf.Lerp(minHeartbeatRate, maxHeartbeatRate, distanceRatio);
            }
        }
        // ---------------------------------------------

        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            GeneratePath();
            timer = pathUpdateRate + Random.Range(-0.05f, 0.05f);
        }

        FollowPath();
        UpdateAnimations();
    }

    void UpdateAnimations()
    {
        if (anim != null)
        {
            // Is the object moving based on our calculated movement vector?
            bool isMoving = currentMovement.sqrMagnitude > 0;
            anim.SetBool("IsMoving", isMoving);

            // Only update direction when moving to keep idle direction
            if (isMoving)
            {
                anim.SetFloat("MoveX", currentMovement.x);
                anim.SetFloat("MoveY", currentMovement.y);
            }
        }
    }

    void GeneratePath()
    {
        Vector2Int start = WorldToGrid(transform.position);
        Vector2Int playerGridPos = WorldToGrid(player.position);
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        Vector2Int finalTarget = (distanceToPlayer <= lockOnDistance)
            ? playerGridPos
            : GetRandomizedTarget(playerGridPos);

        if (start == finalTarget)
        {
            currentPath.Clear();
            currentMovement = Vector2.zero;
            return;
        }

        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        queue.Enqueue(start);
        Dictionary<Vector2Int, Vector2Int> parentMap = new Dictionary<Vector2Int, Vector2Int>();
        parentMap[start] = start;

        while (queue.Count > 0)
        {
            Vector2Int curr = queue.Dequeue();
            if (curr == finalTarget) break;

            foreach (Vector2Int neighbor in GetSmartNeighbors(curr, finalTarget))
            {
                if (grid[neighbor.x, neighbor.y] && !parentMap.ContainsKey(neighbor))
                {
                    queue.Enqueue(neighbor);
                    parentMap[neighbor] = curr;
                }
            }
        }

        if (!parentMap.ContainsKey(finalTarget)) return;

        currentPath.Clear();
        Vector2Int temp = finalTarget;

        while (temp != start)
        {
            currentPath.Add(temp);
            temp = parentMap[temp];
        }
        currentPath.Reverse();

        pathIndex = 0;
        if (currentPath.Count > 0) UpdateJitteredTarget();
    }

    void FollowPath()
    {
        if (currentPath == null || currentPath.Count == 0 || pathIndex >= currentPath.Count)
        {
            currentMovement = Vector2.zero;
            return;
        }

        // Calculate Movement Vector for Animation
        Vector3 directionToTarget = (currentTargetWithJitter - transform.position).normalized;
        currentMovement = new Vector2(directionToTarget.x, directionToTarget.y);

        transform.position = Vector3.MoveTowards(transform.position, currentTargetWithJitter, individualSpeed * Time.deltaTime);

        if (Vector2.Distance(transform.position, currentTargetWithJitter) < 0.15f)
        {
            pathIndex++;
            if (pathIndex < currentPath.Count) UpdateJitteredTarget();
        }
    }

    void UpdateJitteredTarget()
    {
        if (currentPath.Count == 0 || pathIndex >= currentPath.Count) return;

        Vector3 rawCenter = GridToWorld(currentPath[pathIndex]);
        float currentDist = Vector2.Distance(transform.position, player.position);
        float jitter = (currentDist > lockOnDistance) ? randomJitter : 0.05f;
        currentTargetWithJitter = rawCenter + new Vector3(Random.Range(-jitter, jitter), Random.Range(-jitter, jitter), 0);
    }

    // --- Helper Methods ---

    void CreateGridFromTilemap()
    {
        BoundsInt bounds = wallTilemap.cellBounds;
        grid = new bool[bounds.size.x, bounds.size.y];
        gridOffset = new Vector2Int(bounds.xMin, bounds.yMin);

        for (int x = 0; x < bounds.size.x; x++)
        {
            for (int y = 0; y < bounds.size.y; y++)
            {
                Vector3Int localAddr = new Vector3Int(x + gridOffset.x, y + gridOffset.y, 0);
                grid[x, y] = !wallTilemap.HasTile(localAddr);
            }
        }
    }

    IEnumerable<Vector2Int> GetSmartNeighbors(Vector2Int current, Vector2Int target)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>
        {
            current + Vector2Int.up, current + Vector2Int.down,
            current + Vector2Int.left, current + Vector2Int.right
        };

        if (Random.value < curiosity)
        {
            // Shuffle
            for (int i = neighbors.Count - 1; i > 0; i--)
            {
                int r = Random.Range(0, i + 1);
                var t = neighbors[i]; neighbors[i] = neighbors[r]; neighbors[r] = t;
            }
        }
        else
        {
            neighbors = neighbors.OrderBy(n => Vector2Int.Distance(n, target)).ToList();
        }

        foreach (var n in neighbors)
        {
            if (n.x >= 0 && n.x < grid.GetLength(0) && n.y >= 0 && n.y < grid.GetLength(1))
                yield return n;
        }
    }

    Vector2Int GetRandomizedTarget(Vector2Int baseTarget)
    {
        Vector2Int pot = baseTarget + new Vector2Int(Random.Range(-targetSpread, targetSpread + 1), Random.Range(-targetSpread, targetSpread + 1));
        if (pot.x >= 0 && pot.x < grid.GetLength(0) && pot.y >= 0 && pot.y < grid.GetLength(1) && grid[pot.x, pot.y])
            return pot;
        return baseTarget;
    }

    Vector2Int WorldToGrid(Vector3 worldPos)
    {
        Vector3Int cell = wallTilemap.WorldToCell(worldPos);
        return new Vector2Int(Mathf.Clamp(cell.x - gridOffset.x, 0, grid.GetLength(0) - 1), Mathf.Clamp(cell.y - gridOffset.y, 0, grid.GetLength(1) - 1));
    }

    Vector3 GridToWorld(Vector2Int gridPos)
    {
        Vector3Int cell = new Vector3Int(gridPos.x + gridOffset.x, gridPos.y + gridOffset.y, 0);
        return wallTilemap.GetCellCenterWorld(cell);
    }

    public void reset()
    {
        Vector2 worldSpawn = new Vector2(originX, originY);
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.position = worldSpawn;
        }
        transform.position = worldSpawn;
        clearPath();
    }

    private void clearPath()
    {
        currentPath.Clear();
        pathIndex = 0;
        timer = 0;
    }
}