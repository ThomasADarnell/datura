using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Music Tracks")]
    [SerializeField] private AudioClip menuMusic;
    [SerializeField] private AudioClip levelOneMusic;
    [SerializeField] private AudioClip levelTwoMusic;
    [SerializeField] private AudioClip bossOneMusic;
    [SerializeField] private AudioClip bossTwoMusic;
    [SerializeField] private AudioClip killScreenMusic;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip playerStabSound;
    [SerializeField] private AudioClip playerHurtSound;
    [SerializeField] private AudioClip playerDeathSound;
    [SerializeField] private AudioClip enemyHurtSound;
    [SerializeField] private AudioClip enemyDeathSound;
    [SerializeField] private AudioClip buttonClickSound;
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private AudioClip powerupSound;

    [Header("Settings")]
    [SerializeField] private float musicVolume = 0.7f;
    [SerializeField] private float sfxVolume = 1f;
    [SerializeField] private float crossfadeDuration = 1f;

    private Dictionary<string, AudioClip> sceneMusic;
    private string currentScene;
    private bool isCrossfading = false;

    void Awake()
    {
        // Singleton pattern - keep this object alive across scenes
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Setup audio sources if not assigned
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
        }

        // Initialize scene-to-music mapping
        InitializeSceneMusic();

        // Set initial volumes
        musicSource.volume = musicVolume;
        sfxSource.volume = sfxVolume;
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        // Play music for the current scene
        currentScene = SceneManager.GetActiveScene().name;
        PlayMusicForScene(currentScene);
    }

    private void InitializeSceneMusic()
    {
        sceneMusic = new Dictionary<string, AudioClip>
        {
            { "Menu", menuMusic },
            { "Level Select", menuMusic },
            { "Lvl1", levelOneMusic },
            { "Lvl1-3d", levelOneMusic },
            { "Lvl2", levelTwoMusic },
            { "Kill Screen", killScreenMusic },
            { "SampleScene", levelOneMusic },
            { "Test", levelOneMusic }
        };
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string newScene = scene.name;
        if (newScene != currentScene)
        {
            currentScene = newScene;
            PlayMusicForScene(currentScene);
        }
    }

    private void PlayMusicForScene(string sceneName)
    {
        if (sceneMusic.ContainsKey(sceneName) && sceneMusic[sceneName] != null)
        {
            PlayMusic(sceneMusic[sceneName]);
        }
    }

    public void PlayMusic(AudioClip clip, bool fadeTransition = true)
    {
        if (clip == null) return;

        if (musicSource.clip == clip && musicSource.isPlaying)
            return;

        if (fadeTransition && musicSource.isPlaying)
        {
            StartCoroutine(CrossfadeMusic(clip));
        }
        else
        {
            musicSource.clip = clip;
            musicSource.Play();
        }
    }

    private System.Collections.IEnumerator CrossfadeMusic(AudioClip newClip)
    {
        if (isCrossfading) yield break;
        isCrossfading = true;

        float timeElapsed = 0f;
        float startVolume = musicSource.volume;

        // Fade out
        while (timeElapsed < crossfadeDuration / 2)
        {
            timeElapsed += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, timeElapsed / (crossfadeDuration / 2));
            yield return null;
        }

        // Switch music
        musicSource.Stop();
        musicSource.clip = newClip;
        musicSource.Play();

        // Fade in
        timeElapsed = 0f;
        while (timeElapsed < crossfadeDuration / 2)
        {
            timeElapsed += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(0f, musicVolume, timeElapsed / (crossfadeDuration / 2));
            yield return null;
        }

        musicSource.volume = musicVolume;
        isCrossfading = false;
    }

    public void StopMusic(bool fade = true)
    {
        if (fade)
        {
            StartCoroutine(FadeOutMusic());
        }
        else
        {
            musicSource.Stop();
        }
    }

    private System.Collections.IEnumerator FadeOutMusic()
    {
        float timeElapsed = 0f;
        float startVolume = musicSource.volume;

        while (timeElapsed < crossfadeDuration)
        {
            timeElapsed += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, timeElapsed / crossfadeDuration);
            yield return null;
        }

        musicSource.Stop();
        musicSource.volume = musicVolume;
    }

    // Sound effect playback functions
    public void PlaySound(AudioClip clip, float volumeMultiplier = 1f)
    {
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip, volumeMultiplier);
        }
    }

    public void PlayPlayerStab()
    {
        PlaySound(playerStabSound);
    }

    public void PlayPlayerHurt()
    {
        PlaySound(playerHurtSound);
    }

    public void PlayPlayerDeath()
    {
        PlaySound(playerDeathSound);
    }

    public void PlayEnemyHurt()
    {
        PlaySound(enemyHurtSound);
    }

    public void PlayEnemyDeath()
    {
        PlaySound(enemyDeathSound);
    }

    public void PlayButtonClick()
    {
        PlaySound(buttonClickSound);
    }

    public void PlayPickup()
    {
        PlaySound(pickupSound);
    }

    public void PlayPowerup()
    {
        PlaySound(powerupSound);
    }

    public void PlayBossOneMusic()
    {
        PlayMusic(bossOneMusic);
    }

    public void PlayBossTwoMusic()
    {
        PlayMusic(bossTwoMusic);
    }

    // Volume control
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        musicSource.volume = musicVolume;
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        sfxSource.volume = sfxVolume;
    }

    public float GetMusicVolume() => musicVolume;
    public float GetSFXVolume() => sfxVolume;
}
