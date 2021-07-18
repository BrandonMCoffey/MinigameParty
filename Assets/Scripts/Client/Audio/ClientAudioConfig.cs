using UnityEngine;
using UnityEngine.Audio;

namespace Assets.Scripts.Client.Audio {
    [CreateAssetMenu]
    public class ClientAudioConfig : ScriptableObject {
        [Header("Songs")]
        public AudioClip ThemeMusic;

        [Header("Audio Mixer Config")]
        [SerializeField] private AudioMixer _mixer = null;
        [SerializeField] private string _mixerVarMainVolume = "MasterVolume";
        [SerializeField] private string _mixerVarMusicVolume = "MusicVolume";

        public void Configure()
        {
            if (_mixer == null) return;
            _mixer.SetFloat(_mixerVarMainVolume, GetVolumeInDecibels(ClientPrefs.GetMasterVolume()));
            _mixer.SetFloat(_mixerVarMusicVolume, GetVolumeInDecibels(ClientPrefs.GetMusicVolume()));
        }

        private static float GetVolumeInDecibels(float volume)
        {
            if (volume <= 0) volume = 0.0001f;
            return Mathf.Log10(volume) * 20;
        }
    }
}