using System;
using MLAPI;
using MLAPI.Serialization.Pooled;
using MLAPI.Transports;
using UnityEngine;

namespace Assets.Scripts.Shared.Net {
    public enum ConnectStatus {
        Undefined,
        Success,                 //client successfully connected. This may also be a successful reconnect.
        ServerFull,              //can't join, server is already at capacity.
        LoggedInAgain,           //logged in on a separate client, causing this one to be kicked out.
        UserRequestedDisconnect, //Intentional Disconnect triggered by the user. 
        GenericDisconnect,       //server disconnected, but no specific reason given.
    }

    [Serializable]
    public class ConnectionPayload {
        public string ClientGuid;
        public int ClientScene = -1;
        public string PlayerName;
    }

    // Why is there a C2S_ConnectFinished event here? How is that different from the "ApprovalCheck" logic that MLAPI optionally runs when establishing a new client connection?
    // - MLAPI's ApprovalCheck logic doesn't offer a way to return rich data. We need to know certain things directly upon logging in, such as whether the game-layer even wants
    // - us to join (we could fail because the server is full, or some other non network related reason), and also what BossRoomState to transition to. We do this with a Custom
    // - Named Message, which fires on the server immediately after the approval check delegate has run.
    // Why do we need to send a client GUID? What is it? Don't we already have a clientID?
    // - ClientIDs are assigned on login. If you connect to a server, then your connection drops, and you reconnect, you get a new ClientID. This makes it awkward to get back
    // - your old character, which the server is going to hold onto for a fixed timeout. To properly reconnect and recover your character, you need a persistent identifier for
    // - your own client install. We solve that by generating a random GUID and storing it in player prefs, so it persists across sessions of the game.
    public class GameNetPortal : MonoBehaviour {
        public GameObject NetworkManagerObj;
        public string PlayerName;

        public event Action NetworkReadied;
        public event Action<ConnectStatus> ConnectFinished;
        public event Action<ConnectStatus> DisconnectReasonReceived;
        public event Action<ulong, int> ClientSceneChanged;
        public event Action UserDisconnectRequested;

        public NetworkManager NetManager { get; private set; }

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);

            NetManager = NetworkManagerObj.GetComponent<NetworkManager>();
        }

        private void Start()
        {
            NetManager.OnServerStarted += OnNetworkReady;
            NetManager.OnClientConnectedCallback += ClientNetworkReadyWrapper;

            RegisterClientMessageHandlers();
            RegisterServerMessageHandlers();
        }

        private void ClientNetworkReadyWrapper(ulong clientId)
        {
            if (clientId == NetManager.LocalClientId) {
                OnNetworkReady();
            }
        }

        private void OnNetworkReady()
        {
            if (NetManager.IsHost) ConnectFinished?.Invoke(ConnectStatus.Success);

            NetworkReadied?.Invoke();
        }

        public void StartHost(string ipAddress, int port)
        {
            var chosenTransport = NetworkManager.Singleton.NetworkConfig.NetworkTransport;

            switch (chosenTransport) {
                case MLAPI.Transports.UNET.UNetTransport unetTransport:
                    unetTransport.ConnectAddress = ipAddress;
                    unetTransport.ServerListenPort = port;
                    break;
                default:
                    throw new Exception($"unhandled IpHost transport {chosenTransport.GetType()}");
            }

            NetManager.StartHost();
        }

        public void ServerToClientConnectResult(ulong netId, ConnectStatus status)
        {
            using var buffer = PooledNetworkBuffer.Get();
            using var writer = PooledNetworkWriter.Get(buffer);
            writer.WriteInt32((int) status);
            MLAPI.Messaging.CustomMessagingManager.SendNamedMessage("ServerToClientConnectResult", netId, buffer, NetworkChannel.Internal);
        }

        public void ServerToClientSetDisconnectReason(ulong netId, ConnectStatus status)
        {
            using var buffer = PooledNetworkBuffer.Get();
            using var writer = PooledNetworkWriter.Get(buffer);
            writer.WriteInt32((int) status);
            MLAPI.Messaging.CustomMessagingManager.SendNamedMessage("ServerToClientSetDisconnectReason", netId, buffer, NetworkChannel.Internal);
        }

        public void ClientToServerSceneChanged(int newScene)
        {
            if (NetManager.IsHost) {
                ClientSceneChanged?.Invoke(NetManager.ServerClientId, newScene);
            } else if (NetManager.IsConnectedClient) {
                using var buffer = PooledNetworkBuffer.Get();
                using var writer = PooledNetworkWriter.Get(buffer);
                writer.WriteInt32(newScene);
                MLAPI.Messaging.CustomMessagingManager.SendNamedMessage("ClientToServerSceneChanged", NetManager.ServerClientId, buffer, NetworkChannel.Internal);
            }
        }

        private void RegisterClientMessageHandlers()
        {
            MLAPI.Messaging.CustomMessagingManager.RegisterNamedMessageHandler("ServerToClientConnectResult", (senderClientId, stream) => {
                using var reader = PooledNetworkReader.Get(stream);
                ConnectStatus status = (ConnectStatus) reader.ReadInt32();

                ConnectFinished?.Invoke(status);
            });

            MLAPI.Messaging.CustomMessagingManager.RegisterNamedMessageHandler("ServerToClientSetDisconnectReason", (senderClientId, stream) => {
                using var reader = PooledNetworkReader.Get(stream);
                ConnectStatus status = (ConnectStatus) reader.ReadInt32();

                DisconnectReasonReceived?.Invoke(status);
            });
        }

        private void RegisterServerMessageHandlers()
        {
            MLAPI.Messaging.CustomMessagingManager.RegisterNamedMessageHandler("ClientToServerSceneChanged", (senderClientId, stream) => {
                using var reader = PooledNetworkReader.Get(stream);
                int sceneIndex = reader.ReadInt32();

                ClientSceneChanged?.Invoke(senderClientId, sceneIndex);
            });
        }

        private static void UnregisterClientMessageHandlers()
        {
            MLAPI.Messaging.CustomMessagingManager.UnregisterNamedMessageHandler("ServerToClientConnectResult");
            MLAPI.Messaging.CustomMessagingManager.UnregisterNamedMessageHandler("ServerToClientSetDisconnectReason");
        }

        private static void UnregisterServerMessageHandlers()
        {
            MLAPI.Messaging.CustomMessagingManager.UnregisterNamedMessageHandler("ClientToServerSceneChanged");
        }

        public void RequestDisconnect()
        {
            UserDisconnectRequested?.Invoke();
        }

        private void OnDestroy()
        {
            if (NetManager != null) {
                NetManager.OnServerStarted -= OnNetworkReady;
                NetManager.OnClientConnectedCallback -= ClientNetworkReadyWrapper;
            }

            UnregisterClientMessageHandlers();
            UnregisterServerMessageHandlers();
        }
    }
}