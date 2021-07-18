using UnityEngine;

namespace Assets.Scripts.Client.Audio {
    [RequireComponent(typeof(AudioSource))]
    public class ClientMusicPlayer : MonoBehaviour {
        public ClientAudioConfig AudioConfig = null;

        private AudioSource _source;
        public static ClientMusicPlayer Instance;

        private void Awake()
        {
            if (Instance != null && Instance != this) {
                Destroy(gameObject);
            } else {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                _source = GetComponent<AudioSource>();
            }
        }

        public void PlayTrack(AudioClip clip, bool looping = true, bool restart = false)
        {
            if (clip == null) return;
            if (_source.isPlaying) {
                if (!restart && _source.clip == clip) return;
                _source.Stop();
            }
            _source.clip = clip;
            _source.loop = looping;
            _source.time = 0;
            _source.Play();
        }
    }
}