using Mono.Cecil;
using System.Collections.Generic;
using UnityEngine;


/* AudioClip : 실제로 재생되는 사운드 파일
*  AudioSource : 해당 클립을 관리하는 역할
*/

public class SoundManager : Managers<SoundManager>
{
    AudioSource[] audioSources = new AudioSource[(int)Define.Sounds.MaxCount];
    Dictionary<string, AudioClip> audioClips = new Dictionary<string, AudioClip>();

    AudioSource BGMSource { get { return audioSources[(int)Define.Sounds.BGM]; } } // BGM으로 사용할 AudioSource
    AudioSource SFXSource { get { return audioSources[(int)Define.Sounds.SFX]; } } // SFX(효과음)으로 사용할 AudioSource

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void PlayAudioClip(string path, Define.Sounds newSoundType)
    {
        AudioClip clip = GetOrAddAudioClip(path);

        if (newSoundType == Define.Sounds.BGM)
        {
            // BGM 재생 로직..
            BGMSource.clip = clip;
            BGMSource.Play();
        }

        if (newSoundType == Define.Sounds.SFX)
        {
            // 효과음 재생 로직..
            SFXSource.clip = clip;
            SFXSource.PlayOneShot(clip);
        }

    }

    AudioClip GetOrAddAudioClip(string path)
    {
        if (audioClips.TryGetValue(path, out AudioClip clip)) // Get
            return clip;

        clip = Resources.Load<AudioClip>($"Sounds/{path}"); // Add

        if (clip == null)
        {
            Debug.LogWarning($"오디오 클립을 찾을 수 없습니다 : {path}");
        }
        else
        {
            audioClips[path] = clip;
        }

        return clip;
    }
}
