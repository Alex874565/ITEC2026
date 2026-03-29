using DG.Tweening;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    
    public AudioSource SFXSource;
    public AudioSource MusicSource;

    public AudioClip MainMenuMusic;
    public AudioClip GameMusic;
    
    public AudioMixer Mixer;

    public AudioClip currentMusic;
    
    [Header("Volumes")]
    [Range(0f,1f)] [SerializeField] private float masterVolume = 1f;
    [Range(0f,1f)] [SerializeField] private float musicVolume = 1f;
    [Range(0f,1f)] [SerializeField] private float sfxVolume = 1f;

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SetMusicVolume(PlayerPrefs.GetFloat("MusicVolume", .5f));
        SetSFXVolume(PlayerPrefs.GetFloat("SFXVolume", .5f));
        SetMasterVolume(PlayerPrefs.GetFloat("MasterVolume", .5f));
    }
    
    private void Start()
    {
        PlayMusic(MainMenuMusic);
    }
    
    public void PlaySFX(AudioClip audioClip)
    {
        if(SFXSource != null && audioClip != null)
            SFXSource.PlayOneShot(audioClip);
    }

    public void PlayMainMenuMusic()
    {
        PlayMusic(MainMenuMusic);
    }

    public void PlayMusic(AudioClip clip)
    {
        Debug.Log("PLAY MUSIC CALLED");
        Debug.Log(clip);
        Debug.Log(currentMusic);
        if (clip == null || currentMusic == clip) return;
        currentMusic = clip;

        MusicSource.DOKill();
        MusicSource.DOFade(0f, 0.5f).SetUpdate(true).OnComplete(() =>
        {
            MusicSource.clip = clip;
            MusicSource.Play();
            MusicSource.DOFade(musicVolume * masterVolume, 0.5f).SetUpdate(true);
        });
    }
    
    public void StopMusic(float fadeDuration = 0.5f)
    {
        MusicSource.DOKill();
        // SetUpdate(true) is critical for pausing!
        MusicSource.DOFade(0f, fadeDuration).SetUpdate(true).OnComplete(() =>
        {
            MusicSource.Stop();
            MusicSource.clip = null;
            currentMusic = null;
            MusicSource.volume = musicVolume * masterVolume;
        });
    }

    public void SetMusicVolume(float value)
    {
        musicVolume = value;
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.Save();
        ApplyVolumes();
    }
    public float GetMusicVolume() => musicVolume;

    public void SetSFXVolume(float value)
    {
        sfxVolume = value;
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        PlayerPrefs.Save();
        ApplyVolumes();
    }

    public float GetSFXVolume() => sfxVolume;

    public void SetMasterVolume(float value)
    {
        masterVolume = value;
        PlayerPrefs.SetFloat("MasterVolume", masterVolume);
        ApplyVolumes();
    }

    public float GetMasterVolume() => masterVolume;
    
    void ApplyVolumes()
    {
        Mixer.SetFloat("MasterVol", LinearToDb(masterVolume));
        Mixer.SetFloat("SFXVol", LinearToDb(sfxVolume));
        Mixer.SetFloat("MusicVol", LinearToDb(musicVolume));
    }

    private float LinearToDb(float value)
    {
        return Mathf.Log10(value) * 20f;
    }
    
    public AudioSource GetMusicSource()
    {
        return MusicSource;
    }
}