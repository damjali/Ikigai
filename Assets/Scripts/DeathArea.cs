using Players;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathArea : MonoBehaviour
{
    public LevelManager levelManager { get; set; }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        print("Something entered");
        if (collision.TryGetComponent<Player>(out Player player))
        {   
            // AudioManager.instance.PlayDeathSound();
            SceneManager.LoadScene("Scenes/Died Page");
        }
    }
}
