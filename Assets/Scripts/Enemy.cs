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
    public float speed = 3.2f;
    public float pathUpdateRate = 0.2f;
    public float waypointThreshold = 0.3f; 

    [Header("Behavior Settings")]
    public float lockOnDistance = 6f;
    public int targetSpread = 1;
    [Range(0f, 0.4f)] public float randomJitter = 0.05f;

    [Header("Stuck Detection")]
    public float stuckCheckInterval = 0.3f;
    public float stuckThreshold = 0.1f;
    public float recoveryDuration = 0.5f;
    private float stuckTimer;
    private Vector2 lastPosition;
    private bool isRecovering;
    private float recoveryTimer;
    private Vector2 recoveryDir;
    private int consecutiveStuckCount = 0;

    [Header("Stun Settings")]
    private bool isStunned;
    private float stunTimer;

    [Header("Audio Settings")]
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
        rb.gravityScale = 0;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        anim = GetComponent<Animator>();
    }

    void Start()
    {
        if (wallTilemap != null) CreateGridFromTilemap();
        individualSpeed = speed + Random.Range(-0.1f, 0.1f);
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

        if (isStunned)
        {
            stunTimer -= Time.deltaTime;
            rb.linearVelocity = Vector2.zero;
            currentMovement = Vector2.zero;
            UpdateAnimations();
            if (stunTimer <= 0) isStunned = false;
            return;
        }

        HandleHeartbeat();
        
        if (isRecovering)
        {
            PerformRecovery();
        }
        else
        {
            HandleStuckDetection();
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

    public void Stun(float duration)
    {
        isStunned = true;
        stunTimer = duration;
        isRecovering = false;
        currentPath.Clear();
    }

    void HandleHeartbeat()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer <= heartbeatDistance)
        {
            heartbeatTimer -= Time.deltaTime;
            if (heartbeatTimer <= 0)
            {
                if (AudioManager.instance != null) AudioManager.instance.PlayHeartbeat();
                heartbeatTimer = 3f; 
            }
        }
        else heartbeatTimer = 0f;
    }

    void HandleStuckDetection()
    {
        stuckTimer += Time.deltaTime;
        if (stuckTimer >= stuckCheckInterval)
        {
            float distMoved = Vector2.Distance(transform.position, lastPosition);
            if (currentMovement.sqrMagnitude > 0.05f && distMoved < stuckThreshold)
            {
                consecutiveStuckCount++;
                StartRecovery();
            }
            else
            {
                consecutiveStuckCount = 0;
            }
            lastPosition = transform.position;
            stuckTimer = 0;
        }
    }

    void StartRecovery()
    {
        isRecovering = true;
        recoveryTimer = recoveryDuration;

        if (consecutiveStuckCount > 1)
        {
            recoveryDir = -currentMovement.normalized;
            recoveryDir = Quaternion.Euler(0, 0, Random.Range(-45f, 45f)) * recoveryDir;
            recoveryTimer *= 1.5f;
        }
        else
        {
            Vector2[] options = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
            recoveryDir = options[Random.Range(0, options.Length)];
            Vector2Int gPos = WorldToGrid(transform.position);
            foreach(var opt in options)
            {
                Vector2Int n = gPos + new Vector2Int(Mathf.RoundToInt(opt.x), Mathf.RoundToInt(opt.y));
                if (IsValidGridPos(n) && grid[n.x, n.y]) { recoveryDir = opt; break; }
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
            timer = 0;
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

    bool HasLineOfSight(Vector2 targetPos)
    {
        Vector2 start = transform.position;
        float dist = Vector2.Distance(start, targetPos);
        int steps = Mathf.CeilToInt(dist / 0.2f);
        for (int i = 1; i <= steps; i++)
        {
            Vector2 point = Vector2.Lerp(start, targetPos, (float)i / steps);
            if (!IsPointWalkable(point, 0.25f)) return false;
        }
        return true;
    }

    bool IsPointWalkable(Vector2 worldPos, float radius)
    {
        Vector2[] offsets = { Vector2.zero, Vector2.up * radius, Vector2.down * radius, Vector2.left * radius, Vector2.right * radius };
        foreach(var offset in offsets)
        {
            Vector2Int gPos = WorldToGrid(worldPos + offset);
            if (!IsValidGridPos(gPos) || !grid[gPos.x, gPos.y]) return false;
        }
        return true;
    }

    void GeneratePath()
    {
        Vector2Int start = WorldToGrid(transform.position);
        if (!grid[start.x, start.y]) start = FindNearestWalkable(start);

        Vector2Int playerGridPos = WorldToGrid(player.position);
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        Vector2Int finalTarget = (distanceToPlayer <= lockOnDistance || HasLineOfSight(player.position))
            ? playerGridPos
            : GetRandomizedTarget(playerGridPos);

        if (start == finalTarget)
        {
            currentPath.Clear();
            currentMovement = Vector2.zero;
            rb.linearVelocity = Vector2.zero;
            return;
        }

        PriorityQueue<Node> openSet = new PriorityQueue<Node>();
        Dictionary<Vector2Int, Node> allNodes = new Dictionary<Vector2Int, Node>();

        Node startNode = new Node(start, 0, GetHeuristic(start, finalTarget), null);
        openSet.Enqueue(startNode);
        allNodes[start] = startNode;

        Node endNode = null;
        int iter = 0;

        while (openSet.Count > 0 && iter++ < 1500)
        {
            Node curr = openSet.Dequeue();
            if (curr.pos == finalTarget) { endNode = curr; break; }

            foreach (Vector2Int nPos in GetNeighbors(curr.pos))
            {
                if (!grid[nPos.x, nPos.y]) continue;
                float newG = curr.g + 1;
                if (!allNodes.ContainsKey(nPos) || newG < allNodes[nPos].g)
                {
                    Node nNode = new Node(nPos, newG, GetHeuristic(nPos, finalTarget), curr);
                    allNodes[nPos] = nNode;
                    openSet.Enqueue(nNode);
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
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    Vector2Int FindNearestWalkable(Vector2Int start)
    {
        Queue<Vector2Int> q = new Queue<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        q.Enqueue(start);
        visited.Add(start);

        int maxSearch = 25; 
        while (q.Count > 0 && maxSearch-- > 0)
        {
            Vector2Int curr = q.Dequeue();
            if (IsValidGridPos(curr) && grid[curr.x, curr.y]) return curr;

            foreach (Vector2Int n in GetNeighbors(curr))
            {
                if (!visited.Contains(n)) { visited.Add(n); q.Enqueue(n); }
            }
        }
        return start;
    }

    void FollowPath()
    {
        if (currentPath == null || currentPath.Count == 0 || pathIndex >= currentPath.Count)
        {
            if (Vector2.Distance(transform.position, player.position) > 0.5f && HasLineOfSight(player.position))
            {
                Vector2 dir = (player.position - transform.position).normalized;
                currentMovement = dir;
                rb.linearVelocity = dir * individualSpeed;
            }
            else
            {
                currentMovement = Vector2.zero;
                rb.linearVelocity = Vector2.zero;
            }
            return;
        }

        while (pathIndex + 1 < currentPath.Count)
        {
            Vector3 nextPos = GridToWorld(currentPath[pathIndex + 1]);
            if (HasLineOfSight(nextPos)) { pathIndex++; UpdateJitteredTarget(); }
            else break;
        }

        Vector3 targetDir = (currentTargetWithJitter - transform.position).normalized;
        currentMovement = targetDir;
        rb.linearVelocity = currentMovement * individualSpeed;

        if (Vector2.Distance(transform.position, currentTargetWithJitter) < waypointThreshold)
        {
            pathIndex++;
            if (pathIndex < currentPath.Count) UpdateJitteredTarget();
        }
    }

    void UpdateJitteredTarget()
    {
        if (currentPath.Count == 0 || pathIndex >= currentPath.Count) return;
        Vector3 raw = GridToWorld(currentPath[pathIndex]);
        float currentDist = Vector2.Distance(transform.position, player.position);
        float jitter = (currentDist > lockOnDistance) ? randomJitter : 0.02f;
        currentTargetWithJitter = raw + new Vector3(Random.Range(-jitter, jitter), Random.Range(-jitter, jitter), 0);
        
        if (!IsPointWalkable(currentTargetWithJitter, 0.2f)) currentTargetWithJitter = raw;
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
        yield return current + Vector2Int.up;
        yield return current + Vector2Int.down;
        yield return current + Vector2Int.left;
        yield return current + Vector2Int.right;
    }

    bool IsValidGridPos(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < grid.GetLength(0) && pos.y >= 0 && pos.y < grid.GetLength(1);
    }

    Vector2Int GetRandomizedTarget(Vector2Int baseTarget)
    {
        Vector2Int pot = baseTarget + new Vector2Int(Random.Range(-targetSpread, targetSpread + 1), Random.Range(-targetSpread, targetSpread + 1));
        if (IsValidGridPos(pot) && grid[pot.x, pot.y]) return pot;
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
        if (rb != null) { rb.linearVelocity = Vector2.zero; rb.position = worldSpawn; }
        transform.position = worldSpawn;
        isRecovering = false;
        consecutiveStuckCount = 0;
        isStunned = false;
        clearPath();
    }

    private void clearPath() { currentPath.Clear(); pathIndex = 0; timer = 0; }

    private class Node {
        public Vector2Int pos; public float g, h; public float f => g + h; public Node parent;
        public Node(Vector2Int p, float gScore, float hScore, Node prnt) { pos = p; g = gScore; h = hScore; parent = prnt; }
    }

    private class PriorityQueue<T> where T : Node {
        private List<T> data = new List<T>();
        public int Count => data.Count;
        public void Enqueue(T item) {
            data.Add(item); int ci = data.Count - 1;
            while (ci > 0) {
                int pi = (ci - 1) / 2; if (data[ci].f >= data[pi].f) break;
                T tmp = data[ci]; data[ci] = data[pi]; data[pi] = tmp; ci = pi;
            }
        }
        public T Dequeue() {
            int li = data.Count - 1; T frontItem = data[0]; data[0] = data[li]; data.RemoveAt(li); --li; int pi = 0;
            while (true) {
                int ci = pi * 2 + 1; if (ci > li) break;
                int rc = ci + 1; if (rc <= li && data[rc].f < data[ci].f) ci = rc;
                if (data[pi].f <= data[ci].f) break;
                T tmp = data[pi]; data[pi] = data[ci]; data[ci] = tmp; pi = ci;
            }
            return frontItem;
        }
    }
}