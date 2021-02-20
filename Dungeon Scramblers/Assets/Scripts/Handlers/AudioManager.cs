using System.Collections;
using System.Collections.Generic;
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
    private AudioSource bgmSource;
    private AudioSource sndbgmSource;
    private AudioSource sfxSource;

    // Checkers
    private bool firstBGMIsPlaying = false;

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        InitAudioSource(bgmSource, true);
        InitAudioSource(sndbgmSource, true);
        InitAudioSource(sfxSource, false);
    }

    // BGM
    public void PlayBGM(AudioClip musicClip) {
        AudioSource activeSource = (firstBGMIsPlaying) ? bgmSource : sndbgmSource;
        activeSource.clip = musicClip;
        activeSource.volume = 1;
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
            activeSource.volume = (1 - (t / transitionTime));
            yield return null;
        }
        activeSource.Stop();
        activeSource.clip = newClip;
        activeSource.Play();
        for (float t = 0.0f; t < transitionTime; t += Time.deltaTime) {
            activeSource.volume = (t / transitionTime);
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
            activeSource.volume = (1 - (t / transitiontime));
            newSource.volume = (t / transitiontime);
            yield return null;
        }
        activeSource.Stop();
    }
    // SFX
    public void PlaySFX(AudioClip clip, float volume = 1.0f) {
        if (volume > 1)
            volume = 1.0f;
        else if (volume < 0)
            volume = 0f;
        sfxSource.PlayOneShot(clip, volume);
    }

    private void InitAudioSource(AudioSource ini, bool isBGM) {
        ini = this.gameObject.AddComponent<AudioSource>();
        if (isBGM)
            ini.loop = true;
    }

}
