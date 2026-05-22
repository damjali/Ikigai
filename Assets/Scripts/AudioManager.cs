using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Audio Sources")]
    public AudioSource sfxSource; // For quick sounds (medicine, death, etc.)
    public AudioSource heartbeatSource; // NEW: Dedicated source for the long drum track!

    [Header("The Master Switch")]
    public static bool useVoiceSFX = false; // False = Normal, True = Your Voices

    [Header("Normal Audio Clips")]
    public AudioClip normalJantung;
    public AudioClip normalMedicine;
    public AudioClip normalMati;
    public AudioClip normalBearSpeak; 
    public AudioClip normalScared; // 5th Sound!

    [Header("Voice Audio Clips (Best SFX!)")]
    public AudioClip voiceJantung;
    public AudioClip voiceMedicine;
    public AudioClip voiceMati;
    public AudioClip voiceBearSpeak; 
    public AudioClip voiceScared; // 5th Sound!

    void Awake()
    {
        if (instance == null)
            instance = this;
    }

    // --- UI TOGGLE METHOD ---
    public void SetVoiceSFX(bool isToggled)
    {
        useVoiceSFX = isToggled;
        Debug.Log("BEST SFX Mode is now: " + useVoiceSFX);
    }

    // --- PLAY METHODS ---
    public void PlayHeartbeat()
    {
        AudioClip clipToPlay = useVoiceSFX ? voiceJantung : normalJantung;
        
        if (clipToPlay != null)
        {
            // Set the clip if it isn't already set
            if (heartbeatSource.clip != clipToPlay)
            {
                heartbeatSource.clip = clipToPlay;
            }
            
            // The magic fix: Only play if it is NOT already playing!
            if (!heartbeatSource.isPlaying)
            {
                heartbeatSource.Play();
            }
        }
    }

    public void PlayMedicineSound()
    {
        print("Makanmomomomom");
        AudioClip clipToPlay = useVoiceSFX ? voiceMedicine : normalMedicine;
        if(clipToPlay != null) sfxSource.PlayOneShot(clipToPlay);
    }

    public void PlayDeathSound()
    {
        AudioClip clipToPlay = useVoiceSFX ? voiceMati : normalMati;
        if(clipToPlay != null) sfxSource.PlayOneShot(clipToPlay);
    }

    public void PlayBearSpeak()
    {
        AudioClip clipToPlay = useVoiceSFX ? voiceBearSpeak : normalBearSpeak;
        if(clipToPlay != null) sfxSource.PlayOneShot(clipToPlay);
    }

    public void PlayScaredSound()
    {
        AudioClip clipToPlay = useVoiceSFX ? voiceScared : normalScared;
        if(clipToPlay != null) sfxSource.PlayOneShot(clipToPlay);
    }
}