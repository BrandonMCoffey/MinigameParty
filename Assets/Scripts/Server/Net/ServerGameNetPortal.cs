using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Shared.Net;
using MLAPI.SceneManagement;
using UnityEngine;

namespace Assets.Scripts.Server.Net {
    public struct PlayerData {
        public string PlayerName;
        public ulong ClientId;

        public PlayerData(string playerName, ulong clientId)
        {
            PlayerName = playerName;
            ClientId = clientId;
        }
    }

    [RequireComponent(typeof(GameNetPortal))]
    public class ServerGameNetPortal : MonoBehaviour {
        private GameNetPortal _portal;
        private Dictionary<string, PlayerData> _clientData; // client guid, client player data
        private Dictionary<ulong, string> _clientIdToGuid;  // clientId, client guid
        private const int MaxConnectPayload = 1024;
        private Dictionary<ulong, int> _clientSceneMap = new Dictionary<ulong, int>(); // clientId, scene

        private const int MaxLobbyPlayers = 8;

        public int ServerScene => UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;

        private void Start()
        {
            _portal = GetComponent<GameNetPortal>();

            _portal.NetworkReadied += OnNetworkReady;
            _portal.NetManager.ConnectionApprovalCallback += ApprovalCheck;
            _portal.NetManager.OnServerStarted += ServerStartedHandler;

            _clientData = new Dictionary<string, PlayerData>();
            _clientIdToGuid = new Dictionary<ulong, string>();
        }

        private void OnNetworkReady()
        {
            if (!_portal.NetManager.IsServer) {
                enabled = false;
            } else {
                _portal.UserDisconnectRequested += OnUserDisconnectRequest;
                _portal.NetManager.OnClientDisconnectCallback += OnClientDisconnect;
                _portal.ClientSceneChanged += OnClientSceneChanged;

                // Swap to initial game scene
                NetworkSceneManager.SwitchScene("GameMenu");

                if (_portal.NetManager.IsHost) _clientSceneMap[_portal.NetManager.LocalClientId] = ServerScene;
            }
        }

        private void OnUserDisconnectRequest()
        {
            if (_portal.NetManager.IsServer) {
                _portal.NetManager.StopServer();
            }

            _clientData.Clear();
            _clientIdToGuid.Clear();
            _clientSceneMap.Clear();
        }

        private void OnClientDisconnect(ulong clientId)
        {
            _clientSceneMap.Remove(clientId);
            if (_clientIdToGuid.TryGetValue(clientId, out var guid)) {
                _clientIdToGuid.Remove(clientId);

                // Redundancy check
                if (_clientData[guid].ClientId == clientId) {
                    _clientData.Remove(guid);
                }
            }

            // Client is server, deactivate server functions
            if (clientId == _portal.NetManager.LocalClientId) {
                _portal.UserDisconnectRequested -= OnUserDisconnectRequest;
                _portal.NetManager.OnClientDisconnectCallback -= OnClientDisconnect;
                _portal.ClientSceneChanged -= OnClientSceneChanged;
            }
        }

        private void OnClientSceneChanged(ulong clientId, int sceneIndex)
        {
            _clientSceneMap[clientId] = sceneIndex;
        }

        public bool AreAllClientsInServerScene()
        {
            return _clientSceneMap.All(kvp => kvp.Value == ServerScene);
        }

        public bool IsClientInServerScene(ulong clientId)
        {
            return _clientSceneMap.TryGetValue(clientId, out int clientScene) && clientScene == ServerScene;
        }

        public PlayerData? GetPlayerData(ulong clientId)
        {
            if (_clientIdToGuid.TryGetValue(clientId, out string clientGuid)) {
                if (_clientData.TryGetValue(clientGuid, out PlayerData data)) {
                    return data;
                }
                Debug.Log("No PlayerData of matching guid found");
            } else {
                Debug.Log("No client guid found mapped to the given client ID");
            }
            return null;
        }

        public string GetPlayerName(ulong clientId, int playerNum)
        {
            var playerData = GetPlayerData(clientId);
            return playerData != null ? playerData.Value.PlayerName : ("Player" + playerNum);
        }

