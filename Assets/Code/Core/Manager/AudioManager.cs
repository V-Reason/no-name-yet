using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : Singleton<AudioManager>
{
    [Header("音量")]
    [SerializeField] private float _musicVolume = 1f;
    [SerializeField] private float _sfxVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float _pauseVolumeRatio = 0.3f;
    [SerializeField, Range(0.1f, 2f)] private float _musicFadeDuration = 0.5f;

    [Header("音频资源")]
    [SerializeField] private AudioClip _gameplayBgm;

    private AudioSource _musicSource;
    private AudioSource _sfxSource;

    private Queue<AudioSource> _pitchSfxPool;
    private const int PitchSfxPoolSize = 6;

    private Coroutine _fadeCoroutine;

    public float MusicVolume
    {
        get => _musicVolume;
        set
        {
            _musicVolume = Mathf.Clamp01(value);
            ApplyMusicVolume();
        }
    }

    public float SfxVolume
    {
        get => _sfxVolume;
        set
        {
            _sfxVolume = Mathf.Clamp01(value);
            if (_sfxSource) _sfxSource.volume = _sfxVolume;
        }
    }

    public bool IsMusicPlaying => _musicSource && _musicSource.isPlaying;
    public bool IsMusicMuted { get; private set; }
    public bool IsSfxMuted { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        _musicSource = gameObject.AddComponent<AudioSource>();
        _musicSource.loop = true;
        _musicSource.playOnAwake = false;
        _musicSource.volume = _musicVolume;

        _sfxSource = gameObject.AddComponent<AudioSource>();
        _sfxSource.playOnAwake = false;
        _sfxSource.volume = _sfxVolume;

        BuildPitchSfxPool();
    }

    private void OnEnable()
    {
        EventCenter.Subscribe(EventType.GameStateChanged, OnGameStateChanged);
    }

    private void OnDisable()
    {
        EventCenter.Unsubscribe(EventType.GameStateChanged, OnGameStateChanged);
    }

    // ==================== BGM ====================

    public void PlayMusic(AudioClip clip, bool restartIfSame = false)
    {
        if (!clip) return;
        if (!restartIfSame && _musicSource.clip == clip && _musicSource.isPlaying) return;

        _musicSource.clip = clip;
        _musicSource.Play();
    }

    public void StopMusic()
    {
        _musicSource.Stop();
    }

    public void PauseMusic()
    {
        _musicSource.Pause();
    }

    public void ResumeMusic()
    {
        _musicSource.UnPause();
    }

    // ==================== Game State ====================

    private void OnGameStateChanged(object data)
    {
        var state = (GameState)data;
        switch (state)
        {
            case GameState.Menu:
                StopMusic();
                break;
            case GameState.Playing:
                if (_gameplayBgm) PlayMusic(_gameplayBgm);
                ApplyMusicVolume();
                break;
            case GameState.Paused:
                ApplyMusicVolume();
                break;
            case GameState.GameOver:
                StopMusic();
                break;
        }
    }

    private void ApplyMusicVolume()
    {
        if (!_musicSource) return;
        float target = _musicVolume;
        if (GameManager.Instance.CurrentState == GameState.Paused)
            target *= _pauseVolumeRatio;
        if (IsMusicMuted) target = 0f;

        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
        _fadeCoroutine = StartCoroutine(FadeMusicCoroutine(target));
    }

    private IEnumerator FadeMusicCoroutine(float targetVolume)
    {
        float start = _musicSource.volume;
        float elapsed = 0f;
        while (elapsed < _musicFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            _musicSource.volume = Mathf.Lerp(start, targetVolume, elapsed / _musicFadeDuration);
            yield return null;
        }
        _musicSource.volume = targetVolume;
        _fadeCoroutine = null;
    }

    // ==================== SFX ====================

    public void PlaySFX(AudioClip clip, float volumeScale = 1f)
    {
        if (!clip) return;
        float vol = IsSfxMuted ? 0f : _sfxVolume * volumeScale;
        _sfxSource.PlayOneShot(clip, vol);
    }

    public void PlayRandomPitchSFX(AudioClip clip, float volumeScale = 1f, float pitchMin = 0.8f, float pitchMax = 1.2f)
    {
        if (!clip) return;
        StartCoroutine(RandomPitchCoroutine(clip, volumeScale, pitchMin, pitchMax));
    }

    // ==================== Mute ====================

    public void MuteMusic(bool mute)
    {
        IsMusicMuted = mute;
        ApplyMusicVolume();
    }

    public void MuteSfx(bool mute)
    {
        IsSfxMuted = mute;
        _sfxSource.mute = mute;
    }

    public void MuteAll(bool mute)
    {
        MuteMusic(mute);
        MuteSfx(mute);
    }

    // ==================== Pool ====================

    private void BuildPitchSfxPool()
    {
        _pitchSfxPool = new Queue<AudioSource>(PitchSfxPoolSize);
        for (int i = 0; i < PitchSfxPoolSize; i++)
        {
            var go = new GameObject("PooledPitchSFX");
            go.transform.SetParent(transform);
            var src = go.AddComponent<AudioSource>();
            src.playOnAwake = false;
            go.SetActive(false);
            _pitchSfxPool.Enqueue(src);
        }
    }

    private AudioSource GetPooledSource()
    {
        if (_pitchSfxPool.Count == 0)
        {
            var go = new GameObject("PooledPitchSFX");
            go.transform.SetParent(transform);
            var src = go.AddComponent<AudioSource>();
            src.playOnAwake = false;
            return src;
        }
        return _pitchSfxPool.Dequeue();
    }

    private IEnumerator RandomPitchCoroutine(AudioClip clip, float volumeScale, float pitchMin, float pitchMax)
    {
        var source = GetPooledSource();
        source.gameObject.SetActive(true);
        source.clip = clip;
        source.pitch = Random.Range(pitchMin, pitchMax);
        source.volume = (IsSfxMuted ? 0f : _sfxVolume * volumeScale);
        source.Play();

        yield return new WaitForSeconds(clip.length / source.pitch);

        source.gameObject.SetActive(false);
        _pitchSfxPool.Enqueue(source);
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
        if (_pitchSfxPool == null) return;
        while (_pitchSfxPool.Count > 0)
        {
            var src = _pitchSfxPool.Dequeue();
            if (src) Destroy(src.gameObject);
        }
    }
}
