"""Sintetiza música e efeitos sonoros do Neon Survivor em Assets/Resources/Audio.

Uso: python3 Tools/gen_audio.py
"""
import os
import wave

import numpy as np

SR = 22050
OUT = os.path.join(os.path.dirname(__file__), "..", "Assets", "Resources", "Audio")
rng = np.random.default_rng(1)


def t_of(dur):
    return np.arange(int(SR * dur)) / SR


def sweep(f0, f1, dur):
    """Fase de uma varredura de frequência exponencial."""
    f = f0 * (f1 / f0) ** (t_of(dur) / dur)
    return 2 * np.pi * np.cumsum(f) / SR


def square(phase, duty=0.5):
    return np.where((phase / (2 * np.pi)) % 1.0 < duty, 1.0, -1.0)


def saw(phase):
    return 2.0 * ((phase / (2 * np.pi)) % 1.0) - 1.0


def env(n, attack=0.005, release=None, curve=3.0):
    t = np.arange(n) / SR
    a = np.clip(t / max(attack, 1e-4), 0, 1)
    dur = n / SR
    d = (1 - t / dur) ** curve if release is None else np.clip((dur - t) / release, 0, 1)
    return a * d


def lowpass(x, cutoff):
    a = np.exp(-2 * np.pi * cutoff / SR)
    y = np.zeros_like(x)
    acc = 0.0
    for i, v in enumerate(x):
        acc = (1 - a) * v + a * acc
        y[i] = acc
    return y


def noise(dur):
    return rng.uniform(-1, 1, int(SR * dur))


def write(name, x, peak=0.8):
    x = np.asarray(x, dtype=np.float64)
    m = np.max(np.abs(x)) or 1.0
    data = (x / m * peak * 32767).astype(np.int16)
    with wave.open(os.path.join(OUT, name + ".wav"), "wb") as w:
        w.setnchannels(1)
        w.setsampwidth(2)
        w.setframerate(SR)
        w.writeframes(data.tobytes())


# ---------------------------------------------------------------- SFX

def sfx():
    d = 0.09
    write("shoot", square(sweep(1400, 500, d), 0.25) * env(int(SR * d)), 0.45)
    d = 0.12
    write("enemy_shoot", square(sweep(500, 250, d), 0.5) * env(int(SR * d)), 0.4)
    d = 0.06
    write("hit", lowpass(noise(d), 4000) * env(int(SR * d), curve=2), 0.5)
    d = 0.25
    x = lowpass(noise(d), 2500) * env(int(SR * d), curve=2) + 0.6 * square(sweep(400, 60, d)) * env(int(SR * d))
    write("enemy_die", x, 0.55)
    d = 0.07
    write("xp", np.sin(sweep(1300, 2000, d)) * env(int(SR * d), curve=1.5), 0.35)
    notes = [523.25, 659.25, 783.99, 1046.5]
    x = np.concatenate([square(2 * np.pi * f * t_of(0.08), 0.25) * env(int(SR * 0.08), curve=1) for f in notes])
    x = np.concatenate([x, square(2 * np.pi * 1046.5 * t_of(0.25), 0.25) * env(int(SR * 0.25))])
    write("levelup", x, 0.55)
    d = 0.22
    n = noise(d)
    write("dash", (lowpass(n, 6000) - lowpass(n, 800)) * env(int(SR * d), attack=0.03, curve=2), 0.6)
    d = 0.3
    write("player_hurt", square(sweep(260, 70, d)) * env(int(SR * d), curve=1.5), 0.6)
    d = 0.6
    x = np.sin(sweep(160, 35, d)) * env(int(SR * d), curve=2) + 0.5 * lowpass(noise(d), 1200) * env(int(SR * d), curve=4)
    write("emp", x, 0.8)
    d = 0.05
    write("blade", lowpass(noise(d), 9000) * env(int(SR * d), curve=1), 0.3)
    # sirene do boss
    t = t_of(1.6)
    f = 600 + 250 * np.sin(2 * np.pi * 1.25 * t)
    write("boss_alert", square(2 * np.pi * np.cumsum(f) / SR, 0.5) * env(len(t), attack=0.05, release=0.2), 0.5)
    d = 0.04
    write("select", square(2 * np.pi * 900 * t_of(d), 0.5) * env(int(SR * d)), 0.3)
    d = 0.08
    write("pickup_health", np.sin(sweep(600, 1200, d)) * env(int(SR * d), curve=1), 0.45)
    # vitória e derrota
    seq = [(523.25, 0.15), (659.25, 0.15), (783.99, 0.15), (1046.5, 0.6)]
    write("victory", np.concatenate([square(2 * np.pi * f * t_of(dd), 0.25) * env(int(SR * dd), curve=1) for f, dd in seq]), 0.55)
    seq = [(392.0, 0.25), (311.13, 0.25), (261.63, 0.25), (196.0, 0.8)]
    write("gameover", np.concatenate([saw(2 * np.pi * f * t_of(dd)) * env(int(SR * dd), curve=1) for f, dd in seq]), 0.5)


