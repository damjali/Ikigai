using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;
using Players;

public class Bear : MonoBehaviour
{
    public LevelManager levelManager;
    
    [Header("References")]
    public Tilemap wallTilemap;
    public Transform exit;
    private Transform target;

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

    [Header("Spawn Settings")]
    private float originX;
    private float originY;

    private Vector3 currentTargetWithJitter;
    private bool[,] grid;
    private Vector2Int gridOffset;
    private List<Vector2Int> currentPath = new List<Vector2Int>();
    private int pathIndex;
    private float timer;
    private float individualSpeed;
    private Rigidbody2D rb;
    private bool originSaved = false;
    private bool move = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    
    void Start()
    {
        int r1 = Random.Range(0, 2);
        
        if (r1 == 0 && levelManager != null && levelManager.enemies.Count > 0)
        {
            int randomIndex = Random.Range(0, levelManager.enemies.Count);
            target = levelManager.enemies[randomIndex].transform;
        }
        else
        {
            target = exit;
        }
        if (wallTilemap != null) CreateGridFromTilemap();
        
        individualSpeed = speed + Random.Range(-0.5f, 0.5f);

        if (!originSaved)
        {
            originX = transform.position.x;
            originY = transform.position.y;
            originSaved = true;
        }
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

    void Update()
    {
        if (!move) return;
        
        if (target == null || grid == null) return;

        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            GeneratePath();
            timer = pathUpdateRate + Random.Range(-0.05f, 0.05f);
        }

        FollowPath();
    }

    void GeneratePath()
    {
        Vector2Int start = WorldToGrid(transform.position);
        Vector2Int targetGridPos = WorldToGrid(target.position);
        float distanceToTarget = Vector2.Distance(transform.position, target.position);
    
        Vector2Int finalTarget = (distanceToTarget <= lockOnDistance) 
            ? targetGridPos 
            : GetRandomizedTarget(targetGridPos);

        if (start == finalTarget)
        {
            currentPath.Clear();
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

    IEnumerable<Vector2Int> GetSmartNeighbors(Vector2Int current, Vector2Int target)
    {
        List<Vector2Int> neighbors = new List<Vector2Int> 
        { 
            current + Vector2Int.up, current + Vector2Int.down, 
            current + Vector2Int.left, current + Vector2Int.right 
        };

        if (Random.value < curiosity)
        {
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

    void FollowPath()
    {
        if (currentPath == null || currentPath.Count == 0 || pathIndex >= currentPath.Count) return;

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
        float currentDist = Vector2.Distance(transform.position, target.position);
        float jitter = (currentDist > lockOnDistance) ? randomJitter : 0.05f;
        currentTargetWithJitter = rawCenter + new Vector3(Random.Range(-jitter, jitter), Random.Range(-jitter, jitter), 0);
    }

    public void ResetBear()
    {
        Vector2 worldSpawn = new Vector2(originX, originY);

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.position = worldSpawn;
        }

        transform.position = worldSpawn;
        move = false; 
        clearPath();
    }

    private void clearPath()
    {
        currentPath.Clear();
        pathIndex = 0;
        timer = 0; 
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

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.TryGetComponent<Player>(out Player player))
        {
            if (!move) 
            {
                move = true;

                // --- MERGED: Play the Bear Sound ---
                if(AudioManager.instance != null) {
                    AudioManager.instance.PlayBearSpeak(); 
                }

                Destroy(gameObject, 6f); 
                Debug.Log(gameObject.name + " activated. Destroying in 6s.");
            }
        }
        else if (collider.TryGetComponent<Enemy>(out Enemy enemy) || collider.TryGetComponent<Bear>(out Bear bear))
        {
            Destroy(gameObject);
        }
    }
}