using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class AudioManager : MonoBehaviour // The simple version of an Audio Manager. Will be scrapped/updated later if more features are necessary 
{ // (reference: https://youtu.be/tLyj02T51Oc)
         // To use this, please make sure that the AudioManager is in your scene and call the methods here using
         // i.e. AudioManager.Instance.PlaySFX(AudioSource, volume);
    
   
    private static AudioManager instance; //  Singleton
    public static AudioManager Instance {
        get {
            if (instance == null) {
                instance = FindObjectOfType<AudioManager>();
                if (instance == null) { // if it fails to find an instance in the scene..
                    instance = new GameObject("Audio Manager instantiated.", typeof(AudioManager)).GetComponent<AudioManager>();
                }
            }
            return instance;
        }
        private set
        {
            instance = value;
        }
    }

    // Fields for AudioSource objects in the scene
    private static AudioSource bgmSource;
    private static AudioSource sndbgmSource;
    private static  AudioSource sfxSource;
    [SerializeField] AudioClip menuBGM;
    // Checkers
    private bool firstBGMIsPlaying = false;
    public static float bgmVolume{ get; set; }
    public static float sfxVolume { get; set; }
    private static float prevBgmVolume = bgmVolume;
    private static float currentBgmRequestedVol = 1.0f;
    private static float prevSfxVolume = sfxVolume;
    private static float currentSfxRequestedVol = 1.0f;


    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        InitAudioSource(ref bgmSource, true);
        InitAudioSource(ref sndbgmSource, true);
        InitAudioSource(ref sfxSource, false);
        PlayBGM(menuBGM, 1.0f);
    }

    private void OnEnable()
    {
        UpdateHandler.UpdateOccurred += CheckForChange;
    }

    private void OnDisable()
    {
        UpdateHandler.UpdateOccurred -= CheckForChange;
    }

    private void CheckForChange() {
        if (prevBgmVolume != bgmVolume) {
            prevBgmVolume = bgmVolume;
            AudioSource activeSource = (firstBGMIsPlaying) ? bgmSource : sndbgmSource;
            activeSource.volume = bgmVolume * currentBgmRequestedVol;
            SaveVolume(true, bgmVolume);
        }
        if (prevSfxVolume != sfxVolume)
        {
            prevSfxVolume = sfxVolume;
            sfxSource.volume = sfxVolume * currentSfxRequestedVol;
            SaveVolume(false, sfxVolume);
        }
    }
    
    // Saves given volume
    static private void SaveVolume(bool isBGM, float volume)
    {
        MenuManager mm = FindObjectOfType<MenuManager>();
        mm.SaveVolumeSettings(isBGM, volume);
    }

    // Loads the saved volume settings
    static public void LoadVolumeSettings(float bgmV, float sfxV)
    {
        bgmVolume = bgmV;
        sfxVolume = sfxV;
        GameObject sfxSlider = GameObject.FindGameObjectWithTag("SFXSlider");
        GameObject bgmSlider = GameObject.FindGameObjectWithTag("BGMSlider");
        sfxSlider.GetComponent<Slider>().value = sfxVolume;
        bgmSlider.GetComponent<Slider>().value = bgmVolume;
    }


    // BGM
    public void PlayBGM(AudioClip musicClip, float vol = 1.0f) {
        AudioSource activeSource = (firstBGMIsPlaying) ? bgmSource : sndbgmSource;
        if (vol > 1)
            vol = 1.0f;
        else if (vol < 0)
            vol = 0f;
        currentBgmRequestedVol = vol;
        activeSource.clip = musicClip;
        activeSource.volume = bgmVolume*vol;
        activeSource.Play();
    }
    public void PlayBGMWithFade(AudioClip newClip, float transitionTime = 1.0f) {
        AudioSource activeSource = (firstBGMIsPlaying) ? bgmSource : sndbgmSource;
        StartCoroutine(UpdateMusicWithFade(activeSource, newClip, transitionTime));
    }
    private IEnumerator UpdateMusicWithFade(AudioSource activeSource, AudioClip newClip, float transitionTime) {
        if (!activeSource.isPlaying)
            activeSource.Play();

        for (float t = 0.0f; t < transitionTime; t += Time.deltaTime) {
            activeSource.volume = (bgmVolume - ((t / transitionTime)*bgmVolume));
            yield return null;
        }
        activeSource.Stop();
        activeSource.clip = newClip;
        activeSource.Play();
        for (float t = 0.0f; t < transitionTime; t += Time.deltaTime) {
            activeSource.volume = ((t / transitionTime)*bgmVolume);
        }
    }
    public void PlayBGMWithCrossFade(AudioClip newClip, float transitionTime = 1.0f) {
        AudioSource activeSource = (firstBGMIsPlaying) ? bgmSource : sndbgmSource;
        AudioSource newSource = (firstBGMIsPlaying) ? sndbgmSource : bgmSource;
        firstBGMIsPlaying = !firstBGMIsPlaying;

        newSource.clip = newClip;
        newSource.Play();
        StartCoroutine(UpdateMusicWithCrossFade(activeSource, newSource, transitionTime));
    }
    private IEnumerator UpdateMusicWithCrossFade(AudioSource activeSource, AudioSource newSource, float transitiontime) {
        for (float t = 0.0f; t <= transitiontime; t += Time.deltaTime) {
            activeSource.volume = (bgmVolume - ((t / transitiontime)*bgmVolume));
            newSource.volume = ((t / transitiontime)*bgmVolume);
            yield return null;
        }
        activeSource.Stop();
    }
    // SFX
    public static void PlaySFX(AudioClip clip, float volume = 1.0f) {
        if (volume > 1)
            volume = 1.0f;
        else if (volume < 0)
            volume = 0f;
        currentSfxRequestedVol = volume;

        // SFXVOLUME NOT WORKING, ALWAYS 0
        //Debug.Log("Volume SFX: " + volume * sfxVolume);
        //sfxSource.PlayOneShot(clip, volume*sfxVolume);
        sfxSource.PlayOneShot(clip, volume);
    }

    private void InitAudioSource(ref AudioSource ini, bool isBGM) {
        ini = this.gameObject.AddComponent<AudioSource>();
        if (isBGM)
            ini.loop = true;
    }
}
