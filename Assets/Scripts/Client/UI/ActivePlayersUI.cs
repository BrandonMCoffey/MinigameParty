using System.Collections.Generic;
using Assets.Scripts.Server.Net;
using UnityEngine;

namespace Assets.Scripts.Client.UI {
    public class ActivePlayersUI : MonoBehaviour {
        [SerializeField] private ActivePlayerIdentity _playerIdentityPrefab = null;

        private Dictionary<ulong, ActivePlayerIdentity> _clientIdentityMap = new Dictionary<ulong, ActivePlayerIdentity>();

        public void AddPlayer(PlayerData playerData, string playerType)
        {
            ActivePlayerIdentity identity = Instantiate(_playerIdentityPrefab);
            identity.transform.SetParent(transform);
            _clientIdentityMap.Add(playerData.ClientId, identity);
            identity.PlayerNumber.text = "Player " + _clientIdentityMap.Count;
            identity.PlayerName.text = playerData.PlayerName;
            identity.PlayerType.text = playerType;
        }

        public void RemovePlayer(ulong clientId)
        {
            if (_clientIdentityMap.ContainsKey(clientId)) {
                ActivePlayerIdentity identity = _clientIdentityMap[clientId];
                _clientIdentityMap.Remove(clientId);
                Destroy(identity.gameObject);
            }
        }
    }
}