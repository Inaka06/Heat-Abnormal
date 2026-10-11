using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class HeatAbnormalAudioManager : MonoBehaviour
{
    private static HeatAbnormalAudioManager instance;
    private AudioSource musicSource;
    private AudioSource clickSource;
    private AudioClip clickClip;
    private AudioClip[] musicClips;
    private readonly HashSet<int> wiredButtons = new HashSet<int>();
    private int lastMusicIndex = -1;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Create()
    {
        if (instance != null) return;
        var obj = new GameObject("HeatAbnormalAudioManager");
        instance = obj.AddComponent<HeatAbnormalAudioManager>();
        DontDestroyOnLoad(obj);
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
        if (FindObjectsByType<AudioListener>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length == 0)
        {
            gameObject.AddComponent<AudioListener>();
        }
        musicSource = gameObject.AddComponent<AudioSource>();
        clickSource = gameObject.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        clickSource.playOnAwake = false;
        musicSource.loop = false;
        musicSource.volume = 0.18f;
        clickSource.volume = 0.35f;
        clickClip = Resources.Load<AudioClip>("HeatAbnormal/Audio/Click_HA");
        musicClips = Resources.LoadAll<AudioClip>("HeatAbnormal/Audio");
    }

    private void Update()
    {
        if (musicSource != null && !musicSource.isPlaying) PlayNextMusic();
        var buttons = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var button in buttons)
        {
            if (button == null || !wiredButtons.Add(button.GetInstanceID())) continue;
            button.onClick.AddListener(PlayClick);
        }
    }

    private void PlayNextMusic()
    {
        if (musicClips == null || musicClips.Length == 0) return;
        var candidates = new List<AudioClip>();
        foreach (var clip in musicClips)
        {
            if (clip != null && clip != clickClip) candidates.Add(clip);
        }
        if (candidates.Count == 0) return;
        var index = candidates.Count == 1 ? 0 : Random.Range(0, candidates.Count);
        if (candidates.Count > 1 && index == lastMusicIndex) index = (index + 1) % candidates.Count;
        lastMusicIndex = index;
        musicSource.clip = candidates[index];
        musicSource.Play();
    }

    private void PlayClick()
    {
        if (clickClip != null && clickSource != null) clickSource.PlayOneShot(clickClip);
    }
}
