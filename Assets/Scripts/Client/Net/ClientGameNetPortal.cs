using System;
using Assets.Scripts.Shared.Net;
using MLAPI;
using MLAPI.Transports.UNET;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.Client.Net {
    [RequireComponent(typeof(GameNetPortal))]
    public class ClientGameNetPortal : MonoBehaviour {
        private GameNetPortal _portal;

        private const int TimeoutDuration = 10;
        public event Action<ConnectStatus> ConnectFinished;
        public event Action NetworkTimedOut;
        public DisconnectReason DisconnectReason { get; } = new DisconnectReason();

        private void Start()
        {
            _portal = GetComponent<GameNetPortal>();

            _portal.NetworkReadied += OnNetworkReady;
            _portal.ConnectFinished += OnConnectFinished;
            _portal.DisconnectReasonReceived += OnDisconnectReasonReceived;
            _portal.NetManager.OnClientDisconnectCallback += OnDisconnectOrTimeout;
        }

        private void OnNetworkReady()
        {
            if (!_portal.NetManager.IsClient) {
                enabled = false;
                return;
            }

            SceneManager.sceneLoaded += OnSceneLoaded;
            if (!_portal.NetManager.IsHost) _portal.UserDisconnectRequested += OnUserDisconnectRequest;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            _portal.ClientToServerSceneChanged(SceneManager.GetActiveScene().buildIndex);
        }

        private void OnUserDisconnectRequest()
        {
            if (!_portal.NetManager.IsClient) return;
            DisconnectReason.SetDisconnectReason(ConnectStatus.UserRequestedDisconnect);
            _portal.NetManager.StopClient();
        }

        private void OnConnectFinished(ConnectStatus status)
        {
            Debug.Log("Connection finished with status: " + status);

            if (status != ConnectStatus.Success) {
                DisconnectReason.SetDisconnectReason(status);
            }

            ConnectFinished?.Invoke(status);
        }

        private void OnDisconnectReasonReceived(ConnectStatus status)
        {
            DisconnectReason.SetDisconnectReason(status);
        }

        private void OnDisconnectOrTimeout(ulong clientId)
        {
            if (NetworkManager.Singleton.IsConnectedClient || NetworkManager.Singleton.IsHost) return;

            SceneManager.sceneLoaded -= OnSceneLoaded;
            _portal.UserDisconnectRequested -= OnUserDisconnectRequest;

            if (SceneManager.GetActiveScene().name != "MainMenu") {
                NetworkManager.Singleton.Shutdown();
                if (!DisconnectReason.HasTransitionReason) {
                    DisconnectReason.SetDisconnectReason(ConnectStatus.GenericDisconnect);
                }
                SceneManager.LoadScene("MainMenu");
            } else {
                NetworkTimedOut?.Invoke();
            }
        }

        public static void StartClient(GameNetPortal portal, string ipAddress, int port)
        {
            var chosenTransport = NetworkManager.Singleton.NetworkConfig.NetworkTransport;

            switch (chosenTransport) {
                case UNetTransport unetTransport:
                    unetTransport.ConnectAddress = ipAddress;
                    unetTransport.ConnectPort = port;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(chosenTransport));
            }

            ConnectClient(portal);
        }

        private static void ConnectClient(GameNetPortal portal)
        {
            var clientGuid = ClientPrefs.GetClientGuid();
            var payload = JsonUtility.ToJson(new ConnectionPayload()
            {
                ClientGuid = clientGuid,
                ClientScene = SceneManager.GetActiveScene().buildIndex,
                PlayerName = portal.PlayerName
            });

            var payloadBytes = System.Text.Encoding.UTF8.GetBytes(payload);

            portal.NetManager.NetworkConfig.ConnectionData = payloadBytes;
            portal.NetManager.NetworkConfig.ClientConnectionBufferTimeout = TimeoutDuration;

            portal.NetManager.StartClient();
        }

        private void OnDestroy()
        {
            if (_portal == null) return;
            _portal.NetworkReadied -= OnNetworkReady;
            _portal.ConnectFinished -= OnConnectFinished;
            _portal.DisconnectReasonReceived -= OnDisconnectReasonReceived;

            if (_portal.NetManager == null) return;
            _portal.NetManager.OnClientDisconnectCallback -= OnDisconnectOrTimeout;
        }
    }
}