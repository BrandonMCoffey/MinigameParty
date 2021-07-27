using Assets.Scripts.Server.Net;
using Assets.Scripts.Shared.State;
using UnityEngine;
using MLAPI;

namespace Assets.Scripts.Server.State {
    public class ServerGameMenuState : GameStateBehaviour {
        public override GameState ActiveState => GameState.GameMenu;

        private ServerGameNetPortal _serverNetPortal;

        private void Awake()
        {
            _serverNetPortal = GameObject.FindGameObjectWithTag("GameNetPortal").GetComponent<ServerGameNetPortal>();
        }

        public override void NetworkStart()
        {
            base.NetworkStart();
            if (!IsServer) {
                enabled = false;
            } else {
                NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;
            }
        }

        private void OnClientConnected(ulong clientId)
        {
            // Tell clients to call AddPlayer(clientId) on ActivePlayersUI
        }

        private void OnClientDisconnectCallback(ulong clientId)
        {
            // Tell clients to call RemovePlayer(clientId) on ActivePlayersUI
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (NetworkManager.Singleton) {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallback;
            }
        }
    }
}