        /// <summary>
        /// This logic plugs into the "ConnectionApprovalCallback" exposed by MLAPI.NetworkManager, and is run every time a client connects to us.
        /// See GNH_Client.StartClient for the complementary logic that runs when the client starts its connection.
        /// </summary>
        /// <remarks>
        /// Since our game doesn't have to interact with some third party authentication service to validate the identity of the new connection, our ApprovalCheck
        /// method is simple, and runs synchronously, invoking "callback" to signal approval at the end of the method. MLAPI currently doesn't support the ability
        /// to send back more than a "true/false", which means we have to work a little harder to provide a useful error return to the client. To do that, we invoke a
        /// client RPC in the same channel that MLAPI uses for its connection callback. Since that channel ("MLAPI_INTERNAL") is both reliable and sequenced, we can be
        /// confident that our login result message will execute before any disconnect message.
        /// </remarks>
        /// <param name="connectionData">binary data passed into StartClient. In our case this is the client's GUID, which is a unique identifier for their install of the game that persists across app restarts. </param>
        /// <param name="clientId">This is the clientId that MLAPI assigned us on login. It does not persist across multiple logins from the same client. </param>
        /// <param name="callback">The delegate we must invoke to signal that the connection was approved or not. </param>
        private void ApprovalCheck(byte[] connectionData, ulong clientId, MLAPI.NetworkManager.ConnectionApprovedDelegate callback)
        {
            if (connectionData.Length > MaxConnectPayload) {
                callback(false, 0, false, null, null);
                return;
            }

            string payload = System.Text.Encoding.UTF8.GetString(connectionData);
            var connectionPayload = JsonUtility.FromJson<ConnectionPayload>(payload); // https://docs.unity3d.com/2020.2/Documentation/Manual/JSONSerialization.html
            int clientScene = connectionPayload.ClientScene;

            //a nice addition in the future will be to support rejoining the game and getting your same character back. This will require tracking a map of the GUID
            //to the player's owned character object, and cleaning that object on a timer, rather than doing so immediately when a connection is lost. 
            Debug.Log("Host ApprovalCheck: connecting client GUID: " + connectionPayload.ClientGuid);

            //TODO: GOMPS-78. We are saving the GUID, but we have more to do to fully support a reconnect flow (where you get your same character back after disconnect/reconnect).

            ConnectStatus gameReturnStatus = ConnectStatus.Success;

            //Test for Duplicate Login. 
            if (_clientData.ContainsKey(connectionPayload.ClientGuid)) {
                if (Debug.isDebugBuild) {
                    Debug.Log($"Client GUID {connectionPayload.ClientGuid} already exists. Because this is a debug build, we will still accept the connection");
                    while (_clientData.ContainsKey(connectionPayload.ClientGuid)) {
                        connectionPayload.ClientGuid += "_Secondary";
                    }
                } else {
                    ulong oldClientId = _clientData[connectionPayload.ClientGuid].ClientId;
                    StartCoroutine(WaitToDisconnectClient(oldClientId, ConnectStatus.LoggedInAgain));
                }
            }

            //Test for over-capacity Login.
            if (_clientData.Count > MaxLobbyPlayers) {
                gameReturnStatus = ConnectStatus.ServerFull;
            }

            //Populate our dictionaries with the playerData
            if (gameReturnStatus == ConnectStatus.Success) {
                _clientSceneMap[clientId] = clientScene;
                _clientIdToGuid[clientId] = connectionPayload.ClientGuid;
                _clientData[connectionPayload.ClientGuid] = new PlayerData(connectionPayload.PlayerName, clientId);
            }

            callback(false, 0, true, null, null);

            //TODO:MLAPI: this must be done after the callback for now. In the future we expect MLAPI to allow us to return more information as part of
            //the approval callback, so that we can provide more context on a reject. In the meantime we must provide the extra information ourselves,
            //and then manually close down the connection.
            _portal.ServerToClientConnectResult(clientId, gameReturnStatus);
            if (gameReturnStatus != ConnectStatus.Success) {
                //TODO-FIXME:MLAPI Issue #796. We should be able to send a reason and disconnect without a coroutine delay.
                StartCoroutine(WaitToDisconnectClient(clientId, gameReturnStatus));
            }
        }

        private IEnumerator WaitToDisconnectClient(ulong clientId, ConnectStatus reason)
        {
            _portal.ServerToClientSetDisconnectReason(clientId, reason);

            // TODO fix once this is solved: Issue 796 Unity-Technologies/com.unity.multiplayer.mlapi#796
            // this wait is a workaround to give the client time to receive the above RPC before closing the connection
            yield return new WaitForSeconds(0);

            BootClient(clientId);
        }

        public void BootClient(ulong clientId)
        {
            var netObj = MLAPI.Spawning.NetworkSpawnManager.GetPlayerNetworkObject(clientId);
            if (netObj != null) netObj.Despawn(true);
            _portal.NetManager.DisconnectClient(clientId);
        }

        private void ServerStartedHandler()
        {
            _clientData.Add("host_guid", new PlayerData(_portal.PlayerName, _portal.NetManager.LocalClientId));
            _clientIdToGuid.Add(_portal.NetManager.LocalClientId, "host_guid");
        }

        private void OnDestroy()
        {
            if (_portal == null) return;
            _portal.NetworkReadied -= OnNetworkReady;

            if (_portal.NetManager == null) return;
            _portal.NetManager.ConnectionApprovalCallback -= ApprovalCheck;
            _portal.NetManager.OnServerStarted -= ServerStartedHandler;
        }
    }
}