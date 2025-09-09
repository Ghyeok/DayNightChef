using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : SingletonManager<SoundManager>
{
    // 상수(const)를 사용하여 문자열 오타 방지 및 관리 용이성 향상
    private const string BGM_VOLUME_KEY = "BGM_VOLUME";
    private const string SFX_VOLUME_KEY = "SFX_VOLUME";
    private const string BGM_MIXER_PARAM = "BGMVolume";
    private const string SFX_MIXER_PARAM = "SFXVolume";

    public enum SoundTypes { BGM, SFX, MAXCOUNT, }

    private Dictionary<string, AudioClip> audioClips = new Dictionary<string, AudioClip>();
    private AudioSource[] audioSources = new AudioSource[(int)SoundTypes.MAXCOUNT];

    [Header("Audio Mixer")]
    [SerializeField] public AudioMixer audioMixer;
    [SerializeField] public AudioMixerGroup BGMGroup;
    [SerializeField] public AudioMixerGroup SFXGroup;

    private float bgmVolume;
    private float sfxVolume;

    public AudioSource BGMSource => audioSources[(int)SoundTypes.BGM];
    public AudioSource SFXSource => audioSources[(int)SoundTypes.SFX];

    public float BGMVolume => bgmVolume;
    public float SFXVolume => sfxVolume;

    // 싱글톤 초기화는 다른 스크립트의 Awake()보다 먼저 실행되도록 Awake()에서 처리
    public override void Awake()
    {
        base.Awake(); // SingletonManager의 Awake()가 있다면 호출
        InitializeAudioSources();
        LoadSavedVolumes();
    }

    /// <summary>
    /// BGM, SFX 용 AudioSource를 생성하고 초기화합니다.
    /// </summary>
    private void InitializeAudioSources()
    {
        string[] soundNames = Enum.GetNames(typeof(SoundTypes));
        for (int i = 0; i < (int)SoundTypes.MAXCOUNT; i++)
        {
            GameObject go = new GameObject(soundNames[i]);
            go.transform.parent = this.transform;
            audioSources[i] = go.AddComponent<AudioSource>();
        }

        BGMSource.outputAudioMixerGroup = BGMGroup;
        BGMSource.loop = true;

        SFXSource.outputAudioMixerGroup = SFXGroup;
    }

    /// <summary>
    /// PlayerPrefs에 저장된 볼륨 값을 불러옵니다.
    /// </summary>
    private void LoadSavedVolumes()
    {
        bgmVolume = PlayerPrefs.GetFloat(BGM_VOLUME_KEY, 1.0f);
        sfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1.0f);

        audioMixer.SetFloat(BGM_MIXER_PARAM, LinearToDb(bgmVolume));
        audioMixer.SetFloat(SFX_MIXER_PARAM, LinearToDb(sfxVolume));
    }

    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        audioMixer.SetFloat(BGM_MIXER_PARAM, LinearToDb(bgmVolume));
        PlayerPrefs.SetFloat(BGM_VOLUME_KEY, bgmVolume);
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        audioMixer.SetFloat(SFX_MIXER_PARAM, LinearToDb(sfxVolume));
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, sfxVolume);
        PlayerPrefs.Save();
    }

    public void PlayAudioClip(string path, SoundTypes type = SoundTypes.SFX, float volumeScale = 1.0f)
    {
        AudioClip clip = GetOrAddAudioClip(path);
        if (clip == null) return;

        switch (type)
        {
            case SoundTypes.BGM:
                if (BGMSource.isPlaying && BGMSource.clip == clip) return;

                BGMSource.clip = clip;
                BGMSource.Play();
                break;

            case SoundTypes.SFX:
                SFXSource.PlayOneShot(clip, volumeScale);
                break;
        }
    }

    private AudioClip GetOrAddAudioClip(string path)
    {
        if (audioClips.TryGetValue(path, out AudioClip clip))
        {
            return clip;
        }

        // 경로를 배열로 관리하여 순차적으로 탐색
        string[] searchPaths = { $"Audio/BGM/{path}", $"Audio/SFX/{path}", $"Audio/{path}" };
        foreach (var fullPath in searchPaths)
        {
            clip = Resources.Load<AudioClip>(fullPath);
            if (clip != null)
            {
                audioClips.Add(path, clip);
                return clip;
            }
        }

        Debug.LogWarning($"AudioClip not found at path: {path}");
        return null;
    }

    private float LinearToDb(float linear)
    {
        return linear > 0.0001f ? Mathf.Log10(linear) * 20f : -80f;
    }

    private float DbToLinear(float db)
    {
        return Mathf.Pow(10f, db / 20f);
    }
}
