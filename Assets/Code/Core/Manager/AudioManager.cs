using UnityEngine;

public class AudioManager : Singleton<AudioManager>
{
    private AudioSource musicSource;
    private AudioSource sfxSource;

    protected override void Awake()
    {
        base.Awake();

        // 动态添加音源组件
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true; // BGM 循环

        sfxSource = gameObject.AddComponent<AudioSource>();
    }

    // 播放背景音乐
    public void PlayMusic(AudioClip clip, float volume = 1f)
    {
        if (musicSource.clip == clip) return; // 已经在播了就跳过
        musicSource.clip = clip;
        musicSource.volume = volume;
        musicSource.Play();
    }

    // 播放音效
    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        sfxSource.PlayOneShot(clip, volume);
    }

    public void PlayRandomPitchSFX(AudioClip clip, float volume = 1f)
    {
        StartCoroutine(PlayRandomPitchCoroutine(clip, volume));
    }

    private System.Collections.IEnumerator PlayRandomPitchCoroutine(AudioClip clip, float volume)
    {
        GameObject temp = new GameObject("TempSFX");
        temp.transform.SetParent(transform);
        AudioSource source = temp.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = volume;
        source.pitch = Random.Range(0.8f, 1.2f);
        source.Play();
        yield return new WaitForSeconds(clip.length / source.pitch);
        Destroy(temp);
    }
}