using UnityEngine;

/// <summary>
/// All game audio is synthesised at runtime, keeping the project's
/// "no imported assets" rule just like the procedural art. The clips are
/// deliberately short and dry: coin clacks, rail thuds and a soft room pad.
/// </summary>
public static class ProceduralAudio
{
    private const int Rate = 44100;
    private const float TwoPi = Mathf.PI * 2f;

    private static AudioClip Build(string name, float seconds, System.Func<float, float> sample)
    {
        return Build(name, seconds, Rate, sample);
    }

    private static AudioClip Build(string name, float seconds, int rate,
        System.Func<float, float> sample)
    {
        int count = Mathf.Max(16, Mathf.RoundToInt(seconds * rate));
        float[] data = new float[count];
        float peak = 0.0001f;

        for (int i = 0; i < count; i++)
        {
            float v = sample(i / (float)rate);
            data[i] = v;
            float magnitude = Mathf.Abs(v);
            if (magnitude > peak) peak = magnitude;
        }

        // Normalise with a little headroom so stacking several hits never clips.
        float gain = 0.92f / peak;
        for (int i = 0; i < count; i++)
            data[i] *= gain;

        AudioClip clip = AudioClip.Create(name, count, 1, rate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private static float Lowpass(float last, float input, float amount)
    {
        return last + (input - last) * amount;
    }

    /// <summary>Hard acrylic striker on a coin: bright metallic tick.</summary>
    public static AudioClip CoinClack()
    {
        System.Random rng = new System.Random(17);
        float[] lp = new float[1];

        return Build("CoinClack", 0.085f, t =>
        {
            float noise = (float)(rng.NextDouble() * 2.0 - 1.0);
            lp[0] = Lowpass(lp[0], noise, 0.55f);

            float env = Mathf.Exp(-t * 55f);
            float body = Mathf.Sin(TwoPi * 2450f * t) * 0.55f
                       + Mathf.Sin(TwoPi * 3370f * t) * 0.26f;
            float click = lp[0] * Mathf.Exp(-t * 260f) * 0.75f;

            return body * env + click;
        });
    }

    /// <summary>Striker or coin hitting the wooden rail.</summary>
    public static AudioClip RailThud()
    {
        System.Random rng = new System.Random(41);
        float[] lp = new float[1];

        return Build("RailThud", 0.16f, t =>
        {
            float noise = (float)(rng.NextDouble() * 2.0 - 1.0);
            lp[0] = Lowpass(lp[0], noise, 0.10f);

            float env = Mathf.Exp(-t * 24f);
            float tone = Mathf.Sin(TwoPi * 128f * t) * 0.50f
                       + Mathf.Sin(TwoPi * 82f * t) * 0.34f;
            float knock = lp[0] * Mathf.Exp(-t * 55f) * 0.65f;

            return tone * env + knock;
        });
    }

    /// <summary>A piece dropping through a pocket: descending thump.</summary>
    public static AudioClip PocketDrop()
    {
        System.Random rng = new System.Random(73);
        float[] lp = new float[1];

        return Build("PocketDrop", 0.34f, t =>
        {
            float noise = (float)(rng.NextDouble() * 2.0 - 1.0);
            lp[0] = Lowpass(lp[0], noise, 0.06f);

            // Pitch glides down as the piece falls away from the camera.
            float freq = Mathf.Lerp(330f, 86f, Mathf.Clamp01(t / 0.30f));
            float env = Mathf.Clamp01(t / 0.004f) * Mathf.Exp(-t * 9f);
            float tone = Mathf.Sin(TwoPi * freq * t) * 0.7f;
            float thump = lp[0] * Mathf.Exp(-t * 26f) * 0.5f;

            return tone * env + thump;
        });
    }

    /// <summary>The flick of the striker leaving the finger.</summary>
    public static AudioClip Strike()
    {
        System.Random rng = new System.Random(97);
        float[] lp = new float[1];

        return Build("Strike", 0.11f, t =>
        {
            float noise = (float)(rng.NextDouble() * 2.0 - 1.0);
            lp[0] = Lowpass(lp[0], noise, 0.22f);

            float env = Mathf.Clamp01(t / 0.004f) * Mathf.Exp(-t * 46f);
            float tick = Mathf.Sin(TwoPi * 880f * t) * Mathf.Exp(-t * 70f) * 0.35f;
            float whoosh = lp[0] * env * 1.1f;

            return whoosh + tick;
        });
    }

    /// <summary>Two-note buzzer for a foul.</summary>
    public static AudioClip FoulBuzz()
    {
        return Build("FoulBuzz", 0.52f, t =>
        {
            float first = t < 0.24f ? 1f : 0f;
            float second = t >= 0.24f ? 1f : 0f;
            float freq = first * 233f + second * 175f;
            float localT = second > 0f ? t - 0.24f : t;

            float env = Mathf.Clamp01(localT / 0.008f) * Mathf.Exp(-localT * 5.5f);
            // A touch of odd harmonics reads as a referee whistle/buzzer.
            float tone = Mathf.Sin(TwoPi * freq * t) * 0.62f
                       + Mathf.Sin(TwoPi * freq * 3f * t) * 0.20f;

            return tone * env;
        });
    }

    /// <summary>Soft UI tap.</summary>
    public static AudioClip UiClick()
    {
        return Build("UiClick", 0.05f, t =>
        {
            float env = Mathf.Clamp01(t / 0.002f) * Mathf.Exp(-t * 95f);
            return Mathf.Sin(TwoPi * 1550f * t) * env;
        });
    }

    /// <summary>Rising arpeggio for a win.</summary>
    public static AudioClip VictorySting()
    {
        float[] notes = { 523.25f, 659.25f, 783.99f, 1046.50f };
        float[] starts = { 0f, 0.105f, 0.21f, 0.325f };

        return Build("VictorySting", 1.25f, t =>
        {
            float sum = 0f;
            for (int i = 0; i < notes.Length; i++)
            {
                float local = t - starts[i];
                if (local < 0f) continue;

                float env = Mathf.Clamp01(local / 0.012f) * Mathf.Exp(-local * 3.0f);
                float f = notes[i];
                sum += (Mathf.Sin(TwoPi * f * local) * 0.55f
                      + Mathf.Sin(TwoPi * f * 2f * local) * 0.20f
                      + Mathf.Sin(TwoPi * f * 3f * local) * 0.08f) * env;
            }
            return sum * 0.38f;
        });
    }

    /// <summary>Falling three-note phrase for a loss.</summary>
    public static AudioClip DefeatSting()
    {
        float[] notes = { 392.00f, 329.63f, 261.63f };
        float[] starts = { 0f, 0.17f, 0.35f };

        return Build("DefeatSting", 1.15f, t =>
        {
            float sum = 0f;
            for (int i = 0; i < notes.Length; i++)
            {
                float local = t - starts[i];
                if (local < 0f) continue;

                float env = Mathf.Clamp01(local / 0.02f) * Mathf.Exp(-local * 2.5f);
                float f = notes[i];
                sum += (Mathf.Sin(TwoPi * f * local) * 0.55f
                      + Mathf.Sin(TwoPi * f * 2f * local) * 0.14f) * env;
            }
            return sum * 0.36f;
        });
    }

    /// <summary>
    /// Seamless 16 s warm pad for the menu and match background. Every partial
    /// completes whole cycles over the loop, so there is no seam click.
    /// </summary>
    public static AudioClip AmbientLoop()
    {
        const int rate = 22050;
        const float duration = 16f;

        return Build("AmbientLoop", duration, rate, t =>
        {
            float swellA = 0.55f + 0.45f * Mathf.Sin(TwoPi * (1f / 16f) * t);
            float swellB = 0.55f + 0.45f * Mathf.Sin(TwoPi * (2f / 16f) * t + 1.7f);
            float swellC = 0.55f + 0.45f * Mathf.Sin(TwoPi * (3f / 16f) * t + 3.1f);

            // Frequencies are whole-cycle multiples of 1/16 Hz.
            float pad = Mathf.Sin(TwoPi * 130.8125f * t) * 0.30f * swellA
                      + Mathf.Sin(TwoPi * 196.0000f * t) * 0.21f * swellB
                      + Mathf.Sin(TwoPi * 261.6250f * t) * 0.15f * swellC
                      + Mathf.Sin(TwoPi * 329.6250f * t) * 0.10f * swellA;

            return pad * 0.55f;
        });
    }
}
