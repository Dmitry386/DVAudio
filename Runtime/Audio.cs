using DVUnityUtilities;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace DVAudio
{
    public class Audio : SingletonAuto<Audio>
    {
        private Transform _audioSourceParent;

        private AudioSource _regular2D_AudioSource;
        private AudioSource _ambientMusic;

        private AudioClip[] _ambientClips;
        private AudioClip _currentAmbient;

        protected override void OnRegistered()
        {
            _regular2D_AudioSource = Util.CreateGameObjectWithComponent<AudioSource>();
            _ambientMusic = Util.CreateGameObjectWithComponent<AudioSource>();
            _regular2D_AudioSource.hideFlags = HideFlags.HideInHierarchy;

            _audioSourceParent = _regular2D_AudioSource.transform;
            GameObject.DontDestroyOnLoad(_regular2D_AudioSource);

            _regular2D_AudioSource.dopplerLevel = 0;
        }

        public static void SetAmbientMusic(AudioClip[] clips)
        {
            Instance.SetAmbient(clips);
        }

        public static void StopAmbientMusic()
        {
            Instance._ambientClips = null;
        }

        private static void ShowDebugNullableAudioclip()
        {
#if UNITY_EDITOR
            Debug.LogWarning($"Nullable audioclip.");
#endif
        }

        public static void Play2D(AudioClip clip)
        {
            if (clip == null)
            {
                ShowDebugNullableAudioclip();
                return;
            }

            Instance._regular2D_AudioSource.PlayOneShot(clip);
        }

        public static void Play3D(AudioClip clip, Transform t)
        {
            Play3D(clip, t.position);
        }

        public static void Play3D(AudioClip clip, in Vector3 pos)
        {
            if (clip == null)
            {
                ShowDebugNullableAudioclip();
                return;
            }

            var s = Util.CreateGameObjectWithComponent<AudioSource>(Instance._audioSourceParent);
            s.hideFlags = HideFlags.HideInHierarchy;

            s.transform.position = pos;
            Apply3DSettings(s);

            s.PlayOneShot(clip);
            GameObject.Destroy(s.gameObject, clip.length);
        }

        public static AudioSource Play2DLooped(AudioClip clip)
        {
            if (clip == null)
            {
                ShowDebugNullableAudioclip();
                return null;
            }

            var s = Util.CreateGameObjectWithComponent<AudioSource>(Instance._audioSourceParent);
            s.hideFlags = HideFlags.HideInHierarchy;

            s.loop = true;
            s.clip = clip;
            s.Play();

            return s;
        }

        public static AudioSource Play3DLooped(AudioClip clip, in Vector3 pos)
        {
            if (clip == null)
            {
                ShowDebugNullableAudioclip();
                return null;
            }

            var s = Util.CreateGameObjectWithComponent<AudioSource>(Instance._audioSourceParent);
            s.hideFlags = HideFlags.HideInHierarchy;

            s.transform.position = pos;
            Apply3DSettings(s);

            s.loop = true;
            s.clip = clip;
            s.Play();

            return s;
        }

        public static AudioSource Play3DLooped(AudioClip clip, in Transform t)
        {
            if (clip == null)
            {
                ShowDebugNullableAudioclip();
                return null;
            }
            var p = Play3DLooped(clip, t.position);

            if (p != null)
            {
                p.transform.SetParent(t, true);
            }

            return p;
        }

        private static void Apply3DSettings(AudioSource s)
        {
            s.spatialBlend = 1f;
            s.dopplerLevel = 0;

            s.rolloffMode = AudioRolloffMode.Linear;
            s.maxDistance = 20f;
        }

        /// <summary>
        ///  load from Resources\\Audio\\
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static AudioClip Load(string path)
        {
            return Resources.Load<AudioClip>($"Audio\\{path}");
        }

        private void SetAmbient(AudioClip[] clips)
        {
            _ambientClips = clips;
            StartSwitchAmbientMusicRndControl();
        }

        private async void StartSwitchAmbientMusicRndControl()
        {
            while (_ambientClips != null && _ambientMusic != null)
            {
                _ambientMusic.clip = _ambientClips.Where(x => x != _currentAmbient).ToArray().GetRandomElement();
                _ambientMusic.loop = true;
                _ambientMusic.Play();

                await Task.Delay(Mathf.FloorToInt((_ambientMusic.clip.length * 1000) - ((_ambientMusic.time * 1000) + 1)));
            }
        }

        protected override void OnDisposed()
        {
            GameObject.Destroy(_regular2D_AudioSource.gameObject);
        }
    }
}