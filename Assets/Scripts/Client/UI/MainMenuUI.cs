using Assets.Scripts.Client.Net;
using Assets.Scripts.Shared.Net;
using UnityEngine;

namespace Assets.Scripts.Client.UI {
    public class MainMenuUI : MonoBehaviour {
        [SerializeField] private GameObject _hostBtn = null;
        [SerializeField] private GameObject _joinBtn = null;
        [SerializeField] private PopupPanel _responsePopup = null;

        private GameNetPortal _gameNetPortal;
        private ClientGameNetPortal _clientNetPortal;
        
        public const string DefaultIp = "127.0.0.1";
        public const int DefaultPort = 7777;

        private void Start()
        {
            GameObject gamePortalObject = GameObject.FindGameObjectWithTag("GameNetPortal");
            if (gamePortalObject == null) return;
            _gameNetPortal = gamePortalObject.GetComponent<GameNetPortal>();
            _clientNetPortal = gamePortalObject.GetComponent<ClientGameNetPortal>();

            _clientNetPortal.NetworkTimedOut += OnNetworkTimeout;
            _clientNetPortal.ConnectFinished += OnConnectFinished;

            //any disconnect reason set? Show it to the user here. 
            ConnectStatusToMessage(_clientNetPortal.DisconnectReason.Reason, false);
            _clientNetPortal.DisconnectReason.Clear();
        }

        public void OnHostClicked()
        {
            if (_responsePopup == null) return;
            _responsePopup.SetupEnterGameDisplay("Host Game", "Input the host IP below", "Confirm",
                (connectInput, connectPort, playerName) => {
                    if (_gameNetPortal == null) return;
                    _gameNetPortal.PlayerName = playerName;
                    _gameNetPortal.StartHost(PostProcessIpInput(connectInput), connectPort);
                }, DefaultIp, DefaultPort);
            if (_hostBtn != null) _hostBtn.SetActive(false);
            if (_joinBtn != null) _joinBtn.SetActive(false);
        }

        public void OnJoinClicked()
        {
            if (_responsePopup == null) return;
            _responsePopup.SetupEnterGameDisplay("Join Game", "Input the host IP below", "Confirm",
                (connectInput, connectPort, playerName) => {
                    if (_gameNetPortal != null) {
                        _gameNetPortal.PlayerName = playerName;
                        ClientGameNetPortal.StartClient(_gameNetPortal, connectInput, connectPort);
                    }
                    _responsePopup.SetupNotifierDisplay("Connecting", "Attempting to Join " + connectInput + ":" + connectPort, true, string.Empty);
                }, DefaultIp, DefaultPort);
            if (_hostBtn != null) _hostBtn.SetActive(false);
            if (_joinBtn != null) _joinBtn.SetActive(false);
        }

        public void OnClosePopup()
        {
            if (_responsePopup != null) {
                _responsePopup.ResetState();
                _responsePopup.gameObject.SetActive(false);
            }
            if (_hostBtn != null) _hostBtn.SetActive(true);
            if (_joinBtn != null) _joinBtn.SetActive(true);
        }

        private static string PostProcessIpInput(string ipInput)
        {
            string ipAddress = ipInput;
            if (string.IsNullOrEmpty(ipInput)) ipAddress = DefaultIp;
            return ipAddress;
        }

        private void OnConnectFinished(ConnectStatus status)
        {
            ConnectStatusToMessage(status, true);
        }

        private void ConnectStatusToMessage(ConnectStatus status, bool connecting)
        {
            switch (status) {
                case ConnectStatus.Undefined:
                case ConnectStatus.UserRequestedDisconnect:
                    break;
                case ConnectStatus.ServerFull:
                    _responsePopup.SetupNotifierDisplay("Connection Failed", "The Host is full and cannot accept any additional connections", false, "Close", OnClosePopup);
                    break;
                case ConnectStatus.Success:
                    if (connecting) {
                        _responsePopup.SetupNotifierDisplay("Success!", "Joining Now", false, string.Empty);
                    }
                    break;
                case ConnectStatus.LoggedInAgain:
                    _responsePopup.SetupNotifierDisplay("Connection Failed", "You have logged in elsewhere using the same account", false, "Close", OnClosePopup);
                    break;
                case ConnectStatus.GenericDisconnect:
                    var title = connecting ? "Connection Failed" : "Disconnected From Host";
                    var text = connecting ? "Something went wrong" : "The connection to the host was lost";
                    _responsePopup.SetupNotifierDisplay(title, text, false, "Close", OnClosePopup);
                    break;
            }
        }

        private void OnNetworkTimeout()
        {
            _responsePopup.SetupNotifierDisplay("Connection Failed", "Unable to Reach Host/Server", false, "Close", OnClosePopup);
        }

        private void OnDestroy()
        {
            if (_clientNetPortal == null) return;
            _clientNetPortal.NetworkTimedOut -= OnNetworkTimeout;
            _clientNetPortal.ConnectFinished -= OnConnectFinished;
        }
    }
}