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
    public float pathUpdateRate = 0.1f;
    public float rotationSpeed = 10f;

    [Header("Behavior Settings")]
    public float lockOnDistance = 5f;
    public int targetSpread = 1;
    [Range(0f, 0.4f)] public float randomJitter = 0.1f;

    [Header("Intelligence Settings")]
    [Range(0f, 1f)]
    public float curiosity = 0.1f; // Reduced for "smarter" behavior

    [Header("Stuck Detection")]
    public float stuckCheckInterval = 0.5f;
    public float stuckThreshold = 0.05f;
    public float recoveryDuration = 0.8f;
    private float stuckTimer;
    private Vector2 lastPosition;
    private bool isRecovering;
    private float recoveryTimer;
    private Vector2 recoveryDir;

    [Header("Audio Settings (The Heartbeat)")]
    public float heartbeatDistance = 8f; 
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

        individualSpeed = speed + Random.Range(-0.2f, 0.2f);
        lastPosition = transform.position;

        if (!originSaved)
        {
            originX = transform.position.x;
            originY = transform.position.y;
            originSaved = true;
        }
    }

    void Update()
    {
        if (player == null || grid == null) return;

        HandleHeartbeat();
        HandleStuckDetection();

        if (isRecovering)
        {
            PerformRecovery();
        }
        else
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                GeneratePath();
                timer = pathUpdateRate;
            }
            FollowPath();
        }

        UpdateAnimations();
    }

    void HandleHeartbeat()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        if (distanceToPlayer <= heartbeatDistance)
        {
            heartbeatTimer -= Time.deltaTime;
            if (heartbeatTimer <= 0)
            {
                if (AudioManager.instance != null)
                    AudioManager.instance.PlayHeartbeat();
                heartbeatTimer = 3f; 
            }
        }
        else
        {
            heartbeatTimer = 0f;
        }
    }

    void HandleStuckDetection()
    {
        if (isRecovering) return;

        stuckTimer += Time.deltaTime;
        if (stuckTimer >= stuckCheckInterval)
        {
            float distMoved = Vector2.Distance(transform.position, lastPosition);
            if (distMoved < stuckThreshold && currentMovement.sqrMagnitude > 0.1f)
            {
                StartRecovery();
            }
            lastPosition = transform.position;
            stuckTimer = 0;
        }
    }

    void StartRecovery()
    {
        isRecovering = true;
        recoveryTimer = recoveryDuration;
        
        // Try to move in a direction that isn't blocked
        Vector2[] dirs = { Vector2.up, Vector2.down, Vector2.left, Vector2.right, 
                           (Vector2.up + Vector2.right).normalized, (Vector2.up + Vector2.left).normalized,
                           (Vector2.down + Vector2.right).normalized, (Vector2.down + Vector2.left).normalized };
        
        // Pick a random direction first, then refine if needed
        recoveryDir = dirs[Random.Range(0, dirs.Length)];
        
        // Find a walkable neighbor to move towards
        Vector2Int currentGrid = WorldToGrid(transform.position);
        foreach(var dir in dirs)
        {
            Vector2Int neighbor = currentGrid + new Vector2Int(Mathf.RoundToInt(dir.x), Mathf.RoundToInt(dir.y));
            if (IsValidGridPos(neighbor) && grid[neighbor.x, neighbor.y])
            {
                recoveryDir = dir;
                break;
            }
        }
        
        currentPath.Clear();
    }

    void PerformRecovery()
    {
        recoveryTimer -= Time.deltaTime;
        rb.linearVelocity = recoveryDir * individualSpeed;
        currentMovement = recoveryDir;

        if (recoveryTimer <= 0)
        {
            isRecovering = false;
            GeneratePath();
        }
    }

    void UpdateAnimations()
    {
        if (anim != null)
        {
            bool isMoving = rb.linearVelocity.sqrMagnitude > 0.01f;
            anim.SetBool("IsMoving", isMoving);

            if (isMoving)
            {
                anim.SetFloat("MoveX", currentMovement.x);
                anim.SetFloat("MoveY", currentMovement.y);
            }
        }
    }

    bool HasLineOfSight()
    {
        Vector2 start = transform.position;
        Vector2 end = player.position;
        float dist = Vector2.Distance(start, end);
        
        int steps = Mathf.CeilToInt(dist * 3f);
        for (int i = 1; i <= steps; i++)
        {
            Vector2 point = Vector2.Lerp(start, end, (float)i / steps);
            Vector2Int gPos = WorldToGrid(point);
            if (!grid[gPos.x, gPos.y]) return false;
        }
        return true;
    }

    void GeneratePath()
    {
        Vector2Int start = WorldToGrid(transform.position);
        Vector2Int playerGridPos = WorldToGrid(player.position);
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // If we have LoS, we don't need a complex path, just go to the player's grid pos
        Vector2Int finalTarget = (distanceToPlayer <= lockOnDistance || HasLineOfSight())
            ? playerGridPos
            : GetRandomizedTarget(playerGridPos);

        if (start == finalTarget)
        {
            currentPath.Clear();
            currentMovement = Vector2.zero;
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // --- A* Pathfinding ---
        PriorityQueue<Node> openSet = new PriorityQueue<Node>();
        Dictionary<Vector2Int, Node> allNodes = new Dictionary<Vector2Int, Node>();

        Node startNode = new Node(start, 0, GetHeuristic(start, finalTarget), null);
        openSet.Enqueue(startNode);
        allNodes[start] = startNode;

        Node endNode = null;

        while (openSet.Count > 0)
        {
            Node curr = openSet.Dequeue();

            if (curr.pos == finalTarget)
            {
                endNode = curr;
                break;
            }

            foreach (Vector2Int neighborPos in GetNeighbors(curr.pos))
            {
                if (!grid[neighborPos.x, neighborPos.y]) continue;

                float newG = curr.g + 1; // Basic grid distance
                if (!allNodes.ContainsKey(neighborPos) || newG < allNodes[neighborPos].g)
                {
                    Node neighborNode = new Node(neighborPos, newG, GetHeuristic(neighborPos, finalTarget), curr);
                    allNodes[neighborPos] = neighborNode;
                    openSet.Enqueue(neighborNode);
                }
            }
        }

        if (endNode == null) return;

        currentPath.Clear();
        Node temp = endNode;
        while (temp != null && temp.pos != start)
        {
            currentPath.Add(temp.pos);
            temp = temp.parent;
        }
        currentPath.Reverse();

        pathIndex = 0;
        if (currentPath.Count > 0) UpdateJitteredTarget();
    }

    float GetHeuristic(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y); // Manhattan distance
    }

    void FollowPath()
    {
        if (currentPath == null || currentPath.Count == 0 || pathIndex >= currentPath.Count)
        {
            if (Vector2.Distance(transform.position, player.position) > 0.5f)
            {
                // If path is empty but we aren't at player, try moving directly if LoS
                if (HasLineOfSight())
                {
                    Vector2 dir = (player.position - transform.position).normalized;
                    currentMovement = dir;
                    rb.linearVelocity = dir * individualSpeed;
                }
            }
            else
            {
                currentMovement = Vector2.zero;
                rb.linearVelocity = Vector2.zero;
            }
            return;
        }

        // Smooth pathing: Look ahead to see if we can skip nodes
        while (pathIndex + 1 < currentPath.Count)
        {
            Vector3 nextNodeWorld = GridToWorld(currentPath[pathIndex + 1]);
            if (IsPointWalkable(nextNodeWorld))
            {
                pathIndex++;
                UpdateJitteredTarget();
            }
            else break;
        }

        Vector3 directionToTarget = (currentTargetWithJitter - transform.position).normalized;
        currentMovement = new Vector2(directionToTarget.x, directionToTarget.y);
        rb.linearVelocity = currentMovement * individualSpeed;

        if (Vector2.Distance(transform.position, currentTargetWithJitter) < 0.2f)
        {
            pathIndex++;
            if (pathIndex < currentPath.Count) UpdateJitteredTarget();
        }
    }

    bool IsPointWalkable(Vector3 worldPos)
    {
        Vector2Int gPos = WorldToGrid(worldPos);
        return IsValidGridPos(gPos) && grid[gPos.x, gPos.y];
    }

    void UpdateJitteredTarget()
    {
        if (currentPath.Count == 0 || pathIndex >= currentPath.Count) return;

        Vector3 rawCenter = GridToWorld(currentPath[pathIndex]);
        float currentDist = Vector2.Distance(transform.position, player.position);
        
        // Less jitter when close or chasing
        float jitter = (currentDist > lockOnDistance && !HasLineOfSight()) ? randomJitter : 0.02f;
        currentTargetWithJitter = rawCenter + new Vector3(Random.Range(-jitter, jitter), Random.Range(-jitter, jitter), 0);
    }

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

    IEnumerable<Vector2Int> GetNeighbors(Vector2Int current)
    {
        Vector2Int[] neighbors = {
            current + Vector2Int.up, current + Vector2Int.down,
            current + Vector2Int.left, current + Vector2Int.right
        };

        foreach (var n in neighbors)
        {
            if (IsValidGridPos(n))
                yield return n;
        }
    }

    bool IsValidGridPos(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < grid.GetLength(0) && pos.y >= 0 && pos.y < grid.GetLength(1);
    }

    Vector2Int GetRandomizedTarget(Vector2Int baseTarget)
    {
        if (Random.value < curiosity)
        {
            Vector2Int pot = baseTarget + new Vector2Int(Random.Range(-targetSpread, targetSpread + 1), Random.Range(-targetSpread, targetSpread + 1));
            if (IsValidGridPos(pot) && grid[pot.x, pot.y])
                return pot;
        }
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
        isRecovering = false;
        clearPath();
    }

    private void clearPath()
    {
        currentPath.Clear();
        pathIndex = 0;
        timer = 0;
    }

    // --- Helper Classes for A* ---
    private class Node
    {
        public Vector2Int pos;
        public float g;
        public float h;
        public float f => g + h;
        public Node parent;
        public Node(Vector2Int p, float gScore, float hScore, Node prnt)
        {
            pos = p; g = gScore; h = hScore; parent = prnt;
        }
    }

    private class PriorityQueue<T> where T : Node
    {
        private List<T> data = new List<T>();
        public int Count => data.Count;
        public void Enqueue(T item)
        {
            data.Add(item);
            int ci = data.Count - 1;
            while (ci > 0)
            {
                int pi = (ci - 1) / 2;
                if (data[ci].f >= data[pi].f) break;
                T tmp = data[ci]; data[ci] = data[pi]; data[pi] = tmp;
                ci = pi;
            }
        }
        public T Dequeue()
        {
            int li = data.Count - 1;
            T frontItem = data[0];
            data[0] = data[li];
            data.RemoveAt(li);
            --li;
            int pi = 0;
            while (true)
            {
                int ci = pi * 2 + 1;
                if (ci > li) break;
                int rc = ci + 1;
                if (rc <= li && data[rc].f < data[ci].f) ci = rc;
                if (data[pi].f <= data[ci].f) break;
                T tmp = data[pi]; data[pi] = data[ci]; data[ci] = tmp;
                pi = ci;
            }
            return frontItem;
        }
    }
}