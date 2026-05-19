using UnityEngine;

public class DiedPage : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        AudioManager.instance.PlayDeathSound();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
