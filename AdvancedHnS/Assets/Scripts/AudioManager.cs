using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    public AudioSource musicSource;   // for background music
    public AudioSource sfxSource;     // for sound effects (uses PlayOneShot)

    void Awake()
    {
        // Singleton — keep this object alive across all scenes
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>Plays a one‑shot sound effect (can overlap).</summary>
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip);
    }

    /// <summary>Starts playing background music (loops).</summary>
    public void PlayMusic(AudioClip music)
    {
        if (music == null || musicSource == null) return;
        musicSource.clip = music;
        musicSource.loop = true;
        musicSource.Play();
    }

    /// <summary>Stops the background music.</summary>
    public void StopMusic()
    {
        if (musicSource != null) musicSource.Stop();
    }
}