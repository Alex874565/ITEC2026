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

    public AudioClip menuSwitchClip;
    
    public AudioClip currentMusic;
    
    [Header("Volumes")]
    [Range(0f,1f)] [SerializeField] private float masterVolume = 1f;
    [Range(0f,1f)] [SerializeField] private float musicVolume = 1f;
    [Range(0f,1f)] [SerializeField] private float sfxVolume = 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.5f);
        masterVolume = PlayerPrefs.GetFloat("MasterVolume", 0.5f);

        MusicSource.volume = 0f;
        ApplyVolumes();
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
        if (clip == null || currentMusic == clip) return;

        currentMusic = clip;

        MusicSource.DOKill();
        MusicSource.DOFade(0f, 0.5f).SetUpdate(true).OnComplete(() =>
        {
            MusicSource.clip = clip;
            MusicSource.volume = 0f;
            MusicSource.Play();
            MusicSource.DOFade(1f, 0.5f).SetUpdate(true);
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
            MusicSource.volume = 1f;
        });
    }

    public void PlayMenuChangeSFX()
    {
        PlaySFX(menuSwitchClip);
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
        PlayerPrefs.Save();
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
        return value <= 0.0001f ? -80f : Mathf.Log10(value) * 20f;
    }
    
    public AudioSource GetMusicSource()
    {
        return MusicSource;
    }
}