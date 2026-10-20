using System.Collections.Generic;
using UnityEngine;

namespace NeonSurvivor
{
    /// <summary>Música em loop + efeitos sonoros com pool de AudioSources.</summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager I { get; private set; }

        [Range(0, 1)] public float musicVolume = 0.4f;
        [Range(0, 1)] public float sfxVolume = 0.8f;

        AudioSource music;
        readonly AudioSource[] pool = new AudioSource[16];
        int next;
        readonly Dictionary<string, AudioClip> clips = new();
        readonly Dictionary<string, float> lastPlayed = new();

        void Awake()
        {
            I = this;
            music = gameObject.AddComponent<AudioSource>();
            music.clip = Resources.Load<AudioClip>("Audio/music");
            music.loop = true;
            music.volume = musicVolume;
            music.playOnAwake = false;
            for (int i = 0; i < pool.Length; i++)
            {
                pool[i] = gameObject.AddComponent<AudioSource>();
                pool[i].playOnAwake = false;
            }
        }

        public void PlayMusic()
        {
            if (!music.isPlaying) music.Play();
        }

        public void SetMusicPitch(float pitch) => music.pitch = pitch;

        public static void Play(string name, float volume = 1f, float pitchJitter = 0.06f)
        {
            if (I != null) I.PlayClip(name, volume, pitchJitter);
        }

        void PlayClip(string name, float volume, float pitchJitter)
        {
            // evita empilhar o mesmo som dezenas de vezes no mesmo instante
            if (lastPlayed.TryGetValue(name, out var t) && Time.unscaledTime - t < 0.045f) return;
            lastPlayed[name] = Time.unscaledTime;

            if (!clips.TryGetValue(name, out var clip))
            {
                clip = Resources.Load<AudioClip>("Audio/" + name);
                clips[name] = clip;
            }
            if (clip == null) return;

            var src = pool[next];
            next = (next + 1) % pool.Length;
            src.pitch = 1f + Random.Range(-pitchJitter, pitchJitter);
            src.PlayOneShot(clip, volume * sfxVolume);
        }
    }
}
