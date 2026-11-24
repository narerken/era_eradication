using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using Emberfall.Core;
using Emberfall.Save;

namespace Emberfall.Audio
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [SerializeField] private AudioMixer mixer;
        [SerializeField] private AudioMixerGroup musicGroup;
        [SerializeField] private AudioMixerGroup sfxGroup;
        [SerializeField] private AudioMixerSnapshot defaultSnapshot;
        [SerializeField] private AudioMixerSnapshot pausedSnapshot;

        private const int SfxPoolSize = 4;
        private AudioSource[] _sfxPool;
        private int _sfxIndex;

        private AudioSource _musicA;
        private AudioSource _musicB;
        private AudioSource _activeMusic;
        private AudioClip _currentMusicClip;
        private Coroutine _crossfadeRoutine;

        private float _musicVolume = 1f;
        private float _sfxVolume = 1f;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            _sfxPool = new AudioSource[SfxPoolSize];
            for (int i = 0; i < SfxPoolSize; i++)
            {
                var src = gameObject.AddComponent<AudioSource>();
                src.playOnAwake = false;
                src.outputAudioMixerGroup = sfxGroup;
                _sfxPool[i] = src;
            }

            _musicA = gameObject.AddComponent<AudioSource>();
            _musicB = gameObject.AddComponent<AudioSource>();
            foreach (var m in new[] { _musicA, _musicB })
            {
                m.playOnAwake = false;
                m.loop = true;
                m.outputAudioMixerGroup = musicGroup;
                m.volume = 0f;
            }
            _activeMusic = _musicA;

            SceneManager.sceneLoaded += OnSceneLoaded;
            GameEvents.LevelCompleted += PlayWin;
            GameEvents.PlayerDied += PlayLose;
        }

        private void Start()
        {
            ApplySavedVolumes();
            OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        }

        private void ApplySavedVolumes()
        {
            float musicVol = 1f, sfxVol = 1f;
            if (SaveManager.Instance != null)
            {
                var data = SaveManager.Instance.Data;
                musicVol = data.musicVolume;
                sfxVol = data.sfxVolume;
            }
            SetVolumes(musicVol, sfxVol);
        }

        // Volume is applied directly to the AudioSources (not via exposed AudioMixer
        // parameters) so it works regardless of whether the mixer's parameters have
        // been exposed in the editor. Routing through musicGroup/sfxGroup still goes
        // through the AudioMixer, and mixer.SetFloat is still attempted as a no-op-safe
        // extra in case the parameters are exposed (SetFloat returns false and does
        // nothing if the named parameter isn't exposed).
        public void SetVolumes(float music, float sfx)
        {
            _musicVolume = Mathf.Clamp01(music);
            _sfxVolume = Mathf.Clamp01(sfx);

            if (_activeMusic != null && _crossfadeRoutine == null)
            {
                _activeMusic.volume = _musicVolume;
            }

            if (mixer != null)
            {
                mixer.SetFloat("MusicVolume", LinearToDb(_musicVolume));
                mixer.SetFloat("SfxVolume", LinearToDb(_sfxVolume));
            }
        }

        private static float LinearToDb(float v) => v <= 0.0001f ? -80f : Mathf.Log10(v) * 20f;

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "MainMenu") PlayMenuMusic();
            else if (scene.name == "Game") PlayLevelMusic();
        }

        private void PlaySfx(AudioClip clip, float volume = 1f)
        {
            if (clip == null || _sfxPool == null) return;
            var src = _sfxPool[_sfxIndex];
            _sfxIndex = (_sfxIndex + 1) % _sfxPool.Length;
            src.pitch = Random.Range(0.92f, 1.08f);
            src.PlayOneShot(clip, volume * _sfxVolume);
        }

        public void PlayJump() => PlaySfx(ProceduralAudio.Jump());
        public void PlayHit() => PlaySfx(ProceduralAudio.Hit());
        public void PlayPickup() => PlaySfx(ProceduralAudio.Pickup());
        public void PlayEnemyWindUp() => PlaySfx(ProceduralAudio.EnemyWindUp());
        public void PlayDeath() => PlaySfx(ProceduralAudio.Death());
        public void PlayUIClick() => PlaySfx(ProceduralAudio.UIClick(), 0.8f);
        public void PlayWin() => PlaySfx(ProceduralAudio.Win());
        public void PlayLose() => PlaySfx(ProceduralAudio.Lose());

        public void PlayMenuMusic() => CrossfadeTo(ProceduralAudio.MenuMusic());
        public void PlayLevelMusic() => CrossfadeTo(ProceduralAudio.LevelMusic());

        private void CrossfadeTo(AudioClip clip)
        {
            if (clip == _currentMusicClip) return;
            _currentMusicClip = clip;

            var incoming = _activeMusic == _musicA ? _musicB : _musicA;
            var outgoing = _activeMusic;

            incoming.clip = clip;
            incoming.volume = 0f;
            incoming.Play();

            if (_crossfadeRoutine != null) StopCoroutine(_crossfadeRoutine);
            _crossfadeRoutine = StartCoroutine(CrossfadeRoutine(outgoing, incoming));
            _activeMusic = incoming;
        }

        private IEnumerator CrossfadeRoutine(AudioSource outgoing, AudioSource incoming)
        {
            const float duration = 1.2f;
            bool outgoingWasPlaying = outgoing.isPlaying;
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float f = Mathf.Clamp01(t / duration);
                incoming.volume = f * _musicVolume;
                if (outgoingWasPlaying) outgoing.volume = (1f - f) * _musicVolume;
                yield return null;
            }
            incoming.volume = _musicVolume;
            if (outgoingWasPlaying)
            {
                outgoing.volume = 0f;
                outgoing.Stop();
            }
            _crossfadeRoutine = null;
        }

        public void SetPaused(bool paused)
        {
            if (mixer == null) return;
            if (paused && pausedSnapshot != null) pausedSnapshot.TransitionTo(0.25f);
            else if (!paused && defaultSnapshot != null) defaultSnapshot.TransitionTo(0.25f);
        }
    }
}
