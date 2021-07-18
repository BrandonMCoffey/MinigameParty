using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Client.Audio {
    public class AudioSettingsPanel : MonoBehaviour {
        [SerializeField] private Slider _masterVolumeSlider = null;
        [SerializeField] private Slider _musicVolumeSlider = null;
        [SerializeField] private ClientAudioConfig _mixerConfig = null;

        private void OnEnable()
        {
            if (_masterVolumeSlider != null) {
                _masterVolumeSlider.value = ClientPrefs.GetMasterVolume();
                _masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeSliderChanged);
            }

            if (_musicVolumeSlider != null) {
                _musicVolumeSlider.value = ClientPrefs.GetMusicVolume();
                _musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeSliderChanged);
            }

            if (_mixerConfig != null) _mixerConfig.Configure();
        }

        private void OnDisable()
        {
            if (_masterVolumeSlider != null) _masterVolumeSlider.onValueChanged.RemoveListener(OnMasterVolumeSliderChanged);
            if (_musicVolumeSlider != null) _musicVolumeSlider.onValueChanged.RemoveListener(OnMusicVolumeSliderChanged);
        }

        private void OnMasterVolumeSliderChanged(float value)
        {
            ClientPrefs.SetMasterVolume(value);
            if (_mixerConfig != null) _mixerConfig.Configure();
        }

        private void OnMusicVolumeSliderChanged(float value)
        {
            ClientPrefs.SetMusicVolume(value);
            if (_mixerConfig != null) _mixerConfig.Configure();
        }
    }
}