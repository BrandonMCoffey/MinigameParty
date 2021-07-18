using MLAPI;
using UnityEngine;

namespace Assets.Scripts.Shared.State {
    public enum GameState {
        MainMenu,
        GameMenu,
        InGame,
        PostGame
    }

    public abstract class GameStateBehaviour : NetworkBehaviour {
        public virtual bool Persists => false;
        public abstract GameState ActiveState { get; }
        private static GameObject _activeStateObject;

        protected virtual void Start()
        {
            if (_activeStateObject != null) {
                if (_activeStateObject == gameObject) return;

                var previousState = _activeStateObject.GetComponent<GameStateBehaviour>();

                if (previousState.Persists && previousState.ActiveState == ActiveState) {
                    Destroy(gameObject);
                    return;
                }

                Destroy(_activeStateObject);
            }

            _activeStateObject = gameObject;
            if (Persists) {
                DontDestroyOnLoad(gameObject);
            }
        }

        protected virtual void OnDestroy()
        {
            if (!Persists) _activeStateObject = null;
        }

        protected virtual void OnApplicationQuit()
        {
            if (!isActiveAndEnabled) return;

            if (IsHost) {
                NetworkManager.Singleton.StopHost();
            } else if (IsClient) {
                NetworkManager.Singleton.StopClient();
            } else if (IsServer) {
                NetworkManager.Singleton.StopServer();
            }
        }
    }
}