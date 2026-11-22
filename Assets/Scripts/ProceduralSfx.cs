using System.Collections.Generic;
using UnityEngine;

public static class ProceduralSfx
{
    public enum Kind { Success, Error, Unplug, Click, Ready }

    const int SampleRate = 44100;
    static readonly Dictionary<Kind, AudioClip> Cache = new Dictionary<Kind, AudioClip>();

    public static void Play(Kind kind, Vector3 position, float volume = 0.6f)
    {
        if (!Application.isPlaying) return;
        AudioSource.PlayClipAtPoint(Get(kind), position, volume);
    }

    public static AudioClip Get(Kind kind)
    {
        if (Cache.TryGetValue(kind, out var clip) && clip != null) return clip;
        clip = kind switch
        {
            Kind.Success => Build("sfx_success", 0.32f, t => Tone(t < 0.13f ? 660f : 990f, t) * Env(t < 0.13f ? t : t - 0.13f, 0.005f, 0.16f)),
            Kind.Error => Build("sfx_error", 0.38f, t => Square(140f, t) * 0.5f * (0.6f + 0.4f * Mathf.Sin(t * 60f)) * Env(t, 0.005f, 0.3f)),
            Kind.Unplug => Build("sfx_unplug", 0.22f, t => Tone(Mathf.Lerp(620f, 240f, t / 0.22f), t) * Env(t, 0.004f, 0.2f)),
            Kind.Click => Build("sfx_click", 0.05f, t => Tone(2200f, t) * Env(t, 0.001f, 0.04f)),
            _ => Build("sfx_ready", 0.7f, t => (Tone(523f, t) + Tone(659f, t) * Step(t, 0.12f) + Tone(784f, t) * Step(t, 0.24f)) * 0.4f * Env(t, 0.01f, 0.65f)),
        };
        Cache[kind] = clip;
        return clip;
    }

    static float Tone(float freq, float t) => Mathf.Sin(2f * Mathf.PI * freq * t);
    static float Square(float freq, float t) => Mathf.Sign(Mathf.Sin(2f * Mathf.PI * freq * t));
    static float Step(float t, float start) => t >= start ? 1f : 0f;

    static float Env(float t, float attack, float length)
    {
        if (t < 0f) return 0f;
        if (t < attack) return t / attack;
        return Mathf.Exp(-4f * (t - attack) / Mathf.Max(0.001f, length));
    }

    static AudioClip Build(string name, float duration, System.Func<float, float> wave)
    {
        int n = Mathf.CeilToInt(duration * SampleRate);
        var data = new float[n];
        for (int i = 0; i < n; i++)
            data[i] = Mathf.Clamp(wave(i / (float)SampleRate), -1f, 1f) * 0.8f;
        var clip = AudioClip.Create(name, n, 1, SampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
