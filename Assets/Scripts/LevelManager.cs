using System.Collections.Generic;
using Players;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public Player1 player1;
    public Player2 player2;
    public List<Enemy> enemies;
    public List<Bear> bears;
    private List<MonsterBlink> monsterBlinks = new List<MonsterBlink>();
    public GameObject secretDoor;
    public Lvl1Canvas canvas;

    void Start()
    {
        foreach (Enemy e in enemies)
        {
            e.GetComponent<DeathArea>().levelManager = this;
            monsterBlinks.Add(e.GetComponentInChildren<MonsterBlink>());
        }
        
            BlackoutScare blackoutScare = player2.GetComponentInChildren<BlackoutScare>();
            blackoutScare.levelManager = this;
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
            print("Resetting enemy: " + e);
            e.reset();
        }
    }

    public void blinkMonster()
    {
        foreach(MonsterBlink b in monsterBlinks)
         {
             b.Blink();
          }
    }

    public void stopBlink()
    {
        foreach (MonsterBlink b in monsterBlinks)
        {
            b.StopBlink();
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
}
