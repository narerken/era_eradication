using System.Collections.Generic;
using UnityEngine;

namespace Emberfall.Audio
{
    public static class ProceduralAudio
    {
        private const int SampleRate = 44100;
        private static readonly Dictionary<string, AudioClip> _cache = new();

        private static AudioClip Build(string name, float duration, System.Func<float, float> sample)
        {
            if (_cache.TryGetValue(name, out var cached) && cached != null) return cached;

            int n = Mathf.Max(1, Mathf.RoundToInt(duration * SampleRate));
            var data = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)SampleRate;
                data[i] = Mathf.Clamp(sample(t), -1f, 1f);
            }

            var clip = AudioClip.Create(name, n, 1, SampleRate, false);
            clip.SetData(data, 0);
            _cache[name] = clip;
            return clip;
        }

        private static float Env(float t, float duration, float attack, float release)
        {
            float a = attack <= 0f ? 1f : Mathf.Clamp01(t / attack);
            float timeLeft = duration - t;
            float r = release <= 0f ? 1f : Mathf.Clamp01(timeLeft / release);
            return Mathf.Min(a, r);
        }

        public static AudioClip Jump() => Build("Jump", 0.18f, t =>
        {
            float freq = Mathf.Lerp(420f, 820f, Mathf.Clamp01(t / 0.12f));
            float env = Env(t, 0.18f, 0.005f, 0.14f);
            return Mathf.Sin(2f * Mathf.PI * freq * t) * env * 0.5f;
        });

        public static AudioClip Hit() => Build("Hit", 0.14f, t =>
        {
            float noise = Random.Range(-1f, 1f);
            float tone = Mathf.Sin(2f * Mathf.PI * 180f * t);
            float env = Env(t, 0.14f, 0.002f, 0.12f);
            return (noise * 0.6f + tone * 0.4f) * env * 0.7f;
        });

        public static AudioClip Pickup() => Build("Pickup", 0.22f, t =>
        {
            bool first = t < 0.11f;
            float freq = first ? 880f : 1320f;
            float localT = first ? t : t - 0.11f;
            float env = Env(localT, 0.11f, 0.004f, 0.09f);
            return Mathf.Sin(2f * Mathf.PI * freq * t) * env * 0.5f;
        });

        public static AudioClip EnemyWindUp() => Build("WindUp", 0.4f, t =>
        {
            float freq = Mathf.Lerp(220f, 340f, Mathf.Clamp01(t / 0.4f));
            float trem = 0.6f + 0.4f * Mathf.Sin(2f * Mathf.PI * 14f * t);
            float env = Env(t, 0.4f, 0.02f, 0.08f);
            return Mathf.Sin(2f * Mathf.PI * freq * t) * trem * env * 0.55f;
        });

        public static AudioClip Death() => Build("Death", 0.55f, t =>
        {
            float freq = Mathf.Lerp(300f, 70f, Mathf.Clamp01(t / 0.5f));
            float env = Env(t, 0.55f, 0.01f, 0.5f);
            float sine = Mathf.Sin(2f * Mathf.PI * freq * t);
            float square = Mathf.Sign(sine);
            return Mathf.Lerp(sine, square, 0.3f) * env * 0.55f;
        });

        public static AudioClip UIClick() => Build("UIClick", 0.05f, t =>
        {
            float env = Env(t, 0.05f, 0.001f, 0.045f);
            return Mathf.Sin(2f * Mathf.PI * 1200f * t) * env * 0.35f;
        });

        public static AudioClip Win() => Build("Win", 0.54f, t =>
        {
            float[] freqs = { 523f, 659f, 784f };
            int idx = Mathf.Clamp((int)(t / 0.18f), 0, 2);
            float localT = t - idx * 0.18f;
            float env = Env(localT, 0.18f, 0.01f, 0.14f);
            return Mathf.Sin(2f * Mathf.PI * freqs[idx] * t) * env * 0.5f;
        });

        public static AudioClip Lose() => Build("Lose", 0.6f, t =>
        {
            float[] freqs = { 392f, 330f, 262f };
            int idx = Mathf.Clamp((int)(t / 0.2f), 0, 2);
            float localT = t - idx * 0.2f;
            float env = Env(localT, 0.2f, 0.01f, 0.16f);
            return Mathf.Sin(2f * Mathf.PI * freqs[idx] * t) * env * 0.45f;
        });

        public static AudioClip MenuMusic() => Build("MenuMusic", 8f, t =>
        {
            float pad = Mathf.Sin(2f * Mathf.PI * 220f * t) * 0.33f
                      + Mathf.Sin(2f * Mathf.PI * 277f * t) * 0.25f
                      + Mathf.Sin(2f * Mathf.PI * 330f * t) * 0.22f;
            float sway = 0.75f + 0.25f * Mathf.Sin(2f * Mathf.PI * 0.5f * t);
            return pad * sway * 0.3f;
        });

        public static AudioClip LevelMusic() => Build("LevelMusic", 8f, t =>
        {
            float bassGate = 0.5f + 0.5f * Mathf.Sign(Mathf.Sin(2f * Mathf.PI * 2f * t));
            float bass = Mathf.Sin(2f * Mathf.PI * 110f * t) * bassGate * 0.35f;
            float pad = Mathf.Sin(2f * Mathf.PI * 220f * t) * 0.18f
                      + Mathf.Sin(2f * Mathf.PI * 330f * t) * 0.15f
                      + Mathf.Sin(2f * Mathf.PI * 440f * t) * 0.12f;
            return (bass + pad) * 0.5f;
        });
    }
}
