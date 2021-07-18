using System;
using UnityEngine;

namespace Assets.Scripts.Client.Audio {
    public enum MusicOptions {
        None,
        Theme
    }

    public class PlayMusic : MonoBehaviour {
        [Header("Music Choice")]
        [SerializeField] private MusicOptions _musicToPlayer = MusicOptions.None;
        [SerializeField] private AudioClip _overrideMusic = null;

        [Header("Music Settings")]
        [SerializeField] private bool _loopMusic = true;
        [SerializeField] private bool _forceRestart = false;

        private void Start()
        {
            ClientMusicPlayer player = ClientMusicPlayer.Instance;
            if (player == null) return;
            if (_overrideMusic != null) {
                player.PlayTrack(_overrideMusic, _loopMusic, _forceRestart);
            } else {
                switch (_musicToPlayer) {
                    case MusicOptions.None:
                        break;
                    case MusicOptions.Theme:
                        player.PlayTrack(player.AudioConfig.ThemeMusic, _loopMusic, _forceRestart);
                        break;
                }
            }
        }
    }
}