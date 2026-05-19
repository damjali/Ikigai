using System;
using UnityEngine;
using UnityEngine.SceneManagement;
public class PageManager : MonoBehaviour
{
    public static void loadLevel()
    {
        SceneManager.LoadScene("Scenes/Level 1");
    }

    public static void loadInstruction2()
    {
        SceneManager.LoadScene("Scenes/Instruction2");
    }

    public static void loadInstruction1()
    {
        SceneManager.LoadScene("Scenes/Instruction1");
    }


    public static void loadInstruction3()
    {
        SceneManager.LoadScene("Scenes/Instruction3");
    }
    public static void loadMainMenu()
    {
        SceneManager.LoadScene("Scenes/MainMenu");
    }
}
