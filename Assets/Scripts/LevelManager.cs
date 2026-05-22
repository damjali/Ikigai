using System.Collections.Generic;
using Players;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelManager : MonoBehaviour
{
    public Player1 player1;
    public Player2 player2;
    public List<Enemy> enemies;
    public List<Bear> bears;
    private List<MonsterBlink> monsterBlinks = new List<MonsterBlink>();
    public GameObject secretDoor;
    public Lvl1Canvas canvas;

    [Header("Spawning Settings")]
    public Tilemap wallTilemap;
    public GameObject medicinePrefab;
    public GameObject bearPrefab;
    public Transform bearExit;
    public int bearCount = 5;
    public int medicineCount = 3;
    private int medicinesRemaining;

    void Start()
    {
        medicinesRemaining = medicineCount;
        foreach (Enemy e in enemies)
        {
            if (e != null)
            {
                e.GetComponent<DeathArea>().levelManager = this;
                monsterBlinks.Add(e.GetComponentInChildren<MonsterBlink>());
            }
        }
        
        if (player2 != null)
        {
            BlackoutScare blackoutScare = player2.GetComponentInChildren<BlackoutScare>();
            if (blackoutScare != null) blackoutScare.levelManager = this;
        }

        SpawnRandomObjects();
    }

    void SpawnRandomObjects()
    {
        if (wallTilemap == null)
        {
            Debug.LogWarning("LevelManager: wallTilemap is not assigned! Cannot spawn objects.");
            return;
        }

        BoundsInt bounds = wallTilemap.cellBounds;
        int centerX = (bounds.xMin + bounds.xMax) / 2;

        // Left Map Bounds
        BoundsInt leftBounds = new BoundsInt(bounds.xMin, bounds.yMin, 0, centerX - bounds.xMin, bounds.size.y, 1);
        // Right Map Bounds
        BoundsInt rightBounds = new BoundsInt(centerX, bounds.yMin, 0, bounds.xMax - centerX, bounds.size.y, 1);

        List<Vector3> leftWalkable = GetWalkablePositions(leftBounds);
        List<Vector3> rightWalkable = GetWalkablePositions(rightBounds);

        // Spawn Medicine on the Left
        if (medicinePrefab != null && leftWalkable.Count > 0)
        {
            for (int i = 0; i < medicineCount; i++)
            {
                int randomIndex = Random.Range(0, leftWalkable.Count);
                Instantiate(medicinePrefab, leftWalkable[randomIndex], Quaternion.identity);
                leftWalkable.RemoveAt(randomIndex); // Don't spawn on the same spot
                if (leftWalkable.Count == 0) break;
            }
        }

        // Spawn Bears on the Right
        if (bearPrefab != null && rightWalkable.Count > 0)
        {
            for (int i = 0; i < bearCount; i++)
            {
                int randomIndex = Random.Range(0, rightWalkable.Count);
                GameObject bearObj = Instantiate(bearPrefab, rightWalkable[randomIndex], Quaternion.identity);
                Bear bearScript = bearObj.GetComponent<Bear>();
                
                if (bearScript != null)
                {
                    bearScript.wallTilemap = wallTilemap;
                    bearScript.exit = bearExit;
                    bearScript.levelManager = this;
                    bears.Add(bearScript);
                }

                rightWalkable.RemoveAt(randomIndex);
                if (rightWalkable.Count == 0) break;
            }
        }
    }

    private List<Vector3> GetWalkablePositions(BoundsInt bounds)
    {
        List<Vector3> walkable = new List<Vector3>();
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                if (!wallTilemap.HasTile(pos))
                {
                    walkable.Add(wallTilemap.GetCellCenterWorld(pos));
                }
            }
        }
        return walkable;
    }

    void Update()
    {
        UpdatePlayerInput();
    }

    public void resetLevel()
    {
        player1.reset();
        player2.reset();
        foreach (Enemy e in enemies)
        {
            if (e != null)
            {
                print("Resetting enemy: " + e);
                e.reset();
            }
        }
        
        // Note: Bears are destroyed in their own script after a while, 
        // but if they are still there, they might need resetting or clearing.
        // For simplicity, we keep them as they are or the user can add logic to clear them.
    }

    public void blinkMonster()
    {
        foreach(MonsterBlink b in monsterBlinks)
        {
            if (b != null) b.Blink();
        }
    }

    public void stopBlink()
    {
        foreach (MonsterBlink b in monsterBlinks)
        {
            if (b != null) b.StopBlink();
        }
    }
    
    private async void UpdatePlayerInput()
    {
        if (Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.LeftShift) && player1.haveLeft)
        {
            player1.haveLeft = false;
            player2.haveLeft = true;
        }

        if (Input.GetKey(KeyCode.D) && Input.GetKey(KeyCode.LeftShift) && player1.haveRight)
        {
            player1.haveRight = false;
            player2.haveRight = true;
        }

        if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.LeftShift) && player1.haveUp)
        {
            player1.haveUp = false;
            player2.haveUp = true;
        }

        if (Input.GetKey(KeyCode.S) && Input.GetKey(KeyCode.LeftShift) && player1.haveDown)
        {
            player1.haveDown = false;
            player2.haveDown = true;
        }

        if (Input.GetKey(KeyCode.LeftArrow) && Input.GetKey(KeyCode.RightShift) && player2.haveLeft)
        {
            player2.haveLeft = false;
            player1.haveLeft = true;
        }

        if (Input.GetKey(KeyCode.RightArrow) && Input.GetKey(KeyCode.RightShift) && player2.haveRight)
        {
            player2.haveRight = false;
            player1.haveRight = true;
        }

        if (Input.GetKey(KeyCode.UpArrow) && Input.GetKey(KeyCode.RightShift) && player2.haveUp)
        {
            player2.haveUp = false;
            player1.haveUp = true;
        }

        if (Input.GetKey(KeyCode.DownArrow) && Input.GetKey(KeyCode.RightShift) && player2.haveDown)
        {
            player2.haveDown = false;
            player1.haveDown = true;
        }
    }

    public void spawnExit()
    {
        print("spawm");
        canvas.TriggerPopup();
        Instantiate(secretDoor, new Vector2((float)-10.48, (float)-27.7), Quaternion.identity);
    }

    public void StunEnemies(float duration)
    {
        foreach (Enemy e in enemies)
        {
            if (e != null) e.Stun(duration);
        }
    }

    // public void MedicineCollected()
    // {
    //     medicinesRemaining--;
    //     if (medicinesRemaining <= 0)
    //     {
    //         // Check if secret door exists (game hasn't ended/reached exit yet)
    //         GameObject door = GameObject.Find("Secret Door(Clone)");
    //         if (door == null)
    //         {
    //             Debug.Log("No more medicine and exit not spawned. Game Over.");
    //             UnityEngine.SceneManagement.SceneManager.LoadScene("Died Page");
    //         }
    //     }
    // }
}
