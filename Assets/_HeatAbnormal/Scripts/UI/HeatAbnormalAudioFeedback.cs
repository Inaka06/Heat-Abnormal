using UnityEngine;

public sealed class HeatAbnormalAudioFeedback : MonoBehaviour
{
    private AudioSource source;
    private AudioClip successClip;
    private AudioClip warningClip;
    private AudioClip failureClip;

    private void Awake()
    {
        source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 0f;
        successClip = CreateTone("HeatAbnormalSuccess", 660f, 0.12f);
        warningClip = CreateTone("HeatAbnormalWarning", 220f, 0.18f);
        failureClip = CreateTone("HeatAbnormalFailure", 140f, 0.20f);
    }

    public void PlaySuccess() { if (source != null) source.PlayOneShot(successClip); }
    public void PlayWarning() { if (source != null) source.PlayOneShot(warningClip); }
    public void PlayFailure() { if (source != null) source.PlayOneShot(failureClip); }

    private static AudioClip CreateTone(string clipName, float frequency, float duration)
    {
        const int sampleRate = 44100;
        var sampleCount = Mathf.CeilToInt(sampleRate * duration);
        var clip = AudioClip.Create(clipName, sampleCount, 1, sampleRate, false);
        var samples = new float[sampleCount];
        for (var i = 0; i < sampleCount; i++)
        {
            var envelope = 1f - (float)i / sampleCount;
            samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * i / sampleRate) * envelope * 0.12f;
        }
        clip.SetData(samples, 0);
        return clip;
    }
}
