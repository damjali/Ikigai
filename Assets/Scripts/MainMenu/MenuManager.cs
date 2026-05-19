using UnityEngine;
using UnityEngine.SceneManagement; // Wajib ada untuk tukar scene!

public class MenuManager : MonoBehaviour
{
    // Fungsi ni akan dipanggil bila butang ditekan
    public void PlayGame()
    {
        // Tukar "Level1" kepada nama scene map anda yang SEBENAR
        SceneManager.LoadScene("Level1"); 
    }
}