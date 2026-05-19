using Players;
using UnityEngine;
using UnityEngine.SceneManagement;
    
public class SecretDoor : MonoBehaviour
{
    private bool p1Pass = false;
    private bool p2Pass = false;
    void OnTriggerEnter2D(Collider2D collider)
    {
        print("Something entered");
        if (collider.TryGetComponent<Player1>(out Player1 p1))
        {   
            p1Pass = true;
            p1.won();
        }

        if (collider.TryGetComponent<Player2>(out Player2 p2))
        {
            p2Pass = true;
            p2.won();
        }

        if (p1Pass && p2Pass)
        {
            SceneManager.LoadScene("Scenes/EndingScene");
        }
    }
}