# ---------------------------------------------------------------- música synthwave

def music():
    bpm = 112
    beat = 60 / bpm
    bars = 16
    total = int(SR * beat * 4 * bars)
    mix = np.zeros(total)

    def place(sig, start_s, gain=1.0):
        i = int(start_s * SR)
        j = min(total, i + len(sig))
        if i < total:
            mix[i:j] += gain * sig[: j - i]

    def note(f, dur, kind="saw", curve=2.0):
        ph = 2 * np.pi * f * t_of(dur)
        if kind == "saw":
            s = 0.5 * (saw(ph) + saw(ph * 1.006))
        elif kind == "sq":
            s = square(ph, 0.25)
        else:
            s = np.sin(ph)
        return s * env(len(ph), curve=curve)

    hz = lambda m: 440.0 * 2 ** ((m - 69) / 12)
    # Am - F - C - G (raízes em MIDI)
    prog = [(57, [69, 72, 76]), (53, [65, 69, 72]), (48, [64, 67, 72]), (55, [67, 71, 74])]
    kick = np.sin(sweep(150, 40, 0.18)) * env(int(SR * 0.18), curve=2)
    snare = lowpass(noise(0.16), 5000) * env(int(SR * 0.16), curve=3)
    hat = (noise(0.04) - lowpass(noise(0.04), 6000)) * env(int(SR * 0.04), curve=3)
    lead = [76, 74, 72, 74, 76, 79, 76, 74, 72, 71, 72, 74, 72, 69, 67, 69]

    for bar in range(bars):
        root, chord = prog[bar % 4]
        bs = bar * 4 * beat
        section = bar // 4  # 0 intro, 1-3 vão adicionando camadas
        for e in range(8):  # baixo em colcheias
            place(note(hz(root - 12), beat / 2 * 0.9, "saw", 1.5), bs + e * beat / 2, 0.35)
        place(lowpass(note(hz(chord[0]), beat * 4, "saw", 0.5) + note(hz(chord[1]), beat * 4, "saw", 0.5)
                      + note(hz(chord[2]), beat * 4, "saw", 0.5), 1800), bs, 0.12)
        if section >= 1:
            arp = chord + [chord[1] + 12]
            for s16 in range(16):
                place(note(hz(arp[s16 % 4] + 12), beat / 4 * 0.8, "sq", 3), bs + s16 * beat / 4, 0.07)
        for b in range(4):
            place(kick, bs + b * beat, 0.7)
            if b % 2 == 1:
                place(snare, bs + b * beat, 0.35 if section >= 1 else 0.2)
            place(hat, bs + b * beat + beat / 2, 0.18)
        if section >= 2:
            for q in range(4):
                m = lead[(bar * 4 + q) % len(lead)]
                place(note(hz(m), beat * 0.95, "sq", 1.2), bs + q * beat, 0.11)
    write("music", mix, 0.7)


if __name__ == "__main__":
    os.makedirs(OUT, exist_ok=True)
    sfx()
    music()
    print("audio ok:", sorted(os.listdir(OUT)))
